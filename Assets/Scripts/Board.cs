using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Drawing;

public enum Piece {
    WPawn = 0, WBishop = 1, WKnight = 2, WRook = 3, WQueen = 4, WKing = 5,

    BPawn = 6, BBishop = 7, BKnight = 8, BRook = 9, BQueen = 10, BKing = 11,
    None
}

[Flags]
public enum castleTrack
{
    wKing = 0b1000,
    wQueen = 0b0100,
    bKing = 0b0010,
    bQueen = 0b0001,
}
public struct Board
{
    // 0-5 is White
    // 6-11 is Black 
    public ulong[] boards;
    public ulong passantTrack;
    public ulong passantCaptured;
    public int castleTracker;
    public const ulong fileA = 0x0101010101010101;
    public const ulong fileB = 0x0202020202020202;
    public const ulong fileC = 0x0404040404040404;
    public const ulong fileD = 0x0808080808080808;
    public const ulong fileE = 0x1010101010101010;
    public const ulong fileF = 0x2020202020202020;
    public const ulong fileG = 0x4040404040404040;
    public const ulong fileH = 0x8080808080808080;
    // Every hex character maps to 4 bits. Two hex digits is 8 bits. Each row is 8 bits
    // If you notice, rank 1 is 0xFF00000000000000, which is a very large number. Since we start counting at rank 8, rank 8 is near the beginning of the number, and rank 1 is at the front.
    public const ulong rank1 = 0xFF00000000000000;
    public const ulong rank2 = 0x00FF000000000000;
    public const ulong rank3 = 0x0000FF0000000000;
    public const ulong rank4 = 0x000000FF00000000;
    public const ulong rank5 = 0x00000000FF000000;
    public const ulong rank6 = 0x0000000000FF0000;
    public const ulong rank7 = 0x000000000000FF00;
    public const ulong rank8 = 0x00000000000000FF;

