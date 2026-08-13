/**
 * trace-proto-base.js
 * Shared runtime for Trace HTML prototypes.
 *
 * Provides:
 *   - Camera (pan + zoom) with middle-drag, wheel, and trackpad pinch
 *   - Light-field cell rendering (presence + trail)
 *   - Cursor-dot tracking
 *   - Placement object helpers (pop, vanish, burst, ripple)
 *   - Minimal keyboard map (arrow navigation, number brushes, Escape, f-frame)
 *   - World transform application
 *
 * Usage:
 *   <script src="../base/trace-proto-base.js"></script>
 *   The script exports a `TraceProto` object on `window`.
 *   Variant scripts call TraceProto.init(options) to start the runtime.
 *
 * ─────────────────────────────────────────────────────────────────────────────
 * OPTIONS (passed to TraceProto.init)
 *   viewport       HTMLElement  — the .viewport element
 *   world          HTMLElement  — the .world element (transformed layer)
 *   cellSize       number       — cell size in px (default 220)
 *   initialZoom    number       — starting zoom (default 1.0)
 *   initialPan     {x, y}       — starting pan offset (default {x:0, y:0})
 *   onKeyDown      function     — optional additional keydown handler
 *   onPlacementCreate function  — called when a new placement is committed
 * ─────────────────────────────────────────────────────────────────────────────
 */
