using System;

namespace MobileForge
{
    public readonly struct RectF
    {
        public readonly float X, Y, Width, Height;
        public RectF(float x, float y, float width, float height) { X = x; Y = y; Width = width; Height = height; }
        public float Left => MathF.Min(X, X + Width);
        public float Top => MathF.Min(Y, Y + Height);
        public float Right => MathF.Max(X, X + Width);
        public float Bottom => MathF.Max(Y, Y + Height);
        public Vector2 Center => new((Left + Right) / 2, (Top + Bottom) / 2);
        public bool Contains(Vector2 point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        public bool Intersects(RectF other) => Left <= other.Right && Right >= other.Left && Top <= other.Bottom && Bottom >= other.Top;
        public RectF Moved(Vector2 delta) => new(X + delta.X, Y + delta.Y, Width, Height);
        public Vector2 ClosestPoint(Vector2 point) => new(Math.Clamp(point.X, Left, Right), Math.Clamp(point.Y, Top, Bottom));
    }
    public readonly struct CircleF
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public CircleF(Vector2 center, float radius) { Center = center; Radius = MathF.Max(0, radius); }
        public CircleF(float x, float y, float radius) : this(new Vector2(x, y), radius) { }
        public bool Contains(Vector2 point) => (point - Center).LengthSquared <= Radius * Radius;
    }
    // These functions only query geometry. The game decides how objects respond.
    public static class Collision
    {
        public static bool Intersects(RectF a, RectF b) => a.Intersects(b);
        public static bool Intersects(CircleF a, CircleF b) { var radius = a.Radius + b.Radius; return (a.Center - b.Center).LengthSquared <= radius * radius; }
        public static bool Intersects(RectF rect, CircleF circle) { var nearest = rect.ClosestPoint(circle.Center); return (nearest - circle.Center).LengthSquared <= circle.Radius * circle.Radius; }
        public static bool Intersects(CircleF circle, RectF rect) => Intersects(rect, circle);
        public static bool PointIn(RectF rect, Vector2 point) => rect.Contains(point);
        public static bool PointIn(CircleF circle, Vector2 point) => circle.Contains(point);
        public static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var direction = b - a;
            var lengthSquared = direction.LengthSquared;
            if (lengthSquared == 0) return a;
            var t = Math.Clamp(Vector2.Dot(point - a, direction) / lengthSquared, 0, 1);
            return a + direction * t;
        }
        public static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
            => Vector2.Distance(point, ClosestPointOnSegment(point, a, b));
        public static bool Intersects(CircleF circle, Vector2 a, Vector2 b)
            => (circle.Center - ClosestPointOnSegment(circle.Center, a, b)).LengthSquared <= circle.Radius * circle.Radius;
        public static bool Intersects(RectF rect, Vector2 a, Vector2 b)
        {
            if (rect.Contains(a) || rect.Contains(b)) return true;
            var topLeft = new Vector2(rect.Left, rect.Top);
            var topRight = new Vector2(rect.Right, rect.Top);
            var bottomLeft = new Vector2(rect.Left, rect.Bottom);
            var bottomRight = new Vector2(rect.Right, rect.Bottom);
            return SegmentsIntersect(a, b, topLeft, topRight) || SegmentsIntersect(a, b, topRight, bottomRight) ||
                   SegmentsIntersect(a, b, bottomRight, bottomLeft) || SegmentsIntersect(a, b, bottomLeft, topLeft);
        }
        public static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var ab = b - a; var cd = d - c; var denominator = Vector2.Cross(ab, cd);
            if (MathF.Abs(denominator) < 0.000001f)
            {
                if (MathF.Abs(Vector2.Cross(c - a, ab)) > 0.000001f) return false;
                return MathF.Max(MathF.Min(a.X, b.X), MathF.Min(c.X, d.X)) <= MathF.Min(MathF.Max(a.X, b.X), MathF.Max(c.X, d.X)) &&
                       MathF.Max(MathF.Min(a.Y, b.Y), MathF.Min(c.Y, d.Y)) <= MathF.Min(MathF.Max(a.Y, b.Y), MathF.Max(c.Y, d.Y));
            }
            var t = Vector2.Cross(c - a, cd) / denominator;
            var u = Vector2.Cross(c - a, ab) / denominator;
            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }
    }
}
