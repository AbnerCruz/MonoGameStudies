import { makeEditor } from './editor.bundle.js';
import { createPlayer } from './player.js';
import { forgeWorker } from './worker.js';
import { bootStandalone } from './standalone.js';
import { flatZip } from './archive.js';

const $ = id => document.getElementById(id);
const workerSource = forgeWorker.toString();
const dbPromise = new Promise((resolve,reject)=>{
  const req=indexedDB.open('mobileforge-csharp',1);
  req.onupgradeneeded=()=>req.result.createObjectStore('projects',{keyPath:'id'});
  req.onsuccess=()=>resolve(req.result); req.onerror=()=>reject(req.error);
});
async function dbTask(mode,op){const db=await dbPromise;return new Promise((resolve,reject)=>{const tx=db.transaction('projects',mode), store=tx.objectStore('projects');const req=op(store);req.onsuccess=()=>resolve(req.result);req.onerror=()=>reject(req.error);});}
const getProjects=()=>dbTask('readonly',s=>s.getAll());
const putProject=p=>dbTask('readwrite',s=>s.put(p));
const deleteProject=id=>dbTask('readwrite',s=>s.delete(id));
const emptyCode=`using MobileForge;

public class MainGame : Game
{
    public override void Start()
    {
    }

    public override void Update(float dt)
    {
    }

    public override void Draw()
    {
    }
}
`;
const demoMain=`using MobileForge;

public class MainGame : Game
{
    public Player Player = new Player();

    public override void Start()
    {
        Player.X = 32;
        Player.Y = 80;
    }

    public override void Update(float dt)
    {
        Player.Update(dt);
    }

    public override void Draw()
    {
        Clear("#101827");
        Graphics.Text("Toque para mover", 25, 46, 20);
        Player.Draw();
    }
}
`;
const demoPlayer=`using MobileForge;

public class Player
{
    public float X, Y;

    public void Update(float dt)
    {
        if (Input.Down)
        {
            X = Input.X - 24;
            Y = Input.Y - 24;
        }
        if (Input.Key("ArrowRight")) X += 180 * dt;
        if (Input.Key("ArrowLeft")) X -= 180 * dt;
    }

    public void Draw()
    {
        Graphics.Sprite("hero", X, Y, 3);
    }
}
`;
const hero=['........','.111111.','12222221','12322321','12333321','12344321','.155551.','..1..1..'].join('').split('').map(n=>({'1':'#344e70','2':'#80e6b9','3':'#f7d28c','4':'#2c314a','5':'#4b7fe3'}[n]||null));
function newProject(name,example=false){return {id:crypto.randomUUID(),name,updatedAt:Date.now(),active:'MainGame.cs',files:example?[{name:'MainGame.cs',code:demoMain},{name:'Player.cs',code:demoPlayer}]:[{name:'MainGame.cs',code:emptyCode}],sprites:example?[{name:'hero',w:8,h:8,pixels:hero}]:[]};}
let project=null, active='', view='code', editor, player, booted=false, lastBuild=null, saving=null, generation=0;
let activeSprite='', spriteTool='pencil', selectedTile=0, spriteUndo=[], spriteRedo=[], activeMap='', mapTool='pencil', mapTile=0, assetMode='sprites';
const palette=['#101827','#ffffff','#80e6b9','#76d4dc','#ffcc92','#ff8d9c','#c5a3ff','#4b7fe3','#344e70','#f7d28c'];
function notice(message){const t=$('toast');t.textContent=message;t.hidden=false;clearTimeout(notice.timer);notice.timer=setTimeout(()=>t.hidden=true,4800);}
async function ask(title,value=''){
  $('dialogTitle').textContent=title;$('dialogInput').value=value;
  return new Promise(resolve=>{$('nameDialog').onclose=()=>resolve($('nameDialog').returnValue==='ok'?$('dialogInput').value.trim():null);$('nameDialog').showModal();$('dialogInput').focus();});
}
async function home(){generation++;player?.dispose();player=null;booted=false;project=null;lastBuild=null;editor?.clear();$('workspace').hidden=true;$('home').hidden=false;const list=(await getProjects()).sort((a,b)=>b.updatedAt-a.updatedAt);$('projects').replaceChildren();
  if(!list.length){const p=document.createElement('p');p.textContent='Nenhum projeto ainda. Crie um projeto vazio ou abra o exemplo.';$('projects').append(p);}
  for(const p of list){const card=document.createElement('article');card.className='project-card';const open=document.createElement('button');open.className='open-project';const title=document.createElement('strong');title.textContent=p.name;const meta=document.createElement('small');meta.textContent=`${p.files.length} arquivo(s) · ${new Date(p.updatedAt).toLocaleDateString('pt-BR')}`;open.append(title,meta);open.onclick=()=>openProject(p);const actions=document.createElement('div');actions.className='card-actions';const rename=document.createElement('button');rename.textContent='Renomear';rename.onclick=async()=>{const name=await ask('Nome do projeto',p.name);if(name){p.name=name;p.updatedAt=Date.now();await putProject(p);home();}};const del=document.createElement('button');del.textContent='Excluir';del.onclick=async()=>{if(confirm(`Excluir "${p.name}" deste dispositivo? Faça um backup antes se quiser guardá-lo.`)){await deleteProject(p.id);home();}};actions.append(rename,del);card.append(open,actions);$('projects').append(card);}
}
function save(){if(!project)return;$('saveStatus').textContent='Salvando…';clearTimeout(saving);saving=setTimeout(async()=>{if(!project)return;try{project.updatedAt=Date.now();await putProject(project);$('saveStatus').textContent='Salvo neste navegador';}catch(e){$('saveStatus').textContent='Erro ao salvar';notice(e.message);}},250);}
function invalidate(){generation++;lastBuild=null;$('exportGame').disabled=true;$('runtimeStatus').textContent='Alterações pendentes · Execute novamente';}
function setExplorer(open){$('fileExplorer').hidden=!open;$('filesToggle').setAttribute('aria-expanded',String(open));if(open){renderTree();$('fileSearch').focus();}}
function renderTree(){const tree=$('fileTree');tree.replaceChildren();if(!project)return;const query=$('fileSearch').value.toLowerCase();
  const folders=new Map([['',tree]]);
  for(const file of [...project.files].sort((a,b)=>a.name.localeCompare(b.name))){
    if(query&&!file.name.toLowerCase().includes(query))continue;
    const parts=file.name.split('/');let prefix='',parent=tree;
    for(const folder of parts.slice(0,-1)){
      prefix+=(prefix?'/':'')+folder;
      if(!folders.has(prefix)){const group=document.createElement('details');group.open=true;const heading=document.createElement('summary');heading.textContent=folder;group.append(heading);parent.append(group);folders.set(prefix,group);}
      parent=folders.get(prefix);
    }
    const button=document.createElement('button');button.textContent=parts.at(-1);button.title=file.name;button.classList.toggle('selected',file.name===active);button.onclick=()=>selectFile(file.name);parent.append(button);
  }
  if(!tree.children.length){const empty=document.createElement('small');empty.textContent='Nenhum arquivo encontrado.';tree.append(empty);}
}
function renderTabs(){$('tabs').replaceChildren();for(const file of project.files){const button=document.createElement('button');button.textContent=file.name.split('/').at(-1);button.title=file.name;button.classList.toggle('selected',file.name===active);button.onclick=()=>selectFile(file.name);$('tabs').append(button);}renderTree();}
function selectFile(name){active=name;project.active=name;const file=project.files.find(f=>f.name===name);if(!file)return;editor.open(project.id+'/'+name,file.code);editor.diagnostics(file.diagnostics||[]);renderTabs();setExplorer(false);save();}
function showView(name){view=name;for(const section of ['code','sprites','game','errors','guide'])$(section+'View').hidden=section!==name;document.querySelectorAll('.bottom-nav button').forEach(b=>b.classList.toggle('selected',b.dataset.view===name));}
function currentSprite(){return project?.sprites.find(s=>s.name===activeSprite);}
function currentMap(){return project?.tilemaps.find(m=>m.name===activeMap);}
function sheetSize(sprite){return {cw:sprite.cellW||sprite.w,ch:sprite.cellH||sprite.h,cols:Math.floor(sprite.w/(sprite.cellW||sprite.w)),rows:Math.floor(sprite.h/(sprite.cellH||sprite.h))};}
function tilePixels(sprite,index){const {cw,ch,cols}=sheetSize(sprite),startX=index%cols*cw,startY=Math.floor(index/cols)*ch;
  const canvas=document.createElement('canvas');canvas.width=cw;canvas.height=ch;const ctx=canvas.getContext('2d');
  for(let y=0;y<ch;y++)for(let x=0;x<cw;x++){const color=sprite.pixels[(startY+y)*sprite.w+startX+x];if(color){ctx.fillStyle=color;ctx.fillRect(x,y,1,1);}}
  return canvas;
}
function renderTileStrip(container,sprite,selected,click){
  container.replaceChildren();if(!sprite)return;
  const {cols,rows}=sheetSize(sprite),count=cols*rows;
  for(let i=0;i<count;i++){const button=document.createElement('button');button.className='tile-choice';button.classList.toggle('selected',i===selected);button.title='Célula '+i;button.setAttribute('aria-label','Célula '+i);
    const preview=tilePixels(sprite,i);button.append(preview);button.onclick=()=>click(i);container.append(button);}
}
function showAssetMode(mode){assetMode=mode;$('spritePanel').hidden=mode!=='sprites';$('mapPanel').hidden=mode!=='maps';$('assetsSprites').classList.toggle('selected',mode==='sprites');$('assetsMaps').classList.toggle('selected',mode==='maps');if(mode==='maps')renderMaps();}
function renderSpriteList(){
  $('spriteList').replaceChildren();
  for(const sprite of project.sprites){const button=document.createElement('button');button.textContent=sprite.name;button.classList.toggle('selected',sprite.name===activeSprite);button.onclick=()=>selectSprite(sprite.name);$('spriteList').append(button);}
  const sprite=currentSprite();$('spriteEmpty').hidden=!!sprite;$('spriteWorkspace').hidden=!sprite;
  if(sprite){const {cw,ch,cols,rows}=sheetSize(sprite);selectedTile=Math.min(selectedTile,cols*rows-1);
    $('spriteName').textContent=sprite.name;$('spriteDimensions').textContent=`${sprite.w} × ${sprite.h} pixels`;
    $('spriteCellW').value=cw;$('spriteCellH').value=ch;$('tileCount').textContent=`${cols*rows} célula(s)`;
    const canvas=$('spriteCanvas');canvas.width=cw;canvas.height=ch;
    canvas.style.width=cw>=ch?'100%':`${cw/ch*100}%`;
    canvas.style.height=ch>=cw?'100%':`${ch/cw*100}%`;
    paintSpriteCanvas();
    renderTileStrip($('spriteTiles'),sprite,selectedTile,index=>{selectedTile=index;renderSpriteList();});}
}
function paintSpriteCanvas(){const sprite=currentSprite();if(!sprite)return;const canvas=$('spriteCanvas'),ctx=canvas.getContext('2d');ctx.clearRect(0,0,canvas.width,canvas.height);
  ctx.drawImage(tilePixels(sprite,selectedTile),0,0);
}
function selectSprite(name){activeSprite=name;selectedTile=0;spriteUndo=[];spriteRedo=[];project.activeSprite=name;renderSpriteList();save();}
function selectSpriteTool(name){spriteTool=name;for(const tool of ['Pencil','Eraser','Fill','Picker'])$('sprite'+tool).classList.toggle('selected',name===tool.toLowerCase());}
function pushSpriteUndo(){const sprite=currentSprite();spriteUndo.push({name:sprite.name,pixels:[...sprite.pixels]});if(spriteUndo.length>30)spriteUndo.shift();spriteRedo=[];}
function restoreSprite(source,target){const sprite=currentSprite(),state=source.pop();if(!sprite||!state||state.name!==sprite.name)return;
  target.push({name:sprite.name,pixels:[...sprite.pixels]});sprite.pixels=state.pixels;paintSpriteCanvas();renderTileStrip($('spriteTiles'),sprite,selectedTile,index=>{selectedTile=index;renderSpriteList();});invalidate();save();}
