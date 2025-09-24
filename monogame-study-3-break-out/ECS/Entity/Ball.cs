using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics2D.Collisions.Colliders;
using Physics2D.Collisions;
using Physics2D.Movement;
using MyLibrary.InputManager;
using MyLibrary.Random;
using GameObjects;
using Physics2D.Movement.Area;

namespace GameObjects.Entity;

public class Ball : Entity
{
    private Rectangle _rectangle;
    private Vector2 _size;
    private Vector2 _initialPosiion;
    private Vector2 _targetDirection;
    private float _speed = 7f;
    private MovementArea _movementArea;
    private int _playerLife;
    private Vector2 _angle = new Vector2(Rand.Int(20,60), Rand.Int(120, 160));


    public Ball(Texture2D pixel, Vector2 position, Vector2 size, Collider collider, MovementArea movementArea, ref int playerLife) : base(pixel, position, collider)
    {
        _initialPosiion = position;
        _size = size;
        _rectangle = Geometry.NewRect(EntityPosition, _size);

        _targetDirection = Movement.Translate(Geometry.NewAngle(_angle.X, _angle.Y), _speed);
        _movementArea = movementArea;
        _playerLife = playerLife;
    }

    public void Update(List<Entity> entities)
    {
        EntityPosition += _targetDirection;
        EntityPosition = ScreenBounds.ClampToArea(_movementArea, this);
        _rectangle = Geometry.NewRect(EntityPosition, _size);
        EntityCollider.Update(entities);
        BoundsBounceManager();
        CollisionBounceManager();
        _targetDirection.Normalize();
        _targetDirection *= _speed;
    }

    private void BoundsBounceManager()
    {
        Vector2 normal = Vector2.Zero;
        if (!ScreenBounds.IsInArea(_movementArea, this))
        {
            if (EntityCollider is BoxCollider box)
            {
                if (_movementArea is RectangleArea area)
                {
                    if (_rectangle.Bottom >= area.Area.Bottom)
                    {
                        _playerLife -= 1;
                        ResetPos();
                        return;
                    }
                    normal = BoxCollision.NormalHelper(area.Area, _rectangle);
                    if (normal != Vector2.Zero)
                    {
                        normal.Normalize();
                    }
                    if (Vector2.Dot(_targetDirection, normal) < 0)
                    {
                        _targetDirection = Vector2.Reflect(_targetDirection, normal);
                    }
                }
            }
        }
    }

    public void CollisionBounceManager()
    {
        Vector2 normal = Vector2.Zero;
        Vector2 vectorialSum = Vector2.Zero;
        foreach (CollisionResult result in EntityCollisionResults)
        {
            if (result.Collided)
            {
                vectorialSum += result.Normal;
                _speed += 0.25f;
                if (_speed >= 14f)
                {
                    _speed = 14f;
                }
            }
        }
        normal += vectorialSum;
        if (normal != Vector2.Zero)
        {
            normal.Normalize();
        }
        if (Vector2.Dot(_targetDirection, normal) < 0)
        {
            _targetDirection = Vector2.Reflect(_targetDirection, normal);
            foreach (CollisionResult result in EntityCollisionResults)
            {
                if (result.Entity is Paddle player)
                {
                    _targetDirection.X += player._velocity.X;
                    _targetDirection.Y *= 2f;
                    break;
                }
            }
        }
    }

    public void ResetPos()
    {
        EntityPosition = _initialPosiion;
        _targetDirection = Movement.Translate(Geometry.NewAngle(_angle.X, _angle.Y), _speed);
        _speed = 7f;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Pixel, _rectangle, Color.White);
        EntityCollider.Draw(spriteBatch);
    }
}
