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


namespace monogame_study_3_break_out;

public class BreakOut : Core
{
    private static int windowWidth = 800;
    private static int windowHeight = 650;
    private Rectangle screenBounds;
    private List<Entity> entities;
    public int playerLife = 3;
    public Brick[,] Bricks;
    Vector2 _brickSize => new Vector2(windowWidth/10, 20);
    int levels = 6;
    int quantity => (int)(windowWidth / (int)_brickSize.X);

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
        Texture2D _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        Vector2 _playerSize = new Vector2(100, 20);
        Vector2 _playerPosition = new Vector2((screenBounds.Right * 0.5f) - (_playerSize.X * 0.5f), (screenBounds.Bottom - 50));
        Entity player = new Paddle(_pixel, _playerPosition, _playerSize, Color.CornflowerBlue, new BoxCollider(_pixel, _playerPosition, _playerSize), new RectangleArea(screenBounds), 10f);

        Vector2 _boxSize = new Vector2(15, 15);
        Vector2 _boxPosition = new Vector2((windowWidth - _boxSize.X) * 0.5f, (windowHeight - _boxSize.Y) * 0.5f);
        Entity ball = new Ball(_pixel, _boxPosition, _boxSize, new BoxCollider(_pixel, _boxPosition, _boxSize), new RectangleArea(screenBounds), ref playerLife);

        GenerateBricks(_pixel, levels, quantity);

        entities.Add(player);
        entities.Add(ball);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        List<Entity> toRemove = new List<Entity>();
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
                if (brick._life <= 0)
                {
                    toRemove.Add(entity); 
                }
            }
        }
        foreach (Entity remove in toRemove)
        {
            RemoveBrick(remove, (Brick)remove);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin();
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

    public void GenerateBricks(Texture2D pixel, int levels, int quantity)
    {
        for (int x = 0; x < levels; x++)
        {
            for (int y = 0; y < quantity; y++)
            {
                int k = (int)(levels / 3);

                int life = (int)((levels - 1 - x) / k)+1;
                Vector2 position = new Vector2(y * _brickSize.X, x * _brickSize.Y + 30);
                Bricks[x, y] = new Brick(pixel, position, _brickSize, new BoxCollider(pixel, position, _brickSize), life, new Vector2(x, y));
                entities.Add(Bricks[x, y]);
            }
        }
    }

    public void RemoveBrick(Entity entity, Brick target)
    {
        entities.Remove(entity);
        Bricks[(int)target._arrayPos.X, (int)target._arrayPos.Y] = null;
    }
}
