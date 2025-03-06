using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Board board;
    bool isWhiteTurn = true;
    bool isGrabbing;
    ChessPiece currentlySelected;
    ChessPiece enPassantable;
    Stack<Ply> plies = new Stack<Ply>();
    Stack<ChessPiece> taken = new Stack<ChessPiece>();
    Stack<ChessPiece> moved = new Stack<ChessPiece>();
    
    public static BoardManager instance;
    public event Action PlayedPly;

    public ChessPiece[] pieceBoard = new ChessPiece[64];
   

    private bool isPromoting = false;
    private Ply pendingPromotionPly;

    void Start()
    {
        instance = this;
        board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        MagicBitboards.GenerateMagicNumbers();
    }

    void Update()
    {
        if (isPromoting)
        {
            HandlePromotionInput();
            return;
        }

        if(Input.GetKeyDown(KeyCode.Backspace)) {
            Debug.Log("AAA");
            UndoPly();
        }
        
        if(isWhiteTurn) {
            PlayerPly();
        }
        else {
            // PlayerPly();
            int rand = UnityEngine.Random.Range(0, board.BlackHelper.Plies.Count);
            VisualizeMove(board.BlackHelper.Plies[rand]);
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
    
    private void VisualizeMove(Ply newPly, ChessPiece selected = null, ChessPiece pressed = null) {
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
                Debug.Log(pendingPromotionPly.End);
                isPromoting = true;
                return;
            }
        }

        selected.transform.position = Board.IdxToPos(newPly.End);
        selected.idx = newPly.End;

        moved.Push(selected);

        board.PlayPly(newPly);
        plies.Push(newPly);
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

        Debug.Log(pendingPromotionPly.PromoteType);
        Debug.Log(pendingPromotionPly.End);
        board.PlayPly(pendingPromotionPly);
        plies.Push(pendingPromotionPly);
        isWhiteTurn = !isWhiteTurn;
        isPromoting = false;
    }
}
