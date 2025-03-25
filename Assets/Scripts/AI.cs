using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI
{
    Ply? bestPly;

    private int difficultyDepth = 3; // Default depth for medium difficulty

    ZobristMap tTable;
    
    public AI() {
        tTable = new ZobristMap();
        // Debug.Log(Marshal.SizeOf<TTEntry>()*tTable.TranspositionTable.Length/1024/1024 + "mb");
    }

    // Start is called before the first frame update
     static public int[] mg_knight_table = {
    -167, -89, -34, -49,  61, -97, -15, -107,
     -73, -41,  72,  36,  23,  62,   7,  -17,
     -47,  60,  37,  65,  84, 129,  73,   44,
      -9,  17,  19,  53,  37,  69,  18,   22,
     -13,   4,  16,  13,  28,  19,  21,   -8,
     -23,  -9,  12,  10,  19,  17,  25,  -16,
     -29, -53, -12,  -3,  -1,  18, -14,  -19,
    -105, -21, -58, -33, -17, -28, -19,  -23,
}; static public int[] mg_king_table = {
    -65,  23,  16, -15, -56, -34,   2,  13,
     29,  -1, -20,  -7,  -8,  -4, -38, -29,
     -9,  24,   2, -16, -20,   6,  22, -22,
    -17, -20, -12, -27, -30, -25, -14, -36,
    -49,  -1, -27, -39, -46, -44, -33, -51,
    -14, -14, -22, -46, -44, -30, -15, -27,
      1,   7,  -8, -64, -43, -16,   9,   8,
    -15,  36,  12, -54,   8, -28,  24,  14,
};

    static public int[] mg_pawn_table = {
      0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0,
};
    static public int[] mg_bishop_table = {
    -29,   4, -82, -37, -25, -42,   7,  -8,
    -26,  16, -18, -13,  30,  59,  18, -47,
    -16,  37,  43,  40,  35,  50,  37,  -2,
     -4,   5,  19,  50,  37,  37,   7,  -2,
     -6,  13,  13,  26,  34,  12,  10,   4,
      0,  15,  15,  15,  14,  27,  18,  10,
      4,  15,  16,   0,   7,  21,  33,   1,
    -33,  -3, -14, -21, -13, -12, -39, -21,
};
    static public int[] mg_rook_table = {
     32,  42,  32,  51, 63,  9,  31,  43,
     27,  32,  58,  62, 80, 67,  26,  44,
     -5,  19,  26,  36, 17, 45,  61,  16,
    -24, -11,   7,  26, 24, 35,  -8, -20,
    -36, -26, -12,  -1,  9, -7,   6, -23,
    -45, -25, -16, -17,  3,  0,  -5, -33,
    -44, -16, -20,  -9, -1, 11,  -6, -71,
    -19, -13,   1,  17, 16,  7, -37, -26,
};
    static public int[] mg_queen_table = {
    -28,   0,  29,  12,  59,  44,  43,  45,
    -24, -39,  -5,   1, -16,  57,  28,  54,
    -13, -17,   7,   8,  29,  56,  47,  57,
    -27, -27, -16, -16,  -1,  17,  -2,   1,
     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
    -14,   2, -11,  -2,  -5,   2,  14,   5,
    -35,  -8,  11,   2,   8,  15,  -3,   1,
     -1, -18,  -9,  10, -15, -25, -31, -50,
};
    static int[] eg_pawn_table = {
    0, 0, 0, 0, 0, 0, 0, 0,
    178, 173, 158, 134, 147, 132, 165, 187,
    94, 100, 85, 67, 56, 53, 82, 84,
    32, 24, 13, 5, -2, 4, 17, 17,
    13, 9, -3, -7, -7, -8, 3, -1,
    4, 7, -6, 1, 0, -5, -1, -8,
    13, 8, 8, 10, 13, 0, 2, -7,
    0, 0, 0, 0, 0, 0, 0, 0
};

    static int[] eg_knight_table = {
    -58, -38, -13, -28, -31, -27, -63, -99,
    -25, -8, -25, -2, -9, -25, -24, -52,
    -24, -20, 10, 9, -1, -9, -19, -41,
    -17, 3, 22, 22, 22, 11, 8, -18,
    -18, -6, 16, 25, 16, 17, 4, -18,
    -23, -3, -1, 15, 10, -3, -20, -22,
    -42, -20, -10, -5, -2, -20, -23, -44,
    -29, -51, -23, -15, -22, -18, -50, -64
};

    static int[] eg_bishop_table = {
    -14, -21, -11, -8, -7, -9, -17, -24,
    -8, -4, 7, -12, -3, -13, -4, -14,
    2, -8, 0, -1, -2, 6, 0, 4,
    -3, 9, 12, 9, 14, 10, 3, 2,
    -6, 3, 13, 19, 7, 10, -3, -9,
    -12, -3, 8, 10, 13, 3, -7, -15,
    -14, -18, -7, -1, 4, -9, -15, -27,
    -23, -9, -23, -5, -9, -16, -5, -17
};

    static int[] eg_rook_table = {
    13, 10, 18, 15, 12, 12, 8, 5,
    11, 13, 13, 11, -3, 3, 8, 3,
    7, 7, 7, 5, 4, -3, -5, -3,
    4, 3, 13, 1, 2, 1, -1, 2,
    3, 5, 8, 4, -5, -6, -8, -11,
    -4, 0, -5, -1, -7, -12, -8, -16,
    -6, -6, 0, 2, -9, -9, -11, -3,
    -9, 2, 3, -1, -5, -13, 4, -20
};

    static int[] eg_queen_table = {
    -9, 22, 22, 27, 27, 19, 10, 20,
    -17, 20, 32, 41, 58, 25, 30, 0,
    -20, 6, 9, 49, 47, 35, 19, 9,
    3, 22, 24, 45, 57, 40, 57, 36,
    -18, 28, 19, 47, 31, 34, 39, 23,
    -16, -27, 15, 6, 9, 17, 10, 5,
    -22, -23, -30, -16, -16, -23, -36, -32,
    -33, -28, -22, -43, -5, -32, -20, -41
};

    static int[] eg_king_table = {
    -74, -35, -18, -18, -11, 15, 4, -17,
    -12, 17, 14, 17, 17, 38, 23, 11,
    10, 17, 23, 15, 20, 45, 44, 13,
    -8, 22, 24, 27, 26, 33, 26, 3,
    -18, -4, 21, 24, 27, 23, 9, -11,
    -19, -3, 11, 21, 23, 16, 7, -9,
    -27, -11, 4, 13, 14, 4, -5, -17,
    -53, -34, -21, -11, -28, -14, -24, -43
};

    int[][] mg_pesto_table =
    {
    mg_pawn_table,
    mg_knight_table,
    mg_bishop_table,
    mg_rook_table,
    mg_queen_table,
    mg_king_table
};

    int[][] eg_pesto_table =
    {
    eg_pawn_table,
    eg_knight_table,
    eg_bishop_table,
    eg_rook_table,
    eg_queen_table,
    eg_king_table
};

    const int queen = 900, knight = 300, bishop = 310, pawn = 100, rook = 500, king = 2000;  //4000+2000+1600+1240+1200+1800
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
        int endgamescore = (11840 - materialCount(board)) / 100;
        float endgamefloat = endgamescore / 118;
        endgamefloat *= endgamefloat;
        //Debug.Log("EndScore: " + endgamescore);
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
                    wscore += (int)(mg_bishop_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_bishop_table[pos]);
                    if (countPieces(curboard) == 2)
                    {
                        wscore += 25;
                    }

                }
                else if (i == (int)Piece.WKnight)
                {
                    wscore += knight;
                    wscore += (int)(mg_knight_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_knight_table[pos]);
                }
                else if (i == (int)Piece.WRook)
                {
                    wscore += rook;
                    wscore += (int)(mg_rook_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_rook_table[pos]);
                }
                else if (i == (int)Piece.WQueen)
                {
                    wscore += queen;
                    wscore += (int)(mg_queen_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_queen_table[pos]);
                }
                else if (i == (int)Piece.WPawn)
                {

                    wscore += pawn;
                    wscore += (int)(mg_pawn_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_pawn_table[pos]);
                    if ((posbit << 7 & boards[0]) != 0 || (posbit << 9 & boards[0]) != 0) //if pawn is connected
                    {
                        wscore += 25;
                    }
                    if ((posbit << 8 & boards[0]) != 0) //basic punishment for doubled pawns
                    {
                        wscore -= 50;
                    }
                    // Stealing some Sebastian Lague code for passed pawns
                    ulong pawnFile = Board.fileA << (pos%8);
                    ulong fileMask = pawnFile | Math.Max(0, Board.fileA<<((pos-1)%8)) | Math.Min(7, Board.fileA<<((pos+1)%8)); 
                    ulong finalMask = (ulong.MaxValue << (8* (pos/8+1))) & fileMask;
                    if((board.boards[(int)Piece.BPawn] & finalMask) > 0) wscore += 10;
                }
                else if (i == (int)Piece.WKing)
                {
                    wscore += king;
                    wscore += (int)(mg_king_table[pos] * (1 - endgamefloat) + (endgamefloat) * eg_king_table[pos]);
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
                    bscore += (int)(mg_bishop_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_bishop_table[blackpos]);
                    if (countPieces(curboard) == 2)
                    {
                        bscore += 25;
                    }

                }
                else if (i == (int)Piece.BKnight)
                {
                    bscore += knight;
                    bscore += (int)(mg_knight_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_knight_table[blackpos]);

                }
                else if (i == (int)Piece.BRook)
                {
                    bscore += rook;
                    bscore += (int)(mg_rook_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_rook_table[blackpos]);

                }
                else if (i == (int)Piece.BQueen)
                {
                    bscore += queen;
                    bscore += (int)(mg_queen_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_queen_table[blackpos]);

                }
                else if (i == (int)Piece.BPawn)
                {

                    bscore += pawn;
                    bscore += (int)(mg_pawn_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_pawn_table[blackpos]);

                    if ((posbit >> 7 & boards[6]) != 0 || (posbit >> 9 & boards[6]) != 0) //if pawn is connected
                    {
                        bscore += 25;
                    }
                    if ((posbit >> 8 & boards[6]) != 0) //basic punishment for doubled pawns
                    {
                        bscore -= 50;
                    }
                    
                    // Stealing some Sebastian Lague code for passed pawns
                    ulong pawnFile = Board.fileA << (pos%8);
                    ulong fileMask = pawnFile | Math.Max(0, Board.fileA<<((pos-1)%8)) | Math.Min(7, Board.fileA<<((pos+1)%8)); 
                    ulong finalMask = ~(ulong.MaxValue << (8* (pos/8))) & fileMask;
                    if((board.boards[(int)Piece.WPawn] & finalMask) > 0) wscore += 10;
                }
                else if (i == (int)Piece.BKing)
                {
                    bscore += king;
                    bscore += (int)(mg_king_table[blackpos] * (1 - endgamefloat) + (endgamefloat) * eg_king_table[blackpos]);

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
        if (endgamescore > 50)
        {
            score += forceKingToCornerEval(board, side, endgamescore);
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
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
        // the code that you want to measure comes here
        bestPly = null;
        int endgamescore = (11840 - materialCount(BoardManager.instance.board))/100;
        float endgamefloat = endgamescore / 118;
        endgamefloat *= endgamefloat * endgamefloat;
        NegaMax(side, (int)(difficultyDepth*(1-endgamefloat) + (difficultyDepth+4)*endgamefloat), BoardManager.instance.board, -10000, 10000);
        BoardManager.instance.board.SetupMoves();

        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds);
        // Debug.Log("EndGame Score: " + ((11840 - materialCount(BoardManager.instance.board)) / 100));
        // Debug.Log(forceKingToCornerEval(BoardManager.instance.board, side, (11840 - materialCount(BoardManager.instance.board)) / 100));
        return bestPly;
    }


    // If we negate the opposite sides evaluation, and we find the max of those negated scores, we are finding the minimum of the non negated scores
    // That means that this is equivalent to minimax
    // Board b is pass by value (well technically everything is but I mean that Board b is not a pointer), so the values other than the lists/arrays get copied.
    public int NegaMax(Side side, int depth, Board b, int alpha, int beta , bool canSet = true) {
        ulong zKey = ZobristMap.GetZKey(b.boards, b.castleTracker, b.passantTrack, side == Side.White);
        if (tTable.ProbeTable(zKey, depth, alpha, beta, out int eval, out Ply? prevBestPly) && !canSet) {
            // Will either be a good score, or get pruned
            return eval;
        }

        if (depth == 0)
            return evaluateCaptures(b, side, alpha, beta); //Finishes evaluating until all captures resolved
        int max = -1000000;
        
        // Move order based on if the TTable has a best move for this tree already, given the depth was too low to use
        List<Ply> plies;
        if(prevBestPly != null) {
            plies = orderMoves(new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies), (Ply)prevBestPly);
        } else plies = orderMoves(new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies));
        // plies = orderMoves(new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies));
        // plies = new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies);

        if (plies.Count == 0) {
            return -100000;
        }

        Ply ttPly = new();
        int ttType = ZobristMap.TUPPER; // Fails low, meaning we checked every node without improving alpha

        foreach(Ply ply in plies) {
            b.PlayPly(ply, true);
            int score = -NegaMax(side == Side.White ? Side.Black : Side.White, depth-1, b , -beta, -alpha, false); //b -> newB
            if (score > max) { 
                if (canSet) {
                    bestPly = ply; //ply -> newPly
                }
                ttPly = ply;
                max = score;
                
                if(score > alpha) {
                    alpha = score;
                    ttType = ZobristMap.TEXACT; // exact score, this is an actual score, improved alpha
                }
            }
            b.UndoPly(ply);
            if (score >= beta) {
                ttType = ZobristMap.TLOWER; // failed high, meaning alpha >= beta and we break out.
                break;
            }
        }

        tTable.AddTransposition(zKey, ttPly, max, depth, ttType);
        return max;
    }
    public int forceKingToCornerEval(Board board, Side side, int endGameWeight)
    {
        int oppKing, friendlyKing;
        int eval = 14;
        if (side == Side.White)
        {
            friendlyKing = Board.GetLSBIndex(board.boards[5]);
            oppKing = Board.GetLSBIndex(board.boards[11]);
        }
        else
        {
            friendlyKing = Board.GetLSBIndex(board.boards[11]);
            oppKing = Board.GetLSBIndex(board.boards[5]);
        }
        int oppKingRank = oppKing / 8;
        int oppKingFile = oppKing % 8;
        int oppKingToCenterFile = Math.Max(3 - oppKingFile, oppKingFile - 4);
        int oppKingToCenterRank = Math.Max(3 - oppKingRank, oppKingRank - 4);
        eval += oppKingToCenterFile;
        eval += oppKingToCenterRank;

        int friendlyRank = friendlyKing / 8;
        int friendlyFile = friendlyKing % 8;

        int fileDiff = Math.Abs(oppKingToCenterFile - friendlyFile);
        int rankDiff = Math.Abs(oppKingToCenterRank - friendlyRank);
        int dist = fileDiff + rankDiff;

        eval -= dist;
        return (eval * endGameWeight / 50);
    }
    public int materialCount(Board b)
    {
        int count = 0;
        for (int i = (int)Piece.WPawn; i <= (int)Piece.WKing; i++)
        {
            count += countPieces(b.boards[i]) * GetPieceScore((Piece)i);
        }
        for (int i = (int)Piece.BPawn; i <= (int)Piece.BKing; i++)
        {
            count += countPieces(b.boards[i]) * GetPieceScore((Piece)i);
        }
        return count;
    }
    public int evaluateCaptures(Board b, Side side, int alpha, int beta)
    {
            int eval = evaluate(side, b); //eval is stand_pat
      
        int best_val = eval;
            if (eval >= beta)
            {
                return beta;
            }
            if (alpha < eval)
        {
            alpha = eval;
        }
            List<Ply> plies = orderMoves(new List<Ply>((side == Side.White) ? b.WhiteHelper.Plies : b.BlackHelper.Plies).Where(ply => ply.Captured != Piece.None).ToList());
 
            int newEval = eval;
            if (plies.Count == 0)
                {
                    return eval;
                }
            foreach (Ply ply in plies)
            {
                Board newB = b;
                Ply newPly = ply;
                if (ply.Type == Piece.WPawn && ply.End.y == 7)
                    newPly.PromoteType = Piece.WQueen;
                if (ply.Type == Piece.BPawn && ply.End.y == 0)
                    newPly.PromoteType = Piece.BQueen;
                newB.BlackHelper.PinBoards = new List<ulong>(newB.BlackHelper.PinBoards);
                newB.WhiteHelper.PinBoards = new List<ulong>(newB.WhiteHelper.PinBoards);
                newB.boards = (ulong[])newB.boards.Clone();
                newB.PlayPly(newPly, true);
                newEval = -evaluateCaptures(newB, side == Side.White ? Side.Black : Side.White, -beta, -alpha);
                if (newEval >= beta)
                {
                    return newEval;
                }
                if (newEval > best_val)
                {
                    best_val = newEval;
                }
                if (newEval > alpha)
                {
                    alpha = newEval;
                }
            }
        return best_val;
     }

    public List<Ply> orderMoves(List<Ply> plies, Ply bestMove)
    {
        plies.Sort((ply1, ply2) =>
        {
            int value1 = (ply1.Captured != Piece.None) ? GetPieceScore(ply1.Captured) - GetPieceScore(ply1.Type) : 0;
            int value2 = (ply2.Captured != Piece.None) ? GetPieceScore(ply2.Captured) - GetPieceScore(ply2.Type) : 0;
            //Move order by the best previous move;
            if(ply1.Start == bestMove.Start && ply1.End == bestMove.End) value1 = 1000;
            if(ply2.Start == bestMove.Start && ply2.End == bestMove.End) value2 = 1000;
            return value2.CompareTo(value1); // Sort in descending order
        });
        return plies;
    }
    public List<Ply> orderMoves(List<Ply> plies)
    {
        plies.Sort((ply1, ply2) =>
        {
            int value1 = (ply1.Captured != Piece.None) ? GetPieceScore(ply1.Captured) - GetPieceScore(ply1.Type) : 0;
            int value2 = (ply2.Captured != Piece.None) ? GetPieceScore(ply2.Captured) - GetPieceScore(ply2.Type) : 0;
            return value2.CompareTo(value1); // Sort in descending order
        });
        return plies;
    }
    public void setDifficulty(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":
                difficultyDepth = 2; // Lower depth for easier AI
                break;
            case "medium":
                difficultyDepth = 3; // Default depth
                break;
            case "hard":
                difficultyDepth = 5; // Higher depth for harder AI
                break;
            default:
                Debug.LogWarning("Invalid difficulty level. Setting to medium.");
                difficultyDepth = 3;
                break;
        }
    }
}



