using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing;

public enum Piece {
    WPawn = 0, WBishop = 1, WKnight = 2, WRook = 3, WQueen = 4, WKing = 5,

    BPawn = 6, BBishop = 7, BKnight = 8, BRook = 9, BQueen = 10, BKing = 11,
    None
}

[Flags]
public enum CastleTrack
{
    wKing = 0b1000,
    wQueen = 0b0100,
    bKing = 0b0010,
    bQueen = 0b0001,
}
public struct Board
{
    public MovesHelper WhiteHelper;
    public MovesHelper BlackHelper;
    // 0-5 is White
    // 6-11 is Black 
    public ulong[] boards;
    public ulong allWhiteMovesPsuedolegal;
    public ulong allBlackMovesPsuedolegal;
    public sbyte passantTrack;
    public sbyte passantCaptured;
    public sbyte castleTracker;
    public Dictionary<ulong, int> threefoldplies;
    public bool isThreefold;


    public const ulong fileA = 0x0101010101010101;
    public const ulong fileB = 0x0202020202020202;
    public const ulong fileC = 0x0404040404040404;
    public const ulong fileD = 0x0808080808080808;
    public const ulong fileE = 0x1010101010101010;
    public const ulong fileF = 0x2020202020202020;
    public const ulong fileG = 0x4040404040404040;
    public const ulong fileH = 0x8080808080808080;
    // Every hex character maps to 4 bits. Two hex digits is 8 bits. Each row is 8 bits
    // If you notice, rank 1 is 0xFF00000000000000, which is a very large number. Since we start counting at rank 8, rank 8 is near the beginning of the number, and rank 1 is at the front.
    public const ulong rank1 = 0xFF00000000000000;
    public const ulong rank2 = 0x00FF000000000000;
    public const ulong rank3 = 0x0000FF0000000000;
    public const ulong rank4 = 0x000000FF00000000;
    public const ulong rank5 = 0x00000000FF000000;
    public const ulong rank6 = 0x0000000000FF0000;
    public const ulong rank7 = 0x000000000000FF00;
    public const ulong rank8 = 0x00000000000000FF;

