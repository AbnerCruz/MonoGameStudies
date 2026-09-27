// Shared by the editor and offline HTML. All user C# runs in this disposable worker.
export async function forgeWorker() {
  let api; const progress = message => self.postMessage({ debug: message });
  const blobs = new Map();
  const reply = (id, value) => self.postMessage({ id, value });
  self.onmessage = async ({ data }) => {
    const { id, type, payload } = data;
    try {
      if (type === 'boot') {
        progress('Iniciando runtime .NET');
        let moduleUrl;
        let config;
        if (payload.pack) {
          const packed = Uint8Array.from(atob(payload.pack), c => c.charCodeAt(0));
          const stream = new Blob([packed]).stream().pipeThrough(new DecompressionStream('gzip'));
          const assets = await new Response(stream).json();
          const configName = Object.keys(assets).find(name => name.endsWith('.boot.json'));
          config = JSON.parse(atob(assets[configName]));
          for (const [name, encoded] of Object.entries(assets)) {
            let bytes = Uint8Array.from(atob(encoded), c => c.charCodeAt(0));
            if (name.endsWith('.js')) {
              // A stable base keeps .NET URL resolution valid when loaded from a blob.
              let source = new TextDecoder().decode(bytes).replaceAll('import.meta.url', JSON.stringify('https://mobileforge.invalid/_framework/' + name));
              bytes = new TextEncoder().encode(source);
            }
            const mime = name.endsWith('.js') ? 'text/javascript' : name.endsWith('.wasm') ? 'application/wasm' : 'application/octet-stream';
            blobs.set(name, URL.createObjectURL(new Blob([bytes], { type: mime })));
          }
          moduleUrl = blobs.get('dotnet.js');
        } else moduleUrl = new URL('_framework/dotnet.js', payload.base).href;
        const { dotnet } = await import(moduleUrl); progress('Módulo .NET carregado');
        let builder = dotnet.withDiagnosticTracing(false);
        if (config) builder = builder.withConfig(config).withResourceLoader((type, name) => {
          const url = blobs.get(name);
          if (!url) throw new Error('Recurso ausente no HTML: ' + name);
          return url;
        });
        const runtime = await builder.create(); progress('Runtime .NET criado');
        const exports = await runtime.getAssemblyExports(runtime.getConfig().mainAssemblyName);
        api = exports.Forge; progress('API C# pronta');
        reply(id, { ok: true });
      } else if (type === 'compile') reply(id, JSON.parse(api.Compile(JSON.stringify(payload))));
      else if (type === 'load') reply(id, JSON.parse(api.Load(payload)));
      else if (type === 'start') reply(id, JSON.parse(api.Start()));
      else if (type === 'frame') reply(id, JSON.parse(api.Frame(JSON.stringify(payload))));
      else throw new Error('Operação desconhecida: ' + type);
    } catch (error) { reply(id, { ok: false, error: error.stack || String(error) }); }
  };
}
