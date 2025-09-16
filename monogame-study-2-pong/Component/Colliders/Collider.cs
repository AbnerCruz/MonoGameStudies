using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GameObjects;
using System;

namespace Colliders;

public class Collider
{

    public Vector2 size { get; set; }
    public static bool IsBoxColliding(BoxCollider collider, BoxCollider other)
    {
        if (collider.Rectangle.Right > other.Rectangle.Left && collider.Rectangle.Left < other.Rectangle.Right && collider.Rectangle.Bottom > other.Rectangle.Top && collider.Rectangle.Top < other.Rectangle.Bottom)
        {
            return true;
        }
        return false;
    }

    public bool IsCircleColliding(CircleCollider collider, CircleCollider other)
    {
        float absDistance = Vector2.Distance(collider.CirclePosition, other.CirclePosition);
        float distance = absDistance - collider.CircleRadius - other.CircleRadius;
        if (distance < 0)
        {
            return true;
        }
        return false;
    }

    public virtual bool CollisionCheck(Collider collider, Collider other)
    {
        if (collider is BoxCollider box1 && other is BoxCollider box2)
        {
            return IsBoxColliding(box1, box2);
        }
        else if (collider is CircleCollider circle1 && other is CircleCollider circle2)
        {
            return IsCircleColliding(circle1, circle2);
        }
        else if (collider is BoxCollider box3 && other is CircleCollider circle3)
        {
            float closestX = Math.Clamp(circle3.CirclePosition.X, box3.Rectangle.Left, box3.Rectangle.Right);
            float closestY = Math.Clamp(circle3.CirclePosition.Y, box3.Rectangle.Top, box3.Rectangle.Bottom);

            float distanceX = circle3.CirclePosition.X - closestX;
            float distanceY = circle3.CirclePosition.Y - closestY;

            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

            return distanceSquared <= (circle3.CircleRadius * circle3.CircleRadius);
        }
        else if (collider is CircleCollider circle4 && other is BoxCollider box4)
        {
            float closestX = Math.Clamp(circle4.CirclePosition.X, box4.Rectangle.Left, box4.Rectangle.Right);
            float closestY = Math.Clamp(circle4.CirclePosition.Y, box4.Rectangle.Top, box4.Rectangle.Bottom);

            float distanceX = circle4.CirclePosition.X - closestX;
            float distanceY = circle4.CirclePosition.Y - closestY;

            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

            return distanceSquared <= (circle4.CircleRadius * circle4.CircleRadius);
        }
        return false;
    }

    public virtual void Update(Vector2 position, List<Entity> players)
    {
    }

    public virtual bool CollisionCheckUpdate(Collider collider, List<Entity> entities)
    {
        foreach (Entity entity in entities)
        {
            if (collider != entity._collider)
            {
                if(CollisionCheck(collider, entity._collider)) return true;
            }
        }
        return false;
    }

    public virtual Vector2 CollisionNormalVector(Collider collider, Collider other)
    {
        Vector2 normal = Vector2.Zero;
        if (collider is CircleCollider circle1 && other is CircleCollider circle2)
        {
            normal = circle1.CirclePosition - circle2.CirclePosition;
        }

        else if (collider is CircleCollider circle3 && other is BoxCollider box1)
        {
            float closestX = Math.Clamp(circle3.CirclePosition.X, box1.Rectangle.Left, box1.Rectangle.Right);
            float closestY = Math.Clamp(circle3.CirclePosition.Y, box1.Rectangle.Top, box1.Rectangle.Bottom);

            Vector2 closestPoint = new Vector2(closestX, closestY);
            normal = circle3.CirclePosition - closestPoint;
        }

        else if (collider is BoxCollider box2 && other is CircleCollider circle4)
        {
            float closestX = Math.Clamp(circle4.CirclePosition.X, box2.Rectangle.Left, box2.Rectangle.Right);
            float closestY = Math.Clamp(circle4.CirclePosition.Y, box2.Rectangle.Top, box2.Rectangle.Bottom);
            Vector2 closestPoint = new Vector2(closestX, closestY);
            normal = circle4.CirclePosition - closestPoint;
        }

        else if (collider is BoxCollider box3 && other is BoxCollider box4)
        {
            float dx = Math.Min(box3.Rectangle.Right, box4.Rectangle.Right) - Math.Max(box3.Rectangle.Left, box4.Rectangle.Left);
            float dy = Math.Min(box3.Rectangle.Bottom, box4.Rectangle.Bottom) - Math.Max(box3.Rectangle.Top, box4.Rectangle.Top);

            if (dx < dy)
            {
                normal = (box3.Rectangle.Center.X < box4.Rectangle.Center.X) ? -Vector2.UnitX : Vector2.UnitX;
            }
            else
            {
                normal = (box3.Rectangle.Center.Y < box4.Rectangle.Center.Y) ? -Vector2.UnitY : Vector2.UnitY;
            }
        }

        if (normal != Vector2.Zero)
        {
            normal.Normalize();
        }
        return normal;
    }

    public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixel, Vector2 origin)
    {

    }
}