using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using GameObjects;

namespace Colliders;

public class CircleCollider : Collider
{
    public Vector2 CirclePosition { get; set; }
    public float CircleRadius;
    public Color CircleColor;
    public Color CurrentColor;

    public CircleCollider(Vector2 position, float radius, Color color)
    {
        CirclePosition = position;
        CircleRadius = radius;
        base.size = new Vector2(CircleRadius, CircleRadius);
        CircleColor = color;
        CurrentColor = CircleColor;
    }

    public override void Update(Vector2 position, List<Entity> entities)
    {
        CirclePosition = position;
        CurrentColor = CollisionCheckUpdate(this, entities) ? Color.Red : CircleColor;
    }

    public Point[] DrawCircleCollider(Vector2 origin)
    {
        int segments = 120;
        float increment = MathF.PI * 2f / segments;
        Point[] points = new Point[segments];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * increment;
            int x = (int)(origin.X + Math.Cos(angle) * CircleRadius);
            int y = (int)(origin.Y + Math.Sin(angle) * CircleRadius);

            points[i] = new Point(x, y);
        }
        return points;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel, Vector2 origin)
    {
        foreach (Point point in DrawCircleCollider(origin))
        {
            spriteBatch.Draw(pixel, new Rectangle(point, new Point(1,1)), CurrentColor);
        }
    }
}