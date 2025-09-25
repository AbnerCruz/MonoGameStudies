using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MyLibrary;
using GameObjects.Entity;
using GameObjects;
using Physics2D.Collisions.Colliders;
using System.Collections.Generic;
using Physics2D.Movement.Area;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MyLibrary.InputManager;


namespace monogame_study_3_break_out;

public class BreakOut : Core
{
    private static int windowWidth = 800;
    private static int windowHeight = 650;
    private Rectangle screenBounds;
    private static List<Entity> entities;
    public static int playerLife = 3;
    public static Brick[,] Bricks;
    static Vector2 _brickSize => new Vector2(windowWidth / 10, 20);
    static int levels = 6;
    static int quantity => (int)(windowWidth / (int)_brickSize.X);
    private static SpriteFont font;
    private static SpriteFont font2;
    public static int points;
    public static int record;
    public static Texture2D _pixel;
    static List<Entity> toRemove = new List<Entity>();

    public static GameState gameState = GameState.paused;

    Song backgroundMusic;
    public static SoundEffect hitSound;
    public static SoundEffect failSound;
    SoundEffect breakSound;
    static SoundEffect gameOverMusic;

    public BreakOut() : base("BreakOut", windowWidth, windowHeight, false)
    {

    }

    protected override void Initialize()
    {
        screenBounds = Geometry.NewRect(Vector2.Zero, new Vector2(windowWidth, windowHeight));
        entities = new List<Entity>();
        Bricks = new Brick[levels, quantity];
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        Vector2 _playerSize = new Vector2(100, 20);
        Vector2 _playerPosition = new Vector2((screenBounds.Right * 0.5f) - (_playerSize.X * 0.5f), (screenBounds.Bottom - 50));
        Entity player = new Paddle(_pixel, _playerPosition, _playerSize, Color.CornflowerBlue, new BoxCollider(_pixel, _playerPosition, _playerSize), new RectangleArea(screenBounds), 10f);

        Vector2 _boxSize = new Vector2(15, 15);
        Vector2 _boxPosition = new Vector2((windowWidth - _boxSize.X) * 0.5f, (windowHeight - _boxSize.Y) * 0.5f);
        Entity ball = new Ball(_pixel, _boxPosition, _boxSize, new BoxCollider(_pixel, _boxPosition, _boxSize), new RectangleArea(screenBounds));

        GenerateBricks(_pixel, levels, quantity);

        backgroundMusic = Content.Load<Song>("sounds/backgroundsound");
        gameOverMusic = Content.Load<SoundEffect>("sounds/gameOver");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;

        hitSound = Content.Load<SoundEffect>("sounds/hitsound");
        failSound = Content.Load<SoundEffect>("sounds/failsound");
        breakSound = Content.Load<SoundEffect>("sounds/breaksound");

        font = Content.Load<SpriteFont>("fonts/File");
        font2 = Content.Load<SpriteFont>("fonts/font2");

        entities.Add(player);
        entities.Add(ball);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (gameState == GameState.paused)
        {
            if (Input.Keyboard.JustPressed(Keys.Enter))
            {
                gameState = GameState.playing;
                MediaPlayer.Play(backgroundMusic);
            }
        }
        else if (gameState == GameState.playing)
        {
            if (Input.Keyboard.JustPressed(Keys.P))
            {
                MediaPlayer.Pause();
                gameState = GameState.paused;
            }
            List<Entity> entitiesWithCollider = new List<Entity>();
            foreach (Entity entity in entities)
            {
                if (entity.EntityCollider != null)
                {
                    entitiesWithCollider.Add(entity);
                }
            }
            foreach (Entity entity in entities)
            {
                if (entity is Paddle player)
                {
                    player.Update(entitiesWithCollider);
                }
                else if (entity is Ball ball)
                {
                    ball.Update(entitiesWithCollider);
                }
                else if (entity is Brick brick)
                {
                    brick.Update(entitiesWithCollider);
                    if (brick._currentLife <= 0)
                    {
                        toRemove.Add(entity);
                    }
                }
            }
            foreach (Entity remove in toRemove)
            {
                Brick brick = (Brick)remove;
                RemoveBrick(remove, brick);
                breakSound.Play();
            }
        }
        int brickCount = 0;
        foreach (Entity entity in entities)
        {
            if (entity is Brick brick)
            {
                brickCount++;
            }
        }
        if (brickCount <= 0)
        {
            GenerateBricks(_pixel, levels, quantity);
        }
        toRemove.Clear();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin();
        string text = points.ToString();
        Vector2 textSize = font.MeasureString(text);
        Vector2 scorePosition = new Vector2((windowWidth - textSize.X) * 0.5f, (windowHeight - textSize.Y) * 0.5f);
        SpriteBatch.DrawString(font, points.ToString(), scorePosition, Color.Green);
        SpriteBatch.DrawString(font2, "HP:" + playerLife.ToString(), new Vector2(0, 0), Color.Red);
        SpriteBatch.DrawString(font2, "Best Score:" + record.ToString(), new Vector2(0, windowHeight - 50), Color.White);
        foreach (Entity entity in entities)
        {
            if (entity is Paddle player)
            {
                player.Draw(SpriteBatch);
            }
            else if (entity is Ball ball)
            {
                ball.Draw(SpriteBatch);
            }
            else if (entity is Brick brick)
            {
                brick.Draw(SpriteBatch);
            }
        }
        SpriteBatch.End();

        base.Draw(gameTime);
    }

    public static void GenerateBricks(Texture2D pixel, int levels, int quantity)
    {
        for (int x = 0; x < levels; x++)
        {
            for (int y = 0; y < quantity; y++)
            {
                int k = (int)(levels / 3);

                int life = (int)((levels - 1 - x) / k) + 1;
                Vector2 position = new Vector2(y * _brickSize.X, x * _brickSize.Y + 30);
                Bricks[x, y] = new Brick(pixel, position, _brickSize, new BoxCollider(pixel, position, _brickSize), life, new Vector2(x, y));
                entities.Add(Bricks[x, y]);
            }
        }
    }

    public static void RemoveBrick(Entity entity, Brick target)
    {
        if (entities.Contains(entity))
        {
            entities.Remove(entity);
        }
        Bricks[(int)target._arrayPos.X, (int)target._arrayPos.Y] = null;
    }

    public static void UpdatePlayerLife()
    {
        playerLife -= 1;
        if (playerLife <= 0)
        {
            ResetGame();
        }
    }

    public static void ResetGame()
    {
        if (points > record)
        {
            record = points;
        }
        points = 0;
        playerLife = 3;
        List<Entity> toRemove = new List<Entity>();
        foreach (Entity entity in entities)
        {
            if (entity is Ball ball)
            {
                ball.ResetPos();
                ball._speed = 7f;
            }
            else if (entity is Paddle player)
            {
                player.ResetPos();
            }
            else if (entity is Brick brick)
            {
                brick._currentLife = -1;
            }
        }
        MediaPlayer.Pause();
        gameOverMusic.Play();
        gameState = GameState.paused;
    }

    public static void PlayerScore(int value)
    {
        points += value;
    }
}
