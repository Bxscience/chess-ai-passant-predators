using System;
using System.Security.Cryptography;

public class ZobristMap {
    static ulong[,] zArr = new ulong[12,64];
    static ulong[] zEnPassant = new ulong[8];
    static ulong[] zCastleRights = new ulong[4];

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
    }
    
    public static ulong getZKey(ulong[] boards, sbyte castleRights, sbyte enPassantSq) {
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

        return retZKey;
    }
}

public struct ZobristData {
    public Ply BestMove;
    public int Eval;
    public int alpha, beta;
}