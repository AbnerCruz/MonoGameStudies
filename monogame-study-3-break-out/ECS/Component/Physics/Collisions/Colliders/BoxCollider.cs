using Microsoft.Xna.Framework;
using System.Collections.Generic;
using GameObjects;
using GameObjects.Entity;
using static GameObjects.Geometry;
using Microsoft.Xna.Framework.Graphics;

namespace Physics2D.Collisions.Colliders;

public class BoxCollider : Collider
{
    public Rectangle Rectangle;
    private Vector2 Size;

    public BoxCollider(Texture2D pixel, Vector2 position, Vector2 size) : base(position, pixel)
    {
        Size = size;
        Rectangle = Geometry.NewRect(ColliderPosition, Size);
    }

    public override void Update(Vector2 position, List<Entity> entities)
    {
        ColliderPosition = position;
        Rectangle = Geometry.NewRect(ColliderPosition, Size);
        CollisionCaller(entities);
    }

    public void DrawCollisionBorder(SpriteBatch spriteBatch)
    {
        int thickness = 2;

        spriteBatch.Draw(Pixel, new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Width, thickness), CurrentColor);
        spriteBatch.Draw(Pixel, new Rectangle(Rectangle.X, Rectangle.Y + Rectangle.Height - thickness, Rectangle.Width, thickness), CurrentColor);
        spriteBatch.Draw(Pixel, new Rectangle(Rectangle.X, Rectangle.Y, thickness, Rectangle.Height), CurrentColor);
        spriteBatch.Draw(Pixel, new Rectangle(Rectangle.X + Rectangle.Width - thickness, Rectangle.Y, thickness, Rectangle.Height), CurrentColor);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (ShowBorder)
        {
            DrawCollisionBorder(spriteBatch);
        }
    }
}