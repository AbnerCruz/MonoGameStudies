(() => {
  'use strict';
  const $ = (id) => document.getElementById(id);
  const starter = {
    version: 2, name: 'Meu jogo',
    files: [
      { id: 'player', name: 'player.js', code: `class Player {
  constructor() { this.x = 160; this.y = 280; }
  update(game, dt) {
    if (game.pointer.down) {
      this.x = game.pointer.x - 24;
      this.y = game.pointer.y - 24;
    }
    if (game.key('ArrowRight')) this.x += 180 * dt;
    if (game.key('ArrowLeft')) this.x -= 180 * dt;
  }
}
const player = new Player();` },
      { id: 'main', name: 'main.js', code: `function setup(game) {
  // Preparação inicial.
}

function update(game, dt) {
  player.update(game, dt);
}

function draw(game) {
  game.clear('#101827');
  game.text('Toque para mover', 20, 42, 21, '#ffffff');
  game.sprite('hero', player.x, player.y, 3);
}` }
    ],
    sprites: [{ id: 'hero', name: 'hero', w: 16, h: 16, pixels: Array.from({ length: 256 }, (_, i) => {
      const x = i % 16, y = Math.floor(i / 16);
      if (x < 2 || x > 13 || y < 2 || y > 13) return null;
      if ((x >= 5 && x <= 6 || x >= 10 && x <= 11) && y >= 6 && y <= 7) return '#172b36';
      if (y >= 10 && x >= 6 && x <= 9) return '#247d78';
      return '#5ce0b4';
    }) }]
  };
  const clone = (value) => structuredClone(value);
  let project = clone(starter);
  let active = { type: 'file', id: 'main' };
  let tabOrder = ['file:main'];
  let tool = 'brush';
  let spriteUndo = [];
  let drawing = false;
  let saveTimer;
  let database;
  const code = $('code');
  const frame = $('preview');
  const ide = document.querySelector('.ide');

  function log(message, isError = false) {
    const box = $('console');
    box.textContent = message;
    box.classList.toggle('error', isError);
  }
  function setSaved(message) { $('saveStatus').textContent = message; }
  function openDatabase() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open('mobileforge', 1);
      request.onupgradeneeded = () => request.result.createObjectStore('projects');
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }
  function dbRead() {
    return new Promise((resolve, reject) => {
      const request = database.transaction('projects').objectStore('projects').get('current');
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }
  function dbWrite() {
    return new Promise((resolve, reject) => {
      const request = database.transaction('projects', 'readwrite').objectStore('projects').put(clone(project), 'current');
      request.onsuccess = () => resolve();
      request.onerror = () => reject(request.error);
    });
  }
  async function save() {
    if (!database) { setSaved('Armazenamento indisponível — faça backup'); return; }
    try { await dbWrite(); setSaved('Salvo neste aparelho'); }
    catch { setSaved('Falha ao salvar — faça backup'); }
  }
  function scheduleSave() { setSaved('Salvando…'); clearTimeout(saveTimer); saveTimer = setTimeout(save, 350); }
  async function init() {
    try {
      database = await openDatabase();
      const saved = await dbRead();
      if (isValidProject(saved)) project = saved;
      else {
        try {
          const legacy = JSON.parse(localStorage.getItem('mobileforge-web-v1') || 'null');
          if (typeof legacy?.code === 'string') {
            project.name = legacy.name || project.name;
            project.files = [{ id: 'main', name: 'main.js', code: legacy.code }];
          }
        } catch {}
        await save();
      }
    } catch { setSaved('Armazenamento indisponível — faça backup'); }
    $('projectName').value = project.name;
    active = { type: 'file', id: project.files.at(-1).id };
    tabOrder = ['file:' + active.id];
    render();
    play();
  }
  function isValidProject(value) {
    return value && Array.isArray(value.files) && value.files.length > 0 &&
      value.files.every(f => typeof f.id === 'string' && typeof f.name === 'string' && typeof f.code === 'string') &&
      Array.isArray(value.sprites) &&
      value.sprites.every(s => typeof s.name === 'string' && Number.isInteger(s.w) && Number.isInteger(s.h) && s.w > 0 && s.w <= 64 && s.h > 0 && s.h <= 64 && Array.isArray(s.pixels) && s.pixels.length === s.w * s.h);
  }
  function item(type, id) { return (type === 'file' ? project.files : project.sprites).find(x => x.id === id); }
  function openItem(type, id) {
    if (!item(type, id)) return;
    active = { type, id };
    const key = type + ':' + id;
    if (!tabOrder.includes(key)) tabOrder.push(key);
    render();
    $('explorer').classList.remove('open');
    setView(type === 'sprite' ? 'sprites' : 'code');
  }
  function render() {
    const file = item('file', active.id);
    const sprite = item('sprite', active.id);
    $('projectName').value = project.name;
    renderTree('filesList', project.files, 'file');
    renderTree('spritesList', project.sprites, 'sprite');
    const tabs = $('tabs');
    tabs.replaceChildren();
    for (const key of tabOrder) {
      const [type, id] = key.split(':');
      const current = item(type, id);
      if (!current) continue;
      const button = document.createElement('button');
      button.className = 'file-tab' + (active.type === type && active.id === id ? ' selected' : '');
      button.textContent = (type === 'file' ? '⌘  ' : '▦  ') + (type === 'file' ? current.name : current.name + '.sprite');
      button.title = current.name;
      button.addEventListener('click', () => openItem(type, id));
      tabs.append(button);
    }
    $('codeView').hidden = !file || active.type !== 'file';
    $('spriteView').hidden = !sprite || active.type !== 'sprite';
    if (file && active.type === 'file') { code.value = file.code; updateLines(); }
    if (sprite && active.type === 'sprite') { $('spriteTitle').textContent = sprite.name; $('spriteSize').textContent = sprite.w + ' × ' + sprite.h; drawSprite(); }
  }
  function renderTree(target, entries, type) {
    const list = $(target);
    list.replaceChildren();
    for (const entry of entries) {
      const row = document.createElement('div');
      row.className = 'tree-row ' + (type === 'sprite' ? 'sprite ' : '') + (active.type === type && active.id === entry.id ? 'selected' : '');
      const icon = document.createElement('span'); icon.className = 'glyph'; icon.textContent = type === 'file' ? 'JS' : '▦';
      const label = document.createElement('button'); label.className = 'row-label'; label.textContent = entry.name + (type === 'sprite' ? '.sprite' : '');
      label.addEventListener('click', () => openItem(type, entry.id));
      const actions = document.createElement('span'); actions.className = 'row-actions';
      for (const [symbol, title, action] of [['✎', 'Renomear', () => renameEntry(type, entry)], ['×', 'Excluir', () => deleteEntry(type, entry)]]) {
        const button = document.createElement('button'); button.textContent = symbol; button.title = title; button.setAttribute('aria-label', title + ' ' + entry.name); button.addEventListener('click', action); actions.append(button);
      }
      row.append(icon, label, actions);
      list.append(row);
    }
  }
  function updateLines() {
    const lines = code.value.split('\n').length;
    $('lineNumbers').textContent = Array.from({ length: lines }, (_, i) => i + 1).join('\n');
    $('lineNumbers').scrollTop = code.scrollTop;
    const before = code.value.slice(0, code.selectionStart);
    const line = before.split('\n').length;
    const col = before.length - before.lastIndexOf('\n');
    $('cursorStatus').textContent = 'Ln ' + line + ', Col ' + col + ' · JavaScript';
  }
  function setView(view) {
    ide.dataset.view = view;
    for (const [id, section] of [['navCode','code'],['navSprites','sprites'],['navPlay','play'],['navGuide','guide']]) $(id).classList.toggle('active', view === section);
    $('tutorialPanel').hidden = view !== 'guide';
    if (view === 'guide') $('tutorialPanel').scrollTop = 0;
    if (view === 'sprites' && active.type !== 'sprite' && project.sprites.length) openItem('sprite', project.sprites[0].id);
    if (view === 'code' && active.type !== 'file') openItem('file', project.files.at(-1).id);
  }
  function promptText(title, label, initial = '') {
    return new Promise(resolve => {
      $('dialogTitle').textContent = title;
      $('dialogLabel').textContent = label;
      $('dialogInput').value = initial;
      $('dialogOverlay').hidden = false;
      $('dialogInput').focus();
      $('dialogInput').select();
      function close(value) { $('dialogOverlay').hidden = true; $('dialogConfirm').removeEventListener('click', yes); $('dialogCancel').removeEventListener('click', no); $('dialogInput').removeEventListener('keydown', key); resolve(value); }
      const yes = () => close($('dialogInput').value.trim());
      const no = () => close(null);
      const key = (e) => { if (e.key === 'Enter') yes(); if (e.key === 'Escape') no(); };
      $('dialogConfirm').addEventListener('click', yes); $('dialogCancel').addEventListener('click', no); $('dialogInput').addEventListener('keydown', key);
    });
  }
  async function newFile() {
    let name = await promptText('Novo arquivo', 'Nome do arquivo', 'novo.js');
    if (!name) return;
    if (!name.endsWith('.js')) name += '.js';
    if (!/^[\w.-]+\.js$/.test(name) || project.files.some(f => f.name === name)) { log('Nome inválido ou já usado.', true); return; }
    const entry = { id: crypto.randomUUID(), name, code: '// ' + name + '\n' };
    project.files.splice(Math.max(0, project.files.length - 1), 0, entry);
    scheduleSave(); openItem('file', entry.id);
  }
  async function newSprite() {
    const name = await promptText('Novo sprite', 'Nome usado no código', 'sprite');
    if (!name) return;
    if (!/^[A-Za-z][\w-]*$/.test(name) || project.sprites.some(s => s.name === name)) { log('Use um nome único com letras, números, _ ou -.', true); return; }
    const entry = { id: crypto.randomUUID(), name, w: 16, h: 16, pixels: Array(256).fill(null) };
    project.sprites.push(entry); scheduleSave(); openItem('sprite', entry.id);
  }
  async function renameEntry(type, entry) {
    let name = await promptText('Renomear', 'Novo nome', entry.name);
    if (!name) return;
    if (type === 'file' && !name.endsWith('.js')) name += '.js';
    const list = type === 'file' ? project.files : project.sprites;
    if ((type === 'file' ? !/^[\w.-]+\.js$/.test(name) : !/^[A-Za-z][\w-]*$/.test(name)) || list.some(x => x !== entry && x.name === name)) { log('Nome inválido ou já usado.', true); return; }
    entry.name = name; scheduleSave(); render();
  }
  function deleteEntry(type, entry) {
    if (type === 'file' && project.files.length === 1) { log('O projeto precisa de pelo menos um arquivo JS.', true); return; }
    if (!confirm('Excluir ' + entry.name + '?')) return;
    const list = type === 'file' ? project.files : project.sprites;
    list.splice(list.indexOf(entry), 1);
    tabOrder = tabOrder.filter(x => x !== type + ':' + entry.id);
    if (active.id === entry.id) active = { type: 'file', id: project.files.at(-1).id };
    scheduleSave(); render();
  }
  function compiledSource() { return project.files.map(f => '\n// Arquivo: ' + f.name + '\n' + f.code).join('\n'); }
  function escapeHtml(value) { return String(value).replace(/[&<>"']/g, c => ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', '"':'&quot;', "'":'&#39;' })[c]); }
  function gameHtml() {
    const source = JSON.stringify(compiledSource()).replace(/</g, '\\u003c').replace(/>/g, '\\u003e');
    const sprites = JSON.stringify(project.sprites.map(({ name, w, h, pixels }) => ({ name, w, h, pixels }))).replace(/</g, '\\u003c');
    const script = '(' + window.MobileForgeRuntime.toString() + ')(' + source + ',' + sprites + ');';
    return '<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover"><title>' + escapeHtml(project.name) + '</title><style>html,body{margin:0;width:100%;height:100%;overflow:hidden;background:#0b1020}body{display:grid;place-items:center}canvas{display:block;width:min(100vw,56.25vh);height:min(100vh,177.777vw);image-rendering:pixelated;touch-action:none}#game-error{position:fixed;inset:auto 10px 10px;max-height:40vh;overflow:auto;background:#672b39;color:#fff;border-radius:7px;padding:12px;white-space:pre-wrap;font:13px monospace}#game-error[hidden]{display:none}</style></head><body><canvas width="360" height="640"></canvas><div id="game-error" hidden></div><script>' + script.replace(/<\/script/gi, '<\\/script') + '<\/script></body></html>';
  }
  function play() { clearTimeout(saveTimer); void save(); log('Iniciando jogo…'); frame.srcdoc = gameHtml(); setView('play'); }
  function download(blob, filename) { const url = URL.createObjectURL(blob); const a = document.createElement('a'); a.href = url; a.download = filename; document.body.append(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 60000); }
  function filename(ext) { return (project.name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().replace(/[^a-z0-9-]+/g, '-').replace(/^-|-$/g, '') || 'meu-jogo') + ext; }
  function exportGame() { download(new Blob([gameHtml()], { type: 'text/html;charset=utf-8' }), filename('.html')); log('HTML exportado. Abra o arquivo baixado para jogar.'); }
  function exportProject() { download(new Blob([JSON.stringify(project, null, 2)], { type: 'application/json' }), filename('.mobileforge.json')); log('Backup do projeto exportado.'); }
  async function importProject(file) {
    if (file.size > 20 * 1024 * 1024) { log('Backup maior que 20 MB.', true); return; }
    try {
      const parsed = JSON.parse(await file.text());
      if (!isValidProject(parsed)) throw Error('Formato do projeto inválido');
      if (!confirm('Abrir este projeto e substituir o atual? Faça backup antes se necessário.')) return;
      project = parsed; active = { type:'file', id:project.files.at(-1).id }; tabOrder = ['file:' + active.id]; await save(); render(); setView('code'); log('Projeto aberto.');
    } catch (error) { log('Não foi possível abrir o projeto: ' + error.message, true); }
  }
  function paintSprite() {
    const sprite = item('sprite', active.id);
    if (!sprite) return;
    const canvas = $('spriteCanvas'), ctx = canvas.getContext('2d');
    const size = Math.min(512 / sprite.w, 512 / sprite.h);
    ctx.clearRect(0, 0, 512, 512);
    for (let y = 0; y < sprite.h; y++) for (let x = 0; x < sprite.w; x++) {
      ctx.fillStyle = (x + y) % 2 ? '#253144' : '#344257';
      ctx.fillRect(x * size, y * size, size, size);
      const color = sprite.pixels[y * sprite.w + x];
      if (color) { ctx.fillStyle = color; ctx.fillRect(x * size, y * size, size, size); }
    }
    ctx.strokeStyle = '#ffffff18'; ctx.lineWidth = 1;
    for (let x = 0; x <= sprite.w; x++) { ctx.beginPath(); ctx.moveTo(x*size+.5,0); ctx.lineTo(x*size+.5,sprite.h*size); ctx.stroke(); }
    for (let y = 0; y <= sprite.h; y++) { ctx.beginPath(); ctx.moveTo(0,y*size+.5); ctx.lineTo(sprite.w*size,y*size+.5); ctx.stroke(); }
  }
  function drawSprite() { paintSprite(); }
  function pushUndo(sprite) { spriteUndo.push(sprite.pixels.slice()); if (spriteUndo.length > 30) spriteUndo.shift(); }
  function paintAt(event) {
    const sprite = item('sprite', active.id); if (!sprite) return;
    const rect = $('spriteCanvas').getBoundingClientRect();
    const x = Math.floor((event.clientX - rect.left) * 512 / rect.width / (512 / sprite.w));
    const y = Math.floor((event.clientY - rect.top) * 512 / rect.height / (512 / sprite.h));
    if (x < 0 || y < 0 || x >= sprite.w || y >= sprite.h) return;
    const index = y * sprite.w + x;
    const color = tool === 'brush' ? $('pixelColor').value : null;
    if (sprite.pixels[index] === color) return;
    sprite.pixels[index] = color; paintSprite(); scheduleSave();
  }
  async function importPng(file) {
    if (file.type !== 'image/png' || file.size > 2 * 1024 * 1024) { log('Use um PNG de até 2 MB.', true); return; }
    const url = URL.createObjectURL(file);
    try {
      const image = await new Promise((resolve, reject) => { const img = new Image(); img.onload = () => resolve(img); img.onerror = () => reject(Error('PNG inválido')); img.src = url; });
      const ratio = Math.min(1, 64 / Math.max(image.width, image.height));
      const w = Math.max(1, Math.round(image.width * ratio)), h = Math.max(1, Math.round(image.height * ratio));
      const canvas = document.createElement('canvas'); canvas.width = w; canvas.height = h;
      const ctx = canvas.getContext('2d'); ctx.imageSmoothingEnabled = false; ctx.drawImage(image, 0, 0, w, h);
      const rgba = ctx.getImageData(0, 0, w, h).data;
      const pixels = Array.from({ length: w * h }, (_, i) => rgba[i*4+3] < 16 ? null : '#' + [rgba[i*4],rgba[i*4+1],rgba[i*4+2],rgba[i*4+3]].map(n => n.toString(16).padStart(2,'0')).join(''));
      let name = file.name.replace(/\.png$/i, '').replace(/[^A-Za-z0-9_-]/g, '_');
      if (!/^[A-Za-z]/.test(name)) name = 'sprite_' + name;
      let base = name, n = 2; while (project.sprites.some(s => s.name === name)) name = base + '_' + n++;
      const entry = { id: crypto.randomUUID(), name, w, h, pixels };
      project.sprites.push(entry); scheduleSave(); openItem('sprite', entry.id); log('Sprite importado: ' + name + ' (' + w + ' × ' + h + ').');
    } catch (error) { log('Não foi possível importar o PNG: ' + error.message, true); }
    finally { URL.revokeObjectURL(url); }
  }
  function exportSprite() {
    const sprite = item('sprite', active.id); if (!sprite) return;
    const canvas = document.createElement('canvas'); canvas.width = sprite.w; canvas.height = sprite.h;
    const ctx = canvas.getContext('2d'); const image = ctx.createImageData(sprite.w, sprite.h);
    sprite.pixels.forEach((color, i) => {
      if (!color) return; const v = color.slice(1);
      for (let k = 0; k < 3; k++) image.data[i*4+k] = parseInt(v.slice(k*2,k*2+2),16);
      image.data[i*4+3] = v.length === 8 ? parseInt(v.slice(6,8),16) : 255;
    });
    ctx.putImageData(image,0,0); canvas.toBlob(blob => { if (blob) download(blob, sprite.name + '.png'); }, 'image/png');
  }
  // Wiring
  $('newFile').addEventListener('click', newFile); $('newSprite').addEventListener('click', newSprite);
  $('playButton').addEventListener('click', play); $('restartButton').addEventListener('click', play); $('exportButton').addEventListener('click', exportGame);
  $('saveProject').addEventListener('click', exportProject);
  $('importProject').addEventListener('click', () => $('projectInput').click());
  $('projectInput').addEventListener('change', async (e) => { if (e.target.files?.[0]) await importProject(e.target.files[0]); e.target.value = ''; });
  $('importAsset').addEventListener('click', () => $('assetInput').click());
  $('assetInput').addEventListener('change', async (e) => { if (e.target.files?.[0]) await importPng(e.target.files[0]); e.target.value = ''; });
  $('projectName').addEventListener('input', (e) => { project.name = e.target.value; scheduleSave(); });
  code.addEventListener('input', () => { const file = item('file', active.id); if (file && active.type === 'file') { file.code = code.value; updateLines(); scheduleSave(); } });
  code.addEventListener('scroll', () => { $('lineNumbers').scrollTop = code.scrollTop; });
  code.addEventListener('click', updateLines); code.addEventListener('keyup', updateLines);
  code.addEventListener('keydown', (e) => { if (e.key === 'Tab') { e.preventDefault(); code.setRangeText('  ', code.selectionStart, code.selectionEnd, 'end'); code.dispatchEvent(new Event('input')); } });
  $('clearConsole').addEventListener('click', () => log('Console limpo.'));
  $('filesToggle').addEventListener('click', () => $('explorer').classList.add('open'));
  $('closeFiles').addEventListener('click', () => $('explorer').classList.remove('open'));
  $('openTutorial').addEventListener('click', () => { $('explorer').classList.remove('open'); setView('guide'); });
  $('backToEditor').addEventListener('click', () => setView('code'));
  for (const [button, view] of [['navCode','code'],['navSprites','sprites'],['navPlay','play'],['navGuide','guide']]) $(button).addEventListener('click', () => setView(view));
  $('brushTool').addEventListener('click', () => { tool='brush'; $('brushTool').classList.add('active'); $('eraserTool').classList.remove('active'); });
  $('eraserTool').addEventListener('click', () => { tool='eraser'; $('eraserTool').classList.add('active'); $('brushTool').classList.remove('active'); });
  $('undoSprite').addEventListener('click', () => { const sprite=item('sprite',active.id); if(sprite && spriteUndo.length){sprite.pixels=spriteUndo.pop();paintSprite();scheduleSave();} });
  $('clearSprite').addEventListener('click', () => { const sprite=item('sprite',active.id); if(sprite && confirm('Limpar todos os pixels?')){pushUndo(sprite);sprite.pixels.fill(null);paintSprite();scheduleSave();} });
  $('downloadSprite').addEventListener('click', exportSprite);
  $('spriteCanvas').addEventListener('pointerdown', e => { const sprite=item('sprite',active.id); if(!sprite)return; pushUndo(sprite); drawing=true; $('spriteCanvas').setPointerCapture(e.pointerId); paintAt(e); });
  $('spriteCanvas').addEventListener('pointermove', e => { if(drawing)paintAt(e); });
  $('spriteCanvas').addEventListener('pointerup', () => drawing=false);
  $('spriteCanvas').addEventListener('pointercancel', () => drawing=false);
  frame.addEventListener('load', () => { /* Messages below report game startup or errors. */ });
  window.addEventListener('message', e => { if(e.source!==frame.contentWindow || e.data?.mobileForge!==true)return; log(e.data.type==='ready'?'Jogo em execução. Toque na tela.':e.data.message,e.data.type==='error'); });
  document.addEventListener('keydown', e => { if((e.ctrlKey||e.metaKey)&&e.key==='Enter'){e.preventDefault();play();} if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='s'){e.preventDefault();void save();} });
  init();
})();
