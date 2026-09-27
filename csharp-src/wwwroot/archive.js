// ZIP with stored entries. Runtime data is already gzip-compressed.
const encoder = new TextEncoder();
const table = Uint32Array.from({length:256},(_,n)=>{
  for(let i=0;i<8;i++) n=(n&1)?0xedb88320^(n>>>1):n>>>1;
  return n>>>0;
});
function crc32(bytes){let crc=0xffffffff;for(const byte of bytes)crc=table[(crc^byte)&255]^(crc>>>8);return (crc^0xffffffff)>>>0;}
export function flatZip(files){
  const names=new Set(),local=[],central=[];let offset=0;
  const stamp=new Date(),year=Math.max(1980,stamp.getFullYear());
  const dosTime=(stamp.getHours()<<11)|(stamp.getMinutes()<<5)|(stamp.getSeconds()>>>1);
  const dosDate=((year-1980)<<9)|((stamp.getMonth()+1)<<5)|stamp.getDate();
  for(const [name,value] of Object.entries(files)){
    if(!/^[A-Za-z0-9_.-]+$/.test(name)||name==='.'||name==='..'||names.has(name.toLowerCase()))throw Error('Nome inválido ou duplicado no ZIP: '+name);
    names.add(name.toLowerCase());
    const filename=encoder.encode(name),body=typeof value==='string'?encoder.encode(value):value;
    if(!(body instanceof Uint8Array))throw Error('Arquivo inválido: '+name);
    const checksum=crc32(body),header=new Uint8Array(30+filename.length),h=new DataView(header.buffer);
    h.setUint32(0,0x04034b50,true);h.setUint16(4,20,true);h.setUint16(8,0,true);
    h.setUint16(10,dosTime,true);h.setUint16(12,dosDate,true);h.setUint32(14,checksum,true);
    h.setUint32(18,body.length,true);h.setUint32(22,body.length,true);h.setUint16(26,filename.length,true);
    header.set(filename,30);local.push(header,body);
    const entry=new Uint8Array(46+filename.length),c=new DataView(entry.buffer);
    c.setUint32(0,0x02014b50,true);c.setUint16(4,20,true);c.setUint16(6,20,true);
    c.setUint16(12,dosTime,true);c.setUint16(14,dosDate,true);c.setUint32(16,checksum,true);
    c.setUint32(20,body.length,true);c.setUint32(24,body.length,true);c.setUint16(28,filename.length,true);
    c.setUint32(42,offset,true);entry.set(filename,46);central.push(entry);
    offset+=header.length+body.length;
  }
  const centralSize=central.reduce((sum,entry)=>sum+entry.length,0),end=new Uint8Array(22),e=new DataView(end.buffer);
  e.setUint32(0,0x06054b50,true);e.setUint16(8,central.length,true);e.setUint16(10,central.length,true);
  e.setUint32(12,centralSize,true);e.setUint32(16,offset,true);
  return new Blob([...local,...central,end],{type:'application/zip'});
}
