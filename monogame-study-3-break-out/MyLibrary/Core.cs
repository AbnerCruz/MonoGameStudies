using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace  MyLibrary;

public class Core : Game
{
    internal static Core s_instance;
    public static Core Instance => s_instance;

    public static GraphicsDeviceManager Graphics { get; private set; }
    public static new GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static new ContentManager Content { get; private set; }

    public Core(string title, int width, int height, bool fullscreen)
    {
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        s_instance = this;

        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullscreen;
        Graphics.ApplyChanges();

        Window.Title = title;

        Content = base.Content;
        Content.RootDirectory = "Content";

        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GraphicsDevice = base.GraphicsDevice;
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)){ Exit(); }
        base.Update(gameTime);
    }


}