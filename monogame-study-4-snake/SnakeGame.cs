using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace monogame_study_4_snake;

public class SnakeGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private int windowWidth = 1000;
    private int windowHeight = 600;
    public int cellSize { get; set; } = 40;
    private Snake snake;
    private Food food;
    private double totalSeconds;
    private double tick = 0.2;
    private Texture2D pixel;

    public SnakeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = windowWidth;
        _graphics.PreferredBackBufferHeight = windowHeight;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        food = new(windowWidth, windowHeight, cellSize);
        snake = new(windowWidth, windowHeight, cellSize);
        snake.food = food;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        _spriteBatch = new SpriteBatch(GraphicsDevice);        
    }

    protected override void Update(GameTime gameTime)
    {
        totalSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        PlayerInputs();

        while(totalSeconds >= tick)
        {
            if (!snake.IsGameOver)
            {
                snake.SnakeUpdate();
            }
            else
            {
                Console.WriteLine("Game OVER");
            }
            foreach (Vector2 segment in snake.segments)
            {
                Console.WriteLine("Segment: " + segment);
            }
            Console.WriteLine("Food: " + food.position);
            totalSeconds -= tick;
        }


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        snake.Draw(_spriteBatch, pixel);
        food.Draw(_spriteBatch, pixel);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void PlayerInputs()
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Up) && snake.direction != Directions.Down)
        {
            snake.direction = Directions.Up;
        }
        else if (keyboard.IsKeyDown(Keys.Right) && snake.direction != Directions.Left)
        {
            snake.direction = Directions.Right;
        }
        else if (keyboard.IsKeyDown(Keys.Down) && snake.direction != Directions.Up)
        {
            snake.direction = Directions.Down;
        }
        else if (keyboard.IsKeyDown(Keys.Left) && snake.direction != Directions.Right)
        {
            snake.direction = Directions.Left;
        }
    }
}

public class Snake
{
    public bool IsGameOver = false;
    public List<Vector2> segments = new()
    {
        new(){X = 0, Y = 0}
    };
    public Directions direction = Directions.Down;
    public int worldWidth, worldHeight;
    public int cellSize;
    public Food food;

    public Snake(int windowWidth, int windowHeight, int cellSize)
    {
        this.worldWidth = windowWidth;
        this.worldHeight = windowHeight;
        this.cellSize = cellSize;
    }

    public void SnakeUpdate()
    {
        Vector2 headCurrentPosition = segments.FirstOrDefault();
        Vector2 targetPosition = headCurrentPosition + Movement();
        if (targetPosition.X < 0 || targetPosition.X >= worldWidth || targetPosition.Y < 0 || targetPosition.Y >= worldHeight)
        {
            IsGameOver = true;
            return;
        }
        else if (segments.Contains(targetPosition))
        {
            IsGameOver = true;
            return;
        }
        Vector2 newPosition = targetPosition;

        segments.Insert(0, newPosition);
        if (newPosition == food.position)
        {
            Vector2 foodNewPosTarget = food.NewPositon();
            while (segments.Contains(foodNewPosTarget))
            {
                foodNewPosTarget = food.NewPositon();
            }

            food.position = foodNewPosTarget;
        }
        else
        {
            segments.RemoveAt(segments.Count - 1);
        }

    }
    
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        foreach(Vector2 segment in segments)
        {
            spriteBatch.Draw(pixel, new Rectangle(segment.ToPoint(), new Point(cellSize, cellSize)), Color.Green);
        }
    }

    public Vector2 Movement()
    {
        Vector2 movement = Vector2.Zero;
        switch (direction)
        {
            case Directions.Up:
                movement = new Vector2(0, -1); break;
            case Directions.Right:
                movement = new Vector2(1, 0); break;
            case Directions.Down:
                movement = new Vector2(0, 1); break;
            case Directions.Left:
                movement = new Vector2(-1, 0); break;
        }

        return movement * cellSize;
    }
}

public enum Directions
{
    Up,
    Right,
    Down,
    Left
}

public class Food
{
    private Random random = new Random();
    public Vector2 position;
    public int worldWidth;
    public int worldHeight;
    public int cellSize;
    public Food(int worldWidth, int worldHeight, int cellSize)
    {
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;
        this.cellSize = cellSize;
        position = NewPositon();


    }
    
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, new Rectangle(position.ToPoint(), new Point(cellSize, cellSize)), Color.Red);
    }

    public Vector2 NewPositon()
    {
        if (worldWidth != 0 && worldHeight != 0 && cellSize != 0)
        {
            var posX = random.Next(0, (worldWidth / cellSize)) * cellSize;
            var posY = random.Next(0, (worldHeight / cellSize)) * cellSize;
            return new Vector2(posX, posY);
        }
        return Vector2.Zero;
    }
}