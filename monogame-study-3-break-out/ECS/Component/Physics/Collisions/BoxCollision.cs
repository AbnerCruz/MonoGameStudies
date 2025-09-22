using System;
using Microsoft.Xna.Framework;
using Physics2D.Collisions.Colliders;

namespace Physics2D.Collisions;

public static class BoxCollision
{
    public static bool CheckHelper(BoxCollider collider, BoxCollider other)
    {
        Rectangle a = collider.Rectangle;
        Rectangle b = other.Rectangle;

        return a.Right > b.Left && a.Left < b.Right && a.Bottom > b.Top && a.Top < b.Bottom;
    }

    // Please, consider that inside collision you need to turn object1 for object2 in the call
    public static Vector2 NormalHelper(Rectangle object1, Rectangle object2)
    {
        Vector2 normal = Vector2.Zero;
        Vector2 object1Center = new Vector2(object1.X + object1.Width / 2f, object1.Y + object1.Height / 2f);
        Vector2 object2Center = new Vector2(object2.X + object2.Width / 2f, object2.Y + object2.Height / 2f);
        Vector2 diference = object1Center - object2Center;

        float overlapX = (object1.Width + object2.Width) / 2f - Math.Abs(diference.X);
        float overlapY = (object1.Height + object2.Height) / 2f - Math.Abs(diference.Y);

        if (overlapX < overlapY)
        {
            normal.X = diference.X > 0 ? 1 : -1;
        }
        else
        {
            normal.Y = diference.Y > 0 ? 1 : -1;
        }

        return normal;
    }
}