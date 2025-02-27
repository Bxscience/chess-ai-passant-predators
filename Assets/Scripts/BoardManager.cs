using System;
using System.Collections;
using System.Collections.Generic;
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
        board = new Board("rnbqkbnr/pppppppp/P7/1K6/8/8/PPPPPPPP/RNBQKBNR");
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
        
        // Player
        if(Input.GetMouseButtonUp(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
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
                    if((1ul<<(pressed.idx.x + (7-pressed.idx.y)*8) & board.GetMoveParalegal(currentlySelected)) == 0)
                        return;
                    isGrabbing = false;
                    Ply newPly = new Ply(currentlySelected.idx, pressed.idx, currentlySelected.type);
                    if(pressed.type != Piece.None) {
                        newPly.Captured = pressed.type;
                        taken.Push(pressed);
                        pressed.transform.position -= Vector3.up*10;
                    }
                    if(board.IsEnPassant(pressed.idx)) {
                        newPly.Captured = isWhiteTurn ? Piece.BPawn : Piece.WPawn;
                        taken.Push(enPassantable);
                        enPassantable.transform.position -= Vector3.up*10;
                    }
                    currentlySelected.transform.position = Board.IdxToPos(newPly.End);
                    currentlySelected.idx = newPly.End;
                    enPassantable = null;

                    if(currentlySelected.type == Piece.WPawn && currentlySelected.idx.y == 7) {
                        pendingPromotionPly = newPly;
                        isPromoting = true;
                        return;
                    }

                    if(currentlySelected.type == Piece.BPawn && currentlySelected.idx.y == 0) {
                        pendingPromotionPly = newPly;
                        isPromoting = true;
                        return;
                    }
                    
                    if((newPly.Type == Piece.WPawn || newPly.Type == Piece.BPawn) && Vector2Int.Distance(newPly.End, newPly.Start) == 2) {
                        enPassantable = currentlySelected;
                    }

                    moved.Push(currentlySelected);
                    currentlySelected.moved = true;
                    currentlySelected = null;

                    board.PlayPly(newPly);
                    plies.Push(newPly);
                    isWhiteTurn = !isWhiteTurn;
                }
            }
        }
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
            realivePiece.transform.position += Vector3.up*10;
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
        currentlySelected.moved = true;
        currentlySelected = null;

        board.PlayPly(pendingPromotionPly);
        plies.Push(pendingPromotionPly);
        isWhiteTurn = !isWhiteTurn;
        isPromoting = false;
    }
}
