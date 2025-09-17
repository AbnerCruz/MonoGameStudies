using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MyLibrary;
using Entity;
using GameObjects;

namespace monogame_study_3_break_out;

public class BreakOut : Core
{
    private Paddle paddle;
    private static int windowWidth = 1000;
    private static int windowHeight = 650;
    private Rectangle screenBounds;

    public BreakOut() : base("BreakOut", windowWidth, windowHeight, false)
    {

    }

    protected override void Initialize()
    {
        screenBounds = Geometry.NewRect(Vector2.Zero, new Vector2(windowWidth, windowHeight));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Texture2D _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        Vector2 _playerSize = new Vector2(100, 20);
        Vector2 _playerPosition = new Vector2((screenBounds.Right * 0.5f) - (_playerSize.X * 0.5f), (screenBounds.Bottom - 50));
        paddle = new Paddle(_pixel, _playerPosition, _playerSize, Color.CornflowerBlue);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        paddle.Update(screenBounds);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin();
        paddle.Draw(SpriteBatch);
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
