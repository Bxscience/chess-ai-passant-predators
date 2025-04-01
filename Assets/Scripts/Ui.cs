using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class Ui : MonoBehaviour
{
    public GameObject startCanvasObject, whiteOrBlack, playingGameUi, gameOverCanvas, promotionCanvasWhite, promotionCanvasBlack;
    public TMP_Text textWinnerLoser;
    public GameObject Camera; 
    public bool humanGame = false;
    private bool currentSideIsWhite = true;
    public bool promoteQueen,promoteRook,promoteKnight,promoteBishop = false;
void Start(){
    show(startCanvasObject);
    hide(whiteOrBlack);
    hide(gameOverCanvas);
    hide(playingGameUi);
    hide(promotionCanvasBlack);
    hide(promotionCanvasWhite);
    
}

void Update(){
    if (Input.GetKeyDown(KeyCode.F)){
        flipBoard();
    }
    if(humanGame){
        if (currentSideIsWhite != BoardManager.instance.isWhiteTurn)
            {
                currentSideIsWhite = BoardManager.instance.isWhiteTurn; // Update the current side
                flipBoard(); // Flip the board
            }
    }
}
    public void closeButton(){
        hide(gameOverCanvas);
        show(playingGameUi);
    }
    public void backToStart(){
        hide(whiteOrBlack);
        show(startCanvasObject);
    }
    public void gameOver(string overReason){
        hide(playingGameUi);
        show(gameOverCanvas);
        BoardManager.instance.isGameNotActive = true;
        textWinnerLoser.text = overReason;
    }
    public void menuButton(){
        BoardManager.instance.Reset();
        SpawnPieces.instance.Clear();
        SpawnPieces.instance.Fill();
        hide(playingGameUi);
        hide(gameOverCanvas);
        show(startCanvasObject);
        humanGame =false;
    }
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
        hide(whiteOrBlack);
        show(playingGameUi);
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
        hide(whiteOrBlack);
        show(playingGameUi);
    }
    public void aiVsAi(){
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isBlackAI = true;
            BoardManager.instance.isWhiteAI = true;
            Debug.Log("AI will control the Both sides.");
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
        hide(whiteOrBlack);
        show(playingGameUi);
    }
    public void humanHuman(){
        if (BoardManager.instance != null)
        {
            humanGame = true;
            BoardManager.instance.isBlackAI = false;
            BoardManager.instance.isWhiteAI = false;
            Debug.Log("Humans will control the Both sides.");
        }
        else
        {
            Debug.LogError("BoardManager instance is null.");
        }
        hide(startCanvasObject);
        show(playingGameUi);
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
        hide(startCanvasObject);
        show(whiteOrBlack);
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
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void hard()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.whiteAI.setDifficulty("hard");
            BoardManager.instance.blackAI.setDifficulty("hard");
            Debug.Log("AI difficulty set to Hard");
        }
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void hide(GameObject gameObject){
        gameObject.SetActive(false);
    }
    public void show(GameObject gameObject){
        gameObject.SetActive(true);
        if (gameObject == playingGameUi){
            BoardManager.instance.isGameNotActive = false;
        }
    }
    public void flipBoard(){
        startCanvasObject.transform.Rotate(0,0,180);
        whiteOrBlack.transform.Rotate(0,0,180);
        gameOverCanvas.transform.Rotate(0,0,180);
        playingGameUi.transform.Rotate(0, 0, 180);
        promotionCanvasBlack.transform.Rotate(0,0,180);
        promotionCanvasWhite.transform.Rotate(0,0,180);
        Camera.transform.Rotate(0, 0, 180);
    }

    public void promoteToQueen(){
        promoteQueen = true;
    }
    public void promoteToRook(){
        promoteRook = true;
    }
    public void promoteToBishop(){
        promoteBishop = true;
    }
    public void promoteToKnight(){
        promoteKnight = true;
    }
    
    public void resetPromotionValues(){
        promoteBishop=false;
        promoteQueen=false;
        promoteRook=false;
        promoteKnight=false;
    }
    
}