using Microsoft.Xna.Framework;
using System.Collections.Generic;
using GameObjects;
using GameObjects.Entity;
using static GameObjects.Geometry;
using Microsoft.Xna.Framework.Graphics;

namespace Physics2D.Collisions.Colliders;

public class CircleCollider : Collider
{
    public Circle Circle;
    private int Radius;

    public CircleCollider(Texture2D pixel, Vector2 position, int radius) : base(position, pixel)
    {
        Radius = radius;
        Circle = Geometry.NewCircle(ColliderPosition, radius);
    }

    public override void Update(List<Entity> entities)
    {
        ColliderPosition = Owner.EntityPosition;
        Circle = Geometry.NewCircle(ColliderPosition, Radius);
        CollisionCaller(entities);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        
    }
}
