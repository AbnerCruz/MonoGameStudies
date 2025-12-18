using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Projectile
{
    GameManager Manager;
    public Point Size {get; set;}
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    bool enemyProjectile = false;
    public bool Destroyed = false;
    public Color Color = Color.White;

    public Rectangle Collisor {get; set;}

    public Projectile(GameManager manager, Vector2 position, bool isEnemyProjectile = false)
    {
        Manager = manager;
        Size = new Point(6, 15);
        Position = position;
        Velocity = new Vector2(0, -10f);
        enemyProjectile = isEnemyProjectile;
        if(enemyProjectile)
        {
            Color = Color.Red;
        }

        Collisor = new Rectangle(Position.ToPoint(), Size);
    }

    public void Update()
    {
        if (enemyProjectile)
        {
            Position += -Velocity;
            if(Collisor.Intersects(Manager.Player.Collisor))
            {
                Manager.Player.GetHit();
                Destroy();
            }
        }
        else
        {
            Position += Velocity;
            foreach(var barrier in Manager.EnemyBarriers)
            {
                if(Collisor.Intersects(barrier.Collisor))
                {
                    Destroy();
                }
            }
        }
        Collisor = new Rectangle(Position.ToPoint(), Size);

        foreach(var enemy in Manager.EnemyFormation.Enemies)
        {
            if(!enemyProjectile)
            {
                if (Collisor.Intersects(enemy.Collisor))
                {
                    Manager.Score += enemy.Points;
                    enemy.Destroy();
                    Destroy();
                }
            }
            
        }
        

        if (Position.Y < 0 || Position.Y + Size.Y > Manager.ScreenSize.Y)
        {
            Destroy();
        }

    }

    public void Destroy()
    {
        Destroyed = true;
        
    }

    public void Draw(SpriteBatch spriteBatch){
        spriteBatch.Draw(Manager.Pixel, Collisor, Color);
    }
}