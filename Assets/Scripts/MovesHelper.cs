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
    public ulong KingAttackBoard;
    Side side;

    public MovesHelper(Side side) {
        Pinned = 0ul;
        KingAttackBoard = 0;
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
                KingAttackBoard |= 1ul << attackingPos;
                CheckAttackBoard |= 1ul << attackingPos;
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
                CheckAttackBoard |= AddBishopCheckBoard(attackingPos, kingPos);
                KingAttackBoard |= 1ul << attackingPos;

                Checkers.Add(attackingPos);
                break;
            case Piece.BRook:
            case Piece.WRook:
                KingAttackBoard |= 1ul << attackingPos;
                CheckAttackBoard |= AddRookCheckBoard(attackingPos, kingPos);

                Checkers.Add(attackingPos);
                break;
            case Piece.BQueen:
            case Piece.WQueen:
                KingAttackBoard |= 1ul << attackingPos;
                if ((kingPos % 8 == attackingPos % 8) || (kingPos / 8 == attackingPos / 8)) CheckAttackBoard |= AddRookCheckBoard(attackingPos, kingPos);
                else CheckAttackBoard |= AddBishopCheckBoard(attackingPos, kingPos);

                Checkers.Add(attackingPos);
                break;

        } 
        Debug.Log(MagicBitboards.PrintBitBoard(CheckAttackBoard));
        Debug.Log(MagicBitboards.PrintBitBoard(KingAttackBoard));
    }

    public ulong AddBishopCheckBoard(int attackingPos, int kingPos) {
        ulong ray = 0;
        int increment = 0;
        // If king is under attackingPos
        if (kingPos > attackingPos)
            // If kingPos is further right of attackingPos, then shift 9
            // Assuming king is under us
            increment = (kingPos % 8 > attackingPos % 8) ? 9 : 7;
        else
            // If kingPos is further right of attackingPos, then shift 7
            // Assuming king is above us
            increment = -((kingPos % 8 > attackingPos % 8) ? 7 : 9);

        for (int i = attackingPos; i != kingPos; i += increment)
            ray |= 1ul << i;
        return ray;
    }

    public ulong AddRookCheckBoard(int attackingPos, int kingPos) {
        ulong ray = 0;
        // If not equal, then it should be different rows?
        int increment = ((kingPos / 8 != attackingPos / 8) ? 8 : 1) * Math.Sign(kingPos - attackingPos);
        for (int i = attackingPos; i != kingPos; i += increment)
            ray |= 1ul << i;
        return ray;
        
        // int kingRank = kingPos/8;
        // int kingFile = kingPos%8;
        // int attackRank = attackingPos/8;
        // int attackFile = attackingPos%8;
        // Same file
        // if(kingFile==attackFile) {
        //     ulong currentFile = Board.fileA<<kingFile;
        //     if(kingPos>attackingPos) currentFile = (currentFile<<attackRank<<(kingRank+1))>>(kingRank+1);
        //     else currentFile = (currentFile<<attackRank<<(attackRank+1)>>(attackRank+1))<<kingRank;
        //     CheckAttackBoard |= currentFile;
        // } else {
        //     ulong currentRank = Board.rank1 << (8*kingRank);
        //     if(kingPos>attackingPos) currentRank = ;
        //     else ;
        //     CheckAttackBoard |= currentFile;
        // }
    }

    public ulong FilterForLegalMoves(ulong moveBoard, int pos, Piece type, ulong enemyAttacking) {
        if (
            (Pinned & (1ul << pos)) != 0
        // Okay well, if this piece can take whose pinning it, then we shouldn't
        // So this is incomplete
        ) {
            return 0ul;
        }
        if (type == Piece.WKing || type == Piece.BKing)
        {
            if (KingAttackBoard > 0)
            {
                return (moveBoard & ~(enemyAttacking) & KingAttackBoard);
            }
            return (moveBoard & ~(enemyAttacking));
        }
        
        if(Checkers.Count > 1) {
            // More than two pieces checking the king means that only the king can move out of check
            if(type != Piece.WKing || type != Piece.BKing)
                return 0;
            // We need to return here with either of two things
            // The king moved onto one of the checkers which is not protected
            // The king moved off of the checkers board
            // Rn I'm just doing moving off the CheckAttackBoard
            return moveBoard & ~CheckAttackBoard;
        }

        if( CheckAttackBoard == 0 ) {
            return moveBoard;
        }
        // You can only move to block the check, if in check
        return moveBoard & CheckAttackBoard;
    }
}