# MobileForge C# Lab

Prova técnica mobile-first: editor de C# com Roslyn rodando em WebAssembly dentro do navegador. Publicada em `/csharp/` no GitHub Pages. O código de jogos é C# de verdade; a interface HTML/JS conecta compilação, canvas e input.

- Um projeto novo contém apenas `MainGame.cs`, com `Start`, `Update` e `Draw` vazios.
- O exemplo adiciona `Player.cs`, cria `Player` em `MainGame` e desenha um sprite.
- Arquivos são compilados como C# separados, com erros por nome de arquivo e linha.
- Compilação e prévia rodam em Web Worker, com tempo limite e botão Parar. O jogo exportado executa o runtime na página depois da extração do ZIP.
- Cada projeto fica no IndexedDB do navegador; backup/importação em JSON.
- O explorador aceita caminhos como `Actors/Player.cs`, com busca e pastas no editor. `MainGame.cs` continua na raiz do projeto. Cada arquivo mantém seu cursor e histórico de desfazer ao trocar de aba.
- Aba Sprites desenha pixels, importa/exporta PNG, edita células de spritesheet, dispõe de ferramentas de balde, conta-gotas, desfazer/refazer e permite pintar tilemaps.
- `Graphics.Tile` e `Graphics.Tilemap` desenham as peças no jogo C#.
- Exportação é um ZIP com `index.html`, `game.js`, `game-data.js` e `runtime-data.js`, todos diretamente na raiz. É necessário extraí-los juntos. A estrutura de pastas do código no editor não é exportada para o jogo compilado.

## Build

CI usa .NET 9, workload `wasm-tools`, Node e GitHub Actions:

```sh
cd csharp-src
npm ci
npm run bundle
dotnet workload install wasm-tools
dotnet publish ForgeRuntime.csproj -c Release -o out/editor
dotnet publish ForgeRuntime.csproj -c Release -o out/player -p:ForgePlayer=true
python scripts/pack.py out/player/wwwroot wwwroot/player-framework.pack.gz
cp -r out/editor/wwwroot/_framework wwwroot/_framework
```

Para testar localmente, sirva `wwwroot` com um servidor HTTP. O CI executa um teste real em Chromium e verifica compilação de arquivos em pastas, spritesheet, tilemap, diagnóstico de C# inválido, integridade e disposição plana do ZIP, e o jogo extraído aberto de `file://` sem rede nem configurações permissivas.

## Limites

A API `Game`, `Input`, `Graphics` é nossa, inspirada no MonoGame; não é o pacote MonoGame. Ainda faltam física, UI pronta, áudio, NuGet, camadas e ferramentas avançadas de tilemap. O autocomplete atual é lexical, não semântico. A máquina deve suportar WebAssembly e `DecompressionStream`. Alguns provedores `content://` do Android bloqueiam scripts vizinhos ao abrir o `index.html` localmente; o ZIP extraído funciona como site estático. A primeira compilação carrega Roslyn e referências .NET, e pode consumir muita memória em aparelhos modestos. Os testes em Chromium não substituem um teste físico no Moto g32.
