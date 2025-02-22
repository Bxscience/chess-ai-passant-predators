using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Ply
{
    public Vector2Int Start;
    public Vector2Int End;
    public Piece Type;
    public Piece? Captured;

    public Ply(Vector2Int start, Vector2Int end, Piece type) {
        Start = start;
        End = end;
        Type = type;
        Captured = Piece.None;
    }
}
