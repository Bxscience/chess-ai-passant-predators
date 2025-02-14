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
    Board,
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
        //board is 38.5 x 38.5, centered at 0,0
        int loopruns = 0;
        GameObject.Instantiate(Board, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        
        GameObject[] PiecesArray = new GameObject[] { WhitePawn, BlackPawn, WhiteRook, BlackRook, WhiteKnight, BlackKnight, WhiteBishop, BlackBishop, WhiteQueen, BlackQueen, WhiteKing, BlackKing };
        
        for (float i = 0; i < 44; i+=5.5f)
        {
            WhitePawns[loopruns]= GameObject.Instantiate(WhitePawn, new Vector3(i-19.25f, 2, -13.75f), Quaternion.Euler(-90, 0, 0));
            WhitePieces[loopruns] = WhitePawns[loopruns];
            BlackPawns[loopruns] = GameObject.Instantiate(BlackPawn, new Vector3(i - 19.25f, 2, 13.75f), Quaternion.Euler(-90, 0, 0));
            BlackPieces[loopruns] = BlackPawns[loopruns];
            loopruns++;
        }
        loopruns = 0;
        int p = 2;
        int x = 0;
        for (float i = 0; i < 16.5f; i += 5.5f)
        {
            if (p == 6)
            {
                x = 180;
            }
            GameObject.Instantiate(PiecesArray[p], new Vector3(i - 19.25f, 2, -19.75f), Quaternion.Euler(-90, x+-180, 0));
            GameObject.Instantiate(PiecesArray[p], new Vector3(19.25f-i, 2, -19.75f), Quaternion.Euler(-90, x+-180, 0));
            GameObject.Instantiate(PiecesArray[p+1], new Vector3(i-19.25f, 2, 19.75f), Quaternion.Euler(-90, x, 0));
            GameObject.Instantiate(PiecesArray[p + 1], new Vector3(19.25f - i, 2, 19.75f), Quaternion.Euler(-90, x, 0));
            p += 2;
            if (p == 6)
            {
                x = 0;
            }
    
            
        }
        GameObject.Instantiate(PiecesArray[p], new Vector3(-2.75f, 2, -19.75f), Quaternion.Euler(-90, 0, 0));
        GameObject.Instantiate(PiecesArray[p+1], new Vector3(-2.75f, 2, 19.75f), Quaternion.Euler(-90, 0, 0));
        p += 2;
        GameObject.Instantiate(PiecesArray[p], new Vector3(2.75f, 2, -19.75f), Quaternion.Euler(-90, 0, 0));
        GameObject.Instantiate(PiecesArray[p + 1], new Vector3(2.75f, 2, 19.75f), Quaternion.Euler(-90, 0, 0));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
