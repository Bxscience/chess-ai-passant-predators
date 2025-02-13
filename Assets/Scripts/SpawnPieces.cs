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

    //BOARD: -19.25, -13.75, -8.25, -2.75, 2.75, 8.25, 13.75, 19.25

    void Start()
    {
        //board is 38.5 x 38.5, centered at 0,0
        GameObject.Instantiate(Board, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        for (float i = 0; i < 44; i+=5.5f)
        {
            GameObject.Instantiate(WhitePawn, new Vector3(i-19.25f, 2, -19.5f), Quaternion.Euler(-90, 0, 0));
            Debug.Log(i - 19.25f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
