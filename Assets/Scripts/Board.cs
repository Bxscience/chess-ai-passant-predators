using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    // 0-5 is White
    // 6-11 is Black
    public ulong[] boards = new ulong[12];

    public enum Piece {
        WPawn = 0, WBishop = 1, WKnight = 2, WRook = 3, WQueen = 4, WKing = 5,

        BPawn = 6, BBishop = 7, BKnight = 8, BRook = 9, BQueen = 10, BKing = 11
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
                boards[(int)p] |= 1ul<<(idx);
                idx++;
            } else {
                if(Char.IsNumber(c)) {
                    idx+=((int)c)-48;
                }
            }
        }
    }

    public ulong WhitePieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5];
    public ulong BlackPieces => boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];
    public ulong Pieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5] | boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];

    public Vector3 IdxToPos(int x, int y) {
        float min = -19.25f;
        float max = 19.25f;
        float x_pos = (x) * (max - min) / (7) + min;
        float y_pos = (y) * (max - min) / (7) + min;
        return new Vector3(x_pos, 2, y_pos);
    }
}
