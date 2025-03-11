public struct FakeBoardArr {
    public ulong WPawn, WBishop, WKnight, WRook, WQueen, WKing;
    public ulong BPawn, BBishop, BKnight, BRook, BQueen, BKing;

    private ulong Get(int idx) => idx switch {
        0 => WPawn,
        1 => WBishop,
        2 => WKnight,
        3 => WRook,
        4 => WQueen,
        5 => WKing,

        6 => BPawn,
        7 => BBishop,
        8 => BKnight,
        9 => BRook,
        10 => BQueen,
        11 => BKing,
    };

    // private ulong Get(Piece type) => type switch {
    //     Piece.WPawn => WPawn,
    //     Piece.WBishop => WBishop,
    //     Piece.WKnight => WKnight,
    //     Piece.WRook => WRook,
    //     Piece.WQueen => WQueen,
    //     Piece.WKing => WKing,

    //     Piece.BPawn => BPawn,
    //     Piece.BBishop => BBishop,
    //     Piece.BKnight => BKnight,
    //     Piece.BRook => BRook,
    //     Piece.BQueen => BQueen,
    //     Piece.BKing => BKing,
    // };

    private void Set(int idx, ulong value)
    {
        switch (idx) {
            case 0:
                WPawn = value;
                break;
            case 1:
                WBishop = value;
                break;
            case 2:
                WKnight = value;
                break;
            case 3:
                WRook = value;
                break;
            case 4:
                WQueen = value;
                break;
            case 5:
                WKing = value;
                break;

            case 6:
                BPawn = value;
                break;
            case 7:
                BBishop = value;
                break;
            case 8:
                BKnight = value;
                break;
            case 9:
                BRook = value;
                break;
            case 10:
                BQueen = value;
                break;
            case 11:
                BKing = value;
                break;
        };
    }

    // private void Set(Piece type, ulong value)
    // {
    //     switch (type) {
    //         case Piece.WPawn:
    //             WPawn = value;
    //             break;
    //         case Piece.WBishop:
    //             WBishop = value;
    //             break;
    //         case Piece.WKnight:
    //             WKnight = value;
    //             break;
    //         case Piece.WRook:
    //             WRook = value;
    //             break;
    //         case Piece.WQueen:
    //             WQueen = value;
    //             break;
    //         case Piece.WKing:
    //             WKing = value;
    //             break;

    //         case Piece.BPawn:
    //             BPawn = value;
    //             break;
    //         case Piece.BBishop:
    //             BBishop = value;
    //             break;
    //         case Piece.BKnight:
    //             BKnight = value;
    //             break;
    //         case Piece.BRook:
    //             BRook = value;
    //             break;
    //         case Piece.BQueen:
    //             BQueen = value;
    //             break;
    //         case Piece.BKing:
    //             BKing = value;
    //             break;
    //     };
    // }

    public ulong this[int i]
    {
        get => Get(i);
        set => Set(i, value);
    }
    // public ulong this[Piece type]
    // {
    //     get => Get(type);
    //     set => Set(type, value);
    // }
}