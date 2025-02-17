using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Piece {
    WPawn = 0, WBishop = 1, WKnight = 2, WRook = 3, WQueen = 4, WKing = 5,

    BPawn = 6, BBishop = 7, BKnight = 8, BRook = 9, BQueen = 10, BKing = 11,
    None
}

public struct Board
{
    // 0-5 is White
    // 6-11 is Black
    public ulong[] boards;
    
    const ulong fileA = 0x0101010101010101;
    const ulong fileB = 0x0202020202020202;
    const ulong fileC = 0x0404040404040404;
    const ulong fileD = 0x0808080808080808;
    const ulong fileE = 0x1010101010101010;
    const ulong fileF = 0x2020202020202020;
    const ulong fileG = 0x4040404040404040;
    const ulong fileH = 0x8080808080808080;
    const ulong rank2 = 0x00FF000000000000;
    const ulong rank7 = 0x000000000000FF00;

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
        boards = new ulong[12];  
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
            boards[(int)ply.Captured] = ~(~boards[(int)ply.Captured] | 1ul<<end_idx);
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
            // This sets the captured piece's index to 1
            boards[(int)ply.Captured] = boards[(int)ply.Captured] | 1ul<<end_idx;
        }
    }

    public static Vector3 IdxToPos(int x, int y) {
        float min = -19.25f;
        float max = 19.25f;
        float x_pos = (x) * (max - min) / (7) + min;
        float y_pos = (y) * (max - min) / (7) + min;
        return new Vector3(x_pos, 2, y_pos);
    }

    public static Vector3 IdxToPos(Vector2Int idx) {
        return IdxToPos(idx.x, idx.y);
    }
    
    // These moves are paralegal
    public ulong KnightAttacks(ulong pos, Side side, bool _) { 
        // Explained well in the ameye.dev link Ms. Qiu gave us
        // We are just bitshifting to the correct position
        // The attacks a knight can do on the left side can never be in file G or H, otherwise it'd be a wrap around
        // Same with the attacks on the right.
        ulong attacks = (((pos>>6)|(pos<<10)) & ~(fileG|fileH))
        | (((pos>>10)|(pos<<6)) & ~(fileA|fileB))
        | (((pos>>15)|(pos<<17))&~fileH)
        | (((pos>>17)|(pos<<15))&~fileA);
        return attacks & ~(side == Side.White ? WhitePieces : BlackPieces);
    }
    
    // These moves are paralegal
    public ulong PawnAttacks(ulong pos, Side side, bool moved) { 
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong theOps = side != Side.White ? WhitePieces : BlackPieces;

        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacks = pos >> 8
            | (pos>>7 & theOps & ~fileA)
            | (pos>>9 & theOps & ~fileH)
            | (moved ? 0 : pos>>16);

        return attacks & ~sameSide;
    }

    // These moves are paralegal
    public ulong KingAttacks(ulong pos, Side side, bool moved) { 
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        ulong theOps = side != Side.White ? WhitePieces : BlackPieces;
        

        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacks = pos >> 7
            | pos >> 8
            | pos >> 9
            // ▲ up the board
            // ▼ Down the board
            | pos << 9
            | pos << 8
            | pos << 7
            // ▼ Left right movement
            | pos >> 1
            | pos << 1;
        
        if(moved)
            return attacks & ~sameSide;
        
        // These are the squares that the king would move to castle
        const ulong castle_spots = 0x4400000000000044;
        // The king must slide through these lines
        const ulong castle_line = 0xC6000000000000C6;
        
        // Idk how to implement castling well

        return attacks & ~sameSide;
    }
}
