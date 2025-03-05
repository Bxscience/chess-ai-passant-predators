public struct Moves {
    public int StartSquare;
    public Piece Type;
    public ulong MoveBoard;
    public int Count;
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
}