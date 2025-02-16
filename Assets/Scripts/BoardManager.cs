using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    Board board;
    bool isWhiteTurn = true;
    bool isGrabbing;
    ChessPiece currentlySelected;
    Stack<Ply> plies = new Stack<Ply>();
    Stack<ChessPiece> taken = new Stack<ChessPiece>();
    Stack<ChessPiece> moved = new Stack<ChessPiece>();

    public static BoardManager instance;
    public event Action PlayedPly;

    void Start()
    {
        instance = this;
        board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
    }

    void Update()
    {
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
                    if(pressed.side == currentlySelected.side) {
                        return;
                    }
                    isGrabbing = false;
                    Ply newPly = new Ply(currentlySelected.idx, pressed.idx, currentlySelected.type);
                    if(pressed.type != Piece.None) {
                        newPly.Captured = pressed.type;
                        taken.Push(pressed);
                        pressed.transform.position -= Vector3.up*10;
                    }
                    currentlySelected.transform.position = Board.IdxToPos(newPly.End);
                    currentlySelected.idx = newPly.End;

                    moved.Push(currentlySelected);
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
    }
}
