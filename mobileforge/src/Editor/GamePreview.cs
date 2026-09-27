using Android.Content;
using Android.Graphics;
using Android.Views;
using MobileForge.Core;

namespace MobileForge.Editor;

internal sealed class GamePreview : View
{
    private IGame? game;
    private GameInput input;
    private long lastFrame;

    public GamePreview(Context context) : base(context) { }

    public void SetGame(IGame value)
    {
        game = value;
        lastFrame = 0;
        Invalidate();
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);
        canvas.DrawColor(Color.Rgb(19, 22, 34));
        if (game is null) return;

        var scale = Math.Min(Width / (float)GameCanvas.Width, Height / (float)GameCanvas.Height);
        var dx = (Width - GameCanvas.Width * scale) / 2f;
        var dy = (Height - GameCanvas.Height * scale) / 2f;
        var now = Android.OS.SystemClock.UptimeMillis();
        var dt = lastFrame == 0 ? 0f : Math.Clamp((now - lastFrame) / 1000f, 0f, 0.05f);
        lastFrame = now;

        try
        {
            game.Update(input, dt);
            canvas.Save();
            canvas.Translate(dx, dy);
            canvas.Scale(scale, scale);
            game.Draw(new GameCanvas(canvas));
            canvas.Restore();
        }
        catch (Exception ex)
        {
            game = null;
            Android.Util.Log.Error("MobileForge", $"Game stopped: {ex}");
        }
        if (game is not null) PostInvalidateDelayed(16);
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e is null) return false;
        var scale = Math.Min(Width / (float)GameCanvas.Width, Height / (float)GameCanvas.Height);
        var dx = (Width - GameCanvas.Width * scale) / 2f;
        var dy = (Height - GameCanvas.Height * scale) / 2f;
        var touching = e.ActionMasked is MotionEventActions.Down or MotionEventActions.Move;
        input = new GameInput(touching, (e.GetX() - dx) / scale, (e.GetY() - dy) / scale);
        return true;
    }
}
