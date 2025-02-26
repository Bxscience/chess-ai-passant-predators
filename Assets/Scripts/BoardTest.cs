using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTest : MonoBehaviour
{
    void Start()
    {
        MagicBitboards.GenerateMagicNumbers();
        foreach(Magics m in MagicBitboards.RookMagics) {
            string s = "";
            foreach(ulong move in m.moves) s += " " +move;
            Debug.Log("The magic: " + m.magic + " & the items in mvoes: " + s);
        }
    }

    void Update()
    {
        
    }
}
