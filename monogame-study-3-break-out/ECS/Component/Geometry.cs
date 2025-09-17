using Microsoft.Xna.Framework;
namespace GameObjects;

public abstract class Geometry
{
    public static Rectangle NewRect(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.ToPoint(), size.ToPoint());
    }
}