    public readonly ulong WhitePieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5];
    public  readonly ulong BlackPieces => boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];
    public readonly ulong Pieces => boards[0] | boards[1] | boards[2] | boards[3] | boards[4] | boards[5] | boards[6] | boards[7] | boards[8] | boards[9] | boards[10] | boards[11];

    private Piece FromAlgebraic(char c) => c switch
    {
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
        _ => throw new Exception("Not Valid FEN"),
    };

    public Board(string fen) {
        isThreefold = false;
        WhiteHelper = new MovesHelper(Side.White);
        BlackHelper = new MovesHelper(Side.Black);
        threefoldplies = new Dictionary<ulong, int>();
        allWhiteMovesPsuedolegal = 0;
        allBlackMovesPsuedolegal = 0;
        // castleTracker = 0b1111;
        castleTracker = 0b0000;
        //castleTracker = castleTracker & ~castleTrack.wQueen for example
        boards = new ulong[12];
        passantTrack = 0;
        passantCaptured = 0;
        string[] fen_parts = fen.Split(" ");
        int idx = 0;
        for(int i = 0; i < fen_parts[0].Length; i++) {
            char c = fen_parts[0][i];
            if(char.IsLetter(c)) {
                Piece p = FromAlgebraic(c);
                boards[(int)p] |= 1ul<<idx;
                idx++;
            } else {
                if(char.IsNumber(c)) {
                    idx += c - 48;
                }
            }
        }
        for(int i = 0; i < fen_parts[2].Length; i++) {
            char c = fen_parts[2][i];
            if(c == '-') {
                castleTracker = 0b0000;
                break;
            }
            if(c == 'K') {
                castleTracker |= (int)CastleTrack.wKing;
            }
            if(c == 'Q') {
                castleTracker |= (int)CastleTrack.wQueen;
            }
            if(c == 'k') {
                castleTracker |= (int)CastleTrack.bKing;
            }
            if(c == 'q') {
                castleTracker |= (int)CastleTrack.bQueen;
            }
        }
        SetupMoves();
    }
    
    // Play any given ply.
    // Sets stuff in the bitboards, and handles en passants and castles
    public void PlayPly(Ply ply, bool isAI = false) {
        // the start coordinate, as an offset, starting from A1
        // If Start.y is 7, that should correlate with the 8th rank.   
        int start_idx = ply.Start.x + 8*(7-ply.Start.y); 
        int end_idx = ply.End.x + 8*(7-ply.End.y); 
        
        // We not the board so that every point without a piece has a one, and every point with a piece has a 0
        // Then, we can | the notted board with 1 where the piece has moved from, and not it again to make it a 0
        boards[(int)ply.Type] = ~(~boards[(int)ply.Type] | 1ul<<start_idx);
        // This is simpler, just make the bit it moved to a 1
        boards[(int)ply.Type] = boards[(int)ply.Type] | 1ul<<end_idx;
        
        if(ply.Captured != Piece.None) {
            if(end_idx == passantTrack) {
                boards[(int)ply.Captured] = boards[(int)ply.Captured] & ~(1ul<<passantCaptured);
            } else {
                boards[(int)ply.Captured] = ~(~boards[(int)ply.Captured] | 1ul<<end_idx);
            }
        }

        passantTrack = 0;
        passantCaptured = 0;
    
        //king move -> no more castle on that color
        if (ply.Type == Piece.WKing)
        {
            // We have castled
            if(ply.End.x-ply.Start.x>=2) {
                // King side test
                boards[(int)Piece.WRook] = boards[(int)Piece.WRook]  & ~0x8000000000000000ul;
                boards[(int)Piece.WRook] |=  0x2000000000000000ul;
            }
            if (ply.End.x-ply.Start.x<=-2) {
                // Queen side test
                boards[(int)Piece.WRook] &= ~0x0100000000000000ul;
                boards[(int)Piece.WRook] |= 0x0800000000000000ul;
            }
            castleTracker &= ~(int)(CastleTrack.wKing|CastleTrack.wQueen);
        }
        else if (ply.Type == Piece.BKing)
        {
            // We have castled
            if(ply.End.x-ply.Start.x>=2) {
                // King side test
                boards[(int)Piece.BRook] &= ~0x0000000000000080ul;
                boards[(int)Piece.BRook] |=  0x0000000000000020ul;
            }
            if (ply.End.x-ply.Start.x<=-2) {
                // Queen side test
                boards[(int)Piece.BRook] &= ~0x0000000000000001ul;
                boards[(int)Piece.BRook] |= 0x0000000000000008ul;
            }
            castleTracker &= ~(int)(CastleTrack.bKing|CastleTrack.bQueen);
        }

        if (ply.Type == Piece.WRook)
        {
            if (ply.Start == new Vector2Int(7, 0)) //king side rook
            {
                castleTracker &= ~(int)CastleTrack.wKing;
            }
            else if (ply.Start == new Vector2Int(0, 0)) //queen side rook
            {
                castleTracker &= ~(int)CastleTrack.wQueen; 
            }
        }
        else if (ply.Type == Piece.BRook)
        {
            if (ply.Start == new Vector2Int(7, 7)) 
            {
                castleTracker &= ~(int)CastleTrack.bKing;
            }
            else if (ply.Start == new Vector2Int(0, 7))
            {
                castleTracker &= ~(int)CastleTrack.bQueen;
            }
        }


        if (ply.Type == Piece.WPawn && ((ply.End-ply.Start) == new Vector2Int(0,2)))
        {
            passantTrack = (sbyte)(end_idx + 8);
            passantCaptured = (sbyte)end_idx;
        }
        if (ply.Type == Piece.BPawn && ((ply.End - ply.Start) == new Vector2Int(0, -2)))
        {
            passantTrack = (sbyte)(end_idx - 8);
            passantCaptured = (sbyte)end_idx;
        }

        if (ply.Type == Piece.WPawn && ( (1ul<<end_idx) & rank8 ) != 0) {
            Promote(1ul<<end_idx, Side.White, (Piece)ply.PromoteType);
        }
        else if(ply.Type == Piece.BPawn && ( (1ul<<end_idx) & rank1) != 0) {
            Promote(1ul<<end_idx, Side.Black, (Piece)ply.PromoteType);
        }
        
        SetupMoves();
        if(!isAI) return;
        ulong zMap = ZobristMap.GetZKey(boards, castleTracker, passantTrack, ply.Side == Side.White);
        if (ply.isIrreversible()) {
            threefoldplies.Clear();
            isThreefold = false;
        }
        if (threefoldplies.ContainsKey(zMap))
        {
            threefoldplies[zMap]++;
            if (threefoldplies[zMap] >= 3)
            {
                isThreefold = true;
            }
        }
        else
        {
            threefoldplies.Add(zMap, 1);
        }
    }
    
    // This function uses all pieces paralegal moves to determine checks, pins, etc.
    // Then we precompute all legal moves for the AI and for checkmate/stalemate
    public void SetupMoves() {
        BlackHelper.ClearMoves();
        int bKingPos = GetLSBIndex(boards[(int)Piece.BKing]);
        // Lets find all paralegal moves
        allWhiteMovesPsuedolegal = 0;
        for (int i = 0; i <= (int)Piece.WKing; i++)
        {
            ulong board = boards[i];
            while (board > 0)
            {
                int pos = GetLSBIndex(board);
                ulong moveBoard = GetMoveParalegalForChecks(pos, (Piece)i, Side.White);
                allWhiteMovesPsuedolegal |= moveBoard;

                // Check check for bking
                if ((moveBoard & boards[(int)Piece.BKing]) != 0) {
                    BlackHelper.AddCheckAttack((Piece)i, pos, bKingPos, Pieces);
                }
                // Check pins for bpieces
                if( i == (int)Piece.WBishop || i == (int)Piece.WQueen ) {
                    if(Math.Abs(pos/8-bKingPos/8) == Math.Abs(pos%8-bKingPos%8)) {
                        ulong pinnRay = BlackHelper.FindBishopPinRay(pos, bKingPos);
                        ulong intersect = pinnRay & (BlackPieces&~(1ul<<bKingPos));
                        // Essentially, if there is an intersection, then check if that has more than 2 bits
                        // by clearing the first lsb and checking it its == 0
                        // if both checks are true, is there is an intersection and there is only one intersecting piece, that piece is pinned
                        if(intersect > 0 && (intersect & ~(1ul<<GetLSBIndex(intersect))) == 0) {
                            BlackHelper.Pinned |= intersect;
                            BlackHelper.PinBoards.Add(pinnRay);
                        }
                    }
                }
                if( i == (int)Piece.WRook || i == (int)Piece.WQueen ) {
                    if(pos%8 == bKingPos%8 || pos/8 == bKingPos/8) {
                        ulong pinnRay = BlackHelper.FindRookPinRay(pos, bKingPos);
                        ulong intersect = pinnRay & (BlackPieces&~(1ul<<bKingPos));
                        // Essentially, if there is an intersection, then check if that has more than 2 bits
                        // by clearing the first lsb and checking it its == 0
                        // if both checks are true, is there is an intersection and there is only one intersecting piece, that piece is pinned
                        if(intersect > 0 && (intersect & ~(1ul<<GetLSBIndex(intersect))) == 0) {
                            BlackHelper.PinBoards.Add(pinnRay);
                            BlackHelper.Pinned |= intersect;
                        }
                    }
                }

                board &= ~(1ul << pos);
            }
        }

        WhiteHelper.ClearMoves();
        int wKingPos = GetLSBIndex(boards[(int)Piece.WKing]);
        allBlackMovesPsuedolegal = 0;
        for (int i = 6; i <= (int)Piece.BKing; i++)
        {
            ulong board = boards[i];
            while (board > 0)
            {
                int pos = GetLSBIndex(board);
                ulong moveBoard = GetMoveParalegalForChecks(pos, (Piece)i, Side.Black);
                allBlackMovesPsuedolegal |= moveBoard;

                // Check check for wking
                if ((moveBoard & boards[(int)Piece.WKing]) != 0)
                    WhiteHelper.AddCheckAttack((Piece)i, pos, wKingPos, Pieces);
                // Check pins for wpieces
                if( i == (int)Piece.BBishop || i == (int)Piece.BQueen ) {
                    if(Math.Abs(pos/8-wKingPos/8) == Math.Abs(pos%8-wKingPos%8)) {
                        ulong pinnRay = WhiteHelper.FindBishopPinRay(pos, wKingPos);
                        ulong intersect = pinnRay & (WhitePieces&~(1ul<<wKingPos));
                        // Essentially, if there is an intersection, then check if that has more than 2 bits
                        // by clearing the first lsb and checking it its == 0
                        // if both checks are true, is there is an intersection and there is only one intersecting piece, that piece is pinned
                        if(intersect != 0 && (intersect & ~(1ul<<GetLSBIndex(intersect))) == 0) {
                            WhiteHelper.PinBoards.Add(pinnRay);
                            WhiteHelper.Pinned |= intersect;
                        }
                    }
                }
                if( i == (int)Piece.BRook || i == (int)Piece.BQueen ) {
                    if(pos%8 == wKingPos%8 || pos/8 == wKingPos/8) {
                        ulong pinnRay = WhiteHelper.FindRookPinRay(pos, wKingPos);
                        ulong intersect = pinnRay & (WhitePieces&~(1ul<<wKingPos));
                        // Essentially, if there is an intersection, then check if that has more than 2 bits
                        // by clearing the first lsb and checking it its == 0
                        // if both checks are true, is there is an intersection and there is only one intersecting piece, that piece is pinned
                        if(intersect != 0 && (intersect & ~(1ul<<GetLSBIndex(intersect))) == 0) {
                            WhiteHelper.PinBoards.Add(pinnRay);
                            WhiteHelper.Pinned |= intersect;
                        }
                    }
                }

                board &= ~(1ul << pos);
            }
        }
        
        for (int i = (int)Piece.WKing; i >= (int)Piece.WPawn; i--) {
            ulong board = boards[i];
            while (board > 0) {
                int pos = GetLSBIndex(board);
                ulong moveBoard = GetMoveLegal(pos, (Piece)i, Side.White);
                while(moveBoard>0) {
                    int movePos = GetLSBIndex(moveBoard);
                    Ply newPly = new Ply(new Vector2Int(pos%8, 7-pos/8), new Vector2Int(movePos%8, 7-movePos/8), (Piece)i);
                    for(int j = (int)Piece.BPawn; j <= (int)Piece.BKing; j++)
                        if(((1ul<<movePos) & boards[j]) > 0) {
                            newPly.Captured = (Piece)j;
                            break;
                        }
                    if(newPly.Type == Piece.WPawn && ((1ul<<movePos)&rank8)>0) {
                        newPly.PromoteType = Piece.WQueen;
                        WhiteHelper.Plies.Add(newPly);
                        newPly.PromoteType = Piece.WKnight;
                    }
                    WhiteHelper.Plies.Add(newPly);
                    moveBoard &= ~(1ul << movePos);
                }
                board &= ~(1ul << pos);
            }
        }

        for (int i = (int)Piece.BKing; i >= (int)Piece.BPawn; i--) {
            ulong board = boards[i];
            while (board > 0) {
                int pos = GetLSBIndex(board);
                ulong moveBoard = GetMoveLegal(pos, (Piece)i, Side.Black);
                while(moveBoard>0) {
                    int movePos = GetLSBIndex(moveBoard);
                    Ply newPly = new Ply(new Vector2Int(pos%8, 7-pos/8), new Vector2Int(movePos%8, 7-movePos/8), (Piece)i);
                    for(int j = (int)Piece.WPawn; j <= (int)Piece.WKing; j++) {
                        if(((1ul<<movePos) & boards[j]) > 0) {
                            newPly.Captured = (Piece)j;
                            break;
                        }
                    }
                    if(newPly.Type == Piece.BPawn && ((1ul<<movePos)&rank1)>0) {
                        newPly.PromoteType = Piece.BQueen;
                        BlackHelper.Plies.Add(newPly);
                        newPly.PromoteType = Piece.BKnight;
                    }
                    BlackHelper.Plies.Add(newPly);
                    moveBoard &= ~(1ul << movePos);
                }
                board &= ~(1ul << pos);
            }
        }
    }
    
    public bool IsEnPassant(Vector2Int endPos) {
        int sq = endPos.x + (7-endPos.y)*8;
        return sq == passantTrack;
    }

    public void Promote(ulong pos, Side side, Piece promoteType)
    {
        if (side== Side.White) {
            boards[(int)Piece.WPawn] &= ~pos;
            boards[(int)promoteType] |= pos;
        }
        if (side == Side.Black) {
            boards[(int)Piece.BPawn] &= ~pos;
            boards[(int)promoteType] |= pos;
        }
    }

    public void UndoPly(Ply ply) {
        // the start coordinate, as an offset, starting from A1
        // If Start.y is 7, that should correlate with the 8th rank.   
        int start_idx = ply.Start.x + 8 * (7 - ply.Start.y);
        int end_idx = ply.End.x + 8 * (7 - ply.End.y);
        boards[(int)ply.Type] &= ~(1ul << end_idx);
        // Set the start position
        boards[(int)ply.Type] |= 1ul << start_idx;

        // Handle captured pieces
        if (ply.Captured != Piece.None)
        {
            if ((ply.Type == Piece.WPawn && ((ply.End - ply.Start) == new Vector2Int(0, 2))) | (ply.Type == Piece.BPawn && ((ply.End - ply.Start) == new Vector2Int(0, -2))))
            {
                // Handle en passant capture: restore the captured pawn to its original position
                boards[(int)ply.Captured] |= 1ul << passantCaptured;
                passantTrack = 0;
                passantCaptured = 0;
            }
            else
            {
                boards[(int)ply.Captured] |= 1ul << end_idx; //restore captured piece to position
            }
        }
        // Handle castling
        if ((ply.Type == Piece.WKing || ply.Type == Piece.BKing) & Math.Abs(ply.End.x - ply.Start.x)>=2)
        {
            // Determine the rook's start and end positions
            int rookStartIdx, rookEndIdx;
            if (ply.End.x > ply.Start.x)
            { // Kingside castling
                rookStartIdx = 7 + 8 * (7 - ply.Start.y); // H file
                rookEndIdx = 5 + 8 * (7 - ply.Start.y); // F file
                if (ply.Side == Side.White)
                {
                    castleTracker |= (int)CastleTrack.wKing;
                }
                else
                {
                    castleTracker |= (int)CastleTrack.bKing;
                }
            }
            else
            { // Queenside castling
                rookStartIdx = 0 + 8 * (7 - ply.Start.y); // A file
                rookEndIdx = 3 + 8 * (7 - ply.Start.y); // D file
                if (ply.Side == Side.White)
                {
                    castleTracker |= (int)CastleTrack.wQueen;
                }
                else
                {
                    castleTracker |= (int)CastleTrack.bQueen;
                }
            }

            // Move the rook back to its original position
            boards[(int)(ply.Side == Side.White ? Piece.WRook : Piece.BRook)] &= ~(1ul << rookEndIdx); // Clear the rook's end position
            boards[(int)(ply.Side == Side.White ? Piece.WRook : Piece.BRook)] |= 1ul << rookStartIdx; // Set the rook's start position
        }
        if (ply.PromoteType != null)
        {
            // Revert the promoted pawn back to a pawn
            boards[(int)ply.PromoteType] &= ~(1ul << end_idx); // Clear the promoted piece
            boards[(int)ply.Type] |= 1ul << start_idx; // Set the pawn back
        }
    }
    
    // Filters the paralegal moves into legal moves by accounting for checks and pins
    public ulong GetMoveLegal(int square, Piece type, Side side) {
        // CheckStatus cs = GetCheckStatus();
        if(side == Side.White) {
            return WhiteHelper.FilterForLegalMoves(GetMoveParalegal(square, type, side), square, type, allBlackMovesPsuedolegal, castleTracker);
        } else if(side == Side.Black) {
            return BlackHelper.FilterForLegalMoves(GetMoveParalegal(square, type, side), square, type, allWhiteMovesPsuedolegal, castleTracker);
        }
        return 0;
    }
    public ulong GetMoveLegal(ChessPiece piece) {
        return GetMoveLegal(piece.idx.x + 8*(7-piece.idx.y), piece.type, piece.side);
    }
    

    public static Vector3 IdxToPos(int x, int y) {
        float min = -19.25f;
        float max = 19.25f;
        float x_pos = x * (max - min) / 7 + min;
        float y_pos = y * (max - min) / 7 + min;
        return new Vector3(x_pos, 2, y_pos);
    }

    public static Vector3 IdxToPos(Vector2Int idx) {
        return IdxToPos(idx.x, idx.y);
    }
    
    public ulong GetMoveParalegal(ChessPiece piece) => GetMoveParalegal(piece.idx.x + 8*(7-piece.idx.y), piece.type, piece.side);
    
    public ulong GetMoveParalegal(int square, Piece type, Side side) {
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        // if idx.y == 7, then its in the first few bits, because we start fropm A8
        ulong pos = 1ul << square;
        return type switch
        {
            // Leaping
            Piece.WPawn or Piece.BPawn => PawnMovesParalegal(pos, side),
            Piece.WKnight or Piece.BKnight => KnightMovesParalegal(pos, side),
            Piece.WKing or Piece.BKing => KingMovesParalegal(pos, side),
            
            // Sliding (not yet implemented)
            Piece.WBishop or Piece.BBishop => BishopMovesParalegal(square, side),
            Piece.WRook or Piece.BRook => RookMovesParalegal(square, side),
            Piece.WQueen or Piece.BQueen => QueenMovesParalegal(square, side),
            
            // Can't move nothing
            _ => 0ul,
        } & ~sameSide;
    }

    private ulong GetMoveParalegalForChecks(int square, Piece type, Side side)
    {
        // if idx.y == 7, then its in the first few bits, because we start fropm A8
        ulong pos = 1ul << square;
        return type switch
        {
            // Leaping
            Piece.WPawn or Piece.BPawn => PawnMovesParalegalForCheck(pos, side),
            Piece.WKnight or Piece.BKnight => KnightMovesParalegal(pos, side),
            Piece.WKing or Piece.BKing => KingImmediateMoves(pos, side),

            // Sliding (not yet implemented)
            Piece.WBishop or Piece.BBishop => BishopMovesParalegal(square, side),
            Piece.WRook or Piece.BRook => RookMovesParalegal(square, side),
            Piece.WQueen or Piece.BQueen => QueenMovesParalegal(square, side),

            // Can't move nothing
            _ => 0ul,
        };
    }
    public ulong KnightMovesParalegal(ulong pos, Side side) { 
        // Explained well in the ameye.dev link Ms. Qiu gave us
        // We are just bitshifting to the correct position
        // The attacks a knight can do on the left side can never be in file G or H, otherwise it'd be a wrap around
        // Same with the attacks on the right.
        ulong attacks = (((pos>>6)|(pos<<10)) & ~(fileA|fileB))
        | (((pos>>10)|(pos<<6)) & ~(fileG|fileH))
        | (((pos>>15)|(pos<<17) ) & ~fileA)
        | (((pos>>17)|(pos<<15)) & ~fileH);
        return attacks;
    }
    
    public ulong PawnMovesParalegal(ulong pos, Side side) { 
        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacksWhite = (pos >> 8 & ~Pieces)
            | (pos>>7 & BlackPieces & ~fileA)
            | (pos>>9 & BlackPieces & ~fileH) | (pos>>7 & (1ul<<passantTrack) & ~fileA) | (pos>>9 & (1ul<<passantTrack) & ~fileH)
            | (( (pos & rank2) > 0 && ((pos>>8 & Pieces) == 0) && ((pos>>16 & Pieces) == 0)) ? pos>>16 : 0);
        
        ulong attacksBlack = (pos << 8 & ~Pieces)
            | (pos<<9 & WhitePieces & ~fileA)
            | (pos<<7 & WhitePieces & ~fileH) | (pos << 9 & (1ul<<passantTrack) & ~fileA) | (pos << 7 & (1ul<<passantTrack) & ~fileH)
            | (( (pos & rank7) > 0 && ((pos<<8 & Pieces) == 0) && ((pos<<16 & Pieces)==0)) ? pos<<16 : 0);

        if(side == Side.White) 
            return attacksWhite;
        else
            return attacksBlack;
    }
    
    public ulong PawnMovesParalegalForCheck(ulong pos, Side side) { 
        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacksWhite = (pos>>7 & ~fileA)
            | (pos>>9 & ~fileH);
        
        ulong attacksBlack = (pos<<9 & ~fileA)
            | (pos<<7 & ~fileH);

        if(side == Side.White) 
            return attacksWhite;
        else
            return attacksBlack;
    }

    public ulong KingMovesParalegal(ulong pos, Side side) { 
        const ulong wkSlide = 0x6000000000000000;
        const ulong bkSlide = 0x0000000000000060;
        const ulong wqSlide = 0x0E00000000000000;
        const ulong bqSlide = 0x000000000000000E;
        //do the queen slides, 01110 instead of 0110 slid by king position or wtv


        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacks = ( pos >> 7 & ~fileA )
            | pos >> 8
            | ( pos >> 9 & ~fileH )
            // ▲ up the board
            // ▼ Down the board
            | ( pos << 9 & ~fileA )
            | pos << 8
            | ( pos << 7 & ~fileH )
            // ▼ Left right movement
            | ( pos >> 1 & ~fileH )
            | ( pos << 1 & ~fileA );
        if (side == Side.White)
        {
            if (((castleTracker & (int)CastleTrack.wKing) > 0) & ((WhitePieces & wkSlide) == 0))
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)CastleTrack.wQueen) > 0 & ((WhitePieces & wqSlide) == 0))
            {
                attacks = attacks | (pos >> 2);
            }

        }
        else
        {
            if ((castleTracker & (int)CastleTrack.bKing) > 0 & ((BlackPieces & bkSlide) == 0))
            {
                attacks = attacks | (pos << 2);
            }
            if ((castleTracker & (int)CastleTrack.bQueen) > 0 & ((BlackPieces & bqSlide) == 0))
            {
                attacks = attacks | (pos >> 2);
            }
        }

        return attacks;
    }
    
    private ulong KingImmediateMoves(ulong pos, Side side) {
        ulong sameSide = side == Side.White ? WhitePieces : BlackPieces;
        // You can either move forward, or capture
        // The capture on the right can't be in file A, and the capture on the left can't be in file H
        ulong attacks = ( pos >> 7 & ~fileA )
            | pos >> 8
            | ( pos >> 9 & ~fileH )
            // ▲ up the board
            // ▼ Down the board
            | ( pos << 9 & ~fileA )
            | pos << 8
            | ( pos << 7 & ~fileH )
            // ▼ Left right movement
            | ( pos >> 1 & ~fileH )
            | ( pos << 1 & ~fileA );
        return attacks & ~sameSide;
    }

    public ulong RookMovesParalegal(int pos, Side side) {
        ulong moves = MagicBitboards.RookMagics[pos].GetMove(Pieces);

        return moves;
    }

    public ulong BishopMovesParalegal(int pos, Side side) {
        ulong moves = MagicBitboards.BishopMagics[pos].GetMove(Pieces);

        return moves;
    }

    public ulong QueenMovesParalegal(int pos, Side side) {
        ulong rookMoves = RookMovesParalegal(pos, side);
        ulong bishopMoves = BishopMovesParalegal(pos, side);
        return rookMoves | bishopMoves;
    }
    
    // This is from: https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightBinSearch
    public static int GetLSBIndex(ulong b) {
        int c; // c will be the number of zero bits on the right,
        if ((b & 0x1) == 1)
        {
            // special case for odd v (assumed to happen half of the time)
            c = 0;
        }
        else
        {
            c = 1;
            if ((b & 0xffffffff) == 0)
            {
                b >>= 32;
                c += 32;
            }
            if ((b & 0xffff) == 0)
            {
                b >>= 16;
                c += 16;
            }
            if ((b & 0xff) == 0)
            {
                b >>= 8;
                c += 8;
            }
            if ((b & 0xf) == 0)
            {
                b >>= 4;
                c += 4;
            }
            if ((b & 0x3) == 0)
            {
                b >>= 2;
                c += 2;
            }
            c -= (int)(b & 0x1);
        }
        return c;
    }
}
