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

    public override void Update(List<Entity> entities)
    {
        ColliderPosition = Owner.EntityPosition;
        Rectangle = Geometry.NewRect(ColliderPosition, Size);
        CollisionCaller(entities);
    }

    public void DrawCollisionBorder(SpriteBatch spriteBatch)
    {
        int thickness = 1;

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