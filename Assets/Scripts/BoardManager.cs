using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Board board;
    bool isWhiteTurn = true;
    bool isWhiteAI = false;
    bool isBlackAI = true;
    bool isCheckMate = false;
    bool isStaleMate = false;
    bool isGrabbing;
    ChessPiece currentlySelected;
    ChessPiece enPassantable;
    Stack<Ply> plies = new Stack<Ply>();
    Stack<ChessPiece> taken = new Stack<ChessPiece>();
    Stack<ChessPiece> moved = new Stack<ChessPiece>();

    AI blackAI = new AI();
    
    public static BoardManager instance;
    public event Action<Ply> PlayedPly;

    public ChessPiece[] pieceBoard = new ChessPiece[64];
   

    private bool isPromoting = false;
    private Ply pendingPromotionPly;
    //Standard Fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    //Mate in 1 I think: "8/8/8/8/8/8/rr6/k1K5 w - - 0 1"
    //Black 2 rooks vs white king: "8/8/8/8/4K3/8/rr6/k7 w - - 0 1"
    //using other fen seems to break the game
    void Start()
    {
        instance = this;
        MagicBitboards.GenerateMagicNumbers();
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        board = new Board(fen);
        if(fen.Split(" ")[1][0] == 'w')
            isWhiteTurn = true;
        else isWhiteTurn = false;
    }

    void Update()
    {
        if (isPromoting)
        {
            HandlePromotionInput();
            return;
        }

        if(isCheckMate||isStaleMate) {
            return;
        }

        if(board.WhiteHelper.Plies.Count==0) {
            if(board.WhiteHelper.CheckAttackBoard>0) {
                Debug.Log("White loses to checkmate");
                isCheckMate = true;
            } else {
                Debug.Log("Stalemate");
                isStaleMate = true;
            }
            return;
        }
        if(board.BlackHelper.Plies.Count==0) {
            if(board.BlackHelper.CheckAttackBoard>0) {
                Debug.Log("Black loses to checkmate");
                isCheckMate = true;
            } else {
                Debug.Log("Stalemate");
                isStaleMate = true;
            }
            return;
        }

        if(Input.GetKeyDown(KeyCode.Backspace)) {
            UndoPly();
        }
        
        if(isWhiteTurn) {
            if(isWhiteAI)
                VisualizeMove((Ply)blackAI.GetPly(Side.White));
            else
                PlayerPly();

        }
        else {
            if(isBlackAI)
                VisualizeMove((Ply)blackAI.GetPly(Side.Black));
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
                } else {
                    ChessPiece pressed = hit.collider.GetComponent<ChessPiece>();
                    if(pressed.Equals(currentlySelected)) {
                        pressed.transform.position -= Vector3.up*2;
                        isGrabbing = false;
                        return;
                    }
                    if(pressed.side == currentlySelected.side) {
                        currentlySelected.transform.position -= Vector3.up*2;
                        currentlySelected = pressed;
                        currentlySelected.transform.position += Vector3.up*2;
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
    private void VisualizeMove(Ply newPly, ChessPiece selected = null, ChessPiece pressed = null) {
        Debug.Log("Move: " + newPly.ToString());
        if(selected == null) selected = FindPiece(newPly.Type, newPly.Start);
        if(pressed == null) pressed = FindPiece(newPly.Captured, newPly.End);

        if(newPly.Captured != Piece.None) {
            taken.Push(pressed);
            pressed.transform.position -= Vector3.up*12;
            pressed.idxBeforeDeath = pressed.idx;
            pressed.idx = new Vector2Int(-1, -1);
        }
        if((newPly.Type == Piece.WPawn || selected.type == Piece.BPawn) && board.IsEnPassant(newPly.End)) {
            newPly.Captured = isWhiteTurn ? Piece.BPawn : Piece.WPawn;
            taken.Push(enPassantable);
            enPassantable.transform.position -= Vector3.up*12;
        }
        enPassantable = null;
        
        if((newPly.Type == Piece.WPawn || newPly.Type == Piece.BPawn) && Vector2Int.Distance(newPly.End, newPly.Start) == 2) {
            enPassantable = selected;
        }
        
        // We be castling
        if ((newPly.Type == Piece.WKing || newPly.Type == Piece.BKing) && Math.Abs(newPly.End.x - newPly.Start.x) == 2) {
            if(newPly.End.x == 6) {
                ChessPiece rook = newPly.Side == Side.White ? FindPiece(Piece.WRook, new Vector2Int(7, 0)) : FindPiece(Piece.BRook, new Vector2Int(7, 7));
                rook.idx.x = 5;
                rook.transform.position = Board.IdxToPos(rook.idx);
            }
            if(newPly.End.x == 2) {
                ChessPiece rook = newPly.Side == Side.White ? FindPiece(Piece.WRook, new Vector2Int(0, 0)) : FindPiece(Piece.BRook, new Vector2Int(0, 7));
                rook.idx.x = 3;
                rook.transform.position = Board.IdxToPos(rook.idx);
            }
        }

        if(
            (newPly.Type == Piece.WPawn && newPly.End.y == 7)
            || (newPly.Type == Piece.BPawn && newPly.End.y == 0)
        ) {
            if(newPly.PromoteType != null) {
                selected.Promote((Piece)newPly.PromoteType);
            } else {
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
        if(!Help()) {
            // Debug.Log(MagicBitBoards.PrintBitBoard(board.boards     ))
        }
        isWhiteTurn = !isWhiteTurn;
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WBishop : Piece.BBishop;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WRook : Piece.BRook;
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            pendingPromotionPly.PromoteType = isWhiteTurn ? Piece.WKnight : Piece.BKnight;
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
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
    }
}
