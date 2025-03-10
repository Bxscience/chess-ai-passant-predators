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
    public override string ToString()
    {
        // Convert coordinates to algebraic notation (e.g., (0, 0) -> "a1", (4, 7) -> "e8")
        string startSquare = $"{(char)('a' + Start.x)}{Start.y + 1}";
        string endSquare = $"{(char)('a' + End.x)}{End.y + 1}";

        // Get the piece symbol (e.g., "N" for knight, "" for pawn)
        string pieceSymbol = Type switch
        {
            Piece.WKnight or Piece.BKnight => "N",
            Piece.WBishop or Piece.BBishop => "B",
            Piece.WRook or Piece.BRook => "R",
            Piece.WQueen or Piece.BQueen => "Q",
            Piece.WKing or Piece.BKing => "K",
            _ => "P" // Pawns don't get a symbol
        };

        // Add "x" for captures
        string captureSymbol = Captured != Piece.None ? "x" : "";

        // Add promotion notation (e.g., "=Q" for queen promotion)
        string promotionSymbol = PromoteType.HasValue ? $"={PromoteType.Value}" : "";

        // Combine everything (e.g., "Nf3", "exd5", "e8=Q")
        return $"{pieceSymbol}{captureSymbol}{endSquare}{promotionSymbol}";
    }
}
