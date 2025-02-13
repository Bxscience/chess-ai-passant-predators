using System.Collections;
using System;
using System.Collections.Generic;

public class Board
{
    // 0-5 is White
    // 6-11 is Black
    public ulong[] boards = new ulong[12];

    public enum Piece {
        WPawn = 0, WBishop = 1, WKnight = 2, WRook = 3, WQueen = 4, WKing = 5,

        BPawn = 0, BBishop = 1, BKnight = 2, BRook = 3, BQueen = 4, BKing = 5
    }

    private Piece FromAlgebraic(char c) => (c) switch {
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
    };

    public Board(string fen) {
        string[] fen_parts = fen.Split(" ");
        int idx = 0;
        for(int i = 0; i < fen_parts[0].Length; i++) {
            char c = fen_parts[0][i];
            if(Char.IsLetter(c)) {
                Piece p = FromAlgebraic(c);
                boards[(int)p] |= 1ul<<(63-idx);
                idx++;
            } else {
                if(Char.IsNumber(c)) {
                    idx+=((int)c)-48;
                }
            }
        }
        // for(int i = 0; i < 12; i++) {
            
        // }
    }
}
