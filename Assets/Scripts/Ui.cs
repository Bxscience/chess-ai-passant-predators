using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui : MonoBehaviour
{
    public void resignButton()
    {
        Debug.Log("Resign button pressed");
        if (BoardManager.instance != null)
        {
            Side resigningSide = BoardManager.instance.isWhiteTurn ? Side.White : Side.Black;
            BoardManager.instance.resign(resigningSide);
        }
    }
    public void sideChosenBlack(){
        isWhiteAI = true;
    }
    public void sideChosenWhite(){
        isBlackAI = true;
    }
    public void easy(){
        SetDifficulty("easy");
    }
    public void medium(){
        SetDifficulty("medium");
    }
    public void hard(){
        SetDifficulty("hard");

    }
}
 