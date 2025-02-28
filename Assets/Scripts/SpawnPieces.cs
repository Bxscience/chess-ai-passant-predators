using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class SpawnPieces : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject 
    BoardPrefab,
    WhitePawn,
    WhiteBishop,
    WhiteKnight,
    WhiteRook,
    WhiteQueen,
    WhiteKing,
    BlackPawn,
    BlackBishop,
    BlackKnight,
    BlackRook,
    BlackQueen,
    BlackKing;

    //BOARD: -19.25, -13.75, -8.25, -2.75, 2.75, 8.25, 13.75, 19.25, this is in both directions, x and z. Pieces should be at y=2

    public List<ChessPiece>[] pieces = new List<ChessPiece>[12];
    public static SpawnPieces instance;


    void Start()
    {
        instance = this;
        GameObject.Instantiate(BoardPrefab, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        for(int i = 0; i <= 11; i++) {
            pieces[i] = new List<ChessPiece>();
            Piece type = (Piece)i;
            ulong board = BoardManager.instance.board.boards[i];
            for(int j = 0; j < 64; j++) {
                if((board & 1ul<<j) == 1ul<<j) {
                    Vector2Int pos = new Vector2Int(j%8, 7-j/8);
                    GameObject piece = Instantiate(GOFromType(type), Board.IdxToPos(pos), Quaternion.Euler(-90,0,0));
                    piece.GetComponent<ChessPiece>().idx = pos;
                    pieces[((int)type)]
                        .Add(piece.GetComponent<ChessPiece>());
                }
            }
        }
    }
    
    GameObject GOFromType(Piece type) => type switch
    {
        Piece.WPawn => WhitePawn,
        Piece.WBishop => WhiteBishop,
        Piece.WKnight => WhiteKnight,
        Piece.WRook => WhiteRook,
        Piece.WQueen => WhiteQueen,
        Piece.WKing => WhiteKing,
        Piece.BPawn => BlackPawn,
        Piece.BBishop => BlackBishop,
        Piece.BKnight => BlackKnight,
        Piece.BRook => BlackRook,
        Piece.BQueen => BlackQueen,
        Piece.BKing => BlackKing,
        _ => throw new NotImplementedException()
    };

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
