using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public class EnemyBarrier
{
    GameManager Manager;
    public Vector2 Position { get; set; }
    public Point Size { get; set; }
    public Rectangle Collisor { get; set; }
    public float Speed;

    public EnemyBarrier(GameManager manager, Vector2 position)
    {
        Random random = new();
        Manager = manager;
        Position = position;
        Size = new Point(100, 20);
        Collisor = new Rectangle(Position.ToPoint(), Size);
        Speed = random.Next(-5, 5);
        if(Speed == 0)
        {
            Speed = 5;
        }
    }

    public void Update()
    {
        Collisor = new Rectangle(Position.ToPoint(), Size);
        Movement();
    }

    public void Movement()
    {
        bool hitBorder = Position.X <= 0 || Position.X + Size.X >= Manager.ScreenSize.X;
        if (hitBorder)
        {
            Speed = -Speed;
        }
        Position += new Vector2(Speed, 0);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Manager.Pixel, Collisor, Color.White);
    }
}