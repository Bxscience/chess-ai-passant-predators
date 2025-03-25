using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class ZobristMap {
    /* -------------------All the static data stuff related to Zobrist keys------------------- */
    static ulong[,] zArr = new ulong[12,64];
    static ulong[] zEnPassant = new ulong[8];
    static ulong[] zCastleRights = new ulong[4];
    static ulong isBlackTurn = 0;

    public static ulong RandomU64() {
        Span<byte> data = stackalloc byte[8];
        RandomNumberGenerator.Fill(data);
        ulong ret = 0ul;
        for(int i = 0; i < 8; i++) {
            ret |= ((ulong)data[i])<<(i*8);
        }
        return ret;
    }
    
    public static void FillZorbistKeys() {
        for(int i = 0; i < 12; i++) {
            for(int j = 0; j < 64; j++) {
                zArr[i,j] = RandomU64();
            }
        }
        for(int i = 0; i < 8; i++) zEnPassant[i] = RandomU64();
        for(int i = 0; i < 4; i++) zCastleRights[i] = RandomU64();
        isBlackTurn = RandomU64();
    }
    
    public static ulong GetZKey(ulong[] boards, sbyte castleRights, sbyte enPassantSq, bool isWhite) {
        ulong retZKey = 0;
        
        for(int i = 0; i < boards.Length; i++) {
            ulong b = boards[i];
            while (b > 0) {
                int sq = Board.GetLSBIndex(b);
                
                retZKey ^= zArr[i,sq];

                b ^= 1ul<<sq;
            }
        }
        if((castleRights & 0b0001) > 0) retZKey ^= zCastleRights[0];
        if((castleRights & 0b0010) > 0) retZKey ^= zCastleRights[1];
        if((castleRights & 0b0100) > 0) retZKey ^= zCastleRights[2];
        if((castleRights & 0b1000) > 0) retZKey ^= zCastleRights[3];
        if(enPassantSq > 0) retZKey ^= zEnPassant[enPassantSq%8];
        
        if(!isWhite) retZKey ^= isBlackTurn;

        return retZKey;
    }
    /* -------------------End------------------- */

    /* -------------------Begin Non Static Transposition tables------------------- */
    public TTEntry[] TranspositionTable;
    public const int TEXACT = 1, TUPPER = 2, TLOWER = 3;

    public ZobristMap() {
        // Power of 2, 2^21
        TranspositionTable = new TTEntry[2097152];
    }
    
    public TTEntry this[ulong key] {
        get => TranspositionTable[key%(ulong)TranspositionTable.Length];
    }

    public bool HasKey(ulong zKey) {
        return this[zKey].Type != 0;
    }

    public void AddTransposition(ulong[] boards, sbyte castleRights, sbyte enPassantSq, bool isWhite, Ply bestMove, int eval, int depth, int tType) {
        ulong zKey = GetZKey(boards, castleRights, enPassantSq, isWhite);
        if(!HasKey(zKey) || this[zKey].ZKey != zKey) TranspositionTable[zKey%(ulong)TranspositionTable.Length] = new TTEntry(bestMove, eval, zKey, depth, tType);
    }

    public void AddTransposition(ulong zKey, Ply bestMove, int eval, int depth, int tType) {
<<<<<<< HEAD
        if(!HasKey(zKey)) TranspositionTable[zKey%(ulong)TranspositionTable.Length] = new TTEntry(bestMove, eval, zKey, depth, tType);
=======
        if(!HasKey(zKey)) { 
            TranspositionTable[zKey%(ulong)TranspositionTable.Length] = new TTEntry(bestMove, eval, zKey, depth, tType);
        }
        else if(this[zKey].ZKey != zKey && this[zKey].Depth <= depth) {
            TranspositionTable[zKey%(ulong)TranspositionTable.Length] = new TTEntry(bestMove, eval, zKey, depth, tType);
        }
>>>>>>> d7c42fe39460fe026896e1b589dba2c0a380de59
    }
    
    // Returns true if the depth of the entry is greater than the current depth in search. Therefore we can use the evaluated score, or prune the branch
    // Otherwise, the bestPly is still probably the ply we should check first, improving our move ordering
    public bool ProbeTable(ulong zKey, int depth, int alpha, int beta, out int score, out Ply? bestPly) {
        bestPly = null;
        if(HasKey(zKey)) {
            TTEntry entry = this[zKey];
            bestPly = entry.BestMove;
            if(entry.Depth >= depth) {
                switch (entry.Type) {
                    case TEXACT:
                        score = entry.Eval;
                        return true;
                    case TUPPER:
                        if(entry.Eval <= alpha) {
                            score = alpha;
                            return true;
                        }
                        break;
                    case TLOWER:
                        if(entry.Eval >= beta) {
                            score = beta;
                            return true;
                        }
                        break;
                }
            }
        }
        score = 0;
        return false;
    }
}

public struct TTEntry {
    public Ply BestMove;
    public ulong ZKey;
    public int Eval;
    // Either 1 (Exact), 2 (Upper), 3 (Lower)
    public int Type;
    public int Depth;
    public TTEntry(Ply bestMove, int eval, ulong zKey, int depth, int tType) {
        BestMove = bestMove;
        Eval = eval;
        Depth = depth;
        Type = 0;
        ZKey = zKey;
        Type = tType;
    }
}