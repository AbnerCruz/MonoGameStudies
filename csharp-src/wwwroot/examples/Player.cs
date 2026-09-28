using MobileForge;

// CAPÍTULO 2 — posição, entrada e tempo.
// Vector2 armazena dois números: aqui eles são a posição do CENTRO da nave.
// A classe é comum, não herda Game. MainGame decide quando ela roda.
public class Player
{
    public Vector2 Position = new Vector2(180, 470);
    public float Speed = 240; // pixels lógicos por segundo
    public RectF Bounds => new RectF(Position.X - 12, Position.Y - 12, 24, 24);

    public void Update(float dt)
    {
        float axis = 0;
        if (UI.Held("Left") || Input.Key("ArrowLeft") || Input.Key("a")) axis--;
        if (UI.Held("Right") || Input.Key("ArrowRight") || Input.Key("d")) axis++;
        // Duas direções juntas se anulam. Multiplicar por dt torna a
        // distância proporcional ao tempo, não à quantidade de quadros.
        Position += new Vector2(axis, 0) * Speed * dt;
        Position.X = GameMath.Clamp(Position.X, 24, 336);
    }

    public void Draw(float shield)
    {
        if (shield > 0) Graphics.Circle(Position.X, Position.Y, 23, "#245b70");
        // ship mede 8×8 pixels; escala 4 produz 32×32. Subtraímos metade
        // para desenhar pelo centro. A colisão é menor para ser tolerante.
        Graphics.Sprite("ship", Position.X - 16, Position.Y - 16, 4);
    }
}
// Experimento: adicione um eixo Y e normalize a direção antes de multiplicar
// pela velocidade. Assim, andar na diagonal não fica mais rápido.
