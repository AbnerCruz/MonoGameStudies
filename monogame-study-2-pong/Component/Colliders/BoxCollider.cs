using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GameObjects;

namespace Colliders;

public class BoxCollider : Collider
{
    public Vector2 BoxPosition { get; set; }
    public Vector2 BoxSize { get; set; }
    public Rectangle Rectangle { get; set; }
    public Color BoxColor { get; set; }
    public Color CurrentColor { get; set; }

    public BoxCollider(Vector2 position, Vector2 boxSize, Color color)
    {
        BoxPosition = position;
        BoxSize = boxSize;
        base.size = BoxSize;
        BoxColor = color;
        CurrentColor = BoxColor;
        Rectangle = new Rectangle(BoxPosition.ToPoint(), BoxSize.ToPoint());
    }

    public void DrawBoxCollider(SpriteBatch spriteBatch, Texture2D pixel, Rectangle box, int thickness = 1)
    {
        // Topo
        spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, box.Width, thickness), CurrentColor);
        // Esquerda
        spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, thickness, box.Height), CurrentColor);
        // Direita
        spriteBatch.Draw(pixel, new Rectangle(box.Right - thickness, box.Y, thickness, box.Height), CurrentColor);
        // Base
        spriteBatch.Draw(pixel, new Rectangle(box.X, box.Bottom - thickness, box.Width, thickness), CurrentColor);
    }

    public override void Update(Vector2 position, List<Entity> entities)
    {
        BoxPosition = position;
        Rectangle = new Rectangle(BoxPosition.ToPoint(), BoxSize.ToPoint());
        
        CurrentColor = CollisionCheckUpdate(this, entities) ? Color.Red : BoxColor;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel, Vector2 origin)
    {
        DrawBoxCollider(spriteBatch, pixel, Rectangle);
    }
}