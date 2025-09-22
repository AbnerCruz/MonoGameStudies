using Microsoft.Xna.Framework;
using MyLibrary;
using System;
using System.Collections.Generic;
using Physics2D.Collisions.Colliders;
using Microsoft.Xna.Framework.Graphics;
using Physics2D.Collisions;

namespace GameObjects.Entity;

public class Entity
{
    public int Id { get; set; }
    public Vector2 EntityPosition;
    public Collider EntityCollider;
    public Texture2D Pixel;
    public List<CollisionResult> EntityCollisionResults = new List<CollisionResult>();

    public Entity() { }

    public Entity(Texture2D pixel, Vector2 position, Collider entityCollider)
    {
        EntityPosition = position;
        EntityCollider = entityCollider;
        Pixel = pixel;
    }
}