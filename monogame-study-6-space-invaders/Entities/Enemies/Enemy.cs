using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public class EnemyFormation
{
    GameManager Manager;
    public Vector2 Origin { get; set; }
    public List<Enemy> Enemies { get; set; }
    public float Speed = 0.4f;
    int Width;
    int Height;

    public double ShootInterval = 0.4d;
    public double ShootTimer;


    public EnemyFormation(GameManager manager)
    {
        Manager = manager;
        Width = (Manager.ScreenSize.X / Manager.CellSize) - 2;
        Height = (Manager.ScreenSize.Y / 4) / Manager.CellSize;

        Enemies = new();

        Origin = new Vector2((Manager.ScreenSize.X - Width * Manager.CellSize) / 2, Manager.CellSize);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Enemies.Add(new Enemy(Manager, new Vector2(x * Manager.CellSize, y * Manager.CellSize), x + 1));
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        bool hitBorder = Origin.X + (Width * Manager.CellSize) >= Manager.ScreenSize.X || Origin.X <= 0;
        if (hitBorder)
        {
            Speed = -Speed;
            Origin += new Vector2(0, Manager.CellSize / 4);
        }
        Origin += new Vector2(Speed, 0);
        foreach (var enemy in Enemies)
        {
            enemy.Update(gameTime);
        }
        Shoot();
        Enemies.RemoveAll(e => e.Destroyed);
    }

    public void Shoot()
    {
        ShootTimer += Manager.GameTime.ElapsedGameTime.TotalSeconds;
        if (ShootTimer >= ShootInterval)
        {
            ShootTimer -= ShootInterval;
            if (Enemies.Count > 0)
            {
                var random = new System.Random();
                int index = random.Next(Enemies.Count);
                var enemy = Enemies[index];
                enemy.isWarning = true;
            }
        }
    }



    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var enemy in Enemies)
        {
            enemy.Draw(spriteBatch);
        }
    }
}

public class Enemy
{
    GameManager Manager;
    public Vector2 LocalOffset { get; set; }
    public Point Size { get; set; }
    public Rectangle Collisor { get; set; }
    public int Speed = 1;
    public bool Destroyed = false;
    public Color BaseColor = Color.Red;
    public Color CurrentColor;
    public int Points;

    public double WarningInterval = 0.6f;
    public double WarningTimer;
    public bool isWarning = false;


    public Enemy(GameManager manager, Vector2 localOffset, int multiplier)
    {
        Manager = manager;
        LocalOffset = localOffset;
        Size = new Point(Manager.CellSize - 10, Manager.CellSize - 10);
        CurrentColor = BaseColor;
        Points = 10 * multiplier;
    }

    public void Update(GameTime gameTime)
    {
        Collisor = new Rectangle(WorldPosition(Manager.EnemyFormation.Origin).ToPoint(), Size);

        if (isWarning)
        {
            WarningTimer += gameTime.ElapsedGameTime.TotalSeconds;
            CurrentColor = Color.Yellow;
            if (WarningTimer >= WarningInterval)
            {
                WarningTimer -= WarningInterval;
                Vector2 projectilePosition = WorldPosition(Manager.EnemyFormation.Origin) + new Vector2(Size.X / 2 - 2, Size.Y);
                Manager.InstantiateProjectile(projectilePosition, true);
                CurrentColor = BaseColor;
                isWarning = false;
            }
        }
    }



    public Vector2 WorldPosition(Vector2 formationOrigin)
    {
        return formationOrigin + LocalOffset;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Manager.Pixel, Collisor, CurrentColor);
    }

    public void Destroy()
    {
        Destroyed = true;
    }

}