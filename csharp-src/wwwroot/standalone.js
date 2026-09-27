// Bundled into a downloaded HTML. This intentionally runs on the document thread:
// Android's content:// document provider may disallow module Web Workers from blob URLs.
export async function bootStandalone(pack, status = () => {}) {
  status('Preparando runtime C#…');
  const packed = Uint8Array.from(atob(pack), c => c.charCodeAt(0));
  const stream = new Blob([packed]).stream().pipeThrough(new DecompressionStream('gzip'));
  const assets = await new Response(stream).json();
  const configName = Object.keys(assets).find(name => name.endsWith('.boot.json'));
  if (!configName) throw new Error('Configuração do runtime ausente no HTML.');
  const config = JSON.parse(atob(assets[configName]));
  const blobs = new Map();
  for (const [name, encoded] of Object.entries(assets)) {
    let bytes = Uint8Array.from(atob(encoded), c => c.charCodeAt(0));
    if (name.endsWith('.js')) {
      const source = new TextDecoder().decode(bytes).replaceAll('import.meta.url', JSON.stringify('https://mobileforge.invalid/_framework/' + name));
      bytes = new TextEncoder().encode(source);
    }
    const mime = name.endsWith('.js') ? 'text/javascript' : name.endsWith('.wasm') ? 'application/wasm' : 'application/octet-stream';
    blobs.set(name, URL.createObjectURL(new Blob([bytes], { type: mime })));
  }
  status('Iniciando .NET…');
  const moduleUrl = blobs.get('dotnet.js');
  if (!moduleUrl) throw new Error('Módulo .NET ausente no HTML.');
  const { dotnet } = await import(moduleUrl);
  const runtime = await dotnet.withConfig(config).withResourceLoader((type, name) => {
    const url = blobs.get(name);
    if (!url) throw new Error('Recurso ausente no HTML: ' + name);
    return url;
  }).create();
  const exports = await runtime.getAssemblyExports(runtime.getConfig().mainAssemblyName);
  if (!exports.Forge) throw new Error('API do jogo ausente no runtime.');
  status('Executando jogo…');
  return exports.Forge;
}
