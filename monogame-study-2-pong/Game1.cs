using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameObjects;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Pong;

public class Game1 : Game
{
    private int windowWidth = 1280;
    private int windowHeight = 650;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<Entity> entities;
    private Texture2D _middleLine;
    private Point _middleLinePos;
    private SpriteFont _font;

    private Rectangle screenBounds;

    private int score1;
    private int score2;

    public static Ball ball;

    Song backgroundMusic;
    public static SoundEffect hitSound;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

    }

    protected override void Initialize()
    {
        base.Initialize();
        _graphics.PreferredBackBufferWidth = windowWidth;
        _graphics.PreferredBackBufferHeight = windowHeight;
        _graphics.ApplyChanges();
        screenBounds = new Rectangle(0, 0, windowWidth, windowHeight);

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        entities = new List<Entity>();
        Vector2 paddleSize = new Vector2(20, 100);
        Vector2 player1Position = new Vector2(40, (windowHeight * 0.5f) - paddleSize.Y * 0.5f);
        Vector2 player2Position = new Vector2(windowWidth - paddleSize.X - 40, (windowHeight * 0.5f) - paddleSize.Y * 0.5f);
        Texture2D _paddleTexture = new Texture2D(GraphicsDevice, 1, 1);
        _paddleTexture.SetData(new[] { Color.White });

        float _ballRadius = 10;
        Vector2 _ballPosition = new Vector2(windowWidth * 0.5f, windowHeight * 0.5f);

        entities.Add(new Paddle(_paddleTexture, 1, player1Position, paddleSize, Color.Green));
        entities.Add(new Paddle(_paddleTexture, 2, player2Position, paddleSize, Color.Green));
        ball = new Ball(Geometry.NewRectangleTexture(GraphicsDevice), _ballPosition, _ballRadius, Color.Green, score1, score2);
        entities.Add(ball);

        _middleLine = new Texture2D(GraphicsDevice, 1, 1);
        _middleLine.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("fonts/MyFont");

        backgroundMusic = Content.Load<Song>("sounds/8-bit-background-music-for-arcade-game-come-on-mario-164702");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(backgroundMusic);
        hitSound = Content.Load<SoundEffect>("sounds/fast-collision-reverb-14611");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        foreach (Entity entity in entities)
        {
            if (entity is Paddle player)
            {
                player.Update(screenBounds, entities);
            }
            else if (entity is Ball ball)
            {
                ball.Update(screenBounds, entities);
            }
        }
    
        _middleLinePos = new Vector2(windowWidth * 0.5f, 0).ToPoint();

        score1 = ball.Player1Score;
        score2 = ball.Player2Score;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        foreach (Entity entity in entities)
        {
            if (entity is Paddle player)
            {
                player.Draw(_spriteBatch);
            }
            else if (entity is Ball ball)
            {
                ball.Draw(_spriteBatch);
            }
        }

        _spriteBatch.Draw(_middleLine, new Rectangle(_middleLinePos, new Point(1, windowHeight)), Color.Green);
        _spriteBatch.DrawString(_font, "Player1 score: " + score1.ToString(), new Vector2(windowWidth * 0.25f, 0), Color.White);
        _spriteBatch.DrawString(_font, "Player2 score: " + score2.ToString(), new Vector2(windowWidth * 0.75f, 0), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
