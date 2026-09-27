import { makeEditor } from './editor.bundle.js';
import { createPlayer } from './player.js';
import { forgeWorker } from './worker.js';
import { bootStandalone } from './standalone.js';

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
function notice(message){const t=$('toast');t.textContent=message;t.hidden=false;clearTimeout(notice.timer);notice.timer=setTimeout(()=>t.hidden=true,4800);}
async function ask(title,value=''){
  $('dialogTitle').textContent=title;$('dialogInput').value=value;
  return new Promise(resolve=>{$('nameDialog').onclose=()=>resolve($('nameDialog').returnValue==='ok'?$('dialogInput').value.trim():null);$('nameDialog').showModal();$('dialogInput').focus();});
}
async function home(){generation++;player?.dispose();player=null;booted=false;project=null;lastBuild=null;$('workspace').hidden=true;$('home').hidden=false;const list=(await getProjects()).sort((a,b)=>b.updatedAt-a.updatedAt);$('projects').replaceChildren();
  if(!list.length){const p=document.createElement('p');p.textContent='Nenhum projeto ainda. Crie um projeto vazio ou abra o exemplo.';$('projects').append(p);}
  for(const p of list){const card=document.createElement('article');card.className='project-card';const open=document.createElement('button');open.className='open-project';const title=document.createElement('strong');title.textContent=p.name;const meta=document.createElement('small');meta.textContent=`${p.files.length} arquivo(s) · ${new Date(p.updatedAt).toLocaleDateString('pt-BR')}`;open.append(title,meta);open.onclick=()=>openProject(p);const actions=document.createElement('div');actions.className='card-actions';const rename=document.createElement('button');rename.textContent='Renomear';rename.onclick=async()=>{const name=await ask('Nome do projeto',p.name);if(name){p.name=name;p.updatedAt=Date.now();await putProject(p);home();}};const del=document.createElement('button');del.textContent='Excluir';del.onclick=async()=>{if(confirm(`Excluir "${p.name}" deste dispositivo? Faça um backup antes se quiser guardá-lo.`)){await deleteProject(p.id);home();}};actions.append(rename,del);card.append(open,actions);$('projects').append(card);}
}
function save(){if(!project)return;$('saveStatus').textContent='Salvando…';clearTimeout(saving);saving=setTimeout(async()=>{if(!project)return;try{project.updatedAt=Date.now();await putProject(project);$('saveStatus').textContent='Salvo neste navegador';}catch(e){$('saveStatus').textContent='Erro ao salvar';notice(e.message);}},250);}
function invalidate(){generation++;lastBuild=null;$('exportGame').disabled=true;$('runtimeStatus').textContent='Alterações pendentes · Execute novamente';}
function renderTabs(){$('tabs').replaceChildren();for(const file of project.files){const button=document.createElement('button');button.textContent=file.name;button.classList.toggle('selected',file.name===active);button.onclick=()=>selectFile(file.name);$('tabs').append(button);}}
function selectFile(name){active=name;project.active=name;const file=project.files.find(f=>f.name===name);if(!file)return;editor.set(file.code);editor.diagnostics(file.diagnostics||[]);renderTabs();save();}
function showView(name){view=name;for(const section of ['code','game','errors','guide'])$(section+'View').hidden=section!==name;document.querySelectorAll('.bottom-nav button').forEach(b=>b.classList.toggle('selected',b.dataset.view===name));}
async function openProject(p){project=p;active=p.files.find(f=>f.name===p.active)?.name||p.files[0].name;$('home').hidden=true;$('workspace').hidden=false;$('projectName').textContent=p.name;$('saveStatus').textContent='Salvo neste navegador';$('diagnostics').textContent='Os erros aparecerão aqui, com arquivo e linha.';$('errorLabel').textContent='Erros';$('runtimeStatus').textContent='Pronto para compilar';$('exportGame').disabled=true;
  editor??=makeEditor($('editor'),code=>{if(!project)return;project.files.find(f=>f.name===active).code=code;invalidate();save();});
  selectFile(active);showView('code');save();}
