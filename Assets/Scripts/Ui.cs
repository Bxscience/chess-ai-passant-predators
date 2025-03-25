using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public void sideChosenBlack()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isWhiteAI = true; // Set AI for white
            Debug.Log("AI will control the white side.");
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
    }

    public void sideChosenWhite()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isBlackAI = true; // Set AI for black
            Debug.Log("AI will control the black side.");
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
    }

    public void easy()
    {
        if (BoardManager.instance != null)
        {
            if (BoardManager.instance.whiteAI != null && BoardManager.instance.blackAI != null)
            {
                BoardManager.instance.whiteAI.setDifficulty("easy");
                BoardManager.instance.blackAI.setDifficulty("easy");
                Debug.Log("AI difficulty set to Easy");
            }
            else
            {
                Debug.LogError("AI instances are null.");
            }
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
    }

    public void medium()
    {
        if (BoardManager.instance != null)
        {
            if (BoardManager.instance.whiteAI != null && BoardManager.instance.blackAI != null)
            {
                BoardManager.instance.whiteAI.setDifficulty("medium");
                BoardManager.instance.blackAI.setDifficulty("medium");
                Debug.Log("AI difficulty set to Medium");
            }
            else
            {
                Debug.LogError("AI instances are null.");
            }
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
    }

    public void hard()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.whiteAI.setDifficulty("hard");
            BoardManager.instance.blackAI.setDifficulty("hard");
            Debug.Log("AI difficulty set to Hard");
        }
    }
}