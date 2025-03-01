using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Ply
{
    public Vector2Int Start;
    public Vector2Int End;
    public Piece Type;
    public Piece Captured;
    public Piece? PromoteType;
    public Side Side => Type switch
    {
        Piece.WPawn or Piece.WBishop or Piece.WKnight or Piece.WRook or Piece.WQueen or Piece.WKing => Side.White,
        Piece.BPawn or Piece.BBishop or Piece.BKnight or Piece.BRook or Piece.BQueen or Piece.BKing => Side.Black,
        Piece.None => Side.None,
        _ => Side.None,
    };


    public Ply(Vector2Int start, Vector2Int end, Piece type, Piece captured = Piece.None, Piece? promoteType = null) {
        Start = start;
        End = end;
        Type = type;
        Captured = captured;
        PromoteType = promoteType;
    }
}
