using System;
using System.Collections.Generic;
using UnityEngine;

// This is per-side
public struct MovesHelper {
    public List<ulong> PinBoards;
    public List<Ply> Plies;
    // Maybe we use this?
    // It'd contain legal moves for a side.
    // public ulong[] LegalMoves;
    public ulong Pinned;
    // If this is 0, we are not in check
    // Otherwise we are in check
    public ulong CheckAttackBoard;
    public ulong KingAttackBoard;
    public ulong KingFleeBoard;
    public sbyte NumCheckers;

    public MovesHelper(Side _side) {
        Plies = new List<Ply>(216);
        NumCheckers = 0;
        PinBoards = new List<ulong>(8);
        Pinned = 0ul;
        KingAttackBoard = 0ul;
        CheckAttackBoard = 0ul;
        KingFleeBoard = 0ul;
        // LegalMoves = new ulong[64];
        // this.side = side;
    }

    public void ClearMoves() {
        NumCheckers = 0;
        PinBoards.Clear();
        Plies.Clear();
        Pinned = 0ul;
        CheckAttackBoard = 0ul;
        KingFleeBoard = 0ul;
    }

    public void AddCheckAttack(Piece typeAttacking, int attackingPos, int kingPos, ulong allPieces) {
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
                NumCheckers++;
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
                KingFleeBoard |= FindBishopCheckAttack(attackingPos, kingPos, allPieces);
                CheckAttackBoard |= FindBishopPinRay(attackingPos, kingPos) | (1ul<<kingPos);
                KingAttackBoard |= 1ul << attackingPos;

                NumCheckers++;
                break;
            case Piece.BRook:
            case Piece.WRook:
                KingFleeBoard |= FindRookCheckAttack(attackingPos, kingPos, allPieces);
                CheckAttackBoard |= FindRookPinRay(attackingPos, kingPos) | (1ul<<kingPos);
                KingAttackBoard |= 1ul << attackingPos;

                NumCheckers++;
                break;
            case Piece.BQueen:
            case Piece.WQueen:
                KingAttackBoard |= 1ul << attackingPos;
                if ((kingPos % 8 == attackingPos % 8) || (kingPos / 8 == attackingPos / 8)) {
                    KingFleeBoard |= FindRookCheckAttack(attackingPos, kingPos, allPieces);
                    CheckAttackBoard |= FindRookPinRay(attackingPos, kingPos) | (1ul<<kingPos);
                }
                else {
                    KingFleeBoard |= FindBishopCheckAttack(attackingPos, kingPos, allPieces);
                    CheckAttackBoard |= FindBishopPinRay(attackingPos, kingPos) | (1ul<<kingPos);
                }

                NumCheckers++;
                break;

        } 
    }

    public ulong FindRookCheckAttack(int attackingPos, int kingPos, ulong allPieces) {
        ulong ray = 0;
        // If not equal, then it should be different rows?
        int increment = ((kingPos / 8 != attackingPos / 8) ? 8 : 1) * Math.Sign(kingPos - attackingPos);
        ulong allPiecesNoAttacker = allPieces & ~(1ul<<attackingPos) & ~(1ul<<kingPos);
        // Stop when you hit a piece or the edge
        for (int i = attackingPos; (allPiecesNoAttacker & (1ul<<i)) == 0 && i%8 >= 0 && i%8 <= 7 && i/8 >= 0 && i/8<=7; i += increment) {
            ray |= 1ul << i;
        }
        ray |= 1ul << kingPos;
        return ray;
    }

    public ulong FindBishopCheckAttack(int attackingPos, int kingPos, ulong allPieces) {
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

        ulong allPiecesNoAttacker = allPieces & ~(1ul<<attackingPos) & ~(1ul<<kingPos);
        for (int i = attackingPos; (allPiecesNoAttacker & (1ul<<i)) == 0 && i%8 >= 0 && i%8 <= 7 && i/8 >= 0 && i/8<=7; i += increment)
            ray |= 1ul << i;
        ray |= 1ul << kingPos;
        return ray;
    }

    public ulong FindBishopPinRay(int attackingPos, int kingPos) {
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

    public ulong FindRookPinRay(int attackingPos, int kingPos) {
        ulong ray = 0;
        // If not equal, then it should be different rows?
        int increment = ((kingPos / 8 != attackingPos / 8) ? 8 : 1) * Math.Sign(kingPos - attackingPos);
        for (int i = attackingPos; i != kingPos; i += increment)
            ray |= 1ul << i;
        return ray;
    }

    public ulong FilterForLegalMoves(ulong moveBoard, int pos, Piece type, ulong enemyAttacking, int castleTracker) {
        if (
            (Pinned & (1ul << pos)) != 0
        ) {
            for(int i = 0; i < PinBoards.Count; i++) {
                if((PinBoards[i] & (1ul<<pos)) > 0) {
                    return moveBoard & PinBoards[i];
                }
            }
        }
        if (type == Piece.WKing || type == Piece.BKing)
        {
            ulong removeCastleFromKing = 0ul;
            const ulong wkSlide = 0x6000000000000000;
            const ulong bkSlide = 0x0000000000000060;
            const ulong wqSlide = 0x0C00000000000000;
            const ulong bqSlide = 0x000000000000000C;
            if(NumCheckers>0 || ((castleTracker & (int)CastleTrack.wKing) > 0
                && (wkSlide & enemyAttacking) > 0)
                || NumCheckers>0
            ) {
                removeCastleFromKing |= 1ul<<62;
            }
            if(NumCheckers>0 ||((castleTracker & (int)CastleTrack.wQueen) > 0
                && (wqSlide & enemyAttacking) > 0)
            ) {
                removeCastleFromKing |= 1ul<<58;
            }
            if(NumCheckers>0 || ((castleTracker & (int)CastleTrack.bKing) > 0
                && (bkSlide & enemyAttacking) > 0)
            ) {
                removeCastleFromKing |= 1ul<<6;
            }
            if(NumCheckers>0 || ((castleTracker & (int)CastleTrack.bQueen) > 0
                && (bqSlide & enemyAttacking) > 0)
            ) {
                removeCastleFromKing |= 1ul<<2;
            }
            return moveBoard & ~enemyAttacking & ~removeCastleFromKing & ~(KingFleeBoard&~KingAttackBoard);
        } 
        if(NumCheckers > 1) {
            // More than two pieces checking the king means that only the king can move out of check
            if(type != Piece.WKing || type != Piece.BKing)
                return 0;
            // We need to return here with either of two things
            // The king moved onto one of the checkers which is not protected
            // The king moved off of the checkers board
            // Rn I'm just doing moving off the CheckAttackBoard
            // This might not run
            return moveBoard & ~KingFleeBoard;
        }

        if( CheckAttackBoard == 0 ) {
            return moveBoard;
        }
        // You can only move to block the check, if in check
        return moveBoard & CheckAttackBoard;
    }
}