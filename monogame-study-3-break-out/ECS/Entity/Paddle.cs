using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameObjects;
using System;

namespace Entity;

public class Paddle : Entity
{
    
    private Vector2 _position;
    private Vector2 _size;
    private float _speed;

    private Texture2D _pixel;
    private Rectangle _rectangle;
    private Color _currentColor;
    private Color _initialColor;

    public Paddle(Texture2D pixel, Vector2 position, Vector2 size, Color color, float speed = 5f)
    {
        _pixel = pixel;
        _position = position;
        _size = size;
        _initialColor = color;
        _currentColor = _initialColor;
        _speed = speed;

        _rectangle = Geometry.NewRect(_position, _size);
    }

    public void Update(Rectangle screenbounds)
    {
        _position = TranslateHorizontalAxis(screenbounds, _position, _size, _speed, true);
        _rectangle = Geometry.NewRect(_position, _size);
    }

    

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, _rectangle, _currentColor);
    }
}