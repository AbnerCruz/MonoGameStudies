using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InputManager;
using System;

public class Player
{
    GameManager Manager;
    public Point Size { get; set; }
    public Rectangle Collisor { get; set; }
    public Color BaseColor = Color.White;
    public Color CurrentColor;

    //Attributes
    public int Health = 3;
    public float Speed = 5f;

    //Movement
    public int XAxis = 0;
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public bool isHit = false;
    public double HitDuration = 0.3d;
    public double HitTimer = 0;
    public double ColorInterval = 0.05d;
    public double ColorTimer = 0;

    public Player(GameManager manager)
    {
        Manager = manager;
        Size = new Point(40, 30);
        Position = new Vector2(Manager.ScreenSize.X / 2f - Size.X / 2f, Manager.ScreenSize.Y - 50);
        Velocity = new Vector2(0, 0);
        Collisor = new Rectangle(Position.ToPoint(), Size);
        CurrentColor = BaseColor;
    }

    public void Update(GameTime gameTime)
    {
        Collisor = new Rectangle(Position.ToPoint(), Size);
        PlayerInput();
        Movement();

        if (isHit)
        {
            HandleFlash(gameTime);
        }
    }

    public void PlayerInput()
    {
        if (Input.Keyboard.KeyPressed(Keys.Left) || Input.Keyboard.KeyPressed(Keys.A))
        {
            XAxis = -1;
        }
        else if (Input.Keyboard.KeyPressed(Keys.Right) || Input.Keyboard.KeyPressed(Keys.D))
        {
            XAxis = 1;
        }
        else if (Input.Keyboard.KeyReleased(Keys.Left) || Input.Keyboard.KeyReleased(Keys.A) || Input.Keyboard.KeyReleased(Keys.Right) || Input.Keyboard.KeyReleased(Keys.D))
        {
            XAxis = 0;
        }

        if (Input.Keyboard.KeyJustPressed(Keys.Space))
        {
            Console.WriteLine("PEW");
            Manager.InstantiateProjectile(new Vector2(Position.X + Size.X / 2, Position.Y));
        }
    }
    
    public void Movement()
    {
        Velocity = new Vector2(XAxis * Speed, 0);
        if (Position.X + Velocity.X < 0)
        {
            Position = new Vector2(0, Position.Y);
        }
        else if (Position.X + Velocity.X + Size.X > Manager.ScreenSize.X)
        {
            Position = new Vector2(Manager.ScreenSize.X - Size.X, Position.Y);
        }
        else
        {
            Position += Velocity;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Manager.Pixel, Collisor, CurrentColor);
    }

    public void GetHit()
    {
        if (isHit) return;

        Health--;
        isHit = true;
        HitTimer = HitDuration;
        ColorTimer = ColorInterval;
        if (Health <= 0)
        {
            if(Manager.Score > Manager.HighScore)
            {
                Manager.HighScore = Manager.Score;
            }
            Manager.GameState = GameState.GameOver;
        }
    }

    public void HandleFlash(GameTime gameTime)
    {
        HitTimer -= gameTime.ElapsedGameTime.TotalSeconds;

        if (HitTimer <= 0)
        {
            isHit = false;
            CurrentColor = BaseColor;
            return;
        }

        ColorTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if(ColorTimer <= 0)
        {
            ColorTimer = ColorInterval;

            if (CurrentColor == BaseColor)
            {
                CurrentColor = Color.Transparent;
            }
            else
            {
                CurrentColor = BaseColor;
            }
        }
    }
    
}