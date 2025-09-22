using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using GameObjects.Entity;
using Physics2D.Collisions;
using Microsoft.Xna.Framework.Graphics;


namespace Physics2D.Collisions.Colliders;

public abstract class Collider
{
    protected Vector2 ColliderPosition;
    public Entity Owner;
    public Texture2D Pixel;
    protected Color Color;
    protected Color CurrentColor;
    public bool ShowBorder = true;
    public List<CollisionResult> CollisionResults = new List<CollisionResult>();

    public Collider() { }

    public Collider(Vector2 position, Texture2D pixel)
    {
        ColliderPosition = position;
        Owner = new Entity();
        Pixel = pixel;
        Color = Color.Green;
        CurrentColor = Color;
    }

    public void CollisionCaller(List<Entity> entities)
    {
        CollisionResults.Clear();
        Owner.EntityCollisionResults.Clear();
        foreach (Entity other in entities)
        {
            List<CollisionResult> toRemove = new List<CollisionResult>();
            if (other != Owner)
            {
                CollisionResults.Add(CollisionSystem.Collision(Owner.EntityCollider, other.EntityCollider));
                foreach (CollisionResult result in CollisionResults)
                {
                    if (!result.Collided)
                    {
                        toRemove.Add(result);
                    }
                    CurrentColor = result.Collided ? Color.Red : Color;
                }
                foreach (CollisionResult remove in toRemove)
                {
                    CollisionResults.Remove(remove);
                }
                Owner.EntityCollisionResults = CollisionResults;
            }
        }
    }

    public virtual void Update(Vector2 position, List<Entity> entities) { }
    public virtual void Draw(SpriteBatch spriteBatch){}
}

