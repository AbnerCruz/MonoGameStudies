# Plano da engine 2D

## Princípios

- Ciclo explícito `setup → update → draw`, controle direto de estado, input, desenho e recursos pelo código do jogo.
- Ferramentas visuais aceleram a produção de recursos; elas não escondem o funcionamento do runtime.
- Prévia e HTML exportado executam o mesmo motor e os mesmos arquivos, sem serviços externos.
- API pequena, estável e composta por módulos opcionais. Nenhum editor de cenas obrigatório.

## IDE

- [x] Projeto com múltiplos arquivos JS e abas.
- [x] Prévia isolada, console e exportação de HTML único.
- [x] Persistência local e backup JSON.
- [x] Desenho/importação/exportação de sprites.
- [x] Tutorial para o ciclo de jogo e recursos atuais.
- [ ] Editor de código com destaque de sintaxe, autocompletar e diagnóstico por arquivo/linha.
- [ ] Mover arquivos na ordem de execução; suporte real a módulos ES `import`/`export` no empacotador.
- [ ] Múltiplos projetos, pastas, busca, renomeação de referências e histórico de versões.
- [ ] Spritesheet, animações por quadros, paleta, seleção, preenchimento e camadas.
- [ ] Importação de áudio, fontes e mapas; empacotamento e permissões por recurso.
- [ ] Metadados de projeto: resolução, orientação e escala de pixels.

## Motor — desenho e recursos

- [x] Formas básicas, texto, sprites, entrada por toque e teclado.
- [ ] `game.content.load(name)`: recursos nomeados, pré-carregados antes de `setup`.
- [ ] `game.draw.sprite(name, x, y, {scale, rotation, origin, tint, flip})`.
- [ ] `game.camera`: posição, zoom e transformação; opção de pixel snapping.
- [ ] Spritesheet e animação com controle explícito de quadros.
- [ ] Áudio com desbloqueio no primeiro toque, canais e volume.

## UI pronta, sem perder controle

API proposta de desenho imediato no canvas:

```js
function draw(game) {
  const start = game.ui.button('start', { x: 20, y: 540, w: 160, h: 52, text: 'Jogar' });
  game.ui.bar('life', { x: 20, y: 20, w: 180, value: hp, max: 100 });
  if (start.clicked) state = 'playing';
}
```

- [ ] Botão, texto, barra, painel, lista e área de toque.
- [ ] Temas por objeto e estilos customizados; foco por teclado, toque e escala.
- [ ] ID estável por componente; evento `clicked` consumido uma vez por quadro.
- [ ] Renderização em ordem explícita; o jogo escolhe quando e onde desenhar cada componente.
- [ ] UI não captura toques fora de suas áreas e expõe estado de hover/press/foco.

## Física básica opcional

O motor não deve mover todos os objetos automaticamente. Um módulo de física só atua sobre corpos registrados:

```js
const body = game.physics.body({ x: 20, y: 100, w: 24, h: 24, gravity: 600 });
body.vx = 140;
// update: o motor integra em passo fixo; o jogo lê ou corrige body.x/body.y.
```

- [ ] Passo fixo configurável, separado da taxa de desenho.
- [ ] AABB e círculos; corpos estáticos, cinemáticos e dinâmicos.
- [ ] Gravidade, velocidade, impulso, atrito simples e resolução de colisão.
- [ ] Sensores sem resposta física; callbacks `enter/stay/exit`.
- [ ] Camadas/máscaras de colisão, depuração visual e consultas `overlap`/`raycast`.
- [ ] API de baixo nível para usar só detecção de colisão, sem simulação.
- [ ] Testes de determinismo, colisões em bordas e desempenho em celulares modestos.

## Entrega em etapas

1. **IDE utilizável:** arquivos, sprites, tutorial, backup e exportação. Validar no celular em retrato e paisagem.
2. **Conteúdo 2D:** spritesheets, animação, câmera, áudio e recursos.
3. **UI imediata:** controles visuais opcionais e acessibilidade de entrada.
4. **Física:** primitivas de colisão antes da simulação; depois integração em passo fixo.
5. **Qualidade de IDE:** módulos, diagnósticos, autocompletar, projetos múltiplos e testes do exportador.

Cada etapa exige um jogo de exemplo criado na própria IDE e aberto pelo HTML exportado.
