using Android.Content;
using Android.Graphics;
using Android.Views;
using MobileForge.Core;

namespace MobileForge.EditorRuntime;

internal sealed class StandaloneGameView : View
{
    private readonly IGame game;
    private GameInput input;
    private long lastFrame;
    public StandaloneGameView(Context context, IGame game) : base(context) => this.game = game;

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);
        var scale = Math.Min(Width / (float)GameCanvas.Width, Height / (float)GameCanvas.Height);
        var dx = (Width - GameCanvas.Width * scale) / 2f;
        var dy = (Height - GameCanvas.Height * scale) / 2f;
        var now = Android.OS.SystemClock.UptimeMillis();
        var dt = lastFrame == 0 ? 0f : Math.Clamp((now - lastFrame) / 1000f, 0f, 0.05f);
        lastFrame = now;
        game.Update(input, dt);
        canvas.DrawColor(Color.Black);
        canvas.Save();
        canvas.Translate(dx, dy);
        canvas.Scale(scale, scale);
        game.Draw(new GameCanvas(canvas));
        canvas.Restore();
        PostInvalidateDelayed(16);
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e is null) return false;
        var scale = Math.Min(Width / (float)GameCanvas.Width, Height / (float)GameCanvas.Height);
        var dx = (Width - GameCanvas.Width * scale) / 2f;
        var dy = (Height - GameCanvas.Height * scale) / 2f;
        input = new GameInput(e.ActionMasked is MotionEventActions.Down or MotionEventActions.Move,
            (e.GetX() - dx) / scale, (e.GetY() - dy) / scale);
        return true;
    }
}
