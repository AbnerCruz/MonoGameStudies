using System;
using System.Collections.Generic;
using MobileForge;

// ÓRBITA — um jogo completo para ler, executar e modificar.
// 1. Toque em Executar e depois Iniciar missão. Recolha 12 células
//    antes de 60 segundos. Meteoros tiram vida; células perdidas não.
// 2. Leia nesta ordem: MainGame, Player, FallingItem, Starfield, GameHud.
//    Todos os arquivos compilam juntos: a ordem das abas não importa.
// 3. MainGame é o coração: cria os objetos e chama seus métodos.
//    Nenhuma classe ganha vida só porque existe em outro arquivo.
// 4. As bibliotecas em Fonte são as implementações reais, só para leitura.
//    Este projeto é sua cópia editável; abrir o exemplo cria outra cópia.
public class MainGame : Game
{
    // Estas regras pertencem ao jogo, não à engine. Experimente mudá-las.
    public const int Goal = 12;
    public const float Duration = 60f;
    public Player Player = new Player();
    public Starfield Background = new Starfield();
    public GameHud Hud = new GameHud();
    public List<FallingItem> Items = new List<FallingItem>();
    public Randomizer Random = new Randomizer(); // sem semente fixa
    public MissionState State = MissionState.Menu;
    public int Score, Lives = 3;
    public float Remaining = Duration;
    float spawnIn, invulnerable;
    bool pauseWasDown;

    // Start acontece uma vez por execução. Reiniciar uma partida é
    // responsabilidade de Begin(), para não registrar callbacks de novo.
    public override void Start()
    {
        Background.Create(Random);
        Hud.Create();
        UI.On("Begin", Begin);
        UI.On("Pause", TogglePause);
        UI.On("Steering", e => Player.Speed = GameMath.Clamp(e.Number, 120, 360));
        UI.On("Home", () => State = MissionState.Menu);
        Hud.Refresh(this);
    }

    public void Begin()
    {
        Score = 0;
        Lives = 3;
        Remaining = Duration;
        invulnerable = 0;
        spawnIn = 1.2f;
        Player.Position = new Vector2(180, 470);
        Items.Clear();
        // Uma célula inicial central ensina a coleta sem exigir movimento.
        Items.Add(new FallingItem(new Vector2(180, 100), new Vector2(0, 100), true));
        State = MissionState.Playing;
    }

    public void TogglePause()
    {
        if (State == MissionState.Playing) State = MissionState.Paused;
        else if (State == MissionState.Paused) State = MissionState.Playing;
    }

    public override void Update(float dt)
    {
        // Detectar a BORDA da tecla evita alternar pausa a cada quadro.
        bool pauseDown = Input.Key("Escape");
        if (pauseDown && !pauseWasDown) TogglePause();
        pauseWasDown = pauseDown;
        if (State == MissionState.Playing)
        {
            Background.Update(dt);
            Player.Update(dt);
            Remaining = MathF.Max(0, Remaining - dt);
            invulnerable = MathF.Max(0, invulnerable - dt);
            spawnIn -= dt;
            if (spawnIn <= 0)
            {
                // A cada quatro coletas, aumentam a velocidade e a frequência.
                int stage = Math.Min(2, Score / 4);
                bool energy = Random.Chance(0.68f);
                float speed = Random.Range(100, 145) + stage * 25;
                Items.Add(new FallingItem(new Vector2(Random.Range(24, 336), 80),
                    new Vector2(Random.Range(-12, 12), speed), energy));
                spawnIn = 0.8f - stage * 0.12f;
            }
            // Remover de trás para frente mantém válidos os índices restantes.
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                item.Update(dt);
                bool hit = Collision.Intersects(Player.Bounds, item.Bounds);
                if (hit)
                {
                    if (item.IsEnergy) Score++;
                    else if (invulnerable <= 0)
                    {
                        Lives--;
                        invulnerable = 1.2f; // tempo para se recuperar após um impacto
                    }
                }
                if (hit || item.Position.Y > 530) Items.RemoveAt(i);
            }
            // A biblioteca só detecta contatos. Pontos, dano e prioridade
            // entre vitória e derrota são decisões explícitas deste jogo.
            if (Lives <= 0 || Remaining <= 0) State = MissionState.Lost;
            else if (Score >= Goal) State = MissionState.Won;
        }
        // HUD também atualiza nos menus, mas o mundo fica congelado na pausa.
        Hud.Refresh(this);
    }

    public override void Draw()
    {
        Clear("#080f21");
        Background.Draw();
        Graphics.Rect(16, 510, 328, 1, "#28405e");
        foreach (var item in Items) item.Draw();
        Player.Draw(invulnerable);
    }
}

public enum MissionState { Menu, Playing, Paused, Won, Lost }
