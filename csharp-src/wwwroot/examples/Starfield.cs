using MobileForge;

// CAPÍTULO 4 — aleatoriedade sem regras de gameplay impostas pela engine.
// Sorteamos posições só ao criar o fundo. Sortear em Draw faria tudo piscar.
public class Starfield
{
    readonly Vector2[] stars = new Vector2[45];

    public void Create(Randomizer random)
    {
        for (int i = 0; i < stars.Length; i++)
            stars[i] = new Vector2(random.Range(0, 360), random.Range(0, 640));
    }

    public void Update(float dt)
    {
        for (int i = 0; i < stars.Length; i++)
            stars[i].Y = GameMath.Wrap(stars[i].Y + (12 + i % 3 * 10) * dt, 0, 640);
    }

    public void Draw()
    {
        for (int i = 0; i < stars.Length; i++)
            Graphics.Rect(stars[i].X, stars[i].Y, i % 3 == 0 ? 2 : 1, 2, "#486680");
    }
}
// Experimento: new Randomizer(1234) reproduz a mesma sequência, útil para
// depurar. O construtor sem semente mantém as partidas variadas. Veja em
// Fonte/Math.cs também Direction, InsideCircle, Pick e Shuffle.
