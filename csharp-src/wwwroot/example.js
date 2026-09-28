// One editable tutorial project. Framework sources remain read-only separately.
const chapters = ['MainGame.cs', 'Player.cs', 'FallingItem.cs', 'Starfield.cs', 'GameHud.cs'];
const palette = {a:'#8af3d0', b:'#eafbff', c:'#36778c', d:'#ffba80', e:'#75546b', f:'#ba8491'};
const ship = ['...aa...','..abba..','..abba..','.aabbaa.','aaabbaaa','aacbbcaa','..c..c..','..d..d..'];
const energy = ['...aa...','..abba..','.abbbba.','abbbbbba','abbbbbba','.abbbba.','..abba..','...aa...'];
const meteor = ['..ffff..','.ffeeff.','ffeeefff','feeeefff','ffeeeeff','.ffeeef.','..ffff..','...ff...'];
function sprite(name, rows, cellW=8) {
  if(rows.some(row=>row.length!==rows[0].length))throw Error('Sprite inválido: '+name);
  return {name,w:rows[0].length,h:rows.length,cellW,cellH:8,pixels:rows.join('').split('').map(c=>palette[c]||null)};
}
export async function createExample() {
  const files=await Promise.all(chapters.map(async name=>{
    const response=await fetch(new URL('./examples/'+name,import.meta.url));
    if(!response.ok)throw Error('Não foi possível carregar '+name+'. Tente novamente.');
    return {name,code:await response.text()};
  }));
  return {id:crypto.randomUUID(),name:'Órbita · jogo e tutorial',updatedAt:Date.now(),active:'MainGame.cs',files,
    sprites:[sprite('ship',ship),sprite('objects',energy.map((row,i)=>row+meteor[i]))],tilemaps:[]};
}
