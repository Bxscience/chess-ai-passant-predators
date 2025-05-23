using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Board board;
    public bool isWhiteTurn = true;
    public bool isWhiteAI = false;
    public bool isBlackAI = false;
    public Dictionary<ulong, int> threefoldplies;
    public bool isThreefold;
    bool isCheckMate = false;
    bool isStaleMate = false;
    bool isResigned = false;
    public bool isGameNotActive = true;
    bool isGrabbing;
    ChessPiece currentlySelected;
    ChessPiece enPassantable;
    Stack<Ply> plies = new Stack<Ply>();
    Stack<ChessPiece> taken = new Stack<ChessPiece>();
    Stack<ChessPiece> moved = new Stack<ChessPiece>();

    public AI blackAI = new AI();
    public AI whiteAI = new AI();
    public Ui ui;
    
    public static BoardManager instance;
    public event Action<Ply> PlayedPly;
    public event Action<ulong> GrabbedPiece;

    public ChessPiece[] pieceBoard = new ChessPiece[64];
   

    private bool isPromoting = false;
    private Ply pendingPromotionPly;
    //Standard Fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    //Mate in 1 I think: "8/8/8/8/8/8/rr6/k1K5 w - - 0 1"
    //Black 2 rooks vs white king: "8/8/8/8/4K3/8/rr6/k7 w - - 0 1"
    //Pawn Promotion for white check "8/7k/7p/5Pp1/P2R2P1/7P/5K2/8 w - 0 1" 
    void Start()
    {
        instance = this;
        MagicBitboards.GenerateMagicNumbers();
        ZobristMap.FillZorbistKeys();
        threefoldplies = new Dictionary<ulong, int>();
        isThreefold = false;
        isCheckMate = false;
        isStaleMate = false;
        isResigned = false;
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        board = new Board(fen);
        if(fen.Split(" ")[1][0] == 'w')
            isWhiteTurn = true;
        else isWhiteTurn = false;
    }

    public void Reset()
    {
        currentlySelected = null;
        enPassantable = null;
        taken.Clear();
        moved.Clear();
        plies.Clear();
        isGrabbing = false;
        isThreefold = false;
        isGameNotActive = true;
        isCheckMate = false;
        isStaleMate = false;
        threefoldplies.Clear();
        isWhiteAI = false;
        isBlackAI = false;
        isPromoting = false;
        PlayedPly = null;
        GrabbedPiece = null;
        pendingPromotionPly = default;
        ui.resetPromotionValues();
        ZobristMap.FillZorbistKeys();
        pieceBoard = new ChessPiece[64];
        SpawnPieces.instance.Clear();
        SpawnPieces.instance.Fill();
        blackAI = new AI();
        whiteAI = new AI();
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        board = new Board(fen);
        if(fen.Split(" ")[1][0] == 'w')
            isWhiteTurn = true;
        else isWhiteTurn = false;
        HighlightedBoxes.ClearHighlights();
    }

    void Update()
    {
        if (isPromoting)
        {
            if(isWhiteTurn) ui.show(ui.promotionCanvasWhite);
            else ui.show(ui.promotionCanvasBlack);
            HandlePromotionInput();
            return;
        }

        if(isCheckMate||isStaleMate||isResigned||isGameNotActive) {
            return;
        }

        if(board.WhiteHelper.Plies.Count==0) {
            if(board.WhiteHelper.CheckAttackBoard>0) {
                //Debug.Log(MagicBitboards.PrintBitBoard(board.WhiteHelper.CheckAttackBoard));
                //Debug.Log(MagicBitboards.PrintBitBoard(board.WhiteHelper.KingFleeBoard));
               // Debug.Log(MagicBitboards.PrintBitBoard(board.WhiteHelper.KingAttackBoard));
               // Debug.Log("White loses to checkmate");
                ui.gameOver("White loses to checkmate");
                isCheckMate = true;
            } else {
                Debug.Log("Stalemate");
                ui.gameOver("Stalemate");
                isStaleMate = true;
            }
            return;
        }
        if(board.BlackHelper.Plies.Count==0) {
            if(board.BlackHelper.CheckAttackBoard>0) {
                //Debug.Log(MagicBitboards.PrintBitBoard(board.BlackHelper.CheckAttackBoard));
                //Debug.Log(MagicBitboards.PrintBitBoard(board.BlackHelper.KingFleeBoard));
                //Debug.Log(MagicBitboards.PrintBitBoard(board.BlackHelper.KingAttackBoard));
                Debug.Log("Black loses to checkmate");
                ui.gameOver("Black loses to checkmate");
                isCheckMate = true;
            } else {
                Debug.Log("Stalemate");
                ui.gameOver("Stalemate");
                isStaleMate = true;
            }
            return;
        }

        // if(Input.GetKeyDown(KeyCode.Backspace)) {
        //     UndoPly();
        // }
        
        if(isWhiteTurn) {
            if(isWhiteAI)
                VisualizeMove((Ply)whiteAI.GetPly(Side.White));
            else {
                PlayerPly();
            }
        }
        else {
            if(isBlackAI) {
                VisualizeMove((Ply)blackAI.GetPly(Side.Black));
            }
            else
                PlayerPly();
        }
    }
    
    private void PlayerPly() {
        // Player
        if(Input.GetMouseButtonUp(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                if(hit.collider.GetComponent<ChessPiece>() == null) return;
                if(!isGrabbing) {
                    if(hit.collider.GetComponent<ChessPiece>().side != (isWhiteTurn ? Side.White : Side.Black)) {
                        return;
                    }
                    currentlySelected = hit.collider.GetComponent<ChessPiece>();
                    currentlySelected.transform.position += Vector3.up*2;
                    isGrabbing = true;
                    GrabbedPiece.Invoke(board.GetMoveLegal(currentlySelected));
                } else {
                    ChessPiece pressed = hit.collider.GetComponent<ChessPiece>();
                    if(pressed.Equals(currentlySelected)) {
                        pressed.transform.position -= Vector3.up*2;
                        GrabbedPiece.Invoke(0ul);
                        isGrabbing = false;
                        return;
                    }
                    if(pressed.side == currentlySelected.side) {
                        currentlySelected.transform.position -= Vector3.up*2;
                        currentlySelected = pressed;
                        currentlySelected.transform.position += Vector3.up*2;
                        GrabbedPiece.Invoke(board.GetMoveLegal(currentlySelected));
                        return;
                    }
                    if((1ul<<(pressed.idx.x + (7-pressed.idx.y)*8) & board.GetMoveLegal(currentlySelected)) == 0)
                        return;
                    isGrabbing = false;
                    Ply newPly = new Ply(currentlySelected.idx, pressed.idx, currentlySelected.type, pressed.type);
                    
                    VisualizeMove(newPly, currentlySelected, pressed);
                    if(!isPromoting) currentlySelected = null;
                }
            }
        }
    }

    // Plays the ply on the game objects
    private void VisualizeMove(Ply newPly, ChessPiece selected = null, ChessPiece pressed = null)
    {
        Debug.Log("Move: " + newPly.ToString());
        if (selected == null) selected = FindPiece(newPly.Type, newPly.Start);
        if (pressed == null) pressed = FindPiece(newPly.Captured, newPly.End);

        if (newPly.Captured != Piece.None)
        {
            taken.Push(pressed);
            pressed.transform.position -= Vector3.up * 12;
            pressed.idxBeforeDeath = pressed.idx;
            pressed.idx = new Vector2Int(-1, -1);
        }
        if ((newPly.Type == Piece.WPawn || selected.type == Piece.BPawn) && board.IsEnPassant(newPly.End))
        {
            newPly.Captured = isWhiteTurn ? Piece.BPawn : Piece.WPawn;
            taken.Push(enPassantable);
            enPassantable.transform.position -= Vector3.up * 12;
        }
        enPassantable = null;

        if ((newPly.Type == Piece.WPawn || newPly.Type == Piece.BPawn) && Vector2Int.Distance(newPly.End, newPly.Start) == 2)
        {
            enPassantable = selected;
        }

        // We be castling
        if ((newPly.Type == Piece.WKing || newPly.Type == Piece.BKing) && Math.Abs(newPly.End.x - newPly.Start.x) == 2)
        {
            if (newPly.End.x == 6)
            {
                ChessPiece rook = newPly.Side == Side.White ? FindPiece(Piece.WRook, new Vector2Int(7, 0)) : FindPiece(Piece.BRook, new Vector2Int(7, 7));
                rook.idx.x = 5;
                rook.transform.position = Board.IdxToPos(rook.idx);
            }
            if (newPly.End.x == 2)
            {
                ChessPiece rook = newPly.Side == Side.White ? FindPiece(Piece.WRook, new Vector2Int(0, 0)) : FindPiece(Piece.BRook, new Vector2Int(0, 7));
                rook.idx.x = 3;
                rook.transform.position = Board.IdxToPos(rook.idx);
            }
        }

        if (
            (newPly.Type == Piece.WPawn && newPly.End.y == 7)
            || (newPly.Type == Piece.BPawn && newPly.End.y == 0)
        )
        {
            if (newPly.PromoteType != null)
            {
                selected.Promote((Piece)newPly.PromoteType);
            }
            else
            {
                selected.transform.position = Board.IdxToPos(newPly.End);
                selected.idx = newPly.End;
                pendingPromotionPly = newPly;
                isPromoting = true;
                return;
            }
        }

        selected.transform.position = Board.IdxToPos(newPly.End);
        selected.idx = newPly.End;

        moved.Push(selected);

        board.PlayPly(newPly);
        plies.Push(newPly);
        PlayedPly?.Invoke(newPly);
        if (!Help())
        {
            // Debug.Log(MagicBitBoards.PrintBitBoard(board.boards     ))
        }
        

            isWhiteTurn = !isWhiteTurn;
            ulong zMap = ZobristMap.GetZKey(board.boards, board.castleTracker, board.passantTrack, newPly.Side == Side.White);
            if (newPly.isIrreversible())
            {
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
            if (isThreefold)
            {
                Debug.Log("THREEFOLD");
                ui.gameOver("Threefold Repetition");
                isStaleMate = true;
            }
        //Debug.Log("White: " + MagicBitboards.PrintBitBoard(board.WhiteHelper.CheckAttackBoard));
        //Debug.Log("Black: " + MagicBitboards.PrintBitBoard(board.BlackHelper.CheckAttackBoard));
        //Debug.Log("PassantTrack: " + board.passantTrack);
    }
    
    
    public ChessPiece FindPiece(Piece type, Vector2Int idx) {
        if(type == Piece.None) return null;
        foreach(ChessPiece piece in SpawnPieces.instance.pieces[(int)type]) {
            if(piece.idx == idx) return piece;
        }
        return null;
    }
    
    public void UndoPly() {
        if(plies.Count == 0)
            return;

        Ply undoPly = plies.Pop();
        board.UndoPly(undoPly);
        ChessPiece p = moved.Pop();
        p.transform.position = Board.IdxToPos(undoPly.Start);
        if(undoPly.Captured != Piece.None) {
            ChessPiece realivePiece = taken.Pop();
            realivePiece.transform.position += Vector3.up*12;
            realivePiece.idx = realivePiece.idxBeforeDeath;
        }
        isWhiteTurn = !isWhiteTurn;
    }

    private bool Help() {
        ulong[] fakeboard = new ulong[12];
        for(int i = 0; i < 12; i++) {
            List<ChessPiece> currentPieces = SpawnPieces.instance.pieces[i];
            foreach(ChessPiece p in currentPieces) {
                if(p.idx == Vector2Int.one*-1) continue;
                int sq = p.idx.x + 8*(7-p.idx.y);
                fakeboard[i] |= 1ul<<sq;
            }
            if(fakeboard[i] != board.boards[i]) {
                Debug.LogWarning((Piece)i + " " + MagicBitboards.PrintBitBoard(board.boards[i]));
                Debug.LogWarning(MagicBitboards.PrintBitBoard(fakeboard[i]));
                return false;
            }
        }
        return true;
    }
    private void HandlePromotionInput() {
        if (ui.promoteBishop==true) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WBishop : Piece.BBishop;
        } else if (ui.promoteRook==true) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WRook : Piece.BRook;
        } else if (ui.promoteKnight==true) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WKnight : Piece.BKnight;
        } else if (ui.promoteQueen==true) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WQueen : Piece.BQueen;
        } else {
            return;
        }

        currentlySelected.Promote((Piece)pendingPromotionPly.PromoteType);
        moved.Push(currentlySelected);
        currentlySelected = null;

        board.PlayPly(pendingPromotionPly);
        plies.Push(pendingPromotionPly);
        isWhiteTurn = !isWhiteTurn;
        isPromoting = false;
        ui.hide(ui.promotionCanvasWhite);
        ui.hide(ui.promotionCanvasBlack);
        ui.resetPromotionValues();
    }

    public void resign(Side resigningSide)
    {
        string winner = resigningSide == Side.White ? "Black" : "White";
        Debug.Log($"{resigningSide} resigns. {winner} wins!");
        ui.gameOver($"{resigningSide} resigns. {winner} wins!");
        isResigned = true; 
    }
}