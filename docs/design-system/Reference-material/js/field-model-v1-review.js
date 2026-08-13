(() => {
  "use strict";

  const THRESHOLD = 12;
  const Z_STEP = 51;
  const PALETTE = {
    neutral: "#EAEAEA",
    red: "#E2625C",
    violet: "#9E8CEA",
    blue: "#6EA0E2"
  };

  const fixtures = [
    {
      id: "footprints",
      title: "Footprint reach",
      description: "Size expands geometry and logarithmic reach without increasing the matter peak.",
      note: "Compare 1×1 through 8×8 sources. Every occupied square remains Q8 255.",
      bounds: [-17, 17, -10, 10],
      sources: [
        source(-14, -6, 1, 1, 0, "neutral"), source(-9, -6, 2, 1, 0, "red"),
        source(-3, -7, 1, 2, 0, "violet"), source(2, -7, 2, 2, 0, "blue"),
        source(8, -7, 4, 1, 0, "neutral"), source(15, -7, 1, 4, 0, "red"),
        source(-10, 2, 4, 4, 0, "violet"), source(3, 1, 8, 8, 0, "blue")
      ]
    },
    {
      id: "overlap",
      title: "Overlap and saturation",
      description: "Energy adds in a wide accumulator and clamps once; contributors remain observable.",
      note: "The center reaches 255 while moving any source removes only that source’s energy.",
      bounds: [-13, 13, -8, 8],
      sources: [
        source(-7, -2, 1, 1, 0, "violet"), source(-3, -2, 1, 1, 0, "violet"),
        source(4, -3, 1, 1, 0, "red"), source(8, -3, 1, 1, 0, "red"),
        source(4, 1, 1, 1, 0, "blue"), source(8, 1, 1, 1, 0, "blue"),
        source(6, -1, 1, 1, 0, "neutral")
      ]
    },
    {
      id: "colour",
      title: "Perceptual colour",
      description: "Identical tokens coalesce; different tokens mix by fixed-point OKLab energy.",
      note: "Use View to check that brightness and boundary remain legible without reliable hue.",
      bounds: [-14, 14, -8, 8],
      sources: [
        source(-10, -2, 2, 2, 0, "red"), source(-7, -2, 2, 2, 0, "red"),
        source(-1, -2, 2, 2, 0, "red"), source(2, -2, 2, 2, 0, "violet"),
        source(8, -2, 2, 2, 0, "violet"), source(11, -2, 2, 2, 0, "blue")
      ]
    },
    {
      id: "perimeters",
      title: "Field perimeters",
      description: "Borders derive from final composed neighbors, removing seams while preserving gaps.",
      note: "The left group merges; the center pair remains split; the ring preserves its enclosed gap.",
      bounds: [-17, 17, -10, 10],
      sources: [
        source(-15, -3, 2, 2, 0, "neutral"), source(-10, -3, 2, 2, 0, "neutral"),
        source(-4, -3, 1, 1, 0, "violet"), source(2, -3, 1, 1, 0, "violet"),
        source(9, -6, 2, 2, 0, "red"), source(14, -6, 2, 2, 0, "red"),
        source(9, 1, 2, 2, 0, "red"), source(14, 1, 2, 2, 0, "red")
      ]
    },
    {
      id: "layers",
      title: "Layer distance",
      description: "Ordered layer distance attenuates by Q8 51; layer identifiers do not set distance.",
      note: "Move through layers 0–6. Sources occupy adjacent and distant layer orders.",
      bounds: [-12, 12, -8, 8],
      maxLayer: 6,
      sources: [source(-5, -1, 3, 3, 0, "violet"), source(4, -1, 3, 3, 3, "red")]
    },
    {
      id: "replenishment",
      title: "Z replenishment",
      description: "A new direct contribution replenishes a directional pass without reflecting energy backward.",
      note: "The offset source on layer 2 extends the column beyond the layer-0 source’s original reach.",
      bounds: [-12, 12, -8, 8],
      maxLayer: 6,
      sources: [source(-2, -1, 3, 3, 0, "violet"), source(2, 1, 1, 1, 2, "red")]
    },
    {
      id: "signals",
      title: "Signal precedence",
      description: "Selection, focus, trace, and invalid preview remain overlays, not ambient field facts.",
      note: "The dashed invalid preview contributes no energy. Reduced motion changes transition only.",
      bounds: [-14, 14, -8, 8],
      sources: [
        source(-10, -2, 3, 3, 0, "neutral", "selected"),
        source(-3, -2, 3, 3, 0, "violet", "focused"),
        source(4, -2, 3, 3, 0, "blue", "traced")
      ],
      previews: [source(11, -2, 3, 3, 0, "red", "invalid")]
    },
    {
      id: "lod",
      title: "Continuous LOD",
      description: "Maximum presence and mean energy preserve field continuity as each sample covers more cells.",
      note: "This fixture renders aggregated 4×4 samples; the aura remains present rather than disappearing.",
      bounds: [-24, 24, -16, 16],
      lod: 2,
      sources: [
        source(-15, -7, 4, 4, 0, "violet"), source(-4, -2, 8, 8, 0, "red"),
        source(11, 3, 2, 2, 0, "blue"), source(17, -8, 1, 1, 0, "neutral")
      ]
    },
    {
      id: "stale",
      title: "Stale result rejection",
      description: "Only a tile matching model, revision, window, key, and generation may install.",
      note: "Generation 41 is discarded after revision 188 advances the visible request to generation 42.",
      special: "stale",
      bounds: [-12, 12, -8, 8],
      sources: [source(-2, -2, 4, 4, 0, "violet")]
    },
    {
      id: "extreme",
      title: "Unbounded coordinates",
      description: "Chunk windows bound work; they do not impose a coordinate, footprint, or LOD product cap.",
      note: "The local projection is identical near −2,147,483,620. Checked arithmetic governs representation.",
      bounds: [-12, 12, -8, 8],
      coordinateOffset: -2147483620,
      sources: [source(-4, -2, 8, 4, 0, "blue")]
    }
  ];

  const canvas = document.querySelector("#fieldCanvas");
  const context = canvas.getContext("2d");
  const stage = document.querySelector("#canvasStage");
  const nav = document.querySelector("#fixtureNav");
  const title = document.querySelector("#fixtureTitle");
  const description = document.querySelector("#fixtureDescription");
  const note = document.querySelector("#fixtureNote");
  const layerControl = document.querySelector("#layerControl");
  const layerRange = document.querySelector("#layerRange");
  const layerOutput = document.querySelector("#layerOutput");
  const layerAxis = document.querySelector("#layerAxis");
  const visionMode = document.querySelector("#visionMode");
  const inspection = {
    cell: document.querySelector("#inspectCell"),
    field: document.querySelector("#inspectField"),
    contributors: document.querySelector("#inspectContributors"),
    border: document.querySelector("#inspectBorder")
  };

  let activeFixture = fixtures[0];
  let observedLayer = 0;
  let renderState = null;
  let hoveredCell = null;

  fixtures.forEach((fixture, index) => {
    const button = document.createElement("button");
    button.type = "button";
    button.dataset.fixture = fixture.id;
    button.innerHTML = `<span class="number">${String(index + 1).padStart(2, "0")}</span><span>${fixture.title}</span>`;
    button.addEventListener("click", () => selectFixture(fixture));
    nav.append(button);
  });

  function source(x, y, w, h, layer, token, signal = null) {
    return { x, y, w, h, layer, token, signal };
  }

  function selectFixture(fixture) {
    activeFixture = fixture;
    observedLayer = 0;
    title.textContent = fixture.title;
    description.textContent = fixture.description;
    note.textContent = fixture.note;
    layerRange.max = String(fixture.maxLayer || 0);
    layerRange.value = "0";
    layerOutput.value = "0";
    layerControl.hidden = !fixture.maxLayer;
    nav.querySelectorAll("button").forEach((button) => {
      button.setAttribute("aria-current", String(button.dataset.fixture === fixture.id));
    });
    hoveredCell = null;
    updateLayerAxis();
    resetInspection();
    render();
  }

  function updateLayerAxis() {
    const maxLayer = activeFixture.maxLayer || 0;
    layerAxis.hidden = maxLayer === 0;
    layerAxis.innerHTML = Array.from({ length: maxLayer + 1 }, (_, layer) => {
      const className = layer === observedLayer ? "active" : "";
      const marker = activeFixture.sources.some((item) => item.layer === layer) ? "●" : "·";
      return `<span class="${className}"><b>${marker}</b>L${layer}</span>`;
    }).reverse().join("");
  }

  function render() {
    const rect = stage.getBoundingClientRect();
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.max(1, Math.round(rect.width * dpr));
    canvas.height = Math.max(1, Math.round(rect.height * dpr));
    canvas.style.width = `${rect.width}px`;
    canvas.style.height = `${rect.height}px`;
    context.setTransform(dpr, 0, 0, dpr, 0, 0);
    context.clearRect(0, 0, rect.width, rect.height);

    const [minX, maxX, minY, maxY] = activeFixture.bounds;
    const columns = maxX - minX + 1;
    const rows = maxY - minY + 1;
    const cellSize = Math.max(8, Math.min(34, Math.floor(Math.min((rect.width - 46) / columns, (rect.height - 46) / rows))));
    const width = columns * cellSize;
    const height = rows * cellSize;
    const originX = Math.round((rect.width - width) / 2);
    const originY = Math.round((rect.height - height) / 2);

    renderState = { minX, maxX, minY, maxY, columns, rows, cellSize, originX, originY, cells: new Map() };
    computeCells(renderState);
    drawGrid(renderState);

    if (activeFixture.special === "stale") drawStaleResult(renderState);
    if (activeFixture.coordinateOffset !== undefined) drawCoordinateOrigin(renderState);
    if (hoveredCell) drawHover(renderState, hoveredCell.x, hoveredCell.y);
  }

  function computeCells(state) {
    const sampleStep = 2 ** (activeFixture.lod || 0);
    for (let y = state.minY; y <= state.maxY; y += sampleStep) {
      for (let x = state.minX; x <= state.maxX; x += sampleStep) {
        const aggregate = aggregateSample(x, y, sampleStep);
        state.cells.set(key(x, y), aggregate);
      }
    }
  }

  function aggregateSample(x, y, sampleStep) {
    let maximum = emptyCell();
    let energyTotal = 0;
    let countTotal = 0;
    const colorEnergy = {};

    for (let offsetY = 0; offsetY < sampleStep; offsetY += 1) {
      for (let offsetX = 0; offsetX < sampleStep; offsetX += 1) {
        const cell = composeCell(x + offsetX, y + offsetY, observedLayer);
        if (cell.level > maximum.level) maximum = cell;
        energyTotal += cell.level;
        countTotal += cell.contributors;
        Object.entries(cell.tokens).forEach(([token, energy]) => {
          colorEnergy[token] = (colorEnergy[token] || 0) + energy;
        });
      }
    }

    const area = sampleStep * sampleStep;
    return {
      level: maximum.level,
      mean: Math.round(energyTotal / area),
      contributors: countTotal,
      tokens: colorEnergy,
      color: mixedColor(colorEnergy),
      border: 0
    };
  }

  function composeCell(x, y, layer) {
    const maxLayer = activeFixture.maxLayer || 0;
    const direct = Array.from({ length: maxLayer + 1 }, () => ({}));

    activeFixture.sources.forEach((item) => {
      const level = sourceLevel(item, x, y);
      if (level > 0) direct[item.layer][item.token] = (direct[item.layer][item.token] || 0) + level;
    });

    for (let index = 0; index < direct.length; index += 1) direct[index] = capped(direct[index]);

    const below = Array.from({ length: direct.length }, () => ({}));
    const above = Array.from({ length: direct.length }, () => ({}));
    below[0] = direct[0];
    for (let index = 1; index < direct.length; index += 1) {
      below[index] = capped(added(direct[index], attenuated(below[index - 1])));
    }
    above[above.length - 1] = direct[direct.length - 1];
    for (let index = above.length - 2; index >= 0; index -= 1) {
      above[index] = capped(added(direct[index], attenuated(above[index + 1])));
    }

    let tokens = { ...direct[layer] };
    if (layer > 0) tokens = added(tokens, attenuated(below[layer - 1]));
    if (layer < maxLayer) tokens = added(tokens, attenuated(above[layer + 1]));
    tokens = capped(tokens);

    const level = tokenTotal(tokens);
    const contributors = activeFixture.sources.filter((item) => {
      const propagated = Math.max(0, sourceLevel(item, x, y) - Math.abs(item.layer - layer) * Z_STEP);
      return propagated > 0;
    }).length;

    return { level, mean: level, contributors, tokens, color: mixedColor(tokens), border: 0 };
  }

  function sourceLevel(item, x, y) {
    const nearestX = Math.max(item.x, Math.min(x, item.x + item.w - 1));
    const nearestY = Math.max(item.y, Math.min(y, item.y + item.h - 1));
    const distance = Math.abs(x - nearestX) + Math.abs(y - nearestY);
    if (distance === 0) return 255;
    const radius = 2 + Math.ceil(Math.log2(Math.max(item.w, item.h)));
    if (distance > radius) return 0;
    if (distance === 1) return 204;
    return 51 + Math.floor((153 * (radius - distance)) / (radius - 1));
  }

  function drawGrid(state) {
    const step = 2 ** (activeFixture.lod || 0);
    const size = state.cellSize * step;
    const lineColor = visionMode.value === "contrast" ? "#55555b" : "#202023";
    context.lineWidth = 1;

    for (const [cellKey, cell] of state.cells) {
      const [x, y] = cellKey.split(":").map(Number);
      const pixelX = state.originX + (x - state.minX) * state.cellSize;
      const pixelY = state.originY + (y - state.minY) * state.cellSize;
      context.fillStyle = lineColor;
      context.fillRect(pixelX, pixelY, size, size);
      context.fillStyle = "#121213";
      context.fillRect(pixelX + 1, pixelY + 1, size - 1, size - 1);

      if (cell.level > 0) {
        const displayColor = transformedColor(cell.color, visionMode.value);
        const alpha = 0.12 + 0.68 * (cell.level / 255);
        context.fillStyle = rgba(displayColor, alpha);
        context.fillRect(pixelX + 1, pixelY + 1, size - 1, size - 1);
        if (activeFixture.lod) {
          context.fillStyle = rgba(displayColor, 0.24 * (cell.mean / 255));
          context.fillRect(pixelX + size * 0.16, pixelY + size * 0.16, size * 0.68, size * 0.68);
        }
      }
    }

    calculateBorders(state, step);
    drawBorders(state, step);
    drawSources(state);
    drawPreviews(state);
  }

  function calculateBorders(state, step) {
    const neighborBits = [[0, -step, 1], [step, 0, 2], [0, step, 4], [-step, 0, 8]];
    for (const [cellKey, cell] of state.cells) {
      if (cell.level <= THRESHOLD) continue;
      const [x, y] = cellKey.split(":").map(Number);
      cell.border = neighborBits.reduce((mask, [dx, dy, bit]) => {
        const neighbor = state.cells.get(key(x + dx, y + dy));
        return !neighbor || neighbor.level <= THRESHOLD ? mask | bit : mask;
      }, 0);
    }
  }

  function drawBorders(state, step) {
    const size = state.cellSize * step;
    context.strokeStyle = visionMode.value === "contrast" ? "#ffffff" : "rgba(214, 208, 246, 0.66)";
    context.lineWidth = visionMode.value === "contrast" ? 2 : 1;
    context.beginPath();
    for (const [cellKey, cell] of state.cells) {
      if (!cell.border) continue;
      const [x, y] = cellKey.split(":").map(Number);
      const left = state.originX + (x - state.minX) * state.cellSize + 0.5;
      const top = state.originY + (y - state.minY) * state.cellSize + 0.5;
      if (cell.border & 1) { context.moveTo(left, top); context.lineTo(left + size, top); }
      if (cell.border & 2) { context.moveTo(left + size, top); context.lineTo(left + size, top + size); }
      if (cell.border & 4) { context.moveTo(left + size, top + size); context.lineTo(left, top + size); }
      if (cell.border & 8) { context.moveTo(left, top + size); context.lineTo(left, top); }
    }
    context.stroke();
  }

  function drawSources(state) {
    activeFixture.sources.filter((item) => item.layer === observedLayer).forEach((item) => {
      const left = state.originX + (item.x - state.minX) * state.cellSize;
      const top = state.originY + (item.y - state.minY) * state.cellSize;
      const width = item.w * state.cellSize;
      const height = item.h * state.cellSize;
      const color = transformedColor(PALETTE[item.token], visionMode.value);
      context.fillStyle = rgba(color, 0.84);
      context.fillRect(left + 2, top + 2, width - 3, height - 3);
      context.strokeStyle = rgba(color, 1);
      context.lineWidth = item.signal === "focused" ? 2 : 1;
      context.strokeRect(left + 1.5, top + 1.5, width - 3, height - 3);
      drawSignal(item, left, top, width, height);
    });
  }

  function drawSignal(item, left, top, width, height) {
    if (!item.signal) return;
    context.save();
    if (item.signal === "selected") {
      context.strokeStyle = "#ffffff";
      context.lineWidth = 2;
      context.strokeRect(left - 2, top - 2, width + 4, height + 4);
    } else if (item.signal === "focused") {
      context.strokeStyle = "#f0b763";
      context.lineWidth = 2;
      context.strokeRect(left - 3, top - 3, width + 6, height + 6);
    } else if (item.signal === "traced") {
      context.setLineDash([4, 3]);
      context.strokeStyle = "#d7cdfd";
      context.lineWidth = 2;
      context.strokeRect(left - 3, top - 3, width + 6, height + 6);
    }
    context.restore();
  }

  function drawPreviews(state) {
    (activeFixture.previews || []).filter((item) => item.layer === observedLayer).forEach((item) => {
      const left = state.originX + (item.x - state.minX) * state.cellSize;
      const top = state.originY + (item.y - state.minY) * state.cellSize;
      context.save();
      context.setLineDash([5, 4]);
      context.strokeStyle = "#e2625c";
      context.lineWidth = 2;
      context.strokeRect(left + 1, top + 1, item.w * state.cellSize - 2, item.h * state.cellSize - 2);
      context.restore();
    });
  }

  function drawStaleResult(state) {
    const width = Math.min(190, state.columns * state.cellSize * 0.3);
    const left = state.originX + 14;
    const top = state.originY + 14;
    context.fillStyle = "rgba(15,15,16,0.9)";
    context.fillRect(left, top, width, 70);
    context.strokeStyle = "#3a3a3e";
    context.strokeRect(left + 0.5, top + 0.5, width - 1, 69);
    context.font = "11px SFMono-Regular, Consolas, monospace";
    context.fillStyle = "#8d8d92";
    context.fillText("rev 187 · gen 41", left + 12, top + 23);
    context.fillStyle = "#e2625c";
    context.fillText("DISCARDED", left + 12, top + 47);
    context.beginPath();
    context.moveTo(left + width - 34, top + 22);
    context.lineTo(left + width - 14, top + 42);
    context.moveTo(left + width - 14, top + 22);
    context.lineTo(left + width - 34, top + 42);
    context.strokeStyle = "#e2625c";
    context.lineWidth = 2;
    context.stroke();
  }

  function drawCoordinateOrigin(state) {
    const x = state.originX + 10;
    const y = state.originY + state.rows * state.cellSize - 12;
    context.font = "10px SFMono-Regular, Consolas, monospace";
    context.fillStyle = "#85858b";
    context.fillText(`x ${activeFixture.coordinateOffset + state.minX} … ${activeFixture.coordinateOffset + state.maxX}`, x, y);
  }

  function drawHover(state, x, y) {
    const step = 2 ** (activeFixture.lod || 0);
    const sampledX = Math.floor((x - state.minX) / step) * step + state.minX;
    const sampledY = Math.floor((y - state.minY) / step) * step + state.minY;
    const left = state.originX + (sampledX - state.minX) * state.cellSize;
    const top = state.originY + (sampledY - state.minY) * state.cellSize;
    context.strokeStyle = "#ffffff";
    context.lineWidth = 1;
    context.strokeRect(left + 1.5, top + 1.5, state.cellSize * step - 3, state.cellSize * step - 3);
  }

  function added(first, second) {
    const result = { ...first };
    Object.entries(second).forEach(([token, energy]) => {
      result[token] = (result[token] || 0) + energy;
    });
    return result;
  }

  function capped(tokens) {
    const total = tokenTotal(tokens);
    if (total <= 255) return { ...tokens };
    const result = {};
    let distributed = 0;
    const entries = Object.entries(tokens).sort(([a], [b]) => a.localeCompare(b));
    entries.forEach(([token, energy], index) => {
      const value = index === entries.length - 1 ? 255 - distributed : Math.floor((energy * 255) / total);
      result[token] = value;
      distributed += value;
    });
    return result;
  }

  function attenuated(tokens) {
    const total = tokenTotal(tokens);
    if (total <= Z_STEP) return {};
    const target = total - Z_STEP;
    const result = {};
    let distributed = 0;
    const entries = Object.entries(tokens).sort(([a], [b]) => a.localeCompare(b));
    entries.forEach(([token, energy], index) => {
      const value = index === entries.length - 1 ? target - distributed : Math.floor((energy * target) / total);
      result[token] = value;
      distributed += value;
    });
    return result;
  }

  function tokenTotal(tokens) {
    return Object.values(tokens).reduce((sum, value) => sum + value, 0);
  }

  function mixedColor(tokens) {
    const total = tokenTotal(tokens);
    if (total === 0) return PALETTE.neutral;
    const lab = Object.entries(tokens).reduce((sum, [token, energy]) => {
      const value = rgbToOklab(hexToRgb(PALETTE[token]));
      sum.L += value.L * energy;
      sum.a += value.a * energy;
      sum.b += value.b * energy;
      return sum;
    }, { L: 0, a: 0, b: 0 });
    return rgbToHex(oklabToRgb({ L: lab.L / total, a: lab.a / total, b: lab.b / total }));
  }

  function rgbToOklab({ r, g, b }) {
    const linear = [r, g, b].map((value) => {
      const normalized = value / 255;
      return normalized <= 0.04045 ? normalized / 12.92 : ((normalized + 0.055) / 1.055) ** 2.4;
    });
    const l = 0.4122214708 * linear[0] + 0.5363325363 * linear[1] + 0.0514459929 * linear[2];
    const m = 0.2119034982 * linear[0] + 0.6806995451 * linear[1] + 0.1073969566 * linear[2];
    const s = 0.0883024619 * linear[0] + 0.2817188376 * linear[1] + 0.6299787005 * linear[2];
    const lRoot = Math.cbrt(l), mRoot = Math.cbrt(m), sRoot = Math.cbrt(s);
    return {
      L: 0.2104542553 * lRoot + 0.793617785 * mRoot - 0.0040720468 * sRoot,
      a: 1.9779984951 * lRoot - 2.428592205 * mRoot + 0.4505937099 * sRoot,
      b: 0.0259040371 * lRoot + 0.7827717662 * mRoot - 0.808675766 * sRoot
    };
  }

  function oklabToRgb({ L, a, b }) {
    const l = (L + 0.3963377774 * a + 0.2158037573 * b) ** 3;
    const m = (L - 0.1055613458 * a - 0.0638541728 * b) ** 3;
    const s = (L - 0.0894841775 * a - 1.291485548 * b) ** 3;
    const linear = [
      4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s,
      -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s,
      -0.0041960863 * l - 0.7034186147 * m + 1.707614701 * s
    ];
    return Object.fromEntries(["r", "g", "b"].map((channel, index) => {
      const value = linear[index] <= 0.0031308 ? 12.92 * linear[index] : 1.055 * linear[index] ** (1 / 2.4) - 0.055;
      return [channel, Math.round(255 * Math.max(0, Math.min(1, value)))];
    }));
  }

  function transformedColor(hex, mode) {
    const rgb = hexToRgb(hex);
    if (mode === "standard") return hex;
    if (mode === "grayscale") {
      const value = Math.round(0.2126 * rgb.r + 0.7152 * rgb.g + 0.0722 * rgb.b);
      return rgbToHex({ r: value, g: value, b: value });
    }
    if (mode === "contrast") {
      return rgbToHex(Object.fromEntries(Object.entries(rgb).map(([channel, value]) => [channel, value < 128 ? 48 : 238])));
    }
    const matrices = {
      protan: [[0.567, 0.433, 0], [0.558, 0.442, 0], [0, 0.242, 0.758]],
      deutan: [[0.625, 0.375, 0], [0.7, 0.3, 0], [0, 0.3, 0.7]],
      tritan: [[0.95, 0.05, 0], [0, 0.433, 0.567], [0, 0.475, 0.525]]
    };
    const matrix = matrices[mode];
    const values = [rgb.r, rgb.g, rgb.b];
    return rgbToHex({
      r: Math.round(matrix[0].reduce((sum, value, index) => sum + value * values[index], 0)),
      g: Math.round(matrix[1].reduce((sum, value, index) => sum + value * values[index], 0)),
      b: Math.round(matrix[2].reduce((sum, value, index) => sum + value * values[index], 0))
    });
  }

  function hexToRgb(hex) {
    const value = Number.parseInt(hex.slice(1), 16);
    return { r: (value >> 16) & 255, g: (value >> 8) & 255, b: value & 255 };
  }

  function rgbToHex({ r, g, b }) {
    return `#${[r, g, b].map((value) => Math.max(0, Math.min(255, value)).toString(16).padStart(2, "0")).join("")}`;
  }

  function rgba(hex, alpha) {
    const { r, g, b } = hexToRgb(hex);
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  function emptyCell() {
    return { level: 0, mean: 0, contributors: 0, tokens: {}, color: PALETTE.neutral, border: 0 };
  }

  function key(x, y) {
    return `${x}:${y}`;
  }

  function resetInspection() {
    Object.values(inspection).forEach((element) => { element.textContent = "—"; });
  }

  function inspectAt(clientX, clientY) {
    if (!renderState) return;
    const rect = canvas.getBoundingClientRect();
    const x = Math.floor((clientX - rect.left - renderState.originX) / renderState.cellSize) + renderState.minX;
    const y = Math.floor((clientY - rect.top - renderState.originY) / renderState.cellSize) + renderState.minY;
    if (x < renderState.minX || x > renderState.maxX || y < renderState.minY || y > renderState.maxY) {
      hoveredCell = null;
      resetInspection();
      render();
      return;
    }
    const step = 2 ** (activeFixture.lod || 0);
    const sampledX = Math.floor((x - renderState.minX) / step) * step + renderState.minX;
    const sampledY = Math.floor((y - renderState.minY) / step) * step + renderState.minY;
    const cell = renderState.cells.get(key(sampledX, sampledY)) || emptyCell();
    hoveredCell = { x, y };
    const displayX = activeFixture.coordinateOffset === undefined ? x : activeFixture.coordinateOffset + x;
    inspection.cell.textContent = `${displayX}, ${y}, L${observedLayer}`;
    inspection.field.textContent = `${cell.level} / ${(cell.level / 51).toFixed(1)}`;
    inspection.contributors.textContent = String(cell.contributors);
    inspection.border.textContent = `0b${cell.border.toString(2).padStart(4, "0")}`;
    render();
  }

  layerRange.addEventListener("input", () => {
    observedLayer = Number(layerRange.value);
    layerOutput.value = String(observedLayer);
    updateLayerAxis();
    resetInspection();
    render();
  });
  visionMode.addEventListener("change", render);
  canvas.addEventListener("pointermove", (event) => inspectAt(event.clientX, event.clientY));
  canvas.addEventListener("pointerleave", () => {
    hoveredCell = null;
    resetInspection();
    render();
  });
  new ResizeObserver(render).observe(stage);

  selectFixture(fixtures[0]);
})();
