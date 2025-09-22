using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Physics2D.Collisions.Colliders;
using System;

namespace Physics2D.Collisions;

public abstract class CollisionSystem
{
    private static readonly Dictionary<(Type, Type), Func<Collider, Collider, CollisionResult>> rules = new()
    {
        {(typeof(BoxCollider), typeof(BoxCollider)), (self,other) => BoxBox((BoxCollider)self, (BoxCollider)other)},
        {(typeof(BoxCollider), typeof(CircleCollider)), (self,other) => BoxCircle((BoxCollider)self,(CircleCollider)other)},
        {(typeof(CircleCollider), typeof(CircleCollider)), (self,other) => CircleCircle((CircleCollider)self, (CircleCollider)other)}
    };


    public static CollisionResult Collision(Collider a, Collider b)
    {
        var key = (a.GetType(), b.GetType());
        if (rules.TryGetValue(key, out var rule)) return rule(a, b);

        key = (b.GetType(), a.GetType());
        if (rules.TryGetValue(key, out rule)) return rule(b, a);

        throw new NotImplementedException($"Not implemented collision to {a.GetType()} and {b.GetType()}");
    }

    private static CollisionResult BoxBox(BoxCollider collider, BoxCollider other)
    {
        Rectangle a = collider.Rectangle;
        Rectangle b = other.Rectangle;

        bool Collided = BoxCollision.CheckHelper(collider, other);
        Vector2 Normal = BoxCollision.NormalHelper(collider.Rectangle, other.Rectangle);

        CollisionResult result = new CollisionResult(Collided, Normal);

        return result;
    }
    private static CollisionResult BoxCircle(BoxCollider a, CircleCollider b)
    {
        return CollisionResult.None;

    }
    private static CollisionResult CircleCircle(CircleCollider a, CircleCollider b)
    {
        return CollisionResult.None;

    }
}

public struct CollisionResult
{
    public bool Collided;
    public Vector2 Normal;
    public float Depth;

    public CollisionResult(bool collided, Vector2 normal)
    {
        Collided = collided;
        Normal = normal;
    }

    public static readonly CollisionResult None = new(false, Vector2.Zero);
}