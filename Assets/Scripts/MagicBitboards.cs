using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This is just finding a hash function for each square for rooks and bishops
public static class MagicBitboards {
    public static Magics[] RookMagics = new Magics[64];
    public static Magics[] BishopMagics = new Magics[64];
    // Queen magics/moves are just a lookup into both rook and bishop. Just | the two moves together

    public static void GenerateMagicNumbers() {
        // I am pretty sure that this i is 
        for(int i = 0; i < 64; i++) {
            // Rook magics
            RookMagics[i] = new Magics();
            (ulong rank, ulong file) = RankFileMask(i%8, i/8);
            RookMagics[i].movementMask = (file|rank) & ~(1ul<<i)
                & (i%8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileA)
                & (i%8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileH)
                & (i/8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank8)
                & (i/8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank1);
            // Debug.Log("Movement Mask\n" + PrintBitBoard(RookMagics[i].movementMask));
            RookMagics[i].shift = 63-count1s(RookMagics[i].movementMask);
            // Debug.Log(63-RookMagics[i].shift);
            while(!FillTable(ref RookMagics[i], i, true)) {}

            // Debug.Log("Movement mask: "+PrintBitBoard(RookMagics[i].movementMask));
            // for (int j = 0; j < 1; j++) {
            //     if(FillTable(ref RookMagics[i], i, true)) {
            //         break;
            //     }
            // }
            // Debug.Log("Magic: " + RookMagics[i].magic);
            

            // Bishop magics
            BishopMagics[i] = new Magics();
            (ulong positiveDiagnol, ulong negativeDiagnol) = PosNegDiagnolMask(i%8, i/8);
            BishopMagics[i].movementMask = (positiveDiagnol|negativeDiagnol) & ~(1ul<<i)
                & (i%8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileA)
                & (i%8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileH)
                & (i/8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank8)
                & (i/8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank1);
            BishopMagics[i].shift = 63-count1s(BishopMagics[i].movementMask);
            // Debug.Log("Movement Mask\n" + PrintBitBoard(BishopMagics[i].movementMask));
            // Debug.Log(63-BishopMagics[i].shift);
            while(!FillTable(ref BishopMagics[i], i, false)) {}
            
            // for (int j = 0; j < 70; j++) {
            //     if(FillTable(ref BishopMagics[i], i, false)) {
            //         break;
            //     }
            // }
        }
    }
    
    public static bool FillTable(ref Magics magic, int pos, bool isRook = true) {
        ulong test_magic = ZobristMap.RandomU64()&ZobristMap.RandomU64()&ZobristMap.RandomU64();

        Dictionary<ulong, ulong> testBoard = new Dictionary<ulong, ulong>();
        ulong mask = magic.movementMask;
        for (int i = 0; i < 0xFFFF; i++) {
            // Add code here to iterate through board configs
            // This is hard to read
            // blockers is where blockers can be
            ulong blockers = 0;
            // This exists so that i isn't touched, blocker number
            uint b = (uint)i;
            // This is how many digits through i are we
            
            // This is used to ignore places we have already set
            ulong blockerSpotsTaken = 0;
            while(b > 0) {
                ulong digit = b & 1;
                for(int j = 0; j < 64; j++) {
                    if( (mask & (1ul<<j)) == 0 ) continue; // If there is not a 1, continue
                    if( (blockerSpotsTaken & (1ul<<j)) > 0 ) continue; // If we have set in the blocker already, continue
                    blockerSpotsTaken |= 1ul<<j;
                    blockers |= digit<<j;
                    break; // We break out as soon as we find a spot
                }

                b>>=1;
            }
            // We now finally have blockers

            ulong magicIdx = (blockers * test_magic) >> magic.shift;
            ulong moves = isRook ? FindMovesRook(pos, blockers) : FindMovesBishop(new Vector2Int(pos%8, pos/8), blockers);
            // If the testboard contains this magic index AND the moves for this set of blockers is different from whats saved, this magic number is bad
            if(!testBoard.ContainsKey(magicIdx)) {
                testBoard.Add(magicIdx, moves);
            } else if(moves != testBoard[magicIdx]) {
                // We try with a magic
                return false;
            } 
            if(blockers == mask) {
                break;
            }
        }
        magic.magic = test_magic;
        magic.moves = new ulong[testBoard.Keys.Max()+1];
        foreach(ulong key in testBoard.Keys) {
            magic.moves[key] = testBoard[key];
        }
        return true;
    }
    
    public static string PrintBitBoard(ulong u) {
        string s = "";
        for(int i = 63; i>=0; i--) {
            s=((u&(1ul<<i))>>i)+s;
            if(i % 8 == 0) s = '\n'+s;
        }
        s="Black"+s+"\nWhite";
        return s;
    }
    
