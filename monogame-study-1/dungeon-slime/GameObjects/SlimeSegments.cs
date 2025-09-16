using Microsoft.Xna.Framework;

namespace DungeonSlime.GameObjects;

public struct SlimeSegment
{
    public Vector2 At;
    public Vector2 To;
    public Vector2 Direction;
    public Vector2 ReverseDirection => new Vector2(-Direction.X, -Direction.Y);

    
}