using Microsoft.Xna.Framework;
using MyLibrary.Random;
using System;

namespace GameObjects;

public static class Geometry
{
    public static Rectangle NewRect(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.ToPoint(), size.ToPoint());
    }

    public static Circle NewCircle(Vector2 position, int radius)
    {
        return new Circle(position, radius);
    }

    public abstract class Elipse
    {
        public Vector2 Position;
        public Vector2 Axis;
        public Elipse() { }
        public Elipse(Vector2 center, Vector2 radius)
        {
            Position = center;
            Axis = radius;
        }
    }

    public class Circle : Elipse
    {
        public int Radius;
        public Circle() { }
        public Circle(Vector2 center, int radius) : base(center, new Vector2(radius, radius))
        {
            Radius = radius;
        }
    }
    
    public static Vector2 NewAngle(float min, float max)
    {
        float sorted = Rand.Core() >= 0.5f ? max : min;

        float angle = sorted * MathF.PI / 180;
        return new Vector2(MathF.Cos(angle), -MathF.Sin(angle));
    }
}
