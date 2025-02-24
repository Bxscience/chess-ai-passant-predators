using System;

public class MagicBitboards {
    public static int[] RookMagics = new int[64];
    public static int[] BishopMagics = new int[64];
    // Queen magics/moves are just a lookup into both rook and bishop. Just | the two moves together
    
    // We need to fill this
    public static int[] RookMoves;
    public static int[] BishopMoves;
    
    public static ulong FindMovesRook(Vector2Int pos, ulong allPieces) {
        ulong moves = 0ul;

        // Get position as an integer
        // Our position starts from rank 8, so y=0 (which is rank 1), should be a higher value of pos
        // ie a Vec2I of (0,0) which is A1, should be pos=56 
        int square = piece.idx.x + (7-piece.idx.y)*8;
        ulong posBoard = 1ul << square;
        (ulong rank, ulong file) = RankFileMask(pos);
        
        // Set all bits not on the rank or file to 0
        ulong importantPieces = allPieces & (rank|file);
        
        // Along a rank
        // First/last file doesn't matter, we assume we can always capture
        // We'll filter later
        for(int i = 1; i < 7; i++) {
            if(i == pos.x) continue;
            if( (1ul << (square + i - pos.x)) & importantPieces != 0) {
                // Add the new move into the board
                moves += 1ul << (square + i - pos.x);
            }
        }
        
        // Along a file
        // First/last rank doesn't matter, we assume we can always capture
        // We'll filter later
        for(int i = 1; i < 7; i++) {
            if(i == pos.y) continue;
            if( ( 1ul << (pos.x + (7-pos.y)*8) ) & importantPieces != 0) {
                // Add the new move into the board
                moves += 1ul << (pos.x + (7-pos.y)*8);
            }
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
        }

        ulong file = x switch {
            0 => Board.fileA,
            1 => Board.fileB,
            2 => Board.fileC,
            3 => Board.fileD,
            4 => Board.fileE,
            5 => Board.fileF,
            6 => Board.fileG,
            7 => Board.fileH,
        }
        return (rank, file);
    }
    
    public static (ulong, ulong) RankFileMask(Vector2Int pos) {
        ulong rank = pos.y switch {
            0 => Board.rank1,
            1 => Board.rank2,
            2 => Board.rank3,
            3 => Board.rank4,
            4 => Board.rank5,
            5 => Board.rank6,
            6 => Board.rank7,
            7 => Board.rank8,
        }

        ulong file = pos.x switch {
            0 => Board.fileA,
            1 => Board.fileB,
            2 => Board.fileC,
            3 => Board.fileD,
            4 => Board.fileE,
            5 => Board.fileF,
            6 => Board.fileG,
            7 => Board.fileH,
        }
        return (rank, file);
    }
}