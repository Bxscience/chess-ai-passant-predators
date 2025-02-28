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
            // We have castled
            Debug.Log(ply.End.x - ply.Start.x);
            if(Math.Abs(ply.End.x-ply.Start.x)>=2) {
                // King side test
                Debug.Log(boards[(int)Piece.WRook]);
                boards[(int)Piece.WRook] &=  ~(0x8000000000000000ul);
                Debug.Log(boards[(int)Piece.WRook]);
                boards[(int)Piece.WRook] |=  0x2000000000000000ul;
                Debug.Log(boards[(int)Piece.WRook]);
            }
            if (ply.End.x-ply.Start.x<=-3) {
                // Queen side test
                boards[(int)Piece.WRook] &= ~0x020000000000000ul;                
                boards[(int)Piece.WRook] &= ~0x080000000000000ul;                
            }
            castleTracker &= ~(int)(castleTrack.wKing|castleTrack.wQueen);
        }
        else if (ply.Type == Piece.BKing)
        {
            // We have castled
            if(Math.Abs(ply.End.x-ply.Start.x)>=2) {
                                
            }
            castleTracker &= ~(int)(castleTrack.bKing|castleTrack.bQueen);
        }

        if (ply.Type == Piece.WRook)
        {
            if (ply.Start == new Vector2Int(7, 0)) //king side rook
            {
                castleTracker &= ~(int)castleTrack.wKing;
            }
            else if (ply.Start == new Vector2Int(0, 0)) //queen side rook
            {
                castleTracker &= ~(int)castleTrack.wQueen; 
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
    
    public ulong GetMoveLegal(int square, Piece type, Side side) {
        // For check:
        // Get the moves of every piece
        // | them together (for the other side of course)
        // Check if the king bitboard & all_attack_board != 0
        // King is in check
        //
        // For checkmate:
        // ~all_attack_board & paralegal moves of king == 0
        // Checkmate
        // why? if we unset every point where an attack is from the moves of the king, then the king is stuck if its 0
        // Essentially, is every move of the king within the board of attacks
        return 0;
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
    
    public ulong GetMoveParalegal(ChessPiece piece) => GetMoveParalegal(piece.idx.x + 8*(7-piece.idx.y), piece.type, piece.side);
    
    public ulong GetMoveParalegal(int square, Piece type, Side side) {
        // if idx.y == 7, then its in the first few bits, because we start fropm A8
        ulong pos = 1ul << square;
        return type switch
        {
            // Leaping
            Piece.WPawn or Piece.BPawn => PawnMovesParalegal(pos, side),
            Piece.WKnight or Piece.BKnight => KnightMovesParalegal(pos, side),
            Piece.WKing or Piece.BKing => KingMovesParalegal(pos, side),
            
            // Sliding (not yet implemented)
            Piece.WBishop or Piece.BBishop => BishopMovesParalegal(square, side),
            Piece.WRook or Piece.BRook => RookMovesParalegal(square, side),
            Piece.WQueen or Piece.BQueen => QueenMovesParalegal(square, side),
            
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
    
    public ulong PawnMovesParalegal(ulong pos, Side side) { 
        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacksWhite = (pos >> 8 & ~Pieces)
            | (pos>>7 & BlackPieces & ~fileA)
            | (pos>>9 & BlackPieces & ~fileH) | (pos>>7 & (passantTrack) & ~fileA) | (pos>>9 & passantTrack & ~fileH)
            | (( (pos & Board.rank2) == 0 || (pos>>8 & Pieces) != 0) ? 0 : pos>>16);
        
        ulong attacksBlack = (pos << 8 & ~Pieces)
            | (pos<<9 & WhitePieces & ~fileA)
            | (pos<<7 & WhitePieces & ~fileH) | (pos << 9 & (passantTrack) & ~fileA) | (pos << 7 & passantTrack & ~fileH)
            | (( (pos & Board.rank7) == 0 || ((pos<<8 & Pieces) != 0)) ? 0 : pos<<16);

        if(side == Side.White) 
            return attacksWhite & ~WhitePieces;
        else
            return attacksBlack & ~BlackPieces;
    }


    public ulong KingMovesParalegal(ulong pos, Side side) { 
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong theOpps = side != Side.White ? WhitePieces : BlackPieces;
        ulong wkSlide = 0x6000000000000000;
        ulong bkSlide = 0x0000000000000060;
        ulong wqSlide = 0x0E00000000000000;
        ulong bqSlide = 0x000000000000000E;
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
            if (((castleTracker & (int)castleTrack.wKing) > 0) & ((WhitePieces & wkSlide) == 0)) //need to check if the row is clear      & (WhitePieces & wkSlide > 0
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)castleTrack.wQueen) > 0 & ((WhitePieces & wqSlide) == 0))//need to check if the row is clear
            {
                attacks = attacks | (pos >> 2);
            }

        }
        else
        {
            if ((castleTracker & (int)castleTrack.bKing) > 0 & ((BlackPieces & bkSlide) == 0))
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)castleTrack.bQueen) > 0 & ((BlackPieces & bqSlide) == 0))
            {
                attacks = attacks | (pos >> 2);
            }
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
