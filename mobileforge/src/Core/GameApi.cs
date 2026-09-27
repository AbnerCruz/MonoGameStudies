using Android.Graphics;

namespace MobileForge.Core;

public interface IGame
{
    void Update(GameInput input, float deltaTime);
    void Draw(GameCanvas canvas);
}

public readonly record struct GameInput(bool IsTouching, float X, float Y);

public sealed class GameCanvas
{
    public const int Width = 360;
    public const int Height = 640;
    private readonly Canvas canvas;
    private readonly Paint paint = new(PaintFlags.AntiAlias);

    public GameCanvas(Canvas canvas) => this.canvas = canvas;

    public void Clear(uint color) => canvas.DrawColor(new Color(unchecked((int)color)));

    public void Rect(float x, float y, float width, float height, uint color)
    {
        paint.Color = new Color(unchecked((int)color));
        paint.SetStyle(Paint.Style.Fill);
        canvas.DrawRect(x, y, x + width, y + height, paint);
    }

    public void Text(string value, float x, float y, float size, uint color)
    {
        paint.Color = new Color(unchecked((int)color));
        paint.TextSize = size;
        paint.SetStyle(Paint.Style.Fill);
        canvas.DrawText(value, x, y, paint);
    }
}