function showErrors(items){$('diagnostics').replaceChildren();const errors=items.filter(d=>d.severity==='Error');$('errorLabel').textContent=errors.length?'Erros ('+errors.length+')':'Erros';for(const f of project.files)f.diagnostics=items.filter(d=>d.file===f.name);
  if(!items.length){const p=document.createElement('p');p.className='success';p.textContent='Compilado sem erros.';$('diagnostics').append(p);}
  for(const d of items){const b=document.createElement('button');b.className='diagnostic';b.textContent=`${d.file}:${d.line}:${d.column} · ${d.code} · ${d.message}`;const small=document.createElement('small');small.textContent=d.severity==='Error'?'Erro':'Aviso';b.append(small);b.onclick=()=>{if(project.files.some(f=>f.name===d.file)){selectFile(d.file);showView('code');editor.go(d.line);}};$('diagnostics').append(b);}
  editor.diagnostics(project.files.find(f=>f.name===active)?.diagnostics||[]);return errors.length;}
function runtimeError(error){$('runtimeStatus').textContent='Erro no jogo';$('diagnostics').textContent=error.message||String(error);showView('errors');$('stop').disabled=true;notice('Erro na execução. Veja a aba Erros.');}
async function run(){if(!project)return;const runId=++generation;const snapshot=project.files.map(f=>({name:f.name,code:f.code}));lastBuild=null;$('exportGame').disabled=true;$('run').disabled=true;$('runtimeStatus').textContent='Carregando .NET e compilando C#…';showView('game');
  try{player?.dispose();player=createPlayer($('canvas'),workerSource,project.sprites,runtimeError);booted=false;await player.boot({base:new URL('./',import.meta.url).href});if(runId!==generation)return;booted=true;const result=await player.compile(snapshot);if(runId!==generation)return;
    const errors=showErrors(result.diagnostics||[]);if(!result.ok||errors){$('runtimeStatus').textContent='Revise os erros de compilação';showView('errors');return;}
    await player.play(result.assembly);if(runId!==generation)return;lastBuild={assembly:result.assembly,code:JSON.stringify(snapshot)};$('exportGame').disabled=false;$('stop').disabled=false;$('runtimeStatus').textContent='C# compilado • Jogo em execução';
  }catch(error){if(runId===generation)runtimeError(error);}finally{if(runId===generation)$('run').disabled=false;}}
