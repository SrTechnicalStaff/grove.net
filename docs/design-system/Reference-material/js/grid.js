    const CELL=220, SIZES=[1,2,4,8], TYPES=['neutral','note','doc','image'];
    const NOTE_COLORS=[{fill:'#B0524E',glow:'226 98 92'},{fill:'#6E62A6',glow:'158 140 234'},{fill:'#4E6E9C',glow:'110 160 226'}];
    const viewport=document.getElementById('viewport'), world=document.getElementById('world');
    const cellsEl=document.getElementById('cells'), trailEl=document.getElementById('trail'), objsEl=document.getElementById('objs');
    const ghost=document.getElementById('ghost'), ghostLabel=ghost.querySelector('.glabel');
    const marqueeEl=document.getElementById('marquee');
    const hud=document.getElementById('hud'), hudPip=document.getElementById('hudPip');
    const selcountEl=document.getElementById('selcount');

    let objects=[
      { type:'sheet',  col:-9, row:-2, size:4, layer:0, label:'Conflict policy', title:'Sync Ledger',
        body:'Local changes become ledger entries. When two edits disagree, the conflict is written down as a record — never a silent overwrite.', block:['N° 02','Ledger'] },
      { type:'sheet',  col:-3, row:-4, size:2, layer:0, label:'Canonical store', title:'Authority', body:'The SQLite store of record. Edits resolve here first.', block:['N° 01','SQLite'] },
      { type:'sticky', col:2,  row:-2, size:1, layer:0, note:'Conflict UX — who wins?', fill:'#B0524E', glow:'226 98 92' },
      { type:'sticky', col:3,  row:-1, size:1, layer:0, note:'Pin plan to origin.',      fill:'#6E62A6', glow:'158 140 234' },
      { type:'figure', col:-3, row:2,  size:2, layer:1, fig:'Fig. 01 — Site' },
      { type:'sticky', col:5,  row:2,  size:1, layer:1, note:'(lives on layer 2)',       fill:'#4E6E9C', glow:'110 160 226' },
    ];
    let layers=[{id:0,name:null}], curId=0, nextAboveId=1, nextBelowId=1;

    let view={x:0,y:0,s:1};
    let mode='idle';
    let pan=null, createAnchor=null, press=null, move=null, resize=null;
    let brush={ type:'neutral', color:0 };
    let selected=new Set(), docSeq=3, figSeq=2, lastHoverCell={col:0,row:0}, inside=false;
    let marqueeState=null;
    let traceState=null;
    let resizeMode=false, resizeStart=null;

    function apply(){ viewport.style.setProperty('--zoom',view.s); viewport.style.setProperty('--pan-x',view.x+'px'); viewport.style.setProperty('--pan-y',view.y+'px'); renderObjectField(); }
    function cellAt(sx,sy){ return { col:Math.floor((sx-view.x)/view.s/CELL), row:Math.floor((sy-view.y)/view.s/CELL) }; }
    function curIndex(){ return layers.findIndex(l=>l.id===curId); }
    function objAtCell(c,r){ for(const o of objects){ if(o.layer!==curId) continue; if(c>=o.col&&c<o.col+o.size&&r>=o.row&&r<o.row+o.size) return o; } return null; }
    function frameAll(){
      const cur=objects.filter(o=>o.layer===curId); const set=cur.length?cur:objects;
      if(!set.length){ view={x:innerWidth/2,y:innerHeight/2,s:1}; apply(); return; }
      let a=Infinity,b=Infinity,c=-Infinity,d=-Infinity;
      for(const o of set){ a=Math.min(a,o.col*CELL); b=Math.min(b,o.row*CELL); c=Math.max(c,(o.col+o.size)*CELL); d=Math.max(d,(o.row+o.size)*CELL); }
      const pad=200, cw=c-a+pad*2, ch=d-b+pad*2, s=Math.min(innerWidth/cw,innerHeight/ch,1.1);
      view.s=s; view.x=innerWidth/2-((a+c)/2)*s; view.y=innerHeight/2-((b+d)/2)*s; apply();
    }

    const PH_IMG='<rect x="3" y="3" width="18" height="18" rx="1"/><circle cx="8.5" cy="8.5" r="1.6"/><path d="M21 16l-5-5L5 21"/>';
    function fillObjEl(el,o){
      let inner='';
      if(o.type==='sheet'){
        inner=`<div class="sheet__label">${o.label||'Document'}</div>
          <div class="sheet__title" contenteditable data-ph="Untitled">${o.title||''}</div>
          <div class="sheet__rule"></div>
          <div class="sheet__body" contenteditable data-ph="Write…">${o.body||''}</div>
          <div class="sheet__spacer"></div>
          <div class="titleblock"><div>${o.block[0]}</div><div>${o.block[1]}</div></div>`;
      } else if(o.type==='figure'){
        const a=o.src?`<img src="${o.src}" alt="">`:(o.tone?'':`<div class="ph"><svg viewBox="0 0 24 24">${PH_IMG}</svg><span>Image</span></div>`);
        inner=`<div class="img-area"${o.tone?` style="background:${o.tone}"`:''}>${a}</div><div class="img-caption"><span>${o.fig}</span><span class="dim">${o.size}×${o.size}</span></div>`;
      } else { el.style.background=o.fill; inner=`<div class="note" contenteditable data-ph="Note…">${o.note||''}</div>`; }
      el.innerHTML=inner+'<div class="handle"></div>';
    }
    function addObjectEl(o,animate){
      const el=document.createElement('div'); el.className='obj '+o.type;
      el.style.left=(o.col*CELL)+'px'; el.style.top=(o.row*CELL)+'px'; el.style.width=(o.size*CELL)+'px'; el.style.height=(o.size*CELL)+'px';
      fillObjEl(el,o); o._el=el; el._obj=o; objsEl.appendChild(el);
      if(animate){ el.classList.add('placing'); el.addEventListener('animationend',()=>el.classList.remove('placing'),{once:true});
        const bu=document.createElement('div'); bu.className='burst'; bu.style.left=(o.col*CELL)+'px'; bu.style.top=(o.row*CELL)+'px';
        bu.style.width=(o.size*CELL)+'px'; bu.style.height=(o.size*CELL)+'px'; bu.style.setProperty('--bg',o.glow||'234 234 234');
        world.appendChild(bu); bu.addEventListener('animationend',()=>bu.remove(),{once:true}); }
    }
    function buildObjects(){ objsEl.innerHTML=''; for(const o of objects) addObjectEl(o,false); }
    function geom(o){ o._el.style.left=(o.col*CELL)+'px'; o._el.style.top=(o.row*CELL)+'px'; o._el.style.width=(o.size*CELL)+'px'; o._el.style.height=(o.size*CELL)+'px'; }
    function spawnRipple(sx,sy){ const r=document.createElement('div'); r.className='ripple'; r.style.left=sx+'px'; r.style.top=sy+'px'; viewport.appendChild(r); r.addEventListener('animationend',()=>r.remove(),{once:true}); }

    function selectObj(o,shift){
      if(!shift){ for(const s of selected) if(s._el) s._el.classList.remove('selected'); selected.clear(); }
      if(selected.has(o)){ selected.delete(o); o._el.classList.remove('selected'); }
      else { selected.add(o); o._el.classList.add('selected'); }
      updateSelCount(); renderObjectField();
    }
    function deselect(){ for(const s of selected) if(s._el) s._el.classList.remove('selected'); selected.clear(); updateSelCount(); renderObjectField(); cancelTrace(); }
    function updateSelCount(){
      if(selected.size>1){ selcountEl.textContent=selected.size+' selected'; selcountEl.classList.add('visible'); }
      else { selcountEl.textContent=''; selcountEl.classList.remove('visible'); }
    }

    function armTrace(){
      if(selected.size===0) return;
      traceState={items:[...selected].map(o=>({type:o.type,col:o.col,row:o.row,size:o.size,layer:o.layer,note:o.note,fill:o.fill,glow:o.glow,title:o.title,body:o.body,label:o.label,block:o.block,fig:o.fig,src:o.src}))};
      cursorGlow=MODE_TRACE;
      for(const s of selected) if(s._el) s._el.classList.add('traced');
      renderObjectField();
    }
    function stampTrace(){
      if(!traceState) return;
      if(traceState.items[0].layer===curId){ cancelTrace(); return; }
      let anyPlaced=false;
      for(const item of traceState.items){
        const fp={col:item.col,row:item.row,size:item.size};
        if(overlaps(fp.col,fp.row,fp.size,null)){
          const spot=freeSpot(fp.col,fp.row,fp.size);
          fp.col=spot.col; fp.row=spot.row;
        }
        let o;
        if(item.type==='sticky'){ o={type:'sticky',col:fp.col,row:fp.row,size:fp.size,layer:curId,note:item.note,fill:item.fill,glow:item.glow}; }
        else if(item.type==='sheet'){ o={type:'sheet',col:fp.col,row:fp.row,size:fp.size,layer:curId,label:item.label,title:item.title,body:item.body,block:item.block}; }
        else { o={type:'figure',col:fp.col,row:fp.row,size:fp.size,layer:curId,fig:item.fig,src:item.src}; }
        objects.push(o); addObjectEl(o,true); anyPlaced=true;
      }
      if(anyPlaced){ renderLayers(); renderObjectField(); }
    }
    function cancelTrace(){
      for(const s of selected) if(s._el) s._el.classList.remove('traced');
      traceState=null; hideGhost(); cursorGlow='234 234 234';
      renderObjectField();
    }
    function showTraceGhost(){
      if(!traceState||!traceState.items.length) return;
      const item=traceState.items[0];
      const fp={col:item.col,row:item.row,size:item.size};
      const blocked=overlaps(fp.col,fp.row,fp.size,null);
      ghost.style.display='block';
      ghost.style.left=(fp.col*CELL)+'px'; ghost.style.top=(fp.row*CELL)+'px';
      ghost.style.width=(fp.size*CELL)+'px'; ghost.style.height=(fp.size*CELL)+'px';
      ghost.className='ghost'+(blocked?' invalid':' trace');
      ghost.style.setProperty('--gh',MODE_TRACE);
      ghostLabel.textContent=traceState.items.length+' item'+(traceState.items.length>1?'s':'');
    }
    const MODE_RESIZE='90 130 90';
    const MODE_TRACE='196 138 122';
    let cursorGlow='234 234 234';
    function enterResizeMode(){
      if(selected.size===0) return;
      resizeMode=true;
      const first=[...selected][0];
      resizeStart={col:first.col,row:first.row,items:[...selected].map(o=>({obj:o,startSize:o.size}))};
      cursorGlow=MODE_RESIZE;
    }
    function exitResizeMode(){ resizeMode=false; resizeStart=null; hideGhost(); cursorGlow='234 234 234'; }
    function showResizeGhost(targetSize){
      if(!resizeStart) return;
      const ignoreSet=new Set(resizeStart.items.map(i=>i.obj));
      const blocked=resizeStart.items.some(({obj})=>overlaps(obj.col,obj.row,targetSize,targetSize,ignoreSet));
      const first=resizeStart.items[0].obj;
      ghost.style.display='block';
      ghost.style.left=(first.col*CELL)+'px'; ghost.style.top=(first.row*CELL)+'px';
      ghost.style.width=(targetSize*CELL)+'px'; ghost.style.height=(targetSize*CELL)+'px';
      ghost.className='ghost'+(blocked?' invalid':'');
      ghost.style.setProperty('--gh',MODE_RESIZE);
      ghostLabel.textContent=targetSize+'×'+targetSize;
    }
    function applyResize(targetSize){
      if(!resizeStart) return;
      const ignoreSet=new Set(resizeStart.items.map(i=>i.obj));
      let anyBad=false;
      for(const {obj} of resizeStart.items){
        if(overlaps(obj.col,obj.row,targetSize,targetSize,ignoreSet)) anyBad=true;
      }
      if(!anyBad){
        for(const {obj} of resizeStart.items){ obj.size=targetSize; geom(obj); }
      }
      exitResizeMode();
      renderObjectField();
    }
    function renderLayers(){
      for(const o of objects){ if(!o._el) continue; const active=o.layer===curId;
        o._el.style.opacity=active?'1':'0'; o._el.style.pointerEvents=active?'auto':'none'; }
      updateLayerChip();
    }
    function afterLayer(){ renderLayers(); renderObjectField(); refreshHover(); }
    function switchLayer(d){ const i=curIndex()+d; if(i<0||i>=layers.length) return; curId=layers[i].id; deselect(); afterLayer();
      if(traceState){ showTraceGhost(); } else { hideGhost(); } }
    function addLayer(d){
      const baseIdx=layers.findIndex(l=>l.id===0);
      let newIdx, label;
      if(d>0){
        let lastAbove=baseIdx;
        for(let i=baseIdx+1;i<layers.length;i++){
          if(!layers[i].label?.startsWith('B')) lastAbove=i;
          else break;
        }
        newIdx=lastAbove+1;
        label=String(++nextAboveId).padStart(2,'0');
      } else {
        let firstBelow=baseIdx;
        for(let i=baseIdx-1;i>=0;i--){
          if(layers[i].label?.startsWith('B')) firstBelow=i;
          else break;
        }
        newIdx=firstBelow;
        label='B'+String(++nextBelowId).padStart(2,'0');
      }
      const nl={id:Date.now(),name:null,label};
      layers.splice(newIdx,0,nl);
      curId=nl.id;
      deselect(); afterLayer();
    }
    function bringSel(d){ if(!selected.size) return;
      for(const moved of selected){ let ti=layers.findIndex(l=>l.id===moved.layer)+d;
        if(ti<0){ layers.unshift({id:nextLayerId++}); ti=0; } else if(ti>=layers.length){ layers.push({id:nextLayerId++}); ti=layers.length-1; }
        moved.layer=layers[ti].id;
        moved._el.classList.add('brought'); moved._el.addEventListener('animationend',()=>moved._el.classList.remove('brought'),{once:true}); }
      deselect(); renderLayers(); renderObjectField(); }

    const layerrenameEl=document.getElementById('layerrename'), layerrenameInput=document.getElementById('layerrenameinput');
    function deleteLayer(){
      if(layers.length<=1) return;
      const idx=curIndex();
      if(layers[idx].id===0) return;
      const hasContent=objects.some(o=>o.layer===curId);
      if(hasContent) return;
      layers.splice(idx,1);
      curId=layers[Math.min(idx,layers.length-1)].id;
      deselect(); afterLayer();
    }
    function moveLayer(d){
      const idx=curIndex();
      if(layers[idx].id===0) return;
      const ni=idx+d;
      if(ni<0||ni>=layers.length) return;
      if(layers[ni].id===0) return;
      const temp=layers[idx];
      layers[idx]=layers[ni];
      layers[ni]=temp;
      afterLayer();
    }
    function startRenameLayer(){
      const layer=layers[curIndex()];
      layerrenameInput.value=layer.name||'';
      layerrenameEl.classList.add('open');
      setTimeout(()=>{ layerrenameInput.focus(); layerrenameInput.select(); },0);
    }
    function finishRenameLayer(){
      const layer=layers[curIndex()];
      layer.name=layerrenameInput.value.trim()||null;
      layerrenameEl.classList.remove('open');
      updateLayerChip();
    }
    function updateWatermark(){
      const layer=layers[curIndex()];
      const wkname=document.getElementById('wkname');
      const wknum=document.getElementById('wknum');
      wkname.textContent=layer.name||'Untitled';
      wknum.textContent=String(curIndex()+1).padStart(2,'0');
    }
    function getLayerLabel(layer){
      if(layer.id===0) return '01';
      return layer.label||'01';
    }
    function updateLayerChip(){
      const layer=layers[curIndex()];
      const wkname=document.getElementById('wkname');
      const wknum=document.getElementById('wknum');
      wkname.textContent=layer.name||'Untitled';
      wknum.textContent=getLayerLabel(layer);
    }

    const DECAY=0.36, THRESH=0.05, LAYER_STEP=0.3, MAXB=1.0;
    function rampFor(s){ if(s.type==='sticky') return {peak:0.72,R:Math.max(2,s.size+1)}; if(s.size>=8) return {peak:0.86,R:5}; if(s.size===4) return {peak:0.74,R:4}; if(s.size===2) return {peak:0.60,R:3}; return {peak:0.50,R:2}; }
    const SELECT_GLOW='180 210 255';
    const TRACE_GLOW='196 138 122';
    function renderObjectField(){
      const cz=curIndex(); const acc=new Map();
      for(const s of objects){
        const isOnActiveLayer=s.layer===curId;
        const sz=layers.findIndex(l=>l.id===s.layer), atten=Math.abs(sz-cz)*LAYER_STEP, ramp=rampFor(s), size=s.size;
        const isSelected=selected.has(s);
        const isTraced=s._el&&s._el.classList.contains('traced');
        const gp=(isTraced?TRACE_GLOW:(isSelected?SELECT_GLOW:(s.glow||'234 234 234'))).split(' ').map(Number);
        for(let c=s.col-ramp.R;c<s.col+size+ramp.R;c++) for(let r=s.row-ramp.R;r<s.row+size+ramp.R;r++){
          const dC=c<s.col?s.col-c:(c>s.col+size-1?c-(s.col+size-1):0); const dR=r<s.row?s.row-r:(r>s.row+size-1?r-(s.row+size-1):0);
          const dd=dC+dR; if(dd>ramp.R) continue;
          let v=(dd===0?ramp.peak:ramp.peak*Math.pow(DECAY,dd-1))-atten; if(v<THRESH) continue;
          const k=c+','+r; let a=acc.get(k); if(!a){ a={b:0,r:0,g:0,l:0,s:0,t:0,active:false}; acc.set(k,a); }
          a.b+=v; a.r+=v*gp[0]; a.g+=v*gp[1]; a.l+=v*gp[2]; if(isSelected) a.s++; if(isTraced) a.t++; if(isOnActiveLayer) a.active=true;
        }
      }
      function hasActivePresence(c,r){ const a=acc.get(c+','+r); return a&&a.b>=THRESH&&a.active; }
      const frag=document.createDocumentFragment();
      for(const [k,a] of acc){ if(a.b<THRESH) continue; const [c,r]=k.split(',').map(Number);
        const b=Math.min(a.b,MAXB), col=Math.round(a.r/a.b)+' '+Math.round(a.g/a.b)+' '+Math.round(a.l/a.b);
        const drawBorder=a.active;
        const bl=drawBorder&&!hasActivePresence(c-1,r), br=drawBorder&&!hasActivePresence(c+1,r);
        const bt=drawBorder&&!hasActivePresence(c,r-1), bb=drawBorder&&!hasActivePresence(c,r+1);
        const hasBorder=bl||br||bt||bb;
        const cls=hasBorder?(a.s>0?'cell ring-sel':'cell ring'):'cell';
        const el=document.createElement('div'); el.className=cls;
        el.style.left=(c*CELL)+'px'; el.style.top=(r*CELL)+'px';
        el.style.setProperty('--g',col); el.style.setProperty('--b',b);
        const bw = Math.max(1.5, 1.5 / Math.max(view.s, 0.01)).toFixed(2) + 'px';
        if(hasBorder){ el.style.setProperty('--bl',bl?bw:'0'); el.style.setProperty('--br',br?bw:'0');
          el.style.setProperty('--bt',bt?bw:'0'); el.style.setProperty('--bb',bb?bw:'0'); }
        frag.appendChild(el); }
      cellsEl.replaceChildren(frag);
    }

    const STEADY=0.6, TRAIL_DECAY=0.84, TRAIL_MIN=0.03, SPEED_FLOOR=0.3, SPEED_SPAN=2.7;
    const trail=new Map(), trailPool=new Map(); let head=null, lastCell=null, lastPos=null, raf=0;
    function deposit(a,b,energy){ const steps=Math.max(Math.abs(b.col-a.col),Math.abs(b.row-a.row)), n=Math.min(Math.max(steps,1),40);
      for(let i=0;i<=n;i++){ const t=i/n, c=Math.round(a.col+(b.col-a.col)*t), r=Math.round(a.row+(b.row-a.row)*t), k=c+','+r; trail.set(k,Math.max(trail.get(k)||0,energy)); } }
    function loop(){ for(const [k,e] of trail){ const ne=e*TRAIL_DECAY; if(ne<TRAIL_MIN) trail.delete(k); else trail.set(k,ne); }
      const render=new Map(trail); if(head) render.set(head,Math.max(render.get(head)||0,STEADY));
      for(const [k,e] of render){ let el=trailPool.get(k); if(!el){ el=document.createElement('div'); el.className='cell'; const [c,r]=k.split(',').map(Number); el.style.left=(c*CELL)+'px'; el.style.top=(r*CELL)+'px'; trailEl.appendChild(el); trailPool.set(k,el); } el.style.setProperty('--g',cursorGlow); el.style.setProperty('--b',e); }
      for(const [k,el] of trailPool){ if(!render.has(k)){ el.remove(); trailPool.delete(k); } }
      raf=(trail.size>0||head)?requestAnimationFrame(loop):0; }
    function ensureLoop(){ if(!raf) raf=requestAnimationFrame(loop); }

    function snapSize(ext){ for(const s of SIZES) if(s>=ext) return s; return 8; }
    function overlaps(col,row,size,ignoreSet){ for(const o of objects){ if(o.layer!==curId) continue; if(ignoreSet instanceof Set?ignoreSet.has(o):o===ignoreSet) continue; if(col<o.col+o.size && col+size>o.col && row<o.row+o.size && row+size>o.row) return true; } return false; }
    function freeSpot(col,row,size){ for(let rad=0;rad<24;rad++){ for(let dc=-rad;dc<=rad;dc++) for(let dr=-rad;dr<=rad;dr++){ if(Math.max(Math.abs(dc),Math.abs(dr))!==rad) continue; if(!overlaps(col+dc,row+dr,size,null)) return {col:col+dc,row:row+dr,size}; } } return {col,row,size}; }
    function footprint(anchor,cur){ const dC=cur.col-anchor.col, dR=cur.row-anchor.row, ext=Math.max(Math.abs(dC),Math.abs(dR))+1, size=snapSize(ext);
      return { col:dC<0?anchor.col-(size-1):anchor.col, row:dR<0?anchor.row-(size-1):anchor.row, size }; }
    function showGhost(fp){ ghost.style.display='block'; ghost.style.left=(fp.col*CELL)+'px'; ghost.style.top=(fp.row*CELL)+'px';
      ghost.style.width=(fp.size*CELL)+'px'; ghost.style.height=(fp.size*CELL)+'px';
      const glow=brush.type==='note'?NOTE_COLORS[brush.color].glow:'234 234 234'; ghost.style.setProperty('--gh',glow);
      ghost.classList.toggle('invalid',overlaps(fp.col,fp.row,fp.size,null)); ghostLabel.textContent=`${brush.type} · ${fp.size}×${fp.size}`; }
    function hideGhost(){ ghost.style.display='none'; }
    const isTool=()=>brush.type!=='neutral';
    function showMarquee(minC,minR,maxC,maxR){ marqueeEl.classList.add('active');
      marqueeEl.style.left=(minC*CELL)+'px'; marqueeEl.style.top=(minR*CELL)+'px';
      marqueeEl.style.width=((maxC-minC+1)*CELL)+'px'; marqueeEl.style.height=((maxR-minR+1)*CELL)+'px'; }
    function hideMarquee(){ marqueeEl.classList.remove('active'); }

    function refreshHover(){ if(mode!=='idle'||!inside) return;
      if(isTool()){ head=null; if(objAtCell(lastHoverCell.col,lastHoverCell.row)) hideGhost(); else showGhost({col:lastHoverCell.col,row:lastHoverCell.row,size:1}); }
      else { hideGhost(); head=lastHoverCell.col+','+lastHoverCell.row; ensureLoop(); } }

    function createAt(fp){
      let o;
      if(brush.type==='note'){ const c=NOTE_COLORS[brush.color]; o={type:'sticky',col:fp.col,row:fp.row,size:fp.size,layer:curId,note:'',fill:c.fill,glow:c.glow}; }
      else if(brush.type==='doc'){ const n=String(docSeq++).padStart(2,'0'); o={type:'sheet',col:fp.col,row:fp.row,size:fp.size,layer:curId,label:'Document',title:'',body:'',block:['N° '+n,'Doc']}; }
      else { const n=String(figSeq++).padStart(2,'0'); o={type:'figure',col:fp.col,row:fp.row,size:fp.size,layer:curId,fig:'Fig. '+n}; }
      objects.push(o); addObjectEl(o,true); renderLayers(); renderObjectField(); selectObj(o);
      const ed=o._el.querySelector('[contenteditable]'); if(ed) setTimeout(()=>ed.focus(),0);
    }
    function addImageAt(cell,src){ const fp={col:cell.col,row:cell.row,size:2}; if(overlaps(fp.col,fp.row,fp.size,null)) return;
      const n=String(figSeq++).padStart(2,'0'); const o={type:'figure',col:fp.col,row:fp.row,size:2,layer:curId,fig:'Fig. '+n,src}; objects.push(o); addObjectEl(o,true); renderLayers(); renderObjectField(); selectObj(o); }
    function recolor(o,idx){ const c=NOTE_COLORS[idx]; o.fill=c.fill; o.glow=c.glow; if(o._el) o._el.style.background=c.fill; renderObjectField(); }
    function editObjectAt(objEl,clientX,clientY){ const t=document.elementFromPoint(clientX,clientY); let ed=(t&&t.closest)?t.closest('[contenteditable]'):null; if(!ed) ed=objEl.querySelector('[contenteditable]'); if(!ed) return; ed.focus();
      const range=document.caretRangeFromPoint?document.caretRangeFromPoint(clientX,clientY):null; if(range){ const sel=getSelection(); sel.removeAllRanges(); sel.addRange(range); } }
    function deleteObj(o){ const i=objects.indexOf(o); if(i<0) return; objects.splice(i,1); const el=o._el; selected.delete(o); el.classList.add('removing'); el.addEventListener('animationend',()=>el.remove(),{once:true}); renderObjectField(); updateSelCount(); }

    let lastClickTime=0, lastClickObj=null;
    viewport.addEventListener('pointerdown',(e)=>{
      const r=viewport.getBoundingClientRect(), sx=e.clientX-r.left, sy=e.clientY-r.top;
      if(e.button===1){ e.preventDefault(); mode='pan'; pan={x:e.clientX,y:e.clientY,vx:view.x,vy:view.y}; head=null; hideGhost(); viewport.setPointerCapture(e.pointerId); return; }
      if(e.button!==0) return;
      if(resizeMode){ resizeStart.cur=cellAt(sx,sy); mode='resizedrag'; viewport.setPointerCapture(e.pointerId); return; }
      const objEl=e.target.closest('.obj');
      if(objEl){ if(objEl.contains(document.activeElement)&&document.activeElement.isContentEditable) return;
        const now=Date.now(); const isDblClick=(now-lastClickTime<400&&lastClickObj===objEl._obj);
        if(isDblClick){ editObjectAt(objEl,e.clientX,e.clientY); lastClickTime=0; lastClickObj=null; return; }
        lastClickTime=now; lastClickObj=objEl._obj;
        if(!e.shiftKey&&!selected.has(objEl._obj)){ deselect(); }
        press={objEl,o:objEl._obj,sx,sy,cell:cellAt(sx,sy),shift:e.shiftKey}; mode='maybepress'; objEl.classList.add('pressed'); head=null; hideGhost(); viewport.setPointerCapture(e.pointerId); return; }
      if(document.activeElement&&document.activeElement.isContentEditable) document.activeElement.blur();
      if(e.shiftKey||e.ctrlKey||e.metaKey){ marqueeState={anchor:cellAt(sx,sy),base:new Set(selected),shift:e.shiftKey}; mode='marquee'; viewport.setPointerCapture(e.pointerId); return; }
      deselect();
      if(!isTool()) return;
      mode='create'; createAnchor=cellAt(sx,sy); head=null; showGhost(footprint(createAnchor,createAnchor)); spawnRipple(sx,sy); viewport.setPointerCapture(e.pointerId);
    });

    viewport.addEventListener('pointermove',(e)=>{
      const now=performance.now(), r=viewport.getBoundingClientRect(), sx=e.clientX-r.left, sy=e.clientY-r.top; inside=true;
      if(mode==='pan'){ view.x=pan.vx+(e.clientX-pan.x); view.y=pan.vy+(e.clientY-pan.y); apply(); return; }
      if(mode==='create'){ showGhost(footprint(createAnchor,cellAt(sx,sy))); return; }
      if(mode==='resizedrag'){ const cur=cellAt(sx,sy); const dx=cur.col-resizeStart.col, dy=cur.row-resizeStart.row;
        const dist=Math.max(Math.abs(dx),Math.abs(dy));
        const curSize=resizeStart.items[0].obj.size;
        const targetSize=dist===0?curSize:snapSize(dist+1);
        showResizeGhost(targetSize); return; }
      if(mode==='maybepress'){ if(Math.hypot(sx-press.sx,sy-press.sy)>4){ const o=press.o, st=press.cell; press.objEl.classList.remove('pressed');
        if(!press.shift&&!selected.has(o)){ for(const s of selected) if(s._el) s._el.classList.remove('selected'); selected.clear(); selected.add(o); o._el.classList.add('selected'); updateSelCount(); }
        move={clickCol:st.col,clickRow:st.row,bad:false,origPositions:new Map([...selected].map(s=>[s,{col:s.col,row:s.row}]))}; mode='move'; } return; }
      if(mode==='move'){ const cur=cellAt(sx,sy);
        const dCol=cur.col-move.clickCol, dRow=cur.row-move.clickRow;
        let anyBad=false;
        for(const s of selected){ const orig=move.origPositions.get(s); const nc=orig.col+dCol, nr=orig.row+dRow;
          if(overlaps(nc,nr,s.size,s)) anyBad=true; }
        for(const s of selected){ const orig=move.origPositions.get(s); const nc=orig.col+dCol, nr=orig.row+dRow;
          s.col=nc; s.row=nr; geom(s); s._el.style.opacity=anyBad?'0.5':'1'; }
        renderObjectField(); return; }
      if(mode==='marquee'){ const cur=cellAt(sx,sy);
        const minC=Math.min(marqueeState.anchor.col,cur.col), minR=Math.min(marqueeState.anchor.row,cur.row);
        const maxC=Math.max(marqueeState.anchor.col,cur.col), maxR=Math.max(marqueeState.anchor.row,cur.row);
        showMarquee(minC,minR,maxC,maxR);
        const hit=objects.filter(o=>o.layer===curId&&o.col<=maxC&&o.col+o.size>minC&&o.row<=maxR&&o.row+o.size>minR);
        if(marqueeState.shift){ for(const s of marqueeState.base) selected.add(s); }
        else { for(const s of selected) if(s._el) s._el.classList.remove('selected'); selected.clear(); }
        for(const o of hit) selected.add(o);
        for(const o of objects){ if(o._el) o._el.classList.toggle('selected',selected.has(o)); }
        updateSelCount(); return; }
      lastHoverCell=cellAt(sx,sy);
      if(isTool()){ head=null; if(objAtCell(lastHoverCell.col,lastHoverCell.row)) hideGhost(); else showGhost({col:lastHoverCell.col,row:lastHoverCell.row,size:1}); }
      else { hideGhost(); head=lastHoverCell.col+','+lastHoverCell.row; }
      if(lastCell&&lastPos){ const dt=Math.max(now-lastPos.t,1), dist=Math.hypot(sx-lastPos.x,sy-lastPos.y), se=Math.min(1,Math.max(0,(dist/dt-SPEED_FLOOR)/SPEED_SPAN)); if(se>0) deposit(lastCell,lastHoverCell,se); }
      lastCell=lastHoverCell; lastPos={x:sx,y:sy,t:now}; ensureLoop();
    });

    viewport.addEventListener('pointerup',(e)=>{
      const r=viewport.getBoundingClientRect(), sx=e.clientX-r.left, sy=e.clientY-r.top;
      try{ viewport.releasePointerCapture(e.pointerId); }catch(_){}
      if(mode==='pan'){ mode='idle'; refreshHover(); return; }
      if(mode==='create'){ const cur=cellAt(sx,sy), same=cur.col===createAnchor.col&&cur.row===createAnchor.row;
        const fp=same?{col:createAnchor.col,row:createAnchor.row,size:1}:footprint(createAnchor,cur); hideGhost();
        if(!overlaps(fp.col,fp.row,fp.size,null)) createAt(fp); mode='idle'; return; }
      if(mode==='resizedrag'){ const cur=cellAt(sx,sy); const dx=cur.col-resizeStart.col, dy=cur.row-resizeStart.row;
        const dist=Math.max(Math.abs(dx),Math.abs(dy));
        const curSize=resizeStart.items[0].obj.size;
        const targetSize=dist===0?curSize:snapSize(dist+1);
        applyResize(targetSize); mode='idle'; return; }
      if(mode==='maybepress'){ press.objEl.classList.remove('pressed'); const o=press.o;
        if(press.shift){ selectObj(o,true); }
        else { for(const s of selected) if(s._el) s._el.classList.remove('selected'); selected.clear(); selected.add(o); o._el.classList.add('selected'); updateSelCount(); }
        mode='idle'; return; }
      if(mode==='move'){ for(const s of selected){ const orig=move.origPositions.get(s); if(overlaps(s.col,s.row,s.size,s)){ s.col=orig.col; s.row=orig.row; geom(s); } s._el.style.opacity='1'; }
        renderObjectField(); mode='idle'; return; }
      if(mode==='marquee'){ hideMarquee(); mode='idle'; return; }
    });

    viewport.addEventListener('pointerleave',()=>{ inside=false; head=null; lastCell=null; lastPos=null; if(mode==='idle') hideGhost(); });
    viewport.addEventListener('contextmenu',(e)=>{ const objEl=e.target.closest('.obj'); if(!objEl) return; e.preventDefault(); deleteObj(objEl._obj); });
    viewport.addEventListener('wheel',(e)=>{ e.preventDefault(); const r=viewport.getBoundingClientRect(), sx=e.clientX-r.left, sy=e.clientY-r.top;
      const next=Math.max(0.1,Math.min(100,view.s*Math.exp(-e.deltaY*0.0012))); if(next!==view.s){ view.x=sx-(sx-view.x)*next/view.s; view.y=sy-(sy-view.y)*next/view.s; view.s=next; apply(); } },{passive:false});
    viewport.addEventListener('auxclick',(e)=>{ if(e.button===1) e.preventDefault(); });
    viewport.addEventListener('dragover',(e)=>e.preventDefault());
    viewport.addEventListener('drop',(e)=>{ e.preventDefault(); const f=e.dataTransfer&&e.dataTransfer.files[0]; if(!f||!f.type.startsWith('image/')) return;
      const r=viewport.getBoundingClientRect(), cell=cellAt(e.clientX-r.left,e.clientY-r.top); const fr=new FileReader(); fr.onload=()=>addImageAt(cell,fr.result); fr.readAsDataURL(f); });

    function toggleHud(){ const h=hud.classList.toggle('hidden'); hudPip.style.display=h?'flex':'none'; }
    document.getElementById('hudClose').onclick=toggleHud; hudPip.onclick=toggleHud;

    /* ── Slate (memories) — dark chrome, real storage, 3 forms ── */
    const slate=document.getElementById('slate'), slatePanel=document.getElementById('slatePanel'), slateGrid=document.getElementById('slateGrid'),
          slateMeta=document.getElementById('slateMeta'), slateInput=document.getElementById('slateInput');
    let memories=[], memSeq=0, slateForm='half', slateOpen=false, slateQuery=''; const slateSel=new Set();
    function setForm(f){ slateForm=f; slatePanel.classList.remove('full','half','rail'); slatePanel.classList.add(f); slate.classList.toggle('dock', f!=='full');
      document.querySelectorAll('.slate-forms button').forEach(b=>b.classList.toggle('active', b.dataset.form===f)); }
    function widenSlate(d){ const order=['rail','half','full']; let i=order.indexOf(slateForm)+d; i=Math.max(0,Math.min(2,i)); setForm(order[i]); }
    function openSlate(){ slateOpen=true; slate.classList.add('open'); setForm(slateForm); setTimeout(()=>slateInput.focus(),60); }
    function closeSlate(){ slateOpen=false; slate.classList.remove('open'); slateInput.blur(); }
    function updateMeta(){ slateMeta.textContent = slateSel.size ? `${slateSel.size} selected` : `${memories.length} memor${memories.length===1?'y':'ies'}`; }
    function addMemory(m){ m.id=memSeq++; memories.unshift(m); renderSlate(); return m; }
    function renderSlate(){
      const list = memories.filter(m => !slateQuery || (m.type==='text' && (m.text||'').toLowerCase().includes(slateQuery)));
      slateGrid.innerHTML='';
      if(!list.length){ slateGrid.innerHTML = `<div class="slate-empty">${memories.length? 'No matches.' : 'Nothing here yet.<br>Paste or drop an image, or type a note and press Enter.'}</div>`; updateMeta(); return; }
      for(const m of list){ const t=document.createElement('div'); t.className='tile'+(slateSel.has(m.id)?' sel':''); t.dataset.id=m.id;
        if(m.type==='image'){ t.innerHTML=`<span class="tag">image</span><img src="${m.src}" alt="">`; }
        else { t.innerHTML=`<span class="tag">note</span><div class="memo" contenteditable data-ph="Note…">${m.text||''}</div>`; }
        t.addEventListener('click',(e)=>{ if(e.target.closest('[contenteditable]')) return; if(slateSel.has(m.id)){slateSel.delete(m.id);t.classList.remove('sel');}else{slateSel.add(m.id);t.classList.add('sel');} updateMeta(); });
        t.addEventListener('dblclick',(e)=>{ if(e.target.closest('[contenteditable]')) return; placeMemory(m); });
        const memo=t.querySelector('.memo'); if(memo) memo.addEventListener('input',()=>{ m.text=memo.textContent; });
        slateGrid.appendChild(t); }
      updateMeta();
    }
    function quickCapture(){ const v=slateInput.value.trim(); if(!v) return; slateInput.value=''; slateQuery=''; addMemory({type:'text',text:v}); }
    function placeMemory(m){ closeSlate(); const ctr=cellAt(innerWidth/2,innerHeight/2);
      if(m.type==='image'){ const fp=freeSpot(ctr.col-1,ctr.row-1,2); const o={type:'figure',col:fp.col,row:fp.row,size:2,layer:curId,fig:'Memory',src:m.src}; objects.push(o); addObjectEl(o,true); }
      else { const fp=freeSpot(ctr.col,ctr.row,1); const c=NOTE_COLORS[0]; const o={type:'sticky',col:fp.col,row:fp.row,size:1,layer:curId,note:m.text||'',fill:c.fill,glow:c.glow}; objects.push(o); addObjectEl(o,true); }
      renderLayers(); renderObjectField(); }

    document.querySelectorAll('.slate-forms button').forEach(b=>b.onclick=()=>setForm(b.dataset.form));
    document.getElementById('slateScrim').onclick=closeSlate;
    document.getElementById('slateAdd').onclick=()=>{ const m=addMemory({type:'text',text:''}); const memo=slateGrid.querySelector(`[data-id="${m.id}"] .memo`); if(memo) memo.focus(); };
    slateInput.addEventListener('input',()=>{ slateQuery=slateInput.value.trim().toLowerCase(); renderSlate(); });
    slatePanel.addEventListener('dragover',e=>{ e.preventDefault(); });
    slatePanel.addEventListener('drop',e=>{ e.preventDefault(); const f=e.dataTransfer&&e.dataTransfer.files[0]; if(f&&f.type.startsWith('image/')){ const fr=new FileReader(); fr.onload=()=>addMemory({type:'image',src:fr.result}); fr.readAsDataURL(f); } });

    window.addEventListener('paste',(e)=>{
      const items=(e.clipboardData&&e.clipboardData.items)||[];
      if(slateOpen){ for(const it of items){ if(it.type.startsWith('image/')){ const fr=new FileReader(); fr.onload=()=>addMemory({type:'image',src:fr.result}); fr.readAsDataURL(it.getAsFile()); e.preventDefault(); break; } } return; }
      if(document.activeElement&&document.activeElement.isContentEditable) return;
      for(const it of items){ if(it.type.startsWith('image/')){ const fr=new FileReader(); fr.onload=()=>addImageAt(lastHoverCell,fr.result); fr.readAsDataURL(it.getAsFile()); e.preventDefault(); break; } }
    });

    window.addEventListener('keydown',(e)=>{
      if(slateOpen){
        if(e.key==='Escape'){ e.preventDefault(); closeSlate(); return; }
        if(document.activeElement===slateInput){ if(e.key==='Enter') quickCapture(); return; }
        if(document.activeElement&&document.activeElement.isContentEditable) return;
        if(e.key===']'){ e.preventDefault(); widenSlate(1); }
        else if(e.key==='['){ e.preventDefault(); widenSlate(-1); }
        return;
      }
      if(document.activeElement&&document.activeElement.isContentEditable){ if(e.key==='Escape') document.activeElement.blur(); return; }
      if(e.key===' '){ e.preventDefault(); openSlate(); return; }
      const ctrl=e.ctrlKey||e.metaKey;
      if(e.key==='Tab'){ e.preventDefault(); brush.type=TYPES[(TYPES.indexOf(brush.type)+1)%TYPES.length]; refreshHover(); }
      else if(['1','2','3','4','5','6'].includes(e.key)){ brush.color=+e.key-1; if(brush.type==='neutral') brush.type='note'; for(const s of selected) if(s.type==='sticky') recolor(s,brush.color); refreshHover(); }
      else if(e.key==='f'||e.key==='F'){ frameAll(); }
      else if(e.key===']'){ e.preventDefault(); ctrl?bringSel(1):switchLayer(1); }
      else if(e.key==='['){ e.preventDefault(); ctrl?bringSel(-1):switchLayer(-1); }
      else if(e.key==='}'){ e.preventDefault(); addLayer(1); }
      else if(e.key==='{'){ e.preventDefault(); addLayer(-1); }
      else if(e.key==='?'||e.key==='/'){ e.preventDefault(); toggleHud(); }
      else if(e.key==='Escape'){ brush.type='neutral'; deselect(); cancelTrace(); exitResizeMode(); refreshHover(); }
      else if(e.key==='Delete'||e.key==='Backspace'){
        if(selected.size>0){ for(const s of [...selected]) deleteObj(s); }
        else { deleteLayer(); }
      }
      else if(e.key==='n'||e.key==='N'||e.key==='F2'){ e.preventDefault(); startRenameLayer(); }
      else if(e.key==='t'||e.key==='T'){
        if(traceState){ stampTrace(); }
        else if(selected.size>0){ armTrace(); showTraceGhost(); }
      }
      else if(e.key==='r'||e.key==='R'){
        if(resizeMode){ exitResizeMode(); }
        else if(selected.size>0){ enterResizeMode(); }
      }
      else if(e.key==='a'&&ctrl){ e.preventDefault(); for(const o of objects) if(o.layer===curId) selected.add(o);
        for(const s of selected) if(s._el) s._el.classList.add('selected'); updateSelCount(); }
    });

    layerrenameInput.addEventListener('keydown',(e)=>{
      if(e.key==='Enter'){ e.preventDefault(); finishRenameLayer(); }
      else if(e.key==='Escape'){ layerrenameEl.classList.remove('open'); }
      e.stopPropagation();
    });
    layerrenameInput.addEventListener('blur',()=>finishRenameLayer());

    buildObjects(); renderLayers(); frameAll(); renderSlate();
    setTimeout(()=>renderObjectField(),0);
    window.addEventListener('resize',frameAll);
