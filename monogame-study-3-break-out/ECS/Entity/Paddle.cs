using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameObjects.Geometry;
using System;
using Physics2D.Collisions.Colliders;
using System.Collections.Generic;
using MyLibrary.InputManager;
using Physics2D.Movement;
using Physics2D.Movement.Area;

namespace GameObjects.Entity;

public class Paddle : Entity
{
    private Vector2 _size;
    private float _speed;
    public Vector2 _velocity;

    private Rectangle _rectangle;
    private Color _currentColor;
    private Color _initialColor;

    private MovementArea _movementArea;

    public Paddle(Texture2D pixel, Vector2 position, Vector2 size, Color color, Collider collider, MovementArea movementArea, float speed = 0f) : base(pixel, position, collider)
    {
        _velocity = Vector2.Zero;
        _size = size;
        _initialColor = color;
        _currentColor = _initialColor;
        _speed = speed;
        _movementArea = movementArea;

        _rectangle = Geometry.NewRect(position, _size);
    }

    public void Update(List<Entity> entities)
    {
        // EntityPosition = TranslateBothAxis(screenbounds, EntityPosition, _size, _speed, true
        _velocity = Movement.Translate(Input.GetHorizontalAxis(true), 0, _speed);
        EntityPosition += _velocity;
        EntityPosition = ScreenBounds.ClampToArea(_movementArea, this);

        EntityCollider.Update(entities);
        _rectangle = Geometry.NewRect(EntityPosition, _size);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Pixel, _rectangle, _currentColor);
        EntityCollider.Draw(spriteBatch);
    }
}