using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using InputManager;

public class GameManager
{
    public Point ScreenSize { get; set; } = new Point(500, 600);
    public int CellSize = 50;
    public GameState GameState { get; set; } = GameState.Paused;
    public Texture2D Pixel { get; set; }
    public GameTime GameTime;


    public Player Player { get; set; }
    public List<Projectile> Projectiles { get; set; }
    public EnemyFormation EnemyFormation { get; set; }
    public List<EnemyBarrier> EnemyBarriers { get; set; }

    public SpriteFont _font;
    public int HighScore;
    public int Score;


    public void Start()
    {
        Player = new Player(this);
        Projectiles = new List<Projectile>();
        EnemyFormation = new EnemyFormation(this);
        EnemyBarriers = new()
        {
            new(this, new Vector2(ScreenSize.X  / 4, ScreenSize.Y - 150)),
            new(this, new Vector2(ScreenSize.X * 3 / 4, ScreenSize.Y - 220)),
        };

    }

    public void Update()
    {
        switch(GameState)
        {
            case GameState.Paused:
                Paused();
                break;
            case GameState.Playing:
                Playing();
                break;
            case GameState.GameOver:
                GameOver();
                break;
        }
    }

    public void Playing()
    {
        Player.Update(GameTime);
        foreach (var projectile in Projectiles)
        {
            projectile.Update();
        }
        EnemyFormation.Update(GameTime);
        foreach (var barrier in EnemyBarriers)
        {
            barrier.Update();

        }
        if (Input.Keyboard.KeyJustPressed(Keys.P))
        {
            GameState = GameState.Paused;
        }

        DestroyProjectiles();
    }

    public void GameOver()
    {
        if (Input.Keyboard.KeyJustPressed(Keys.Enter))
        {
            Start();
            GameState = GameState.Playing;
        }
    }
    
    public void Paused()
    {
        if (Input.Keyboard.KeyJustPressed(Keys.P))
        {
            GameState = GameState.Playing;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Player.Draw(spriteBatch);
        foreach (var projectile in Projectiles)
        {
            projectile.Draw(spriteBatch);
        }
        foreach (var barrier in EnemyBarriers)
        {
            barrier.Draw(spriteBatch);
        }
        EnemyFormation.Draw(spriteBatch);
        spriteBatch.DrawString(_font, "Game State: "+ GameState, new Vector2(0,0), Color.White);
        spriteBatch.DrawString(_font, "High Score: " + HighScore, new Vector2(0, ScreenSize.Y - 20), Color.White);
        spriteBatch.DrawString(_font, "Score: " + Score, new Vector2(0, ScreenSize.Y - 40), Color.White);
    }

    public void InstantiateProjectile(Vector2 position, bool isEnemyProjectile = false)
    {
        var newProjectile = new Projectile(this, position, isEnemyProjectile);
        Projectiles.Add(newProjectile);
    }
    
    public void DestroyProjectiles()
    {
        List<Projectile> ToRemove = Projectiles.Where(p => p.Destroyed).ToList();
        foreach(var projectile in ToRemove)
        {
            Console.WriteLine("Removing projectile");
        }
        Projectiles.RemoveAll(p => p.Destroyed);
    }
}

public enum GameState
{
    Paused,
    Playing,
    GameOver,
}