(function (global) {
  'use strict';

  /* ─────────────────────────────────────────────────────────────────────────
   * CONSTANTS
   * ───────────────────────────────────────────────────────────────────────── */
  const CELL = 220;
  const ZOOM_MIN = 0.08;
  const ZOOM_MAX = 4.0;
  const ZOOM_STEP = 0.001;
  const TRAIL_DECAY = 0.84;
  const TRAIL_MIN = 0.03;
  const TRAIL_STEADY = 0.6;
  const TRAIL_SPEED_FLOOR = 0.3;
  const TRAIL_SPEED_SPAN = 2.7;
  const AURA_FALLOFF = 0.36;  // exponential: peak * 0.36^(d-1)

  /* ─────────────────────────────────────────────────────────────────────────
   * AURA CONFIG
   * Peak/radius values from docs/spatial-architecture/04-layers.md §1A.
   * Do not retune.
   * ───────────────────────────────────────────────────────────────────────── */
  const AURA_CONFIG = [
    null,
    { peak: 0.72, radius: 2 }, // size 1 (sticky 1x1)
    { peak: 0.60, radius: 3 }, // size 2 (doc 2x2)
    null,
    { peak: 0.74, radius: 4 }, // size 4
    null, null, null,
    { peak: 0.86, radius: 5 }, // size 8
  ];

  /* ─────────────────────────────────────────────────────────────────────────
   * STATE
   * ───────────────────────────────────────────────────────────────────────── */
  let viewport, world;
  let zoom = 1.0;
  let panX = 0, panY = 0;
  let panning = false;
  let panStartX, panStartY, panOriginX, panOriginY;
  let prevMouseX = 0, prevMouseY = 0, prevMouseT = performance.now();
  let trail = {};       // key: 'col,row' → energy 0..1
  let trailRafId = null;
  let cells = {};       // key: 'col,row' → DOM element
  let placements = [];  // array of { col, row, size, el, ... }
  let activeBrush = 'neutral';
  let cursorDot = null;
  let opts = {};

  /* ─────────────────────────────────────────────────────────────────────────
   * CAMERA
   * ───────────────────────────────────────────────────────────────────────── */
  function applyTransform() {
    const px = panX + 'px', py = panY + 'px', z = zoom;
    viewport.style.setProperty('--pan-x', px);
    viewport.style.setProperty('--pan-y', py);
    viewport.style.setProperty('--zoom', z);
    if (world) world.style.transform = `translate(${px}, ${py}) scale(${z})`;
  }

  function screenToWorld(sx, sy) {
    return { x: (sx - panX) / zoom, y: (sy - panY) / zoom };
  }

  function worldToCell(wx, wy) {
    return { col: Math.floor(wx / CELL), row: Math.floor(wy / CELL) };
  }

  function screenToCell(sx, sy) {
    const w = screenToWorld(sx, sy);
    return worldToCell(w.x, w.y);
  }

  function zoomAt(cx, cy, delta) {
    const wBefore = screenToWorld(cx, cy);
    zoom = Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, zoom * (1 + delta)));
    const wAfter = screenToWorld(cx, cy);
    panX += (wAfter.x - wBefore.x) * zoom;
    panY += (wAfter.y - wBefore.y) * zoom;
    applyTransform();
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * LIGHT FIELD & TRAIL
   * ───────────────────────────────────────────────────────────────────────── */
  function cellKey(col, row) { return col + ',' + row; }

  function ensureCell(col, row) {
    const k = cellKey(col, row);
    if (cells[k]) return cells[k];
    const el = document.createElement('div');
    el.className = 'cell';
    el.style.left = col * CELL + 'px';
    el.style.top  = row * CELL + 'px';
    world.appendChild(el);
    cells[k] = el;
    return el;
  }

  function setCellBrightness(col, row, brightness, glow) {
    const el = ensureCell(col, row);
    el.style.setProperty('--b', brightness.toFixed(3));
    if (glow) el.style.setProperty('--g', glow);
    else el.style.removeProperty('--g');
  }

  function trailLoop() {
    let any = false;
    for (const k in trail) {
      trail[k] *= TRAIL_DECAY;
      if (trail[k] < TRAIL_MIN) {
        delete trail[k];
        const [c, r] = k.split(',').map(Number);
        setCellBrightness(c, r, 0);
      } else {
        const [c, r] = k.split(',').map(Number);
        setCellBrightness(c, r, trail[k]);
        any = true;
      }
    }
    trailRafId = any ? requestAnimationFrame(trailLoop) : null;
  }

  function energyFromVelocity(vx, vy, dt) {
    if (dt <= 0) return 0;
    const v = Math.sqrt(vx * vx + vy * vy) / dt;
    return Math.min(1, Math.max(0, (v - TRAIL_SPEED_FLOOR) / TRAIL_SPEED_SPAN));
  }

  function paintTrail(sx, sy) {
    const now = performance.now();
    const dt = now - prevMouseT;
    const e = energyFromVelocity(sx - prevMouseX, sy - prevMouseY, dt);
    if (e > 0) {
      const c0 = screenToCell(prevMouseX, prevMouseY);
      const c1 = screenToCell(sx, sy);
      const steps = Math.max(Math.abs(c1.col - c0.col), Math.abs(c1.row - c0.row), 1);
      for (let i = 0; i <= steps; i++) {
        const col = Math.round(c0.col + (c1.col - c0.col) * i / steps);
        const row = Math.round(c0.row + (c1.row - c0.row) * i / steps);
        const k = cellKey(col, row);
        trail[k] = Math.min(TRAIL_STEADY, (trail[k] || 0) + e * 0.4);
      }
      if (!trailRafId) trailRafId = requestAnimationFrame(trailLoop);
    }
    prevMouseX = sx; prevMouseY = sy; prevMouseT = now;
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * AURA (presence from placements — additive, capped at 1)
   * ───────────────────────────────────────────────────────────────────────── */
  function recomputeAura() {
    // Clear existing aura cells (only cells without active trail)
    for (const k in cells) {
      if (!trail[k]) {
        const [c, r] = k.split(',').map(Number);
        setCellBrightness(c, r, 0);
      }
    }
    // Accumulate aura from each placement
    const acc = {};
    for (const p of placements) {
      const cfg = AURA_CONFIG[p.size];
      if (!cfg) continue;
      for (let dc = -cfg.radius; dc <= cfg.radius + p.size - 1; dc++) {
        for (let dr = -cfg.radius; dr <= cfg.radius + p.size - 1; dr++) {
          const col = p.col + dc;
          const row = p.row + dr;
          // Chebyshev distance from nearest footprint edge
          const distC = Math.max(0, dc < 0 ? -dc : dc - (p.size - 1));
          const distR = Math.max(0, dr < 0 ? -dr : dr - (p.size - 1));
          const d = Math.max(distC, distR);
          const brightness = d === 0 ? cfg.peak : cfg.peak * Math.pow(AURA_FALLOFF, d - 1);
          const k = cellKey(col, row);
          acc[k] = Math.min(1, (acc[k] || 0) + brightness);
        }
      }
    }
    for (const k in acc) {
      const [c, r] = k.split(',').map(Number);
      setCellBrightness(c, r, acc[k]);
    }
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * PARTICLES — burst and ripple
   * ───────────────────────────────────────────────────────────────────────── */
  function spawnBurst(el) {
    const rect = el.getBoundingClientRect();
    const b = document.createElement('div');
    b.className = 'burst';
    b.style.cssText = `position:fixed;left:${rect.left}px;top:${rect.top}px;width:${rect.width}px;height:${rect.height}px;`;
    document.body.appendChild(b);
    b.addEventListener('animationend', () => b.remove(), { once: true });
  }

  function spawnRipple(sx, sy) {
    const r = document.createElement('div');
    r.className = 'ripple';
    r.style.left = sx + 'px';
    r.style.top  = sy + 'px';
    document.body.appendChild(r);
    r.addEventListener('animationend', () => r.remove(), { once: true });
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * PLACEMENT HELPERS
   * ───────────────────────────────────────────────────────────────────────── */
  function makePlacementEl(col, row, size, innerHTML) {
    const el = document.createElement('div');
    el.className = 'obj placing';
    el.style.left   = col * CELL + 'px';
    el.style.top    = row * CELL + 'px';
    el.style.width  = size * CELL + 'px';
    el.style.height = size * CELL + 'px';
    if (innerHTML) el.innerHTML = innerHTML;
    el.addEventListener('animationend', () => el.classList.remove('placing'), { once: true });
    world.appendChild(el);
    return el;
  }

  function removePlacement(p) {
    p.el.classList.add('removing');
    p.el.addEventListener('animationend', () => p.el.remove(), { once: true });
    placements = placements.filter(x => x !== p);
    recomputeAura();
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * FRAMING
   * ───────────────────────────────────────────────────────────────────────── */
  function frameAll() {
    if (!placements.length) { panX = 0; panY = 0; zoom = 1; applyTransform(); return; }
    const pad = CELL;
    const minCol = Math.min(...placements.map(p => p.col));
    const maxCol = Math.max(...placements.map(p => p.col + p.size));
    const minRow = Math.min(...placements.map(p => p.row));
    const maxRow = Math.max(...placements.map(p => p.row + p.size));
    const vW = viewport.offsetWidth, vH = viewport.offsetHeight;
    const contentW = (maxCol - minCol) * CELL + pad * 2;
    const contentH = (maxRow - minRow) * CELL + pad * 2;
    zoom = Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, Math.min(vW / contentW, vH / contentH)));
    panX = vW / 2 - ((minCol + maxCol) / 2) * CELL * zoom;
    panY = vH / 2 - ((minRow + maxRow) / 2) * CELL * zoom;
    applyTransform();
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * EVENT WIRING
   * ───────────────────────────────────────────────────────────────────────── */
  function onMouseDown(e) {
    if (e.button === 1) {
      panning = true;
      panStartX = e.clientX; panStartY = e.clientY;
      panOriginX = panX; panOriginY = panY;
      e.preventDefault();
    }
  }

  function onMouseMove(e) {
    if (cursorDot) { cursorDot.style.left = e.clientX + 'px'; cursorDot.style.top = e.clientY + 'px'; }
    paintTrail(e.clientX, e.clientY);
    if (panning) {
      panX = panOriginX + (e.clientX - panStartX);
      panY = panOriginY + (e.clientY - panStartY);
      applyTransform();
    }
  }

  function onMouseUp(e) {
    if (e.button === 1) panning = false;
  }

  function onWheel(e) {
    e.preventDefault();
    if (e.ctrlKey) {
      zoomAt(e.clientX, e.clientY, -e.deltaY * ZOOM_STEP * 5);
    } else if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) {
      panX -= e.deltaX;
      panY -= e.deltaY;
      applyTransform();
    } else {
      zoomAt(e.clientX, e.clientY, -e.deltaY * ZOOM_STEP);
    }
  }

  function onKeyDown(e) {
    const tag = e.target && e.target.tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA' || (e.target && e.target.isContentEditable)) return;
    if (e.key === 'f' && !e.shiftKey && !e.ctrlKey && !e.metaKey) { e.preventDefault(); frameAll(); return; }
    if (e.key === 'Escape') { activeBrush = 'neutral'; return; }
    const brushes = { '1': 'neutral', '2': 'note', '3': 'doc', '4': 'text', '5': 'sketch' };
    if (!e.ctrlKey && !e.metaKey && brushes[e.key]) { e.preventDefault(); activeBrush = brushes[e.key]; return; }
    if (opts.onKeyDown) opts.onKeyDown(e, { activeBrush, zoom, panX, panY });
  }

  /* ─────────────────────────────────────────────────────────────────────────
   * PUBLIC API
   * ───────────────────────────────────────────────────────────────────────── */
  const TraceProto = {
    init(options) {
      opts      = options || {};
      viewport  = opts.viewport || document.querySelector('.viewport');
      world     = opts.world   || document.querySelector('.world');
      cursorDot = document.querySelector('.cursor-dot');
      zoom      = opts.initialZoom || 1.0;
      panX      = (opts.initialPan && opts.initialPan.x) || 0;
      panY      = (opts.initialPan && opts.initialPan.y) || 0;

      applyTransform();

      viewport.addEventListener('mousedown', onMouseDown);
      window.addEventListener('mousemove', onMouseMove);
      window.addEventListener('mouseup', onMouseUp);
      viewport.addEventListener('wheel', onWheel, { passive: false });
      window.addEventListener('keydown', onKeyDown);
      return this;
    },

    /** Register a placement object for aura computation. */
    addPlacement(p) { placements.push(p); recomputeAura(); return p; },

    /** Remove a placement (plays vanish animation). */
    removePlacement(p) { removePlacement(p); },

    /**
     * Create a positioned .obj div with the pop entrance animation.
     * @param {number} col
     * @param {number} row
     * @param {number} size  — in cells (1, 2, 4, 8)
     * @param {string} [innerHTML]
     * @returns {HTMLElement}
     */
    makePlacementEl(col, row, size, innerHTML) {
      return makePlacementEl(col, row, size, innerHTML);
    },

    /** Spawn a burst particle at the placement element's screen position. */
    spawnBurst(el) { spawnBurst(el); },

    /** Spawn a ripple particle at screen coordinates. */
    spawnRipple(sx, sy) { spawnRipple(sx, sy); },

    /** Frame all placements, or reset camera if none. */
    frameAll() { frameAll(); },

    /** Convert screen px to grid cell {col, row}. */
    screenToCell(sx, sy) { return screenToCell(sx, sy); },

    get activeBrush() { return activeBrush; },
    set activeBrush(v) { activeBrush = v; },
    get zoom() { return zoom; },
    get panX() { return panX; },
    get panY() { return panY; },
  };

  global.TraceProto = TraceProto;

}(window));
