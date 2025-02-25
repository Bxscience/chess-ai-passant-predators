using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public Vector2Int idx;
    [SerializeField]
    public Piece type;
    public bool moved = false;
    
    [SerializeField]
    MeshFilter meshFilter;

    [SerializeField]
    Mesh WKnightMesh, BKnightMesh, WBishopMesh, BBishopMesh, WRookMesh, BRookMesh, WQueenMesh, BQueenMesh;
    
    public Side side => type switch
    {
        Piece.WPawn or Piece.WBishop or Piece.WKnight or Piece.WRook or Piece.WQueen or Piece.WKing => Side.White,
        Piece.BPawn or Piece.BBishop or Piece.BKnight or Piece.BRook or Piece.BQueen or Piece.BKing => Side.Black,
        Piece.None => Side.None,
        _ => Side.None,
    };
    
    public void Promote(Piece promoteType) {
        meshFilter.mesh = promoteType switch {
            Piece.WKnight => WKnightMesh,
            Piece.BKnight => BKnightMesh,
            Piece.WBishop => WBishopMesh,
            Piece.BBishop => BBishopMesh,
            Piece.WRook => WRookMesh,
            Piece.BRook => BRookMesh,
            Piece.WQueen => WQueenMesh,
            Piece.BQueen => BQueenMesh,
        };
    }
}

public enum Side {
    White, Black, None
}
