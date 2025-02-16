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

    public static BoardManager instance;

    void Start()
    {
        instance = this;
        board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.position+ray.direction*100);
            if(Physics.Raycast(ray, out hit)) {
                Debug.Log(hit.collider.GetComponent<ChessPiece>().type);
                if(!isGrabbing) {
                    currentlySelected = hit.collider.GetComponent<ChessPiece>();
                    currentlySelected.transform.position += Vector3.up*2;
                    isGrabbing = true;
                } else {
                    isGrabbing = false;
                    ChessPiece pressed = hit.collider.GetComponent<ChessPiece>();
                    Ply newPly = new Ply(currentlySelected.idx, pressed.idx, currentlySelected.type);
                    if(pressed.type != Piece.None) {
                        newPly.Captured = pressed.type;
                        taken.Push(pressed);
                        pressed.transform.position -= Vector3.up*10;
                    }
                    currentlySelected.transform.position = Board.IdxToPos(newPly.End);
                    board.PlayPly(newPly);
                    plies.Push(newPly);
                }
            }
        }
    }
    
    public void UndoPly() {
        Ply undoPly = plies.Pop();
        board.UndoPly(undoPly);
        if(undoPly.Captured != Piece.None) {
            ChessPiece realivePiece = taken.Pop();
            realivePiece.transform.position += Vector3.up*10;
        }
    }
}
