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
    private static int windowWidth = 1000;
    private static int windowHeight = 650;
    private Rectangle screenBounds;
    private List<Entity> entities;
    public int playerLife = 3;

    public BreakOut() : base("BreakOut", windowWidth, windowHeight, false)
    {

    }

    protected override void Initialize()
    {
        screenBounds = Geometry.NewRect(Vector2.Zero, new Vector2(windowWidth, windowHeight));
        entities = new List<Entity>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Texture2D _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        Vector2 _playerSize = new Vector2(100, 20);
        Vector2 _playerPosition = new Vector2((screenBounds.Right * 0.5f) - (_playerSize.X * 0.5f), (screenBounds.Bottom - 50));
        Entity player = new Paddle(_pixel, _playerPosition, _playerSize, Color.CornflowerBlue, new BoxCollider(_pixel, _playerPosition, _playerSize), new RectangleArea(screenBounds), 8f);
        player.EntityCollider.Owner = player;

        Vector2 _boxSize = new Vector2(15, 15);
        Vector2 _boxPosition = new Vector2((windowWidth - _boxSize.X) * 0.5f , (windowHeight - _boxSize.Y) * 0.5f);
        Entity ball = new Ball(_pixel, _boxPosition, _boxSize, new BoxCollider(_pixel, _boxPosition, _boxSize), new RectangleArea(screenBounds), ref playerLife);
        ball.EntityCollider.Owner = ball;

        entities.Add(player);
        entities.Add(ball);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
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
        }
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
