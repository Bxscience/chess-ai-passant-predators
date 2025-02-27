using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MagicBitboards {
    public static Magics[] RookMagics = new Magics[64];
    public static Magics[] BishopMagics = new Magics[64];
    // Queen magics/moves are just a lookup into both rook and bishop. Just | the two moves together

    public static void GenerateMagicNumbers() {
        // I am pretty sure that this i is 
        for(int i = 63; i < 64; i++) {
            // Rook magics
            RookMagics[i] = new Magics();
            (ulong rank, ulong file) = RankFileMask(i%8, i/8);
            RookMagics[i].movementMask = (file|rank) & ~(1ul<<(63-i));
                // & (i/8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileA)
                // & (i/8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.fileH)
                // & (i%8 == 0 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank7) // These last two may be flipped around
                // & (i%8 == 7 ? 0xFFFFFFFFFFFFFFFF : ~Board.rank1);
            // Debug.Log(RookMagics[i].movementMask);
            // while(!FillTable(ref RookMagics[i])) {}
            
            for (int j = 0; j < 1000; j++) {
                if(FillTable(ref RookMagics[i])) {
                    break;
                }
            }
            Debug.Log("Magic: " + RookMagics[i].magic);
            // Debug.Log(" ");
        }
    }
    
    public static bool FillTable(ref Magics magic) {
        ulong test_magic = RandU64()&RandU64()&RandU64();

        Dictionary<ulong, ulong> testBoard = new Dictionary<ulong, ulong>();
        ulong mask = magic.movementMask;
        for (int i = 0; i < (int)0xFFFF; i++) {
            // Add code here to iterate through board configs
            // This is hard to read
            // blockers is where blockers can be
            ulong blockers = 0;
            // This exists so that i isn't touched, blocker number
            uint b = (uint)i;
            // This is how many digits through i are we
            
            ulong blockerSpotsTaken = 0;
            while(b > 0) {
                uint digit = b & 1;
                
                for(int j = 0; j < 64; j++) {
                    if( (mask & (1ul<<j)) == 0 ) continue;
                    if( (blockerSpotsTaken & (1ul<<j)) > 0 ) continue;
                    blockerSpotsTaken |= 1ul<<j;
                    blockers |= digit<<j;
                    break;
                }

                b>>=1;
            }
            // We now finally have blockers

            // the shift should be by some number, i'm just doing last 14 bits for now
            ulong magicIdx = (blockers * test_magic) >> (64-16);
            ulong moves = FindMovesRook(new Vector2Int(i/8, i%8), blockers);
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
    
    public static ulong RandU64() {
        System.Random random = new System.Random();
        byte[] bytes = new byte[8];
        random.NextBytes(bytes);
        return BitConverter.ToUInt64(bytes);
    }
    
    public static ulong FindMovesRook(Vector2Int pos, ulong allPieces) {
        ulong moves = 0ul;

        // Get position as an integer
        // Our position starts from rank 8, so y=0 (which is rank 1), should be a higher value of pos
        // ie a Vec2I of (0,0) which is A1, should be pos=56 
        int square = pos.x + (7-pos.y)*8;
        ulong posBoard = 1ul << square;
        (ulong rank, ulong file) = RankFileMask(pos);
        
        // Set all bits not on the rank or file to 0
        ulong importantPieces = allPieces & (rank|file);
        
        // Along a rank
        // First/last file doesn't matter, we assume we can always capture
        // We'll filter later
        for(int i = pos.x+1; i < 7; i++) {
            //Add the new move into the board
            moves += 1ul << (square + i - pos.x);

            if( ((1ul << (square + i)) & importantPieces) == 0) {
                break;
            }
        }
        for(int i = pos.x-1; i > 1; i--) {
            //Add the new move into the board
            moves += 1ul << (square + i);

            if( ((1ul << (square + i - pos.x)) & importantPieces) == 0) {
                break;
            }
        }
        
        // Along a file
        // First/last rank doesn't matter, we assume we can always capture
        // We'll filter later
        for(int i = pos.y+1; i < 7; i++) {
            // Add the new move into the board
            moves += 1ul << (pos.x + (7-i)*8);

            if( (moves & importantPieces) == 0) {
                break;
            }
        }

        for(int i = pos.y-1; i > 1; i--) {
            // Add the new move into the board
            moves += 1ul << (pos.x + (7-i)*8);

            if( (moves & importantPieces) == 0) {
                break;
            }
        }
        return moves;
    }
        
    public static ulong FindMovesBishop(Vector2Int pos, ulong allPieces) {
        ulong moves = 0ul;
        Vector2Int negSlope = new(-1,1);

        // Get position as an integer
        // Our position starts from rank 8, so y=0 (which is rank 1), should be a higher value of pos
        // ie a Vec2I of (0,0) which is A1, should be pos=56 
        int square = pos.x + (7-pos.y)*8;
        
        Vector2Int rayPos = pos + Vector2Int.one;
        while(
            rayPos.x > 1 && rayPos.x < 7
            && rayPos.y > 1 && rayPos.y < 7
        ) {
            moves += 1ul << (rayPos.x + (7-rayPos.y)*8);
            if((allPieces & moves) == 0) {
                break;
            }
            rayPos += Vector2Int.one;
        }

        rayPos = pos - Vector2Int.one;
        while(
            rayPos.x > 1 && rayPos.x < 7
            && rayPos.y > 1 && rayPos.y < 7
        ) {
            moves += 1ul << (rayPos.x + (7-rayPos.y)*8);
            if((allPieces & moves) == 0) {
                break;
            }
            rayPos -= Vector2Int.one;
        }

        rayPos = pos + negSlope;
        while(
            rayPos.x > 1 && rayPos.x < 7
            && rayPos.y > 1 && rayPos.y < 7
        ) {
            moves += 1ul << (rayPos.x + (7-rayPos.y)*8);
            if((allPieces & moves) == 0) {
                break;
            }
            rayPos += negSlope;
        }

        rayPos = pos - negSlope;
        while(
            rayPos.x > 1 && rayPos.x < 7
            && rayPos.y > 1 && rayPos.y < 7
        ) {
            moves += 1ul << (rayPos.x + (7-rayPos.y)*8);
            if((allPieces & moves) == 0) {
                break;
            }
            rayPos -= negSlope;
        }

        return moves;
    }

    public static (ulong, ulong) RankFileMask(int x, int y) {
        ulong rank = y switch {
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

        ulong file = (7-x) switch
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
        ulong rank = pos.y switch
        {
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

        ulong file = (7-pos.x) switch {
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
}

public struct Magics {
    public ulong[] moves;
    public ulong magic;
    public ulong movementMask;
}