    public static int count1s(ulong mask) {
        int b = 0;
        for(int i = 0; i < 64; i++) {
            if((mask&(1ul<<i)) > 0) b++;
        }
        return b;
    }
    
    public static ulong FindMovesRook(int pos, ulong allPieces) {
        ulong moves = 0ul;

        // Along a rank
        for (int i = pos % 8 + 1; i <= 7; i++) {
            ulong changedBit = 1ul << (i + 8 * (pos / 8));
            moves |= changedBit;
            if ((changedBit & allPieces) != 0) break;
        }
        for (int i = pos % 8 - 1; i >= 0; i--) {
            ulong changedBit = 1ul << (i + 8 * (pos / 8));
            moves |= changedBit;
            if ((changedBit & allPieces) != 0) break;
        }

        // Along a file
        for (int i = pos / 8 + 1; i <= 7; i++) {
            ulong changedBit = 1ul << (pos % 8 + i * 8);
            moves |= changedBit;
            if ((changedBit & allPieces) != 0) break;
        }
        for (int i = pos / 8 - 1; i >= 0; i--) {
            ulong changedBit = 1ul << (pos % 8 + i * 8);
            moves |= changedBit;
            if ((changedBit & allPieces) != 0) break;
        }

        return moves;
    }
        
    public static ulong FindMovesBishop(Vector2Int pos, ulong allPieces) {
        ulong moves = 0ul;
        Vector2Int[] directions = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var direction in directions) {
            Vector2Int rayPos = pos + direction;
            while (rayPos.x >= 0 && rayPos.x <= 7 && rayPos.y >= 0 && rayPos.y <= 7) {
                ulong changedBit = 1ul << (rayPos.x + rayPos.y * 8);
                moves |= changedBit;
                if ((allPieces & changedBit) != 0) break;
                rayPos += direction;
            }
        }
        return moves;
    }
    
    public static (ulong, ulong) PosNegDiagnolMask(int x, int y) {
        ulong posDiagnol = 0;
        ulong negDiagnol = 0;

        Vector2Int negSlope = new(-1,1);
        //Idk why its not (7-y)*8 like everywhere else, but this makes it match so /shrug/ 
        int square = x + y*8;

        Vector2Int rayPos = new Vector2Int(x,y) + Vector2Int.one;
        while(
            rayPos.x >= 0 && rayPos.x <= 7
            && rayPos.y >= 0 && rayPos.y <= 7
        ) {
            posDiagnol |= 1ul << (rayPos.x + rayPos.y*8);
            rayPos += Vector2Int.one;
        }

        rayPos = new Vector2Int(x, y) - Vector2Int.one;
        while(
            rayPos.x >= 0 && rayPos.x <= 7
            && rayPos.y >= 0 && rayPos.y <= 7
        ) {
            posDiagnol |= 1ul << (rayPos.x + rayPos.y*8);
            rayPos -= Vector2Int.one;
        }

        rayPos = new Vector2Int(x, y) + negSlope;
        while(
            rayPos.x >= 0 && rayPos.x <= 7
            && rayPos.y >= 0 && rayPos.y <= 7
        ) {
            negDiagnol |= 1ul << (rayPos.x + rayPos.y*8);
            rayPos += negSlope;
        }

        rayPos = new Vector2Int(x, y) - negSlope;
        while(
            rayPos.x >= 0 && rayPos.x <= 7
            && rayPos.y >= 0 && rayPos.y <= 7
        ) {
            negDiagnol |= 1ul << (rayPos.x + rayPos.y*8);
            rayPos -= negSlope;
        }
        return (posDiagnol, negDiagnol);
    }

    public static (ulong, ulong) RankFileMask(int x, int y) {
        ulong rank = (7-y) switch {
            0 => Board.rank1,
            1 => Board.rank2,
            2 => Board.rank3,
            3 => Board.rank4,
            4 => Board.rank5,
            5 => Board.rank6,
            6 => Board.rank7,
            7 => Board.rank8,

            _ => Board.rank1,
        };

        ulong file = x switch
        {
            0 => Board.fileA,
            1 => Board.fileB,
            2 => Board.fileC,
            3 => Board.fileD,
            4 => Board.fileE,
            5 => Board.fileF,
            6 => Board.fileG,
            7 => Board.fileH,

            _ => Board.fileA,
        };
        return (rank, file);
    }
    
    public static (ulong, ulong) RankFileMask(Vector2Int pos) {
        return RankFileMask(pos.x, pos.y);
    }
}

public struct Magics {
    public ulong[] moves;
    public ulong magic;
    public ulong movementMask;
    public int shift;
    
    public ulong GetMove(ulong boardPosition) {
        ulong important = boardPosition & movementMask;
        return moves[(important*magic)>>shift];
    }
}