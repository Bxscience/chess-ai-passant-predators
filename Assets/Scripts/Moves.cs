using Unity.VisualScripting;
using UnityEngine;

public struct Moves {
    public int StartSquare;
    public Piece Type;
    public ulong MoveBoard;
    public int Count;

    private Side GetSide() => Type switch
    {
        Piece.WPawn or Piece.WBishop or Piece.WKnight or Piece.WRook or Piece.WQueen or Piece.WKing => Side.White,
        Piece.BPawn or Piece.BBishop or Piece.BKnight or Piece.BRook or Piece.BQueen or Piece.BKing => Side.Black,
        Piece.None => Side.None,
        _ => Side.None,
    };

    public Moves(ulong moveBoards, int pos, Piece type) {
        StartSquare = pos;
        Type = type;
        MoveBoard = moveBoards;
        Count=0;
        while(moveBoards>0) {
            Count++;
            moveBoards &= ~(1ul<<Board.GetLSBIndex(moveBoards));
        }
    }
    
    // Alternate approach to what I have in board
    // Might be better? might also be worse
    // Its fully a space vs time complexity trade off.
    public Ply GetMoveAt(int idx, ref Board b) {
        // if(idx>=Count) return null;
        ulong board = MoveBoard;
        int pos = 0;
        for(int i = 0; i <= idx; i++) {
            pos = Board.GetLSBIndex(board);
            board &= ~(1ul<<pos);
        }
        
        Ply newPly = new Ply(new Vector2Int(StartSquare%8, 7-StartSquare/8), new Vector2Int(pos%8, 7-pos/8), Type);
        if(GetSide() == Side.White) {
            for(int j = (int)Piece.BPawn; j <= (int)Piece.BKing; j++)
                if(((1ul<<pos) & b.boards[j]) > 0) {
                    newPly.Captured = (Piece)j;
                    break;
                }
            if(newPly.Type == Piece.WPawn && ((1ul<<pos)&Board.rank8)>0)
                newPly.PromoteType = Piece.WQueen;

            return newPly;
        }
        for(int j = (int)Piece.WPawn; j <= (int)Piece.WKing; j++)
            if(((1ul<<pos) & b.boards[j]) > 0) {
                newPly.Captured = (Piece)j;
                break;
            }
        if(newPly.Type == Piece.BPawn && ((1ul<<pos)&Board.rank1)>0)
            newPly.PromoteType = Piece.BQueen;

        return newPly;
    }
}