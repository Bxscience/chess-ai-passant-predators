using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI
{
    Ply? bestPly;
  
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
    public int[] mg_bishop_table = {
    -29,   4, -82, -37, -25, -42,   7,  -8,
    -26,  16, -18, -13,  30,  59,  18, -47,
    -16,  37,  43,  40,  35,  50,  37,  -2,
     -4,   5,  19,  50,  37,  37,   7,  -2,
     -6,  13,  13,  26,  34,  12,  10,   4,
      0,  15,  15,  15,  14,  27,  18,  10,
      4,  15,  16,   0,   7,  21,  33,   1,
    -33,  -3, -14, -21, -13, -12, -39, -21,
};
    public int[] mg_rook_table = {
     32,  42,  32,  51, 63,  9,  31,  43,
     27,  32,  58,  62, 80, 67,  26,  44,
     -5,  19,  26,  36, 17, 45,  61,  16,
    -24, -11,   7,  26, 24, 35,  -8, -20,
    -36, -26, -12,  -1,  9, -7,   6, -23,
    -45, -25, -16, -17,  3,  0,  -5, -33,
    -44, -16, -20,  -9, -1, 11,  -6, -71,
    -19, -13,   1,  17, 16,  7, -37, -26,
};
    public int[] mg_queen_table = {
    -28,   0,  29,  12,  59,  44,  43,  45,
    -24, -39,  -5,   1, -16,  57,  28,  54,
    -13, -17,   7,   8,  29,  56,  47,  57,
    -27, -27, -16, -16,  -1,  17,  -2,   1,
     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
    -14,   2, -11,  -2,  -5,   2,  14,   5,
    -35,  -8,  11,   2,   8,  15,  -3,   1,
     -1, -18,  -9,  10, -15, -25, -31, -50,
};
    const int queen = 900, knight = 300, bishop = 310, pawn = 100, rook = 500, king = 2000;
    public int GetPieceScore(Piece type) => type switch
    {
        Piece.WPawn or Piece.BPawn => pawn,
        Piece.WBishop or Piece.BBishop => bishop,
        Piece.WKnight or Piece.BKnight => knight,
        Piece.WRook or Piece.BRook => rook,
        Piece.WQueen or Piece.BQueen => queen,
        Piece.WKing or Piece.BKing => king,
        _ => 0
    };

    // Evaluate function uses piece scores to determine who has material and spatial advantages, amoung other things.
    public int evaluate(Side side, Board board)
    {
        ulong[] boards = board.boards;
        //ulong whiteDefended = board.allWhiteMovesPsuedolegal & board.WhitePieces;
        //ulong blackDefended = board.allBlackMovesPsuedolegal & board.BlackPieces;
        //ideally i want o have all fully legal moves but for now I am gonna do paralegal bc idk how exactly
       
        int score = 0;
        int wscore = 0;
        int bscore = 0;
            for (int i = 0; i <= (int)Piece.WKing; i++) //pawns, bishop, knight, rook, q, k
            {
                ulong curboard = boards[i];
                while (curboard > 0) 
                {
                    int pos = Board.GetLSBIndex(curboard);
                    ulong posbit = 1ul << pos;
                   
                    if (i == (int)Piece.WBishop)
                    {
                        wscore += bishop;
                        wscore += mg_bishop_table[pos];
                        if (countPieces(curboard) == 2)
                        {
                            wscore += 25;
                        }
                        
                    }
                    else if (i == (int)Piece.WKnight)
                    {
                        wscore += knight;
                        wscore += mg_knight_table[pos];
                    }
                    else if (i == (int)Piece.WRook)
                    {
                        wscore += rook;
                        wscore += mg_rook_table[pos];
                    }
                    else if(i == (int)Piece.WQueen)
                    {
                        wscore += queen;
                        wscore += mg_queen_table[pos];
                    }
                    else if(i == (int)Piece.WPawn)
                    {

                        wscore += pawn;
                        wscore += mg_pawn_table[pos];
                        if ((posbit <<7 & boards[0]) != 0 || (posbit<<9 & boards[0]) != 0) //if pawn is connected
                        {  
                            wscore += 25;
                        }
                        if ((posbit << 8 & boards[0]) != 0) //basic punishment for doubled pawns
                        {
                            wscore -= 50;
                        }
                    }
                    else if (i == (int)Piece.WKing)
                    {
                        wscore += king;
                        wscore += mg_king_table[pos];
                    }
                
                curboard &= ~(1ul << pos);
                }
            }
        
        
            for (int i = 6; i <= (int)Piece.BKing; i++) //pawns, bishop, knight, rook, q, k
            {
                ulong curboard = boards[i];
                while (curboard > 0)
                {
                    int pos = Board.GetLSBIndex(curboard);
                ulong posbit = 1ul << pos;

                int blackpos = 56 - pos + (pos % 8);
                    if (i == (int)Piece.BBishop)
                    {
                        bscore += bishop;
                        bscore += mg_bishop_table[blackpos];
                        if (countPieces(curboard) == 2)
                        {
                            bscore += 25;
                        }

                    }
                    else if (i == (int)Piece.BKnight)
                    {
                        bscore += knight;
                        bscore += mg_knight_table[blackpos];
                    }
                    else if (i == (int)Piece.BRook)
                    {
                        bscore += rook;
                        bscore += mg_rook_table[blackpos];
                    }
                    else if (i == (int)Piece.BQueen)
                    {
                        bscore += queen;
                        bscore += mg_queen_table[blackpos];
                    }
                    else if (i == (int)Piece.BPawn)
                    {

                        bscore += pawn;
                        bscore += mg_pawn_table[blackpos];
                        if ((posbit >> 7 & boards[6]) != 0 || (posbit >> 9 & boards[6]) != 0) //if pawn is connected
                        {
                            bscore += 25;
                        }
                       if ((posbit >> 8 & boards[6]) != 0) //basic punishment for doubled pawns
                        {
                        bscore -= 50;
                        }
                  
                    }
                    else if (i == (int)Piece.BKing)
                    {
                        bscore += king;
                        bscore += mg_king_table[blackpos];
                    }
                    curboard &= ~(1ul << pos);
                }
            }
        if (side == Side.White)
        {
            score = wscore - bscore;
        }
        else
        {
            score = bscore - wscore;
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

    public Ply? GetPly(Side side) {
        bestPly = null;
        NegaMax(side, 4, BoardManager.instance.board, -10000, 10000);
        return bestPly;
    }


    // This nega max mostly works.
    // Essentially, we test every move.
    // The beauty is that if we negate the opposite sides evaluation, and we find the max of those negated scores, we are finding the minimum of the non negated scores
    // That means that this is equivalent to minimax
    // Board b is pass by value (well technically everything is but I mean that Board b is not a pointer), so the values other than the lists/arrays get copied.
    public int NegaMax(Side side, int depth, Board b, int alpha, int beta, bool canSet = true) {
        if( depth == 0 ) 
            return evaluate(side, b);
        int max = -10000;
        List<Ply> plies = new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies);
        if(plies.Count == 0)
            return evaluate(side, b);
        foreach(Ply ply in plies) {
            Board newB = b.DeepCopy();
            Ply newPly = ply;
            if(ply.Type == Piece.WPawn && ply.End.y == 7)
                newPly.PromoteType = Piece.WQueen;
            if(ply.Type == Piece.BPawn && ply.End.y == 0)
                newPly.PromoteType = Piece.BQueen;
            newB.PlayPly(newPly);
            int score = -NegaMax(side == Side.White ? Side.Black : Side.White, depth-1, newB, -beta, -alpha, false);
            if(score > max) {
                if(canSet)
                    bestPly = newPly;
                max = score;
                alpha = Mathf.Max(alpha, score);
            }
            if (score >= beta)
                break;
        }
        return max;
    }    
    // public int materialCount(Board b) {
    //     int wCount = 0;
    //     for(int i = (int)Piece.WPawn; i <= (int)Piece.WKing; i++) {
    //         wCount+=countPieces(b.boards[i])*GetPieceScore((Piece)i);
    //     }
    //     int bCount = 0;
    //     for(int i = (int)Piece.BPawn; i <= (int)Piece.BKing; i++) {
    //         bCount+=countPieces(b.boards[i])*GetPieceScore((Piece)i);
    //     }
    //     return wCount - bCount;
    // }

}



