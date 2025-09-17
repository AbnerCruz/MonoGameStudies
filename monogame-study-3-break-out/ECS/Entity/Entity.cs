using Microsoft.Xna.Framework;
using MyLibrary;
using System;

namespace Entity;

public class Entity
{
    public int Id { get; set; }

    public Vector2 TranslateHorizontalAxis(Rectangle bounds, Vector2 position, Vector2 size, float speed, bool smooth = false)
    {
        Vector2 moveDirection = Vector2.Zero;
        float x = InputManager.HorizontalAxis(smooth);
        moveDirection.X = x;

        Vector2 newPosition = position + moveDirection * speed;
        newPosition = boundsCheck(bounds, newPosition, size);

        return newPosition;
    }

    public Vector2 TranslateVerticalAxis(Rectangle bounds, Vector2 position, Vector2 size, float speed, bool smooth = false)
    {
        Vector2 moveDirection = Vector2.Zero;
        float y = InputManager.VerticalAxis(smooth);
        moveDirection.Y = y;

        Vector2 newPosition = position + moveDirection * speed;
        newPosition = boundsCheck(bounds, newPosition, size);

        return newPosition;
    }

    public Vector2 Translate(Rectangle bounds, Vector2 position, Vector2 size, float speed, bool smooth = false)
    {
        float separated_x = TranslateHorizontalAxis(bounds, position, size, speed, smooth).X;
        float separated_y = TranslateVerticalAxis(bounds, position, size, speed, smooth).Y;

        Vector2 newPosition = new Vector2(separated_x, separated_y);
        newPosition = boundsCheck(bounds, newPosition, size);

        return newPosition; 
    }

    public Vector2 boundsCheck(Rectangle bounds, Vector2 position, Vector2 size)
    {
        if (position.X < bounds.Left) position.X = bounds.Left;
        if (position.X + size.X > bounds.Right) position.X = bounds.Right - size.X;

        if (position.Y < bounds.Top) position.Y = bounds.Top;
        if (position.Y + size.Y > bounds.Bottom) position.Y = bounds.Bottom - size.Y;

        // Better way to implement
        // position.X = Math.Clamp(position.X, bounds.Left, bounds.Right - size.X);
        // position.Y = Math.Clamp(position.Y, bounds.Top, bounds.Bottom - size.Y);

        return position;
    }
}