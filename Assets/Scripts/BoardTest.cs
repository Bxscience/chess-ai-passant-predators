using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTest : MonoBehaviour
{
    void Start()
    {
        MagicBitboards.GenerateMagicNumbers();
        foreach(Magics m in MagicBitboards.RookMagics) {
            Debug.Log(m.magic + " " + m.moves.ToString());
        }
    }

    void Update()
    {
        
    }
}
