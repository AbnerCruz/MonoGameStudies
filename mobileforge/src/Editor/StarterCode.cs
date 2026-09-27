namespace MobileForge.Editor;

internal static class StarterCode
{
    internal const string Source = """
        using MobileForge.Core;

        public class MyGame : IGame
        {
            float x = 160, y = 280;

            public void Update(GameInput input, float deltaTime)
            {
                if (input.IsTouching) { x = input.X - 20; y = input.Y - 20; }
            }

            public void Draw(GameCanvas canvas)
            {
                canvas.Clear(0xFF141824);
                canvas.Text("Toque para mover", 20, 45, 22, 0xFFFFFFFF);
                canvas.Rect(x, y, 40, 40, 0xFF59D9B2);
            }
        }
        """;
}
