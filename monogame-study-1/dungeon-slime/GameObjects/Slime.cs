using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace DungeonSlime.GameObjects;

public class Slime
{
    private static readonly TimeSpan s_movementTimer = TimeSpan.FromMilliseconds(200);

    private TimeSpan _movementTimer;

    private float _movementProgress;

    private Vector2 _nextDirection;

    private float _stride;

    private List<SlimeSegment> _segments;

    private AnimatedSprite _sprite;

    private Queue<Vector2> _inputBuffer;

    private const int MAX_BUFFER_SIZE = 2;

    public event EventHandler BodyCollision;

    public Slime(AnimatedSprite sprite)
    {
        _sprite = sprite;
    }

    public void Initialize(Vector2 startingPosition, float stride)
    {
        _segments = new List<SlimeSegment>();

        _stride = stride;

        SlimeSegment head = new SlimeSegment();
        head.At = startingPosition;
        head.To = startingPosition + new Vector2(_stride, 0);
        head.Direction = Vector2.UnitX;

        _segments.Add(head);

        _nextDirection = head.Direction;
        _movementTimer = TimeSpan.Zero;

        _inputBuffer = new Queue<Vector2>(MAX_BUFFER_SIZE);
    }

    private void HandleInput()
    {
        Vector2 potentialNextDirection = Vector2.Zero;
        if (GameController.MoveUp())
        {
            potentialNextDirection = -Vector2.UnitY;
        }
        else if (GameController.MoveDown())
        {
            potentialNextDirection = Vector2.UnitY;
        }
        else if (GameController.MoveLeft())
        {
            potentialNextDirection = -Vector2.UnitX;
        }
        else if (GameController.MoveRight())
        {
            potentialNextDirection = Vector2.UnitX;
        }

        if (potentialNextDirection != Vector2.Zero && _inputBuffer.Count < MAX_BUFFER_SIZE)
        {
            Vector2 validateAgainst = _inputBuffer.Count > 0 ? _inputBuffer.Last() : _segments[0].Direction;

            float dot = Vector2.Dot(potentialNextDirection, validateAgainst);
            if (dot >= 0)
            {
                _inputBuffer.Enqueue(potentialNextDirection);
            }
        }
    }

    private void Move()
    {
        if (_inputBuffer.Count > 0)
        {
            _nextDirection = _inputBuffer.Dequeue();
        }

        SlimeSegment head = _segments[0];

        head.Direction = _nextDirection;

        head.At = head.To;

        head.To = head.At + head.Direction * _stride;

        _segments.Insert(0, head);
        _segments.RemoveAt(_segments.Count - 1);

        for (int i = 1; i < _segments.Count; i++)
        {
            SlimeSegment segment = _segments[i];
            if (head.At == segment.At)
            {
                if (BodyCollision != null)
                {
                    BodyCollision.Invoke(this, EventArgs.Empty);
                }
                return;
            }
        }
    }

    public void Grow()
    {
        SlimeSegment tail = _segments[_segments.Count - 1];
        SlimeSegment newTail = new SlimeSegment();
        newTail.At = tail.To + tail.ReverseDirection * _stride;
        newTail.To = tail.At;
        newTail.Direction = Vector2.Normalize(tail.At - newTail.At);

        _segments.Add(newTail);
    }

    public void Update(GameTime gameTime)
    {
        _sprite.Update(gameTime);
        HandleInput();
        _movementTimer += gameTime.ElapsedGameTime;

        if (_movementTimer >= s_movementTimer)
        {
            _movementTimer -= s_movementTimer;
            Move();
        }

        _movementProgress = (float)(_movementTimer.TotalSeconds / s_movementTimer.TotalSeconds);
    }

    public void Draw()
    {
        foreach (SlimeSegment segment in _segments)
        {
            Vector2 pos = Vector2.Lerp(segment.At, segment.To, _movementProgress);
            _sprite.Draw(Core.SpriteBatch, pos);
        }
    }

    public Circle GetBounds()
    {
        SlimeSegment head = _segments[0];
        Vector2 pos = Vector2.Lerp(head.At, head.To, _movementProgress);
        Circle bounds = new Circle(
            (int)(pos.X + (_sprite.Width * 0.5f)),
            (int)(pos.Y + (_sprite.Height * 0.5f)),
            (int)(_sprite.Width * 0.5f)
        );

        return bounds;
    }
}
