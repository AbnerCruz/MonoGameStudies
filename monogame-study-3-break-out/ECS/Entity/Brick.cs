using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics2D.Collisions.Colliders;
using System;
using System.Collections.Generic;
using Physics2D.Collisions;
using monogame_study_3_break_out;

namespace GameObjects.Entity;

public class Brick : Entity
{
    private Vector2 _size;
    private Rectangle _rectangle;
    private Color _initialColor;
    private Color _currentColor;
    public int _life;
    public Vector2 _arrayPos;

    public Brick(Texture2D pixel, Vector2 position, Vector2 size, Collider collider, int life, Vector2 arrayPos) : base(pixel, position, collider)
    {
        _size = size;
        _rectangle = Geometry.NewRect(position, size);
        _life = life;
        _arrayPos = arrayPos;
        _initialColor = InitialColormanager();
        _currentColor = _initialColor;
    }

    public void Update(List<Entity> entities)
    {
        EntityCollider.Update(entities);
        CollisionManagement(entities);
    }

    public Color InitialColormanager()
    {
        if (_life == 1)
        {
            return Color.CornflowerBlue;
        }
        else if (_life == 2)
        {
            return Color.Yellow;
        }
        else if (_life == 3)
        {
            return Color.Purple;
        }
        else if (_life == 4)
        {
            return Color.Red;
        }
        
        return Color.Orange;
    }

    public void CollisionManagement(List<Entity> entities)
    {
        foreach (CollisionResult result in EntityCollisionResults)
        {
            if (result.Entity is Ball ball)
            {
                _life -= 1;
            }
        }
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Pixel, _rectangle, _currentColor);
        EntityCollider.Draw(spriteBatch);
    }
}