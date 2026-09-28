using System;
using System.Collections.Generic;

namespace MobileForge
{
    // Value type: suitable for positions, velocities, directions and arbitrary 2D data.
    public struct Vector2 : IEquatable<Vector2>
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 Zero => new(0, 0);
        public static Vector2 One => new(1, 1);
        public float LengthSquared => X * X + Y * Y;
        public float Length => MathF.Sqrt(LengthSquared);
        public Vector2 Normalized => Length > 0 ? this / Length : Zero;
        public float Angle => MathF.Atan2(Y, X);
        public Vector2 Perpendicular => new(-Y, X);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);
        public static Vector2 operator *(Vector2 a, float value) => new(a.X * value, a.Y * value);
        public static Vector2 operator *(float value, Vector2 a) => a * value;
        public static Vector2 operator /(Vector2 a, float value) => new(a.X / value, a.Y / value);
        public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
        public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);
        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object? other) => other is Vector2 value && Equals(value);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static float Distance(Vector2 a, Vector2 b) => (a - b).Length;
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * t;
        public static Vector2 Reflect(Vector2 direction, Vector2 normal) => direction - 2 * Dot(direction, normal) * normal;
        public static Vector2 FromAngle(float radians) => new(MathF.Cos(radians), MathF.Sin(radians));
        public static Vector2 Project(Vector2 value, Vector2 onto)
            => onto.LengthSquared == 0 ? Zero : onto * (Dot(value, onto) / onto.LengthSquared);
        public static Vector2 Rotate(Vector2 value, float radians) => new(value.X * MathF.Cos(radians) - value.Y * MathF.Sin(radians), value.X * MathF.Sin(radians) + value.Y * MathF.Cos(radians));
        public static Vector2 ClampMagnitude(Vector2 value, float maximum)
        {
            maximum = MathF.Max(0, maximum);
            return value.LengthSquared > maximum * maximum ? value.Normalized * maximum : value;
        }
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float distance)
        {
            var delta = target - current;
            return delta.Length <= distance || delta.Length == 0 ? target : current + delta.Normalized * MathF.Max(0, distance);
        }
    }

    public static class GameMath
    {
        public const float Pi = MathF.PI;
        public const float Tau = 2 * MathF.PI;
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;
        public static float InverseLerp(float a, float b, float value) => a == b ? 0 : (value - a) / (b - a);
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax) => Lerp(toMin, toMax, InverseLerp(fromMin, fromMax, value));
        public static float SmoothStep(float t) { t = Clamp(t, 0, 1); return t * t * (3 - 2 * t); }
        public static float DegreesToRadians(float degrees) => degrees * Pi / 180;
        public static float RadiansToDegrees(float radians) => radians * 180 / Pi;
        public static float Wrap(float value, float min, float max) { var span = max - min; return span <= 0 ? min : ((value - min) % span + span) % span + min; }
    }

    // Unseeded by default; give a seed when repeatable worlds are useful.
    public sealed class Randomizer
    {
        readonly Random random;
        public Randomizer() => random = new Random();
        public Randomizer(int seed) => random = new Random(seed);
        public int Int(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
        public float Float() => random.NextSingle();
        public float Range(float min, float max) => min + Float() * (max - min);
        public bool Chance(float probability) => Float() < GameMath.Clamp(probability, 0, 1);
        public Vector2 Direction() => Vector2.FromAngle(Range(0f, GameMath.Tau));
        public T Pick<T>(IReadOnlyList<T> items)
        {
            if (items.Count == 0) throw new ArgumentException("A coleção precisa ter pelo menos um item.", nameof(items));
            return items[Int(0, items.Count)];
        }
        public void Shuffle<T>(IList<T> items)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = Int(0, i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
        public Vector2 InsideCircle(float radius = 1)
        {
            var angle = Range(0, GameMath.Tau);
            var distance = MathF.Sqrt(Float()) * radius;
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
        }
    }
}
