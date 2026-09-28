using System;
using MobileForge;

// CAPÍTULO 5 — UI declarativa, eventos C# e publicação.
// UI.Set instala HTML/CSS permitido em uma camada sobre o canvas.
// onClick dispara uma ação; onPress representa um botão MANTIDO pressionado.
// UI.On conecta esses nomes ao C# em MainGame.Start. Não há JavaScript no jogo.
// A engine bloqueia seleção/menus de texto só no jogo, preservando o editor.
public class GameHud
{
    public void Create()
    {
        // Usamos percentuais na composição para caber na prévia e na tela cheia.
        // A área da nave termina antes dos controles: seu dedo não a esconde.
        UI.Set("""
        <style>
        * { box-sizing:border-box; }
        .hud { position:absolute; inset:3% 5% auto; display:flex; gap:8px; align-items:center; justify-content:space-between; font-size:12px; }
        .stats { display:flex; flex-direction:column; gap:5px; }
        .tag { color:#82ead1; font-size:10px; letter-spacing:2px; }
        button { border:1px solid #557186; border-radius:14px; background:#142b40; color:#eafbff; padding:10px 14px; font-weight:700; }
        button:active { background:#306b76; transform:scale(.97); }
        button:focus-visible { outline:3px solid #8af3d0; outline-offset:2px; }
        .primary { background:#8af3d0; color:#082532; border-color:#8af3d0; width:100%; }
        .overlay { position:absolute; inset:0; display:grid; place-items:center; background:#080f21db; pointer-events:auto; padding:6%; }
        .card { width:100%; max-width:320px; padding:22px; border:1px solid #344f69; border-radius:24px; background:#101f34; text-align:center; }
        h1 { font-size:34px; letter-spacing:4px; margin:8px 0; }
        h2 { font-size:23px; margin:10px 0; }
        p { color:#b5cadc; font-size:13px; line-height:1.6; margin:12px 0 18px; }
        .ship { width:40px; height:40px; image-rendering:pixelated; }
        .controls { position:absolute; left:5%; right:5%; bottom:4%; display:flex; justify-content:space-between; align-items:center; gap:12px; }
        .controls button { width:30%; height:64px; font-size:24px; touch-action:none; }
        .controls small { color:#91a9bf; font-size:10px; text-align:center; line-height:1.5; }
        label { display:block; font-size:12px; color:#b5cadc; margin:18px 0; }
        input { width:100%; accent-color:#8af3d0; margin-top:12px; }
        .secondary { width:100%; margin-top:10px; }
        </style>
        <div id="hud" class="hud">
          <div class="stats"><span id="stage" class="tag"></span><strong id="score"></strong><span id="health"></span></div>
          <button onClick="Pause" aria-label="Pausar missão">Ⅱ</button>
        </div>
        <div id="controls" class="controls">
          <button onPress="Left" aria-label="Mover para esquerda">←</button>
          <small>SEGURE PARA MOVER<br>ou use A / D</small>
          <button onPress="Right" aria-label="Mover para direita">→</button>
        </div>
        <section id="menu" class="overlay"><div class="card">
          <img class="ship" src="sprite:ship" alt="Nave de resgate">
          <div class="tag">RESGATE DE ENERGIA</div><h1>ÓRBITA</h1>
          <p>Recolha 12 células verdes em 60 segundos.<br>Desvie dos meteoros. Você tem 3 vidas.<br>Três etapas, uma missão.</p>
          <button class="primary" onClick="Begin">Iniciar missão</button>
          <p>Um jogo inteiro para aprender C#.<br>Leia os capítulos nos cinco arquivos.</p>
        </div></section>
        <section id="pause" class="overlay"><div class="card">
          <div class="tag">RESPIRA UM POUCO</div><h2>Missão pausada</h2>
          <label>Velocidade da nave <span id="speed"></span><input id="steering" type="range" min="120" max="360" value="240" onInput="Steering" aria-label="Velocidade da nave"></label>
          <button class="primary" onClick="Pause">Continuar</button>
          <button class="secondary" onClick="Home">Voltar ao início</button>
        </div></section>
        <section id="result" class="overlay"><div class="card">
          <div class="tag">FIM DA MISSÃO</div><h2 id="resultTitle"></h2><p id="summary"></p>
          <button class="primary" onClick="Begin">Jogar novamente</button>
          <button class="secondary" onClick="Home">Voltar ao início</button>
        </div></section>
        """);
        UI.SetValue("steering", "240");
    }

    public void Refresh(MainGame game)
    {
        bool playing = game.State == MissionState.Playing;
        bool ended = game.State == MissionState.Won || game.State == MissionState.Lost;
        UI.SetVisible("menu", game.State == MissionState.Menu);
        UI.SetVisible("pause", game.State == MissionState.Paused);
        UI.SetVisible("result", ended);
        UI.SetVisible("controls", playing);
        UI.SetVisible("hud", playing);
        // Não chame UI.Set a cada frame: isso recriaria os botões e o slider.
        // Atualizações pontuais preservam foco, toque e o valor sendo arrastado.
        UI.SetText("score", $"{game.Score} / {MainGame.Goal} células");
        UI.SetText("stage", $"ETAPA {Math.Min(3, 1 + game.Score / 4)} / 3");
        UI.SetText("health", $"{game.Lives} vidas · {MathF.Ceiling(game.Remaining)} s");
        UI.SetText("speed", ((int)game.Player.Speed).ToString());
        UI.SetText("resultTitle", game.State == MissionState.Won ? "Missão cumprida!" : "Missão encerrada");
        UI.SetText("summary", game.State == MissionState.Won
            ? $"Você recuperou {game.Score} células e trouxe energia à estação. Boa viagem, piloto!"
            : $"{game.Score} células recuperadas. " + (game.Lives <= 0 ? "Sua nave ficou sem escudo." : "O tempo acabou.") + " Tente outra rota!");
    }
}
// DO COMEÇO AO FIM
// 1. Execute o projeto sem alterações e jogue uma partida.
// 2. Abra Player.cs, altere Speed e execute de novo: o C# será recompilado.
// 3. Em Arte, pinte ship; selecione objects para editar as duas células.
// 4. Em MainGame, mude Goal, Duration e a probabilidade de energia.
// 5. Acrescente um objeto, crie-o no MainGame e chame Update/Draw explicitamente.
// 6. Teste vitória, derrota, pausa, reinício e os botões mantidos pressionados.
// 7. Um erro? Abra Problemas e toque na mensagem para ir ao arquivo e linha.
// 8. Em opções do projeto, baixe o backup para guardar os fontes editáveis.
// 9. Após executar com sucesso, baixe o ZIP do jogo. Todos os arquivos ficam
//    na raiz. Extraia juntos e abra index.html ou hospede essa pasta.
//    Alguns provedores content:// do Android bloqueiam arquivos vizinhos;
//    o ZIP pode ser hospedado como site estático, sem servidor C#.
// Próximo desafio: crie um item de cura ou um botão de escudo temporário.
// Defina sua regra no jogo e reutilize os vetores, colisões e eventos da engine.
