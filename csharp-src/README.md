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
- Bibliotecas em `wwwroot/framework/`: `Game.cs`, `Math.cs`, `Collision.cs`, `UI.cs`. São os mesmos arquivos embutidos no compilador e exibidos em modo somente leitura na área Fonte, acessível também do menu inicial.
- UI aceita HTML/CSS declarativo, `onClick`, `onInput`, `onChange`, `onPress` e callbacks de C#, com controles posicionados sobre o canvas e sprites em `<img src="sprite:hero">`. `UI.SetText`, `UI.SetValue` e `UI.SetVisible` atualizam elementos por ID sem reconstruir a interface.
- `Vector2`, `GameMath`, `Randomizer`, `RectF`, `CircleF`, `Collision` oferecem matemática e consultas de geometria, incluindo segmentos contra círculos e retângulos, sem regras de movimento impostas.
- Exportação é um ZIP com `index.html`, `game.js`, `game-data.js` e `runtime-data.js`, todos diretamente na raiz. É necessário extraí-los juntos. A estrutura de pastas do código no editor não é exportada para o jogo compilado.

## Build

CI usa .NET 9, workload `wasm-tools`, Node e GitHub Actions:

```sh
cd csharp-src
npm ci
npm run bundle
dotnet workload install wasm-tools
dotnet publish ForgeRuntime.csproj -c Release -o /tmp/mobileforge-editor
dotnet publish ForgeRuntime.csproj -c Release -o /tmp/mobileforge-player -p:ForgePlayer=true
python scripts/pack.py /tmp/mobileforge-player/wwwroot wwwroot/player-framework.pack.gz
cp -r /tmp/mobileforge-editor/wwwroot/_framework wwwroot/_framework
```

Para testar localmente, sirva `wwwroot` com um servidor HTTP. O CI executa um teste real em Chromium e verifica compilação de arquivos em pastas, spritesheet, tilemap, diagnóstico de C# inválido, integridade e disposição plana do ZIP, e o jogo extraído aberto de `file://` sem rede nem configurações permissivas.

## Limites

A API `Game`, `Input`, `Graphics` é nossa, inspirada no MonoGame; não é o pacote MonoGame. Não há simulação automática de física, áudio, NuGet, camadas e ferramentas avançadas de tilemap. O autocomplete atual é lexical, não semântico. A máquina deve suportar WebAssembly e `DecompressionStream`. Alguns provedores `content://` do Android bloqueiam scripts vizinhos ao abrir o `index.html` localmente; o ZIP extraído funciona como site estático. A primeira compilação carrega Roslyn e referências .NET, e pode consumir muita memória em aparelhos modestos. Os testes em Chromium não substituem um teste físico no Moto g32.

## Interface 0.8.0

A tela de código tem ações em menus, explorador recolhível, modo foco, busca, ajuste de fonte e quebra de linhas. Ferramentas de arquivos só aparecem na área de código. Na arte, configurações de células e mapas ficam recolhidas, e o canvas oferece zoom e grade. O CI registra capturas em 390 × 844 e verifica que a interface não transborda horizontalmente.

A área Fonte permite estudar Game.cs, Math.cs, Collision.cs e UI.cs sem criar projeto. Math.cs inclui vetores, interpolação e um gerador aleatório com semente opcional, escolhas, embaralhamento e direções.

## 0.9 — Órbita, tutorial jogável

O botão “Órbita · jogar e aprender” cria uma cópia editável do único exemplo.
Os cinco arquivos em `wwwroot/examples/` explicam o ciclo completo de um jogo:
MainGame, Player, FallingItem, Starfield e GameHud. A antiga aba Guia foi removida.
Projetos vazios continuam apenas com Start, Update e Draw.

A UI continua declarativa em HTML/CSS, com callbacks em C#. A superfície do jogo
bloqueia seleção, arraste e menu contextual; o editor e os fontes continuam
selecionáveis. Controles mantidos pressionados rastreiam cada ponteiro, liberando
corretamente em cancelamento, perda de captura, ocultação e perda de foco.

A CI cobre o núcleo (compilação, sprites, mapas, diagnósticos, ZIP offline) e a
missão (toque nativo, multitoque, pausa, slider, reinício, vitória e duas derrotas).
O exemplo real também é exportado e aberto offline. Para os finais, os testes
reduzem Goal/Duration ou colocam um meteoro na nave usando os mesmos fontes.
