using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameObjects;

public class Geometry
{
    public static Rectangle NewRectangle(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.ToPoint(), size.ToPoint());
    }


    public static Vector2 GetClosestPointInRectangle(Vector2 position, Rectangle rectangle)
    {
        float closestX = Math.Clamp(position.X, rectangle.Left, rectangle.Right);
        float closestY = Math.Clamp(position.Y, rectangle.Top, rectangle.Bottom);
        return new Vector2(closestX, closestY);
    }

    public static Texture2D NewRectangleTexture(GraphicsDevice graphicsDevice)
    {
        Texture2D rectangle = new Texture2D(graphicsDevice, 1, 1);
        rectangle.SetData(new[] { Color.White });
        return rectangle;
    }

    public static Vector2[] NewCircle(Vector2 position, float radius, int segments)
    {
        float increment = MathF.PI * 2f / segments;
        Vector2[] points = new Vector2[segments];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * increment;
            int x = (int)(position.X + Math.Cos(angle) * radius);
            int y = (int)(position.Y + Math.Sin(angle) * radius);
            points[i] = new Vector2(x, y);
        }
        return points;
    }
}