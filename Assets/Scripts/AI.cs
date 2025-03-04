using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    // Start is called before the first frame update
  
    public int[] mg_knight_table = {
    -167, -89, -34, -49,  61, -97, -15, -107,
     -73, -41,  72,  36,  23,  62,   7,  -17,
     -47,  60,  37,  65,  84, 129,  73,   44,
      -9,  17,  19,  53,  37,  69,  18,   22,
     -13,   4,  16,  13,  28,  19,  21,   -8,
     -23,  -9,  12,  10,  19,  17,  25,  -16,
     -29, -53, -12,  -3,  -1,  18, -14,  -19,
    -105, -21, -58, -33, -17, -28, -19,  -23,
}; public int[] mg_king_table = {
    -65,  23,  16, -15, -56, -34,   2,  13,
     29,  -1, -20,  -7,  -8,  -4, -38, -29,
     -9,  24,   2, -16, -20,   6,  22, -22,
    -17, -20, -12, -27, -30, -25, -14, -36,
    -49,  -1, -27, -39, -46, -44, -33, -51,
    -14, -14, -22, -46, -44, -30, -15, -27,
      1,   7,  -8, -64, -43, -16,   9,   8,
    -15,  36,  12, -54,   8, -28,  24,  14,
};

    public int[] mg_pawn_table = {
      0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0,
};

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int evaluate(Side side, Board board)
    {
        ulong[] boards = board.boards;
        int score = 0;
        int queen = 900, knight = 300, bishop = 310, pawn = 100, rook = 500, king = 2000;
        if (side == Side.White)
        {
            for (int i = 0; i <= (int)Piece.WKing; i++) //pawns, bishop, knight, rook, q, k
            {
                ulong curboard = boards[i];
                while (curboard > 0) 
                {
                    int pos = Board.GetLSBIndex(curboard);
                    ulong moveBoard = board.GetMoveParalegal(pos, (Piece)i, Side.White);
                    board.allWhiteMovesPsuedolegal |= moveBoard;

                   
                    // Check pins for bpieces
                    if (i == (int)Piece.WBishop)
                    {
                        score += bishop;
                        if (countPieces(curboard) == 2)
                        {
                            score += 25;
                        }
                        
                    }
                    else if (i == (int)Piece.WKnight)
                    {
                        score += knight;
                        score += mg_knight_table[pos];
                    }
                    else if (i == (int)Piece.WRook)
                    {
                        score += rook;
                    }
                    else if(i == (int)Piece.WQueen)
                    {
                        score += queen;
                    }
                    else if(i == (int)Piece.WPawn)
                    {

                        score += pawn;
                        score += mg_pawn_table[pos];
                    }
                    else
                    {
                        score += king;
                        score += mg_king_table[pos];
                    }
                    curboard &= ~(1ul << pos);
                }
            }
        }
        else
        {

        }
        return score;
    }
    public int countPieces(ulong pieces)
    {
        int count = 0;
        while (pieces != 0)
        {
            // Count the rightmost set bit and remove it
            count++;
            pieces &= pieces - 1; // This removes the rightmost 1 bit
        }
        return count;
    }
}



