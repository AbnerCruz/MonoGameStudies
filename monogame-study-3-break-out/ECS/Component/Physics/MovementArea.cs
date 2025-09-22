using System;
using GameObjects;
using GameObjects.Entity;
using Microsoft.Xna.Framework;
using Physics2D.Collisions.Colliders;

namespace Physics2D.Movement.Area;

public class MovementArea {
    Vector2 AreaLocation;

    public MovementArea(Vector2 areaLocation)
    {
        AreaLocation = areaLocation;
    }
}

public class RectangleArea : MovementArea
{
    public Vector2 Position;
    public Vector2 Size;
    public Rectangle Area;
    public RectangleArea(Rectangle rectangle) : base(new Vector2(rectangle.X, rectangle.Y))
    {
        Area = rectangle;
    }
    public RectangleArea(Vector2 position, Vector2 size) : base(position)
    {
        Position = position;
        Size = size;
        Area = Geometry.NewRect(Position, Size);
    }
}

public class ElipseArea : MovementArea
{
    Vector2 Position;
    Vector2 Axis;

    public ElipseArea(Vector2 position, Vector2 axis) : base(position)
    {
        Position = position;
        Axis = axis;
    }
}

public class CircleArea : ElipseArea
{
    Vector2 Position;
    float Radius;

    public CircleArea(Vector2 position, float radius) : base(position, new Vector2(radius, radius))
    {
        Position = position;
        Radius = radius;
    }
}

public static class ScreenBounds
{
    public static bool IsInArea(MovementArea area, Entity entity)
    {
        if (entity.EntityCollider is BoxCollider box)
        {
            Rectangle a = box.Rectangle;
            if (area is RectangleArea rect)
            {
                Rectangle b = rect.Area;
                return (a.Left > b.Left && a.Top > b.Top && a.Right < b.Right && a.Bottom < b.Bottom);
            }
        }
        return false;
    }

    public static Vector2 ScreenBoundsNormal()
    {
        return Vector2.Zero;
    }

    public static Vector2 ClampToArea(MovementArea area, Entity entity)
    {
        if (area is RectangleArea rect)
        {
            Rectangle a = rect.Area;
            Vector2 entityPos = entity.EntityPosition;
            if (entity.EntityCollider is BoxCollider box)
            {
                Rectangle size = box.Rectangle;
                return new Vector2(
                    MathHelper.Clamp(entityPos.X, a.Left, a.Right - size.Width),
                    MathHelper.Clamp(entityPos.Y, a.Top, a.Bottom - size.Height)
                );
            }
        }
        else if (area is CircleArea circle)
        {
            throw new NotImplementedException();
        }
        return entity.EntityPosition;
    }
}