function download(name,content,type){const url=URL.createObjectURL(new Blob([content],{type}));const link=document.createElement('a');link.href=url;link.download=name;document.body.append(link);link.click();link.remove();setTimeout(()=>URL.revokeObjectURL(url),60000);}
function escapeHTML(value){return value.replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
function safeJS(value){return JSON.stringify(value).replaceAll('<','\\u003c').replaceAll('\u2028','\\u2028').replaceAll('\u2029','\\u2029');}
async function exportGame(){if(!lastBuild||!project||lastBuild.code!==JSON.stringify(project.files.map(f=>({name:f.name,code:f.code})))){notice('Execute o código atual antes de exportar.');return;}
  const p=project, build=lastBuild;$('exportGame').disabled=true;$('runtimeStatus').textContent='Montando HTML offline…';
  try{const response=await fetch('player-framework.pack.gz');if(!response.ok)throw new Error('Pacote offline indisponível.');const bytes=new Uint8Array(await response.arrayBuffer());let text64='';for(let i=0;i<bytes.length;i+=32768)text64+=String.fromCharCode(...bytes.subarray(i,i+32768));const pack=btoa(text64);
    const html=`<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover"><title>${escapeHTML(p.name)}</title><style>html,body{margin:0;width:100%;height:100%;background:#101827;display:grid;place-items:center;overflow:hidden}canvas{height:min(100dvh,177.78vw);width:min(100vw,56.25dvh);image-rendering:pixelated;touch-action:none}#error{position:fixed;bottom:8px;background:#732c42;color:white;padding:12px;font:14px sans-serif;max-height:40vh;overflow:auto}</style></head><body><canvas width="360" height="640" tabindex="0"></canvas><div id="error" hidden></div><div id="status" style="position:fixed;bottom:16px;color:#a5b5c9;font:13px system-ui">Carregando jogo…</div><script>const createPlayer=${createPlayer.toString()};const bootStandalone=${bootStandalone.toString()};const error=document.getElementById('error'),status=document.getElementById('status');const report=e=>{status.hidden=true;error.hidden=false;error.textContent=e.stack||e.message||String(e)};const player=createPlayer(document.querySelector('canvas'),null,${safeJS(p.sprites)},report,pack=>bootStandalone(pack,message=>status.textContent=message));(async()=>{try{await player.boot({pack:${safeJS(pack)}});await player.play(${safeJS(build.assembly)});status.hidden=true}catch(e){report(e)}})();</script></body></html>`;
    download(p.name.replace(/[^a-z0-9_-]/gi,'-')+'.html',html,'text/html');$('runtimeStatus').textContent='HTML offline baixado';notice(`Jogo exportado (${(new Blob([html]).size/1048576).toFixed(1)} MB).`);
  }catch(e){notice(e.message);$('runtimeStatus').textContent='Falha na exportação';}finally{$('exportGame').disabled=false;}}
$('newProject').onclick=async()=>{const name=await ask('Nome do novo projeto','Meu jogo');if(!name)return;const p=newProject(name);await putProject(p);openProject(p);};
$('openExample').onclick=async()=>{const p=newProject('Exemplo Player',true);await putProject(p);openProject(p);};
$('importProject').onclick=()=>$('importInput').click();$('importInput').onchange=async e=>{const file=e.target.files[0];if(!file)return;try{const p=JSON.parse(await file.text());if(!p.name||!Array.isArray(p.files)||!p.files.length||p.files.some(f=>!f.name?.endsWith('.cs')||typeof f.code!=='string'))throw new Error('Projeto inválido.');p.id=crypto.randomUUID();p.updatedAt=Date.now();p.sprites??=[];await putProject(p);openProject(p);}catch(error){notice(error.message);}e.target.value='';};
$('homeButton').onclick=async()=>{clearTimeout(saving);if(project)await putProject(project);home();};
$('run').onclick=run;$('stop').onclick=()=>{generation++;player?.dispose();player=null;booted=false;$('stop').disabled=true;$('runtimeStatus').textContent='Jogo parado';};
$('exportGame').onclick=exportGame;
$('backup').onclick=()=>{if(project)download(project.name.replace(/[^a-z0-9_-]/gi,'-')+'.mobileforge.json',JSON.stringify(project,null,2),'application/json');};
$('addFile').onclick=async()=>{const name=await ask('Novo arquivo C#','Player.cs');if(!name)return;if(!/^[A-Za-z_][\w-]*\.cs$/.test(name)||project.files.some(f=>f.name.toLowerCase()===name.toLowerCase())){notice('Use um nome .cs válido e ainda não utilizado.');return;}project.files.push({name,code:`public class ${name.slice(0,-3).replace(/-/g,'_')}\n{\n}\n`});invalidate();selectFile(name);showView('code');};
$('renameFile').onclick=async()=>{if(active==='MainGame.cs'){notice('MainGame.cs é o arquivo principal.');return;}const name=await ask('Renomear arquivo',active);if(!name)return;if(!/^[A-Za-z_][\w-]*\.cs$/.test(name)||project.files.some(f=>f.name.toLowerCase()===name.toLowerCase())){notice('Nome inválido ou já utilizado.');return;}project.files.find(f=>f.name===active).name=name;invalidate();selectFile(name);};
$('deleteFile').onclick=()=>{if(active==='MainGame.cs'){notice('MainGame.cs é necessário.');return;}if(!confirm(`Excluir ${active}?`))return;project.files=project.files.filter(f=>f.name!==active);invalidate();selectFile('MainGame.cs');};
document.querySelectorAll('.bottom-nav button').forEach(b=>b.onclick=()=>showView(b.dataset.view));
document.querySelectorAll('.typing-tools button').forEach(b=>b.onclick=()=>editor.insert(b.dataset.insert));
window.addEventListener('pagehide',()=>{if(project)putProject(project);});
home().catch(e=>notice('Armazenamento indisponível: '+e.message));
