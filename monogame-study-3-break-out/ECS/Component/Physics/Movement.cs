using Microsoft.Xna.Framework;

namespace Physics2D.Movement;

public class Movement()
{
    public static Vector2 Translate(Vector2 axis, float speed = 1f)
    {
        Vector2 direction = Vector2.Zero;
        direction = new Vector2(axis.X, axis.Y);

        if (direction != Vector2.Zero)
        {
            if (direction.Length() > 1)
            {
                direction /= direction.Length();
            }
        }

        return direction * speed;
    }

    public static Vector2 Translate(float x, float y, float speed = 1f)
    {
        Vector2 direction = Vector2.Zero;
        direction = new Vector2(x, y);

        if (direction != Vector2.Zero)
        {
            if (direction.Length() > 1)
            {
                direction /= direction.Length();
            }
        }

        return direction * speed;
    }
}