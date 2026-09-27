# MobileForge C# Lab

Prova técnica mobile-first: editor de C# com Roslyn rodando em WebAssembly dentro do navegador. Publicada em `/csharp/` no GitHub Pages. O código de jogos é C# de verdade; a interface HTML/JS conecta compilação, canvas e input.

- Um projeto novo contém apenas `MainGame.cs`, com `Start`, `Update` e `Draw` vazios.
- O exemplo adiciona `Player.cs`, cria `Player` em `MainGame` e desenha um sprite.
- Arquivos são compilados como C# separados, com erros por nome de arquivo e linha.
- Compilação e prévia rodam em Web Worker, com tempo limite e botão Parar. O HTML exportado executa o runtime na página para funcionar ao abrir pelo provedor de downloads do Android.
- Cada projeto fica no IndexedDB do navegador; backup/importação em JSON.
- Aba Sprites desenha pixels, importa/exporta PNG e inclui os assets no HTML offline.
- Exportação é um HTML único que embute a assembly do jogo, sprite e runtime .NET. Não pede rede ao ser aberto. O arquivo pode ser grande.

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

Para testar localmente, sirva `wwwroot` com um servidor HTTP. O HTML exportado é independente do servidor. O CI executa um teste real em Chromium e verifica compilação de dois arquivos, diagnóstico de C# inválido e jogo exportado aberto de `file://` sem rede nem configurações permissivas.

## APK de demonstração

O workflow `build-csharp-apk.yml` compila o exemplo C# pelo editor, testa o HTML offline e o coloca em um aplicativo Android com WebView. O APK debug assinado é publicado como artifact `MobileForge-CSharp-Example-APK` do workflow, após verificação da assinatura e execução em emulador. Esse APK contém o exemplo do CI, não os projetos salvos no navegador do usuário.

O wrapper em `android/` pode receber outro `app/src/main/assets/game.html` exportado pelo editor e ser compilado com Gradle 8.11.1 / Android Gradle Plugin 8.10.1. A etapa futura para um botão “Exportar APK” por projeto precisa enviar o HTML a um serviço de build autenticado ou usar um empacotador e assinador local. GitHub Pages só entrega arquivos estáticos; o navegador não possui as ferramentas Android para gerar e assinar um APK de forma direta.

## Limites

A API `Game`, `Input`, `Graphics` é nossa, inspirada no MonoGame; não é o pacote MonoGame. Ainda faltam física, UI pronta, áudio, NuGet. O autocomplete atual é lexical, não semântico. A máquina deve suportar WebAssembly e `DecompressionStream` para abrir o HTML offline. A primeira compilação carrega Roslyn e referências .NET, e pode consumir muita memória em aparelhos modestos. Os testes em Chromium não substituem um teste físico no Moto g32.