function drawAt(event){const sprite=currentSprite();if(!sprite)return;const rect=$('spriteCanvas').getBoundingClientRect();
  const {cw,ch,cols}=sheetSize(sprite),localX=Math.floor((event.clientX-rect.left)*cw/rect.width),localY=Math.floor((event.clientY-rect.top)*ch/rect.height);
  if(localX<0||localY<0||localX>=cw||localY>=ch)return;
  const x=selectedTile%cols*cw+localX,y=Math.floor(selectedTile/cols)*ch+localY,index=y*sprite.w+x;
  if(spriteTool==='picker'){const color=sprite.pixels[index];if(color&&/^#[0-9a-f]{6}/i.test(color))$('spriteColor').value=color.slice(0,7);selectSpriteTool('pencil');return;}
  const color=spriteTool==='eraser'?null:$('spriteColor').value,previous=sprite.pixels[index];
  if(previous===color)return;
  if(spriteTool==='fill'){
    const todo=[[localX,localY]],seen=new Set();
    while(todo.length){const [px,py]=todo.pop();if(px<0||py<0||px>=cw||py>=ch||seen.has(py*cw+px))continue;seen.add(py*cw+px);
      const at=(Math.floor(selectedTile/cols)*ch+py)*sprite.w+selectedTile%cols*cw+px;
      if(sprite.pixels[at]!==previous)continue;sprite.pixels[at]=color;todo.push([px-1,py],[px+1,py],[px,py-1],[px,py+1]);}
  }else sprite.pixels[index]=color;
  paintSpriteCanvas();renderTileStrip($('spriteTiles'),sprite,selectedTile,n=>{selectedTile=n;renderSpriteList();});invalidate();save();
}
function renderMaps(){const list=$('mapList');list.replaceChildren();for(const map of project.tilemaps){const b=document.createElement('button');b.textContent=map.name;b.classList.toggle('selected',map.name===activeMap);b.onclick=()=>{activeMap=map.name;renderMaps();save();};list.append(b);}
  const map=currentMap();$('mapEmpty').hidden=!!map;$('mapWorkspace').hidden=!map;if(!map)return;
  $('mapName').textContent=map.name;$('mapDimensions').textContent=`${map.w} × ${map.h} células`;$('mapWidth').value=map.w;$('mapHeight').value=map.h;
  const select=$('mapSheet');select.replaceChildren();for(const sprite of project.sprites){const option=document.createElement('option');option.value=sprite.name;option.textContent=sprite.name;select.append(option);}
  if(!project.sprites.some(s=>s.name===map.sheet)){map.sheet=project.sprites[0]?.name||'';map.cells.fill(-1);}select.value=map.sheet;
  const sheet=project.sprites.find(s=>s.name===map.sheet);if(!sheet)return;
  const {cols,rows}=sheetSize(sheet);mapTile=Math.min(mapTile,cols*rows-1);
  renderTileStrip($('mapPalette'),sheet,mapTile,index=>{mapTile=index;renderMaps();});paintMap();
}
function paintMap(){const map=currentMap(),sheet=project.sprites.find(s=>s.name===map?.sheet);if(!map||!sheet)return;
  const {cw,ch}=sheetSize(sheet),canvas=$('mapCanvas');canvas.width=map.w*cw;canvas.height=map.h*ch;
  const ctx=canvas.getContext('2d');ctx.fillStyle='#172234';ctx.fillRect(0,0,canvas.width,canvas.height);ctx.imageSmoothingEnabled=false;
  const tiles=new Map();
  for(let i=0;i<map.cells.length;i++)if(map.cells[i]>=0){const index=map.cells[i];if(!tiles.has(index))tiles.set(index,tilePixels(sheet,index));ctx.drawImage(tiles.get(index),i%map.w*cw,Math.floor(i/map.w)*ch);}
  ctx.strokeStyle='#d4edff35';ctx.lineWidth=1;
  for(let x=0;x<=map.w;x++){ctx.beginPath();ctx.moveTo(x*cw+.5,0);ctx.lineTo(x*cw+.5,canvas.height);ctx.stroke();}
  for(let y=0;y<=map.h;y++){ctx.beginPath();ctx.moveTo(0,y*ch+.5);ctx.lineTo(canvas.width,y*ch+.5);ctx.stroke();}
}
function mapAt(event){const map=currentMap();if(!map)return;const rect=$('mapCanvas').getBoundingClientRect(),x=Math.floor((event.clientX-rect.left)*map.w/rect.width),y=Math.floor((event.clientY-rect.top)*map.h/rect.height);
  if(x<0||y<0||x>=map.w||y>=map.h)return;const at=y*map.w+x,value=mapTool==='eraser'?-1:mapTile;
  if(map.cells[at]===value)return;map.cells[at]=value;paintMap();invalidate();save();
}
async function openProject(p){project=p;p.sprites??=[];p.tilemaps??=[];active=p.files.find(f=>f.name===p.active)?.name||p.files[0].name;activeSprite=p.sprites.find(s=>s.name===p.activeSprite)?.name||p.sprites[0]?.name||'';activeMap=p.tilemaps.find(m=>m.name===p.activeMap)?.name||p.tilemaps[0]?.name||'';$('fileSearch').value='';$('home').hidden=true;$('workspace').hidden=false;$('projectName').textContent=p.name;$('saveStatus').textContent='Salvo neste navegador';$('diagnostics').textContent='Os erros aparecerão aqui, com arquivo e linha.';$('errorLabel').textContent='Erros';$('runtimeStatus').textContent='Pronto para compilar';$('exportGame').disabled=true;
  editor??=makeEditor($('editor'),code=>{if(!project)return;project.files.find(f=>f.name===active).code=code;invalidate();save();});
  selectFile(active);renderSpriteList();showAssetMode('sprites');showView('code');save();}
function showErrors(items){$('diagnostics').replaceChildren();const errors=items.filter(d=>d.severity==='Error');$('errorLabel').textContent=errors.length?'Erros ('+errors.length+')':'Erros';for(const f of project.files)f.diagnostics=items.filter(d=>d.file===f.name);
  if(!items.length){const p=document.createElement('p');p.className='success';p.textContent='Compilado sem erros.';$('diagnostics').append(p);}
  for(const d of items){const b=document.createElement('button');b.className='diagnostic';b.textContent=`${d.file}:${d.line}:${d.column} · ${d.code} · ${d.message}`;const small=document.createElement('small');small.textContent=d.severity==='Error'?'Erro':'Aviso';b.append(small);b.onclick=()=>{if(project.files.some(f=>f.name===d.file)){selectFile(d.file);showView('code');editor.go(d.line);}};$('diagnostics').append(b);}
  editor.diagnostics(project.files.find(f=>f.name===active)?.diagnostics||[]);return errors.length;}
function runtimeError(error){$('runtimeStatus').textContent='Erro no jogo';$('diagnostics').textContent=error.message||String(error);showView('errors');$('stop').disabled=true;notice('Erro na execução. Veja a aba Erros.');}
async function run(){if(!project)return;const runId=++generation;const snapshot=project.files.map(f=>({name:f.name,code:f.code}));lastBuild=null;$('exportGame').disabled=true;$('run').disabled=true;$('runtimeStatus').textContent='Carregando .NET e compilando C#…';showView('game');
  try{player?.dispose();player=createPlayer($('canvas'),workerSource,project.sprites,runtimeError,null,project.tilemaps);booted=false;await player.boot({base:new URL('./',import.meta.url).href});if(runId!==generation)return;booted=true;const result=await player.compile(snapshot);if(runId!==generation)return;
    const errors=showErrors(result.diagnostics||[]);if(!result.ok||errors){$('runtimeStatus').textContent='Revise os erros de compilação';showView('errors');return;}
    await player.play(result.assembly);if(runId!==generation)return;lastBuild={assembly:result.assembly,code:JSON.stringify(snapshot)};$('exportGame').disabled=false;$('stop').disabled=false;$('runtimeStatus').textContent='C# compilado • Jogo em execução';
  }catch(error){if(runId===generation)runtimeError(error);}finally{if(runId===generation)$('run').disabled=false;}}
function download(name,content,type){const url=URL.createObjectURL(new Blob([content],{type}));const link=document.createElement('a');link.href=url;link.download=name;document.body.append(link);link.click();link.remove();setTimeout(()=>URL.revokeObjectURL(url),60000);}
function escapeHTML(value){return value.replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
function safeJS(value){return JSON.stringify(value).replaceAll('<','\\u003c').replaceAll('\u2028','\\u2028').replaceAll('\u2029','\\u2029');}
async function exportGame(){if(!lastBuild||!project||lastBuild.code!==JSON.stringify(project.files.map(f=>({name:f.name,code:f.code})))){notice('Execute o código atual antes de exportar.');return;}
  const p=project, build=lastBuild;$('exportGame').disabled=true;$('runtimeStatus').textContent='Montando ZIP offline…';
  try{const response=await fetch('player-framework.pack.gz');if(!response.ok)throw new Error('Pacote offline indisponível.');const bytes=new Uint8Array(await response.arrayBuffer());let text64='';for(let i=0;i<bytes.length;i+=32768)text64+=String.fromCharCode(...bytes.subarray(i,i+32768));const pack=btoa(text64);
    const html=`<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover"><title>${escapeHTML(p.name)}</title><style>html,body{margin:0;width:100%;height:100%;background:#101827;display:grid;place-items:center;overflow:hidden}canvas{height:min(100dvh,177.78vw);width:min(100vw,56.25dvh);image-rendering:pixelated;touch-action:none}#error{position:fixed;bottom:8px;background:#732c42;color:white;padding:12px;font:14px sans-serif;max-height:40vh;overflow:auto}</style></head><body><canvas width="360" height="640" tabindex="0"></canvas><div id="error" hidden></div><div id="status" style="position:fixed;bottom:16px;color:#a5b5c9;font:13px system-ui">Carregando jogo…</div><script src="runtime-data.js"></script><script src="game-data.js"></script><script src="game.js"></script></body></html>`;
    const script=`const createPlayer=${createPlayer.toString()};const bootStandalone=${bootStandalone.toString()};const error=document.getElementById('error'),status=document.getElementById('status');const report=e=>{status.hidden=true;error.hidden=false;error.textContent=e.stack||e.message||String(e)};const player=createPlayer(document.querySelector('canvas'),null,window.MobileForgeGame.sprites,report,pack=>bootStandalone(pack,message=>status.textContent=message),window.MobileForgeGame.tilemaps);(async()=>{try{await player.boot({pack:window.MobileForgeRuntimePack});await player.play(window.MobileForgeGame.assembly);status.hidden=true}catch(e){report(e)}})();`;
    const zip=flatZip({'index.html':html,'game.js':script,'game-data.js':'window.MobileForgeGame='+safeJS({assembly:build.assembly,sprites:p.sprites,tilemaps:p.tilemaps})+';','runtime-data.js':'window.MobileForgeRuntimePack='+safeJS(pack)+';'});
    download(p.name.replace(/[^a-z0-9_-]/gi,'-')+'.zip',zip,'application/zip');$('runtimeStatus').textContent='ZIP do jogo baixado';notice(`Jogo exportado (${(zip.size/1048576).toFixed(1)} MB). Extraia e abra index.html.`);
  }catch(e){notice(e.message);$('runtimeStatus').textContent='Falha na exportação';}finally{$('exportGame').disabled=false;}}
$('newProject').onclick=async()=>{const name=await ask('Nome do novo projeto','Meu jogo');if(!name)return;const p=newProject(name);await putProject(p);openProject(p);};
$('openExample').onclick=async()=>{const p=newProject('Exemplo Player',true);await putProject(p);openProject(p);};
$('importProject').onclick=()=>$('importInput').click();$('importInput').onchange=async e=>{const file=e.target.files[0];if(!file)return;try{const p=JSON.parse(await file.text());if(!p.name||!Array.isArray(p.files)||!p.files.length||p.files.some(f=>!f.name?.endsWith('.cs')||typeof f.code!=='string'))throw new Error('Projeto inválido.');p.id=crypto.randomUUID();p.updatedAt=Date.now();p.sprites??=[];await putProject(p);openProject(p);}catch(error){notice(error.message);}e.target.value='';};
$('homeButton').onclick=async()=>{clearTimeout(saving);if(project)await putProject(project);home();};
$('run').onclick=run;$('stop').onclick=()=>{generation++;player?.dispose();player=null;booted=false;$('stop').disabled=true;$('runtimeStatus').textContent='Jogo parado';};
$('exportGame').onclick=exportGame;
$('backup').onclick=()=>{if(project)download(project.name.replace(/[^a-z0-9_-]/gi,'-')+'.mobileforge.json',JSON.stringify(project,null,2),'application/json');};
const sourcePath=/^(?:[A-Za-z_][\w-]*\/)*[A-Za-z_][\w-]*\.cs$/;
$('filesToggle').onclick=()=>setExplorer($('fileExplorer').hidden);
$('closeExplorer').onclick=()=>setExplorer(false);
$('fileSearch').oninput=renderTree;
$('addFile').onclick=async()=>{const name=await ask('Novo arquivo C# (pastas opcionais)','Actors/Player.cs');if(!name)return;if(!sourcePath.test(name)||project.files.some(f=>f.name.toLowerCase()===name.toLowerCase())){notice('Use um caminho .cs válido e ainda não utilizado.');return;}const className=name.split('/').at(-1).slice(0,-3).replace(/-/g,'_');project.files.push({name,code:`public class ${className}\n{\n}\n`});invalidate();selectFile(name);showView('code');};
$('renameFile').onclick=async()=>{if(active==='MainGame.cs'){notice('MainGame.cs é o arquivo principal.');return;}const name=await ask('Renomear ou mover arquivo',active);if(!name)return;if(!sourcePath.test(name)||project.files.some(f=>f.name.toLowerCase()===name.toLowerCase())){notice('Caminho inválido ou já utilizado.');return;}const old=active;project.files.find(f=>f.name===active).name=name;editor.rename(project.id+'/'+old,project.id+'/'+name);invalidate();selectFile(name);};
$('deleteFile').onclick=()=>{if(active==='MainGame.cs'){notice('MainGame.cs é necessário.');return;}if(!confirm(`Excluir ${active}?`))return;editor.forget(project.id+'/'+active);project.files=project.files.filter(f=>f.name!==active);invalidate();selectFile('MainGame.cs');};
$('newSprite').onclick=async()=>{const name=await ask('Nome do sprite','hero');if(!name)return;
  if(!/^[a-zA-Z_][\w-]{0,39}$/.test(name)||project.sprites.some(s=>s.name.toLowerCase()===name.toLowerCase())){notice('Use um nome de até 40 caracteres, sem espaços e ainda não usado.');return;}
  project.sprites.push({name,w:16,h:16,pixels:Array(256).fill(null)});invalidate();selectSprite(name);showView('sprites');};
$('importSprite').onclick=()=>$('spriteInput').click();
$('spriteInput').onchange=async event=>{const file=event.target.files?.[0];event.target.value='';if(!file)return;
  try{if(file.type!=='image/png'&&!file.name.toLowerCase().endsWith('.png'))throw new Error('Selecione um arquivo PNG.');
    const name=await ask('Nome do sprite importado',file.name.replace(/\.png$/i,'').replace(/[^\w-]/g,'_').slice(0,40));if(!name)return;
    if(!/^[a-zA-Z_][\w-]{0,39}$/.test(name)||project.sprites.some(s=>s.name.toLowerCase()===name.toLowerCase()))throw new Error('Nome inválido ou já utilizado.');
    const bitmap=await createImageBitmap(file);try{
      if(bitmap.width<1||bitmap.height<1||bitmap.width>256||bitmap.height>256)throw new Error('Use um PNG entre 1 × 1 e 256 × 256 pixels.');
      const canvas=document.createElement('canvas');canvas.width=bitmap.width;canvas.height=bitmap.height;const ctx=canvas.getContext('2d',{willReadFrequently:true});ctx.drawImage(bitmap,0,0);
      const data=ctx.getImageData(0,0,bitmap.width,bitmap.height).data;const pixels=[];
      for(let i=0;i<data.length;i+=4){pixels.push(data[i+3]===0?null:'#'+[data[i],data[i+1],data[i+2],data[i+3]].map(v=>v.toString(16).padStart(2,'0')).join(''));}
      const cellW=bitmap.width>16&&bitmap.width%16===0?16:bitmap.width,cellH=bitmap.height>16&&bitmap.height%16===0?16:bitmap.height;
      project.sprites.push({name,w:bitmap.width,h:bitmap.height,cellW,cellH,pixels});invalidate();selectSprite(name);showAssetMode('sprites');showView('sprites');
    }finally{bitmap.close();}
  }catch(error){notice(error.message||String(error));}};
$('deleteSprite').onclick=()=>{if(!currentSprite())return;if(!confirm(`Excluir o sprite ${activeSprite}?`))return;
  const removed=activeSprite;project.sprites=project.sprites.filter(s=>s.name!==removed);activeSprite=project.sprites[0]?.name||'';project.activeSprite=activeSprite;
  for(const map of project.tilemaps)if(map.sheet===removed){map.sheet=activeSprite;map.cells.fill(-1);}
  renderSpriteList();invalidate();save();};
$('newSheet').onclick=async()=>{const name=await ask('Nome do spritesheet','tiles');if(!name)return;
  if(!/^[a-zA-Z_][\w-]{0,39}$/.test(name)||project.sprites.some(s=>s.name.toLowerCase()===name.toLowerCase())){notice('Nome inválido ou já utilizado.');return;}
  project.sprites.push({name,w:64,h:64,cellW:16,cellH:16,pixels:Array(64*64).fill(null)});invalidate();selectSprite(name);showAssetMode('sprites');showView('sprites');};
function updateCells(){const sprite=currentSprite();if(!sprite)return;const cw=Number($('spriteCellW').value),ch=Number($('spriteCellH').value);
  if(!Number.isInteger(cw)||!Number.isInteger(ch)||cw<1||ch<1||sprite.w%cw||sprite.h%ch){notice('As células devem dividir exatamente largura e altura da imagem.');renderSpriteList();return;}
  sprite.cellW=cw;sprite.cellH=ch;selectedTile=0;for(const map of project.tilemaps)if(map.sheet===sprite.name)map.cells.fill(-1);renderSpriteList();invalidate();save();}
$('spriteCellW').onchange=updateCells;$('spriteCellH').onchange=updateCells;
$('spritePencil').onclick=()=>selectSpriteTool('pencil');$('spriteEraser').onclick=()=>selectSpriteTool('eraser');
$('spriteFill').onclick=()=>selectSpriteTool('fill');$('spritePicker').onclick=()=>selectSpriteTool('picker');
$('spriteUndo').onclick=()=>restoreSprite(spriteUndo,spriteRedo);$('spriteRedo').onclick=()=>restoreSprite(spriteRedo,spriteUndo);
for(const color of palette){const button=document.createElement('button');button.type='button';button.style.background=color;button.title=color;button.setAttribute('aria-label','Cor '+color);button.onclick=()=>{$('spriteColor').value=color;selectSpriteTool('pencil');};$('spritePalette').append(button);}
$('spriteColor').oninput=()=>selectSpriteTool('pencil');
$('spriteCanvas').addEventListener('pointerdown',event=>{event.preventDefault();$('spriteCanvas').setPointerCapture(event.pointerId);if(spriteTool!=='picker')pushSpriteUndo();drawAt(event);});
$('spriteCanvas').addEventListener('pointermove',event=>{if(event.buttons&1&&['pencil','eraser'].includes(spriteTool))drawAt(event);});
$('downloadSprite').onclick=()=>{const sprite=currentSprite();if(!sprite)return;const canvas=document.createElement('canvas');canvas.width=sprite.w;canvas.height=sprite.h;const ctx=canvas.getContext('2d');for(let i=0;i<sprite.pixels.length;i++)if(sprite.pixels[i]){ctx.fillStyle=sprite.pixels[i];ctx.fillRect(i%sprite.w,Math.floor(i/sprite.w),1,1);}
  const link=document.createElement('a');link.download=sprite.name+'.png';link.href=canvas.toDataURL('image/png');document.body.append(link);link.click();link.remove();};
$('assetsSprites').onclick=()=>showAssetMode('sprites');$('assetsMaps').onclick=()=>showAssetMode('maps');
$('newMap').onclick=async()=>{if(!project.sprites.length){notice('Crie ou importe um spritesheet primeiro.');showAssetMode('sprites');return;}
  const name=await ask('Nome do tilemap','level');if(!name)return;if(!/^[a-zA-Z_][\w-]{0,39}$/.test(name)||project.tilemaps.some(m=>m.name.toLowerCase()===name.toLowerCase())){notice('Nome inválido ou já utilizado.');return;}
  project.tilemaps.push({name,sheet:project.sprites[0].name,w:20,h:20,cells:Array(400).fill(-1)});activeMap=name;project.activeMap=name;mapTile=0;renderMaps();invalidate();save();};
$('deleteMap').onclick=()=>{if(!currentMap()||!confirm(`Excluir o mapa ${activeMap}?`))return;project.tilemaps=project.tilemaps.filter(m=>m.name!==activeMap);activeMap=project.tilemaps[0]?.name||'';renderMaps();invalidate();save();};
$('mapSheet').onchange=()=>{const map=currentMap();map.sheet=$('mapSheet').value;map.cells.fill(-1);mapTile=0;renderMaps();invalidate();save();};
function resizeMap(){const map=currentMap();if(!map)return;const w=Number($('mapWidth').value),h=Number($('mapHeight').value);
  if(!Number.isInteger(w)||!Number.isInteger(h)||w<1||h<1||w>64||h>64){notice('Use dimensões entre 1 e 64 células.');renderMaps();return;}
  const cells=Array(w*h).fill(-1);for(let y=0;y<Math.min(h,map.h);y++)for(let x=0;x<Math.min(w,map.w);x++)cells[y*w+x]=map.cells[y*map.w+x];
  map.w=w;map.h=h;map.cells=cells;renderMaps();invalidate();save();}
$('mapWidth').onchange=resizeMap;$('mapHeight').onchange=resizeMap;
$('mapPencil').onclick=()=>{mapTool='pencil';$('mapPencil').classList.add('selected');$('mapEraser').classList.remove('selected');};
$('mapEraser').onclick=()=>{mapTool='eraser';$('mapEraser').classList.add('selected');$('mapPencil').classList.remove('selected');};
$('mapCanvas').addEventListener('pointerdown',event=>{event.preventDefault();$('mapCanvas').setPointerCapture(event.pointerId);mapAt(event);});
$('mapCanvas').addEventListener('pointermove',event=>{if(event.buttons&1)mapAt(event);});
document.querySelectorAll('.bottom-nav button').forEach(b=>b.onclick=()=>showView(b.dataset.view));
document.querySelectorAll('.typing-tools button').forEach(b=>b.onclick=()=>editor.insert(b.dataset.insert));
window.addEventListener('keydown',event=>{if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='s'){event.preventDefault();if(project){clearTimeout(saving);project.updatedAt=Date.now();putProject(project).then(()=>$('saveStatus').textContent='Salvo neste navegador').catch(error=>notice(error.message));}}});
window.addEventListener('pagehide',()=>{if(project)putProject(project);});
home().catch(e=>notice('Armazenamento indisponível: '+e.message));
