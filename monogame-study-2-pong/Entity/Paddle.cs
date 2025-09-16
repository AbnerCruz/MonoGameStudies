using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Colliders;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Pong;


namespace GameObjects;

public class Paddle : Entity
{
    private Texture2D _pixel;
    private Vector2 _position;
    private Vector2 _size;
    private Rectangle _rectangle;
    private Color _color;
    public BoxCollider _paddleCollider;
    private int _index;
    private float _speed = 10f;
    private bool _showColliderLines;
    public int rand;

    public Paddle(Texture2D pixel, int index, Vector2 position, Vector2 size, Color color)
    {
        _pixel = pixel;
        _index = index;
        _position = position;
        _size = size;
        _paddleCollider = new BoxCollider(_position, _size, Color.White);
        base._collider = _paddleCollider;
        _showColliderLines = true;


        _rectangle = new Rectangle(_position.ToPoint(), _size.ToPoint());
        _color = color;
    }

    public void Update(Rectangle screenBounds, List<Entity> entities)
    {
        Move(screenBounds);
        _rectangle = new Rectangle(_position.ToPoint(), _size.ToPoint());
        _paddleCollider.Update(_position, entities);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, _rectangle, Color.Green);
        if (_showColliderLines)
        {
            Vector2 origin = new Vector2(_position.X + _size.X * 0.5f, _position.Y + _size.Y * 0.5f);
            _paddleCollider.Draw(spriteBatch, _pixel, origin);
        }
    }

    private void Move(Rectangle screenBounds)
    {
        if (_index == 1)
        {
            if (_paddleCollider.Rectangle.Top > 0)
            {
                if (Pong.Game1.ball.BallVelocity.X < 0)
                {
                    float paddleCenter = _position.Y + _size.Y * 0.5f;
                    float targetY = Pong.Game1.ball.BallPosition.Y + rand;

                    float speed = _speed;
                    if (paddleCenter <= targetY)
                    {
                        _position.Y += speed;
                    }
                    else if (paddleCenter > targetY)
                    {
                        _position.Y -= speed;
                    }
                }
            }
            else
            {
                _position.Y = 1;
            }
            if (_paddleCollider.Rectangle.Bottom >= screenBounds.Bottom)
            {
                _position.Y = screenBounds.Bottom - _size.Y - 1;
            }
        }
        else if (_index == 2)
        {
            if (_paddleCollider.Rectangle.Top > 0)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    _position.Y -= _speed;
                }
            }
            if (_paddleCollider.Rectangle.Bottom < screenBounds.Bottom)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    _position.Y += _speed;
                }
            }
        }
    }
}