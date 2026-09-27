# MobileForge Web

IDE e framework 2D em JavaScript para criar um jogo no navegador e exportar um único HTML jogável. A experiência é guiada por código, inspirada no ciclo de jogo do MonoGame.

## Versão atual

- Projeto com vários arquivos JavaScript; execução na ordem mostrada no explorador.
- Editor com abas, prévia isolada, console de erros e salvamento local em IndexedDB.
- Sprites desenhados em pixels ou importados de PNG (até 64 × 64 nesta versão); exportação de PNG.
- Tutorial dentro da IDE e backup/restauração do projeto em JSON.
- HTML exportado contém motor, código e sprites; não depende de servidor.

Abra `index.html` por um servidor estático ou GitHub Pages. Não há dependências de build.

## Estrutura

- `index.html`: IDE e tutorial.
- `styles.css`: interface responsiva.
- `runtime.js`: motor usado na prévia e serializado no HTML exportado.
- `app.js`: projeto, editor, sprites, persistência e exportação.

## Convenção dos arquivos

Os arquivos JavaScript são concatenados dentro de um escopo compartilhado na ordem da lista. O arquivo inicial `main.js` fica por último. Esta versão ainda não implementa `import`/`export` ES Modules; classes e funções declaradas antes de `main.js` ficam disponíveis a ele.

## API atual

```js
function setup(game) {}
function update(game, dt) {}
function draw(game) {
  game.clear('#101827');
  game.sprite('hero', 100, 100, 3);
}
```

`game.width`, `game.height`, `game.pointer`, `game.key(nome)`, `game.clear(cor)`, `game.rect(x,y,w,h,cor)`, `game.circle(x,y,r,cor)`, `game.text(texto,x,y,tamanho,cor)`, `game.sprite(nome,x,y,escala)`.

## Limites conhecidos

- Um projeto ativo por navegador, com backup JSON para transferência. A perda dos dados do navegador também apaga o projeto local se não houver backup.
- A resolução lógica atual é 360 × 640 e os sprites importados maiores que 64 pixels são reduzidos.
- O editor usa textarea com números de linha; conclusão de código, módulos ES e depuração com breakpoints ainda não estão disponíveis.
- Código do usuário roda em iframe isolado na prévia. O HTML exportado executa o código como qualquer arquivo HTML local; abra apenas jogos de fontes confiáveis.

Veja [PLANO.md](PLANO.md) para a evolução da IDE e das APIs opcionais.
