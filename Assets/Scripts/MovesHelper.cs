using System;
using System.Collections.Generic;

public struct MovesHelper {
    public List<int> Checkers;
    // public ulong[] LegalMoves;
    public ulong Pinned;
    public ulong CheckAttackBoard;
    Side side;
    
    public MovesHelper(Side side) {
        Pinned = 0ul;
        Checkers = new List<int>();
        CheckAttackBoard = 0;
        // LegalMoves = new ulong[64];
        this.side = side;
    }
    
    public void ClearMoves() {
        Checkers.Clear();
        Pinned = 0ul;
        CheckAttackBoard = 0ul;
    }
    
    public void AddCheckAttack(Piece typeAttacking, int attackingPos, int kingPos) {
        // Find out how this type is attacking the king
        // Add to the check attack board
        // So for a sliding piece, its the row/col/diag that they would go to attack
        // And for a leaping piece, its that pieces pos
        switch (typeAttacking) {
            // All the leaping pieces
            case Piece.BKing:
            case Piece.BKnight:
            case Piece.BPawn:
            case Piece.WKing:
            case Piece.WKnight:
            case Piece.WPawn:
                CheckAttackBoard |= 1ul<<attackingPos;
                Checkers.Add(attackingPos);
                break;
            // All the sliding pieces
            case Piece.BBishop:
            case Piece.BRook:
            case Piece.BQueen:
            case Piece.WBishop:
            case Piece.WRook:
            case Piece.WQueen:
                // From that piece pos to king pos
                // Add that path to CheckAttackBoard
                // That way we can determine what moves are legal
                // If a non-king piece move onto CheckAttackBoard, or the king moves off of the CheckAttackBoard or captures the piece, then it should be legal
                // given that CheckAttackBoard>0
                // This doesn't account for pinned pieces, which is where Pinned comes into play.
                Checkers.Add(attackingPos);
                break;
        }
    }
}