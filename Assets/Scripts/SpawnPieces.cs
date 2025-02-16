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

    GameObject[] WhitePawns = new GameObject[8];
    GameObject[] WhitePieces = new GameObject[16];
    GameObject[] BlackPawns = new GameObject[8];
    GameObject[] BlackPieces = new GameObject[16];

    void Start()
    {
        GameObject.Instantiate(BoardPrefab, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        for(int i = 0; i <= 11; i++) {
            Piece type = (Piece)i;
            ulong board = BoardManager.instance.board.boards[i];
            for(int j = 0; j < 64; j++) {
                if((board & 1ul<<j) == 1ul<<j) {
                    Vector2Int pos = new Vector2Int(j%8, 7-j/8);
                    GameObject piece = Instantiate(GOFromType(type), Board.IdxToPos(pos), Quaternion.Euler(-90,0,0));
                    piece.GetComponent<ChessPiece>().idx = pos;
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

    void Attempt1() {
        //board is 38.5 x 38.5, centered at 0,0
        int loopruns = 0;
        GameObject.Instantiate(BoardPrefab, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        
        GameObject[] PiecesArray = new GameObject[] { WhitePawn, BlackPawn, WhiteRook, BlackRook, WhiteKnight, BlackKnight, WhiteBishop, BlackBishop, WhiteQueen, BlackQueen, WhiteKing, BlackKing };
        
        for (float i = 0; i < 8; i++)
        {
            WhitePawns[loopruns] = GameObject.Instantiate(WhitePawn, new Vector3(i*5.5f-19.25f, 2, -13.75f), Quaternion.Euler(-90, 0, 0));
            WhitePawns[loopruns].GetComponent<ChessPiece>().idx = new Vector2Int((int)i, 1);
            WhitePieces[loopruns] = WhitePawns[loopruns];

            BlackPawns[loopruns] = GameObject.Instantiate(BlackPawn, new Vector3(i*5.5f - 19.25f, 2, 13.75f), Quaternion.Euler(-90, 0, 0));
            BlackPawns[loopruns].GetComponent<ChessPiece>().idx = new Vector2Int((int)i, 6);
            BlackPieces[loopruns] = BlackPawns[loopruns];
            loopruns++;
        }
        int p = 2;
        int x = 0;
        for (float i = 0; i < 16.5f; i += 5.5f)
        {
            if (p == 6)
            {
                x = 180;
            }
            WhitePieces[loopruns] = GameObject.Instantiate(PiecesArray[p], new Vector3(i - 19.25f, 2, -19.75f), Quaternion.Euler(-90, x+-180, 0));
            loopruns++;
            WhitePieces[loopruns] = GameObject.Instantiate(PiecesArray[p], new Vector3(19.25f-i, 2, -19.75f), Quaternion.Euler(-90, x+-180, 0));
            loopruns++;
            BlackPieces[loopruns] = GameObject.Instantiate(PiecesArray[p+1], new Vector3(i-19.25f, 2, 19.75f), Quaternion.Euler(-90, x, 0));
            loopruns++;
            BlackPieces[loopruns] = GameObject.Instantiate(PiecesArray[p + 1], new Vector3(19.25f - i, 2, 19.75f), Quaternion.Euler(-90, x, 0));
            loopruns++;

            p += 2;
            if (p == 6)
            {
                x = 0;
            }
    
            
        }
        WhitePieces[loopruns] = GameObject.Instantiate(PiecesArray[p], new Vector3(-2.75f, 2, -19.75f), Quaternion.Euler(-90, 0, 0));
        loopruns++;
        BlackPieces[loopruns] = GameObject.Instantiate(PiecesArray[p+1], new Vector3(-2.75f, 2, 19.75f), Quaternion.Euler(-90, 0, 0));
        loopruns++;
        p += 2;
        WhitePieces[loopruns] = GameObject.Instantiate(PiecesArray[p], new Vector3(2.75f, 2, -19.75f), Quaternion.Euler(-90, 0, 0));
        loopruns++;
        BlackPieces[loopruns] = GameObject.Instantiate(PiecesArray[p + 1], new Vector3(2.75f, 2, 19.75f), Quaternion.Euler(-90, 0, 0)); 

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
