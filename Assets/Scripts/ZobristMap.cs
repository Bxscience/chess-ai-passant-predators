using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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
    
    public static ulong getZKey(ulong[] boards, sbyte castleRights, sbyte enPassantSq, bool isWhite) {
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
    public ZobristData[] TranspositionTable;

    public ZobristMap(int mb) {
        TranspositionTable = new ZobristData[mb*1024*1024/Marshal.SizeOf<ZobristData>()];
        for(int i = 0; i < TranspositionTable.Length; i++) {
            TranspositionTable[i] = new ZobristData();
            TranspositionTable[i].Init = false;
        }
    }
    
    public ZobristData this[ulong key] {
        get => TranspositionTable[key%(ulong)TranspositionTable.Length];
    }

    public bool hasKey(ulong zKey) {
        return this[zKey].Init;
    }

    public void AddTransposition(ulong[] boards, sbyte castleRights, sbyte enPassantSq, bool isWhite, Ply bestMove, int eval, int alpha, int beta) {
        ulong zKey = ZobristMap.getZKey(boards, castleRights, enPassantSq, isWhite);
        if(!hasKey(zKey)) TranspositionTable[zKey%(ulong)TranspositionTable.Length] = new ZobristData(bestMove, eval, alpha, beta, true);
    }
}

public struct ZobristData {
    public Ply BestMove;
    public int Eval;
    public int Alpha, Beta;
    public bool Init;
    public ZobristData(Ply bestMove, int eval, int alpha, int beta, bool init) {
        BestMove = bestMove;
        Eval = eval;
        Alpha = alpha;
        Beta = beta;
        Init = init;
    }
}