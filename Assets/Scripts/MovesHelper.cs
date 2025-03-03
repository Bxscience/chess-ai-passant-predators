using System;
using System.Collections.Generic;
using UnityEngine;

// This is per-side
public struct MovesHelper {
    public List<int> Checkers;
    // Maybe we use this?
    // It'd contain legal moves for a side.
    // public ulong[] LegalMoves;
    public ulong Pinned;
    // If this is 0, we are not in check
    // Otherwise we are in check
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
            
                // From that piece pos to king pos
                // Add that path to CheckAttackBoard
                // That way we can determine what moves are legal
                // If a non-king piece move onto CheckAttackBoard, or the king moves off of the CheckAttackBoard or captures the piece, then it should be legal
                // given that CheckAttackBoard>0
                // This doesn't account for pinned pieces, which is where Pinned comes into play.
            case Piece.BBishop:
            case Piece.WBishop:
                AddBishopCheckBoard(attackingPos, kingPos);

                Checkers.Add(attackingPos);
                break;
            case Piece.BRook:
            case Piece.WRook:
                AddRookCheckBoard(attackingPos, kingPos);

                Checkers.Add(attackingPos);
                break;
            case Piece.BQueen:
            case Piece.WQueen:
                if(kingPos % 8 == attackingPos % 8) AddRookCheckBoard(attackingPos, kingPos);
                else AddBishopCheckBoard(attackingPos, kingPos);

                Checkers.Add(attackingPos);
                break;
        }
    }
    
    private void AddBishopCheckBoard(int attackingPos, int kingPos) {
        int increment = 0;
        // If king is under attackingPos
        if(kingPos>attackingPos)
            // If kingPos is further right of attackingPos, then shift 9
            // Assuming king is under us
            increment = (kingPos%8 > attackingPos%8) ? 9 : 7;
        else
            // If kingPos is further right of attackingPos, then shift 7
            // Assuming king is above us
            increment = -((kingPos%8 > attackingPos%8) ? 7 : 9);

        for(int i = attackingPos; i != kingPos; i += increment)
            CheckAttackBoard |= 1ul<<i;
    }
    
    private void AddRookCheckBoard(int attackingPos, int kingPos) {
        int increment = 0;
        // If not equal, then it should be different rows?
        increment = ((kingPos/8 != attackingPos/8) ? 8 : 1)*Math.Sign(kingPos-attackingPos);
        for(int i = attackingPos; i != kingPos; i += increment)
            CheckAttackBoard |= 1ul<<i;
    }
    
    public ulong FilterForLegalMoves(ulong moveBoard, int pos) {
        if( 
            (Pinned & (1ul<<pos)) != 0
            // Okay well, if this piece can take whose pinning it, then we shouldn't
            // So this is incomplete
        ) {
            return 0ul;
        }
        if( CheckAttackBoard == 0 ) {
            return moveBoard;
        }
        // You can only move to block the check, if in check
        return moveBoard & CheckAttackBoard;
    }
}