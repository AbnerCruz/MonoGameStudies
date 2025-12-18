using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using InputManager;

namespace monogame_study_6_space_invaders;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameManager _gameManager = new GameManager();
    private double _elapsedTime;
    private double tickRate = 1d / 60d;
    private int _framCounter;
    private double _fpsTimer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = _gameManager.ScreenSize.X;
        _graphics.PreferredBackBufferHeight = _gameManager.ScreenSize.Y;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _gameManager.Start();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _gameManager.Pixel = new Texture2D(GraphicsDevice, 1, 1);
        _gameManager.Pixel.SetData(new[] { Color.White });
        _gameManager._font = Content.Load<SpriteFont>("File");
    }

    protected override void Update(GameTime gameTime)
    {
        _gameManager.GameTime = gameTime;
        Input.Keyboard.Update();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
        while (_elapsedTime >= tickRate)
        {
            _gameManager.Update();

            _elapsedTime -= tickRate;
        }

        _fpsTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_fpsTimer >= 1d)
        {
            _framCounter = 0;
            _fpsTimer -= 1d;
        }


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _gameManager.Draw(_spriteBatch);
        _spriteBatch.End();
        
        _framCounter++;

        base.Draw(gameTime);
    }
}
