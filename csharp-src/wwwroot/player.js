// Kept self-contained so the identical player is embedded in exported games.
export function createPlayer(canvas, workerSource, assets = [], onError = console.error, inlineBoot = null, maps = []) {
  const ctx = canvas.getContext('2d');
  let worker, workerUrl, serial = 0, running = false, raf = 0, last = 0, frameCount = 0;
  const pending = new Map(), keys = new Set(), sprites = new Map(), tilemaps = new Map(maps.map(map=>[map.name,map]));
  const pointer = { x: 0, y: 0, down: false };
  for (const sprite of assets) {
    const surface = document.createElement('canvas'); surface.width = sprite.w; surface.height = sprite.h;
    const sc = surface.getContext('2d');
    sprite.pixels.forEach((color, index) => { if (color) { sc.fillStyle = color; sc.fillRect(index % sprite.w, Math.floor(index / sprite.w), 1, 1); } });
    sprites.set(sprite.name, {surface,cellW:sprite.cellW||sprite.w,cellH:sprite.cellH||sprite.h});
  }
  const controller = new AbortController(), events = { signal: controller.signal };
  function position(e) { const b = canvas.getBoundingClientRect(); pointer.x = (e.clientX - b.left) * canvas.width / b.width; pointer.y = (e.clientY - b.top) * canvas.height / b.height; }
  canvas.addEventListener('pointerdown', e => { position(e); pointer.down = true; canvas.setPointerCapture(e.pointerId); }, events);
  canvas.addEventListener('pointermove', position, events);
  for (const name of ['pointerup','pointercancel','lostpointercapture']) canvas.addEventListener(name, () => pointer.down = false, events);
  window.addEventListener('keydown', e => { if (document.activeElement?.matches('input,textarea,[contenteditable="true"]')) return; keys.add(e.key); if(e.key.startsWith('Arrow') || e.key === ' ') e.preventDefault(); }, events);
  window.addEventListener('keyup', e => keys.delete(e.key), events);
  window.addEventListener('blur', () => { keys.clear(); pointer.down = false; }, events);
  document.addEventListener('visibilitychange', () => { keys.clear(); pointer.down = false; last = performance.now(); }, events);
  function stop(reason = 'Execução encerrada.') {
    running = false; cancelAnimationFrame(raf); worker?.terminate(); worker = null;
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
      const result = await call('frame', { ...pointer, keys: [...keys], dt }, 3000);
      if (!running) return;
      draw(result.commands); frameCount++; canvas.dataset.frames = frameCount;
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
    async play(assembly) { await call('load', assembly); await call('start', null, 5000); running = true; last = performance.now(); raf = requestAnimationFrame(tick); },
    stop,
    dispose() { stop(); controller.abort(); },
    get frames() { return frameCount; }
  };
}
