# MobileForge — protótipo 0.1

Editor de jogos 2D em C# para Android. O projeto contém duas aplicações:

- `src/Editor`: editor de texto, compilação C# com Roslyn, prévia em Canvas Android, toque e salvamento local.
- `src/GameTemplate`: jogo Android independente que compila o mesmo código C# como parte do APK.
- `src/Core`: API compartilhada entre editor e jogo exportado.

## Estado real

O código-fonte inicial está implementado, mas **ainda não foi compilado nem testado num Android**. Este ambiente não tem .NET SDK, Android SDK nem aparelho conectado. Não há APK pronto. A compilação de C# em tempo de execução no Android é uma hipótese técnica a validar no primeiro teste em aparelho. O botão `Exportar fonte` gera `Game.cs` para o projeto independente; a geração de um APK dentro do editor ainda não está implementada.

## Primeiro teste

Em um computador com .NET SDK e workload Android:

```sh
dotnet workload install android
dotnet build src/Editor/MobileForge.Editor.csproj -f net9.0-android
dotnet build src/GameTemplate/MobileForge.Game.csproj -f net9.0-android
```

Instalar o APK de debug do editor num telefone, tocar em **Jogar** e verificar se o quadrado acompanha o dedo. Se o Android impedir o carregamento do assembly gerado por Roslyn, a próxima decisão é executar C# em um processo/runtime próprio ou adaptar o modo de prévia; não se deve prometer execução de C# arbitrário sem esse teste.

## API inicial

`IGame` define `Update(GameInput input, float deltaTime)` e `Draw(GameCanvas canvas)`. A tela lógica mede 360 × 640, adaptada ao tamanho físico preservando proporção. `GameCanvas` suporta `Clear`, `Rect` e `Text`. Os arquivos são gravados no armazenamento privado do editor. A prévia aceita toque e não tem permissões de rede.

## Próximos marcos

1. Compilar e instalar o editor; validar Roslyn, toque, rotação e salvamento em um Android físico.
2. Abrir projetos com vários arquivos, diagnóstico com linha/coluna e importação de imagens.
3. Criar template de jogo assinado e construir APK no próprio aparelho. Isso requer portar/testar a cadeia de recursos, empacotamento, alinhamento e assinatura para Android/ARM64, com limite de memória e armazenamento.
4. Adicionar sprites, cenas, colisão, áudio e editor visual de mapa, mantendo a API C# compatível entre prévia e APK.

O nome `MobileForge` é provisório.
