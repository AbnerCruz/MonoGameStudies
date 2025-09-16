using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Colliders;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace GameObjects;

public class Ball : Entity
{
    private Texture2D _pixel;
    private Vector2 BallInitialPosition;
    public Vector2 BallPosition;
    private float BallRadius;
    private CircleCollider BallCollider;
    private Color BallColor;
    private float BallSpeed;
    public Vector2 BallVelocity;
    public int Player1Score;
    public int Player2Score;

    public Ball(Texture2D pixel, Vector2 position, float radius, Color color, int score1, int score2)
    {
        BallInitialPosition = position;
        BallPosition = BallInitialPosition;
        BallRadius = radius;
        BallColor = color;
        BallCollider = new CircleCollider(BallPosition, BallRadius, Color.White);
        base._collider = BallCollider;
        _pixel = pixel;
        BallSpeed = 10f;
        BallVelocity = RandomInitialVelocity();

        Player1Score = score1;
        Player2Score = score2;
    }

    public void Update(Rectangle bounds, List<Entity> entities)
    {
        BallCollider.Update(BallPosition, entities);
        Move(bounds);
        foreach (Entity entity in entities)
        {
            if (entity != this && BallCollider.CollisionCheck(BallCollider, entity._collider))
            {
                Pong.Game1.hitSound.Play();
                Vector2 normal = BallCollider.CollisionNormalVector(BallCollider, entity._collider);

                float penetration = 0f;
                if (entity._collider is BoxCollider box)
                {
                    Vector2 closestPoint = Geometry.GetClosestPointInRectangle(BallPosition, box.Rectangle);
                    penetration = (BallRadius) - Vector2.Distance(BallPosition, closestPoint);

                    if (penetration > 0)
                    {
                        BallPosition += normal * (penetration + 1f);
                    }
                }
                else if (entity._collider is CircleCollider circle)
                {
                    float dist = Vector2.Distance(BallPosition, circle.CirclePosition);
                    penetration = ((BallRadius) + circle.CircleRadius) - dist;
                    if (penetration > 0)
                    {
                        BallPosition += normal * penetration;
                    }
                }

                if (Vector2.Dot(BallVelocity, normal) < 0)
                {
                    BallVelocity = Vector2.Reflect(BallVelocity, normal);
                    if (entity is Paddle player)
                    {
                        player.rand = Random.Shared.Next(-100, 100);
                    }
                }
            }
        }
    }

    private void DrawFilledCircle(SpriteBatch spriteBatch)
    {
        int radius = (int)BallRadius;
        for (int y = -radius; y <= radius; y++)
        {
            int xSpan = (int)Math.Sqrt(radius * radius - y * y);
            for (int x = -xSpan; x <= xSpan; x++)
            {
                Vector2 point = new Vector2(BallPosition.X + x, BallPosition.Y + y);
                spriteBatch.Draw(_pixel, Geometry.NewRectangle(point, Vector2.One), BallColor);
            }
        }
    }

    private Vector2 RandomInitialVelocity()
    {
        float minAngle = 30f * (float)Math.PI / 180f;
        float maxAngle = 50f * (float)Math.PI / 180f;

        bool toRight = Random.Shared.Next(2) == 0;

        float angle = (float)(Random.Shared.NextDouble() * (maxAngle - minAngle) + minAngle);

        if (!toRight)
        {
            angle = MathF.PI - angle;
        }

        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 direction = new Vector2(x, y);
        return direction;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawFilledCircle(spriteBatch);
        BallCollider.Draw(spriteBatch, _pixel, BallPosition);
    }

    private void Move(Rectangle bounds)
    {
        BallPosition = BallPositionUpdate(bounds);
        if (Keyboard.GetState().IsKeyDown(Keys.R))
        {
            BallPosition = ResetSet();
        }
    }

    private Vector2 BallPositionUpdate(Rectangle bounds)
    {
        Vector2 newPosition = BallPosition + (BallVelocity * BallSpeed);
        Vector2 normal = Vector2.Zero;
        if (BallPosition.X < bounds.Left)
        {
            newPosition = ResetSet();
            Player2Score++;
        }
        else if (BallPosition.X + BallRadius > bounds.Right)
        {
            newPosition = ResetSet();
            Player1Score++;
        }
        if (BallPosition.Y < bounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newPosition.Y = bounds.Top;
        }
        else if (BallPosition.Y > bounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newPosition.Y = bounds.Bottom - BallRadius;
        }

        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            BallVelocity = Vector2.Reflect(BallVelocity, normal);
        }

        return newPosition;
    }

    private Vector2 ResetSet()
    {
        BallVelocity = RandomInitialVelocity();
        return BallInitialPosition;
    }

}