    public readonly ulong WhitePieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5];
    public readonly ulong BlackPieces => boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];
    public readonly ulong Pieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5] | boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];

    private Piece FromAlgebraic(char c) => c switch
    {
        'R' => Piece.WRook,
        'N' => Piece.WKnight,
        'B' => Piece.WBishop,
        'Q' => Piece.WQueen,
        'K' => Piece.WKing,
        'P' => Piece.WPawn,

        'r' => Piece.BRook,
        'n' => Piece.BKnight,
        'b' => Piece.BBishop,
        'q' => Piece.BQueen,
        'k' => Piece.BKing,
        'p' => Piece.BPawn,
        _ => throw new Exception("Not Valid FEN"),
    };

    public Board(string fen) {
        castleTracker = 0b1111;
        //castleTracker = castleTracker & ~castleTrack.wQueen for example
        boards = new ulong[12];
        passantTrack = 0;
        passantCaptured = 0;
        string[] fen_parts = fen.Split(" ");
        int idx = 0;
        for(int i = 0; i < fen_parts[0].Length; i++) {
            char c = fen_parts[0][i];
            if(char.IsLetter(c)) {
                Piece p = FromAlgebraic(c);
                boards[(int)p] |= 1ul<<idx;
                idx++;
            } else {
                if(char.IsNumber(c)) {
                    idx += c - 48;
                }
            }
        }
    }
    
    public void PlayPly(Ply ply) {
        // the start coordinate, as an offset, starting from A1
        // If Start.y is 7, that should correlate with the 8th rank.   
        int start_idx = ply.Start.x + 8*(7-ply.Start.y); 
        int end_idx = ply.End.x + 8*(7-ply.End.y); 
        
        // We not the board so that every point without a piece has a one, and every point with a piece has a 0
        // Then, we can | the notted board with 1 where the piece has moved from, and not it again to make it a 0
        boards[(int)ply.Type] = ~(~boards[(int)ply.Type] | 1ul<<start_idx);
        // This is simpler, just make the bit it moved to a 1
        boards[(int)ply.Type] = boards[(int)ply.Type] | 1ul<<end_idx;
        
        if(ply.Captured != Piece.None) {
            if((1ul<<end_idx & passantTrack) != 0) {
                boards[(int)ply.Captured] = boards[(int)ply.Captured] & ~passantCaptured;
            } else {
                boards[(int)ply.Captured] = ~(~boards[(int)ply.Captured] | 1ul<<end_idx);
            }
        }

        passantTrack = 0;
        passantCaptured = 0;
    
        //king move -> no more castle on that color
        if (ply.Type == Piece.WKing)
        {
            castleTracker = castleTracker & ~(int)(castleTrack.wKing) & ~(int)(castleTrack.wQueen);
        }
        else if (ply.Type == Piece.BKing)
        {
            castleTracker = castleTracker & ~(int)(castleTrack.bKing) & ~(int)(castleTrack.bQueen);
        }

        if (ply.Type == Piece.WRook)
        {
            if (ply.Start == new Vector2Int(7, 0)) //king side rook
            {
                castleTracker = castleTracker & ~(int)(castleTrack.wKing);
            }
            else if (ply.Start == new Vector2Int(0, 0)) //queen side rook
            {
                castleTracker = castleTracker & ~(int)(castleTrack.wQueen); 
            }
        }
        else if (ply.Type == Piece.BRook)
        {
            if (ply.Start == new Vector2Int(7, 7)) 
            {
                castleTracker = castleTracker & ~(int)(castleTrack.bKing);
            }
            else if (ply.Start == new Vector2Int(0, 7))
            {
                castleTracker = castleTracker & ~(int)(castleTrack.bQueen);
            }
        }


        if (ply.Type == Piece.WPawn && ((ply.End-ply.Start) == new Vector2Int(0,2)))
        {
            passantTrack = 1ul << end_idx << 8;
            passantCaptured = 1ul << end_idx;
        }
        if (ply.Type == Piece.BPawn && ((ply.End - ply.Start) == new Vector2Int(0, -2)))
        {
            passantTrack = 1ul << end_idx >> 8;
            passantCaptured = 1ul << end_idx;
        }

        

        if (ply.Type == Piece.WPawn && ( (1ul<<end_idx) & rank8 ) != 0) {
            Promote(1ul<<end_idx, Side.White, (Piece)ply.PromoteType);
        }
        else if(ply.Type == Piece.BPawn && ( (1ul<<end_idx) & rank1) != 0) {
            Promote(1ul<<end_idx, Side.Black, (Piece)ply.PromoteType);
        }
        
    }
    
    public bool IsEnPassant(Vector2Int endPos) {
        int sq = endPos.x + (7-endPos.y)*8;
        return (1ul<<sq & passantTrack) != 0;
    }

    public void Promote(ulong pos, Side side, Piece promoteType)
    {
        if (side== Side.White) {
            boards[(int)Piece.WPawn] = boards[(int)Piece.WPawn] & ~pos;
            boards[(int)promoteType] |= pos;
        }
        if (side == Side.Black) {
            boards[(int)Piece.BPawn] = boards[(int)Piece.BPawn] & ~pos;
            boards[(int)promoteType] |= pos;
        }
    }
    public void UndoPly(Ply ply) {
        // the start coordinate, as an offset, starting from A1
        // If Start.y is 7, that should correlate with the 8th rank.   
        int start_idx = ply.Start.x + 8*(7-ply.Start.y); 
        int end_idx = ply.End.x + 8*(7-ply.End.y); 
        
        // This sets the end pos to 0
        boards[(int)ply.Type] = ~(~boards[(int)ply.Type] | 1ul<<end_idx);
        // This sets the start pos to 1
        boards[(int)ply.Type] = boards[(int)ply.Type] | 1ul<<start_idx;
        
        if(ply.Captured != Piece.None) {
            if((1ul<<end_idx & passantTrack) != 0) {
                boards[(int)ply.Captured] = boards[(int)ply.Captured] | passantCaptured;
            } else {
                boards[(int)ply.Captured] = boards[(int)ply.Captured] | 1ul<<end_idx;
            }
        }
    }

    public static Vector3 IdxToPos(int x, int y) {
        float min = -19.25f;
        float max = 19.25f;
        float x_pos = x * (max - min) / 7 + min;
        float y_pos = y * (max - min) / 7 + min;
        return new Vector3(x_pos, 2, y_pos);
    }

    public static Vector3 IdxToPos(Vector2Int idx) {
        return IdxToPos(idx.x, idx.y);
    }
    
    public ulong GetMoveParalegal(ChessPiece piece) {
        // if idx.y == 7, then its in the first few bits, because we start fropm A8
        ulong pos = 1ul << (piece.idx.x + (7-piece.idx.y)*8);
        return piece.type switch
        {
            // Leaping
            Piece.WPawn or Piece.BPawn => PawnMovesParalegal(pos, piece.side, piece.moved),
            Piece.WKnight or Piece.BKnight => KnightMovesParalegal(pos, piece.side),
            Piece.WKing or Piece.BKing => KingMovesParalegal(pos, piece.side, piece.moved),
            
            // Sliding (not yet implemented)
            Piece.WBishop or Piece.BBishop => BishopMovesParalegal(piece.idx.x + (7-piece.idx.y)*8, piece.side),
            Piece.WRook or Piece.BRook => RookMovesParalegal(piece.idx.x + (7-piece.idx.y)*8, piece.side),
            Piece.WQueen or Piece.BQueen => QueenMovesParalegal(piece.idx.x + (7-piece.idx.y)*8, piece.side),
            
            // Can't move nothing
            _ => 0ul,
        };
    }

    public ulong KnightMovesParalegal(ulong pos, Side side) { 
        // Explained well in the ameye.dev link Ms. Qiu gave us
        // We are just bitshifting to the correct position
        // The attacks a knight can do on the left side can never be in file G or H, otherwise it'd be a wrap around
        // Same with the attacks on the right.
        ulong attacks = (((pos>>6)|(pos<<10)) & ~(fileA|fileB))
        | (((pos>>10)|(pos<<6)) & ~(fileG|fileH))
        | (((pos>>15)|(pos<<17) ) & ~fileA)
        | (((pos>>17)|(pos<<15)) & ~fileH);
        return attacks & ~(side == Side.White ? WhitePieces : BlackPieces);
    }
    
    public ulong PawnMovesParalegal(ulong pos, Side side, bool moved) { 
        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacksWhite = (pos >> 8 & ~Pieces)
            | (pos>>7 & BlackPieces & ~fileA)
            | (pos>>9 & BlackPieces & ~fileH) | (pos>>7 & (passantTrack) & ~fileA) | (pos>>9 & passantTrack & ~fileH)
            | ((moved || (pos>>8 & Pieces) != 0) ? 0 : pos>>16);
        
        ulong attacksBlack = (pos << 8 & ~Pieces)
            | (pos<<9 & WhitePieces & ~fileA)
            | (pos<<7 & WhitePieces & ~fileH) | (pos << 9 & (passantTrack) & ~fileA) | (pos << 7 & passantTrack & ~fileH)
            | ((moved || ((pos<<8 & Pieces) != 0)) ? 0 : pos<<16);

        if(side == Side.White) 
            return attacksWhite & ~WhitePieces;
        else
            return attacksBlack & ~BlackPieces;
    }


    public ulong KingMovesParalegal(ulong pos, Side side, bool moved) { 
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong theOpps = side != Side.White ? WhitePieces : BlackPieces;
        ulong wkSlide = 6 << 60;
        ulong bkSlide = 6 << 4;
        //do the queen slides, 01110 instead of 0110 slid by king position or wtv
        

        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacks = ( pos >> 7 & ~fileA )
            | pos >> 8
            | ( pos >> 9 & ~fileH )
            // ▲ up the board
            // ▼ Down the board
            | ( pos << 9 & ~fileA )
            | pos << 8
            | ( pos << 7 & ~fileH )
            // ▼ Left right movement
            | ( pos >> 1 & ~fileH )
            | ( pos << 1 & ~fileA );
        if (side == Side.White)
        {
            if ((castleTracker & (int)castleTrack.wKing) > 0 ) //need to check if the row is clear
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)castleTrack.wQueen) > 0) //need to check if the row is clear
            {
                attacks = attacks | (pos >> 3);
            }
        }
        else
        {
            if ((castleTracker & (int)castleTrack.bKing) > 0) //need to check if the row is clear
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)castleTrack.bQueen) > 0) //need to check if the row is clear
            {
                attacks = attacks | (pos >> 3);
            }
        }
        if (moved)
        {
           
            return attacks & ~sameSide;

        }
        
        // These are the squares that the king would move to castle
        // const ulong castleSpots = 0x4400000000000044;
        
        // The king must slide through these lines
        // const ulong castleLine = 0xC6000000000000C6;
        
        // Idk how to implement castling well

        return attacks & ~sameSide;
    }

    public ulong RookMovesParalegal(int pos, Side side) {
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong moves = 0ul;
        moves = MagicBitboards.RookMagics[pos].GetMove(Pieces);

        return moves & ~sameSide;
    }

    public ulong BishopMovesParalegal(int pos, Side side) {
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong moves = MagicBitboards.BishopMagics[pos].GetMove(Pieces);

        return moves & ~sameSide;
    }

    public ulong QueenMovesParalegal(int pos, Side side) {
        ulong rookMoves = RookMovesParalegal(pos, side);
        ulong bishopMoves = BishopMovesParalegal(pos, side);
        return rookMoves | bishopMoves;
    }
}
