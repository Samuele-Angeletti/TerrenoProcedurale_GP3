using System;

/// <summary>
/// Coordinata discreta sulla griglia di pathfinding.
/// Struct immutabile, hashable, senza dipendenze Unity.
/// </summary>
public readonly struct GridNode : IEquatable<GridNode>
{
    public int X { get; }
    public int Y { get; }

    public GridNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(GridNode other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is GridNode n && Equals(n);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";

    public static bool operator ==(GridNode a, GridNode b) => a.Equals(b);
    public static bool operator !=(GridNode a, GridNode b) => !a.Equals(b);
}
