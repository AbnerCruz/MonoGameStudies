// Kept self-contained so the identical player is embedded in exported games.
export function createPlayer(canvas, workerSource, assets = [], onError = console.error, inlineBoot = null, maps = []) {
  const ctx = canvas.getContext('2d');
  let worker, workerUrl, serial = 0, running = false, raf = 0, last = 0, frameCount = 0;
  const pending = new Map(), keys = new Set(), sprites = new Map(), tilemaps = new Map(maps.map(map=>[map.name,map]));
  const pointer = { x: 0, y: 0, down: false };
  const uiHost = document.createElement('div');
  uiHost.className = 'mobileforge-ui';
  Object.assign(uiHost.style, {position:'fixed', zIndex:'5', pointerEvents:'none', overflow:'hidden'});
  document.body.append(uiHost);
  const shadow = uiHost.attachShadow({mode:'open'});
  let currentMarkup = '', uiEvents = [];
  const appliedUI = new Map();
  const uiHeld = new Set();
  const heldPointers = new Map();
  function clearHeld() { heldPointers.clear(); uiHeld.clear(); }
  function releasePointer(id) {
    heldPointers.delete(id);
    uiHeld.clear();
    for(const {name} of heldPointers.values())uiHeld.add(name);
  }
  const actionName = /^([A-Za-z_][\w]*)(?:\(\))?$/;
  function alignUI() {
    const fullscreen=document.fullscreenElement;
    const parent=fullscreen?.contains(canvas)?fullscreen:document.body;
    if(uiHost.parentElement!==parent)parent.append(uiHost);
    const rect=canvas.getBoundingClientRect();
    Object.assign(uiHost.style,{left:rect.left+'px',top:rect.top+'px',width:rect.width+'px',height:rect.height+'px'});
  }
  function uiAction(raw, element) {
    const match=raw?.trim().match(actionName);
    if (match) uiEvents.push({name:match[1],id:element.id||'',value:String(element.value??'')});
    return match?.[1];
  }
  function renderUI(markup) {
    if (markup === currentMarkup) return;
    currentMarkup = markup;
    clearHeld();
    appliedUI.clear();
    shadow.replaceChildren();
    if (!markup) return;
    const base=document.createElement('style');
    base.textContent=':host{font:16px system-ui;color:white;user-select:none;-webkit-user-select:none;-webkit-touch-callout:none}.root,.root *{user-select:none!important;-webkit-user-select:none!important;-webkit-touch-callout:none!important}.root input:not([type=range]):not([type=button]):not([type=checkbox]):not([type=radio]){user-select:text!important;-webkit-user-select:text!important}.root{position:relative;width:100%;height:100%;box-sizing:border-box;pointer-events:none}.root [hidden]{display:none!important}.root button,.root input,.root select{pointer-events:auto;touch-action:none;font:inherit}.root button{min-width:44px;min-height:44px;cursor:pointer}';
    const root=document.createElement('div');root.className='root';
    const parsed=new DOMParser().parseFromString(markup,'text/html');
    const allowed=new Set(['DIV','SPAN','P','SECTION','HEADER','FOOTER','BUTTON','INPUT','LABEL','IMG','PROGRESS','OUTPUT','STRONG','SMALL','H1','H2','H3','BR','STYLE']);
    const attributes=new Set(['class','id','title','type','value','min','max','step','placeholder','disabled','for','alt','src','style','aria-label','role']);
    function copy(from,to) {
      for(const node of from.childNodes) {
        if(node.nodeType===Node.TEXT_NODE){to.append(document.createTextNode(node.textContent));continue;}
        if(node.nodeType!==Node.ELEMENT_NODE||!allowed.has(node.tagName))continue;
        const element=document.createElement(node.tagName.toLowerCase());
        if(node.tagName==='STYLE') { element.textContent=node.textContent;to.append(element);continue; }
        for(const attr of node.attributes) {
          const key=attr.name.toLowerCase();
          if(attributes.has(key)) {
            if(key==='src') { if(node.tagName==='IMG'&&attr.value.startsWith('sprite:')){const asset=sprites.get(attr.value.slice(7));if(asset)element.src=asset.surface.toDataURL();} }
            else if(key==='style'&&!/url\s*\(|@import/i.test(attr.value))element.setAttribute(attr.name,attr.value);
            else if(key!=='style')element.setAttribute(attr.name,attr.value);
          }
        }
        for(const [attribute,event] of [['onclick','click'],['oninput','input'],['onchange','change']]) {
          const action=node.getAttribute(attribute);
          if(actionName.test(action?.trim()||'')){element.style.pointerEvents='auto';element.addEventListener(event,()=>uiAction(action,element));}
        }
        const press=node.getAttribute('onpress');
        if(actionName.test(press?.trim()||'')) {
          const name=press.trim().match(actionName)[1];
          element.style.pointerEvents='auto';element.style.touchAction='none';
          element.addEventListener('pointerdown',e=>{if(e.button!==0||element.disabled)return;e.preventDefault();heldPointers.set(e.pointerId,{name,element});uiHeld.add(name);element.setPointerCapture(e.pointerId);});
          for(const event of ['pointerup','pointercancel','lostpointercapture'])element.addEventListener(event,e=>releasePointer(e.pointerId));
        }
        copy(node,element);to.append(element);
      }
    }
    copy(parsed.head,root);copy(parsed.body,root);
    shadow.append(base,root);alignUI();
  }
  function renderUIState(states) {
    for(const [id,state] of Object.entries(states)) {
      const element=shadow.getElementById(id);
      if(!element)continue;
      const old=appliedUI.get(id)||{};
      if(state.Text!==null&&state.Text!==undefined&&state.Text!==old.Text)element.textContent=state.Text;
      if(state.Value!==null&&state.Value!==undefined&&state.Value!==old.Value&&'value' in element)element.value=state.Value;
      if(state.Visible!==null&&state.Visible!==undefined&&state.Visible!==old.Visible)element.hidden=!state.Visible;
      if(state.Visible===false)for(const [pointerId,held] of heldPointers)if(element.contains(held.element))releasePointer(pointerId);
      appliedUI.set(id,{...state});
    }
  }
  for (const sprite of assets) {
    const surface = document.createElement('canvas'); surface.width = sprite.w; surface.height = sprite.h;
    const sc = surface.getContext('2d');
    sprite.pixels.forEach((color, index) => { if (color) { sc.fillStyle = color; sc.fillRect(index % sprite.w, Math.floor(index / sprite.w), 1, 1); } });
    sprites.set(sprite.name, {surface,cellW:sprite.cellW||sprite.w,cellH:sprite.cellH||sprite.h});
  }
  const controller = new AbortController(), events = { signal: controller.signal };
  // Suppress native text/drag menus on gameplay surfaces, not in the IDE.
  const oldCanvasStyle = {userSelect:canvas.style.userSelect, webkitUserSelect:canvas.style.webkitUserSelect, webkitTouchCallout:canvas.style.webkitTouchCallout};
  Object.assign(canvas.style,{userSelect:'none',webkitUserSelect:'none',webkitTouchCallout:'none'});
  const textInput = target => target instanceof Element && target.matches('input:not([type=range]):not([type=button]):not([type=checkbox]):not([type=radio]),textarea');
  for(const name of ['selectstart','dragstart','contextmenu']) {
    canvas.addEventListener(name,e=>e.preventDefault(),events);
    shadow.addEventListener(name,e=>{if(!textInput(e.target))e.preventDefault();},events);
  }
  function position(e) { const b = canvas.getBoundingClientRect(); pointer.x = (e.clientX - b.left) * canvas.width / b.width; pointer.y = (e.clientY - b.top) * canvas.height / b.height; }
  canvas.addEventListener('pointerdown', e => { e.preventDefault(); position(e); pointer.down = true; canvas.setPointerCapture(e.pointerId); }, events);
  canvas.addEventListener('pointermove', position, events);
  for (const name of ['pointerup','pointercancel','lostpointercapture']) canvas.addEventListener(name, () => pointer.down = false, events);
  window.addEventListener('keydown', e => { if ((shadow.activeElement||document.activeElement)?.matches('input,textarea,[contenteditable="true"]')) return; keys.add(e.key); if(e.key.startsWith('Arrow') || e.key === ' ') e.preventDefault(); }, events);
  window.addEventListener('keyup', e => keys.delete(e.key), events);
  window.addEventListener('blur', () => { keys.clear(); clearHeld(); pointer.down = false; }, events);
  document.addEventListener('visibilitychange', () => { keys.clear(); clearHeld(); pointer.down = false; last = performance.now(); }, events);
  function stop(reason = 'Execução encerrada.') {
    running = false; cancelAnimationFrame(raf); worker?.terminate(); worker = null;
    renderUI(''); uiEvents=[]; clearHeld();
    if (workerUrl) URL.revokeObjectURL(workerUrl);
    for (const task of pending.values()) { clearTimeout(task.timer); task.reject(new Error(reason)); } pending.clear();
  }
  function call(type, payload, timeout = 10000) {
    if (!worker) return Promise.reject(new Error('Runtime encerrado.'));
    return new Promise((resolve, reject) => {
      const id = ++serial;
      const timer = setTimeout(() => { stop('Tempo excedido. Verifique laços infinitos no seu código.'); }, timeout);
      pending.set(id, { resolve, reject, timer }); worker.postMessage({ id, type, payload });
    });
  }
  function draw(commands) {
    function tile(sheet,index,x,y,scale){
      const asset=sprites.get(sheet);if(!asset)throw new Error(`Spritesheet "${sheet}" não encontrado.`);
      const {surface,cellW,cellH}=asset,columns=Math.floor(surface.width/cellW),rows=Math.floor(surface.height/cellH);
      if(!Number.isInteger(index)||index<0||index>=columns*rows)throw new Error(`Célula ${index} não existe em "${sheet}".`);
      ctx.imageSmoothingEnabled=false;
      ctx.drawImage(surface,(index%columns)*cellW,Math.floor(index/columns)*cellH,cellW,cellH,x,y,cellW*scale,cellH*scale);
    }
    for (const c of commands) {
      ctx.fillStyle = c.color || '#fff';
      if (c.type === 'clear') ctx.fillRect(0,0,canvas.width,canvas.height);
      else if (c.type === 'rect') ctx.fillRect(c.x,c.y,c.width,c.height);
      else if (c.type === 'circle') { ctx.beginPath(); ctx.arc(c.x,c.y,Math.max(0,c.radius),0,Math.PI*2); ctx.fill(); }
      else if (c.type === 'text') { ctx.font = `${c.size}px system-ui`; ctx.fillText(c.text,c.x,c.y); }
      else if (c.type === 'sprite') { const asset = sprites.get(c.name); if (!asset) throw new Error(`Sprite "${c.name}" não encontrado.`); ctx.imageSmoothingEnabled = false; ctx.drawImage(asset.surface,c.x,c.y,asset.surface.width*c.scale,asset.surface.height*c.scale); }
      else if (c.type === 'tile') tile(c.sheet,c.index,c.x,c.y,c.scale);
      else if (c.type === 'tilemap') {
        const map=tilemaps.get(c.name);if(!map)throw new Error(`Tilemap "${c.name}" não encontrado.`);
        const sheet=sprites.get(map.sheet);if(!sheet)throw new Error(`Spritesheet "${map.sheet}" não encontrado.`);
        for(let i=0;i<map.cells.length;i++)if(map.cells[i]>=0)tile(map.sheet,map.cells[i],c.x+(i%map.w)*sheet.cellW*c.scale,c.y+Math.floor(i/map.w)*sheet.cellH*c.scale,c.scale);
      }
    }
  }
  async function tick(now) {
    if (!running) return;
    const dt = Math.min((now-last)/1000, .05); last = now;
    try {
      alignUI();
      const outgoing=uiEvents;uiEvents=[];
      const result = await call('frame', { ...pointer, keys: [...keys], dt, uiEvents:outgoing, uiHeld:[...uiHeld] }, 3000);
      if (!running) return;
      draw(result.commands);renderUI(result.ui||'');renderUIState(result.uiState||{});frameCount++; canvas.dataset.frames = frameCount;
      raf = requestAnimationFrame(tick);
    } catch (error) { if (running) { stop(); onError(error); } }
  }
  return {
    async boot(payload) {
      stop();
      if (payload.pack && inlineBoot) {
        const api = await inlineBoot(payload.pack);
        worker = { terminate() {}, postMessage({ id, type, payload: value }) {
          queueMicrotask(() => {
            let result;
            try {
              if (type === 'load') result = api.Load(value);
              else if (type === 'start') result = api.Start();
              else if (type === 'frame') result = api.Frame(JSON.stringify(value));
              else throw new Error('Operação inválida: ' + type);
              const data = JSON.parse(result);
              const task = pending.get(id); if (!task) return;
              clearTimeout(task.timer); pending.delete(id);
              if (data.ok === false && data.error) task.reject(new Error(data.error)); else task.resolve(data);
            } catch (error) { const task = pending.get(id); if (task) { clearTimeout(task.timer); pending.delete(id); task.reject(error); } }
          });
        } };
        return { ok: true };
      }
      workerUrl = URL.createObjectURL(new Blob([`(${workerSource})();`], { type:'text/javascript' }));
      worker = new Worker(workerUrl, { type: 'module' });
      worker.onmessage = ({data}) => {
        if (data.debug) { console.info('MobileForge:', data.debug); return; }
        const task = pending.get(data.id); if (!task) return;
        clearTimeout(task.timer); pending.delete(data.id);
        if (data.value?.ok === false && data.value.error) task.reject(new Error(data.value.error)); else task.resolve(data.value);
      };
      worker.onerror = e => { const message = e.message || 'Falha no runtime C#.'; stop(message); onError(new Error(message)); };
      return call('boot', payload, 180000);
    },
    compile: files => call('compile', files, 90000),
    async play(assembly) { frameCount=0;canvas.dataset.frames='0';await call('load', assembly); await call('start', null, 5000); running = true; last = performance.now(); raf = requestAnimationFrame(tick); },
    stop,
    dispose() { stop(); controller.abort(); uiHost.remove();Object.assign(canvas.style,oldCanvasStyle); },
    get frames() { return frameCount; }
  };
}
