using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTest : MonoBehaviour
{
    void Start()
    {
        Board b = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        ulong finalB = 0;
        for(int i = 0; i < b.boards.Length; i++) {
            finalB |= b.boards[i];
        }
        // Debug.Log(finalB);
        Debug.Log(b.boards[(int)Piece.BQueen]);
        // Debug.Log(b.BlackPieces);
        // Debug.Log(b.IdxToPos(1,2));
    }

    void Update()
    {
        
    }
}
