using MobileForge;

// CAPÍTULO 3 — dados, colisão e spritesheet.
// Células e meteoros compartilham movimento, mas não a resposta ao contato.
// MainGame consulta Bounds e escolhe a regra. Não existe física automática.
public class FallingItem
{
    public Vector2 Position;
    public Vector2 Velocity;
    public bool IsEnergy;
    public CircleF Bounds => new CircleF(Position.X, Position.Y, IsEnergy ? 10 : 13);

    public FallingItem(Vector2 position, Vector2 velocity, bool isEnergy)
    {
        Position = position;
        Velocity = velocity;
        IsEnergy = isEnergy;
    }

    public void Update(float dt)
    {
        Position += Velocity * dt;
        // Rebater a velocidade é uma escolha NOSSA. O vetor permite usar
        // qualquer outra resposta: parar, deslizar, acelerar, atravessar…
        if (Position.X < 20 || Position.X > 340)
        {
            Position.X = GameMath.Clamp(Position.X, 20, 340);
            Velocity.X = -Velocity.X;
        }
    }

    public void Draw()
    {
        // objects é uma imagem 16×8 dividida em duas células 8×8:
        // índice 0 = energia; índice 1 = meteoro. Edite na aba Arte.
        Graphics.Tile("objects", IsEnergy ? 0 : 1, Position.X - 16, Position.Y - 16, 4);
    }
}
// Experimento: adicione uma aceleração e faça Velocity += aceleração * dt.
// Para objetos muito rápidos, use consultas de segmento ou subpassos;
// testar apenas a posição final pode atravessar um alvo entre dois quadros.
