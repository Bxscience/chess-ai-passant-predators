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


    void Start()
    {
        //board is 0.4 by 0.4, centered at 0,0
        GameObject.Instantiate(Board, new Vector3(0,0,0), Quaternion.Euler(-90,0,0));
        for (float i = 0; i < 0.46; i+=0.0575f)
        {
            GameObject.Instantiate(WhitePawn, new Vector3(i-0.195f, 0.0175f, -0.125f), Quaternion.Euler(-90, 0, 0));
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
