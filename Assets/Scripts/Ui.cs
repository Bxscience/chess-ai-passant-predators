using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class Ui : MonoBehaviour
{
    public GameObject startCanvasObject, whiteOrBlack, playingGameUi, gameOverCanvas, promotionCanvasWhite, promotionCanvasBlack, whiteDifficulty, blackDifficulty;
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
    hide(whiteDifficulty);
    hide(blackDifficulty);
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
        }
        hide(whiteOrBlack);
        show(playingGameUi);
    }
    public void sideChosenWhite()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isBlackAI = true; // Set AI for black
        }
        hide(whiteOrBlack);
        show(playingGameUi);
    }
    public void aiVsAi(){
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isBlackAI = true;
            BoardManager.instance.isWhiteAI = true;
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
        }
        hide(startCanvasObject);
        show(playingGameUi);
    }
    public void shadman()
    {
        BoardManager.instance.whiteAI.setDifficulty("shadman");
        BoardManager.instance.blackAI.setDifficulty("shadman");
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void dahik()
    {
        BoardManager.instance.whiteAI.setDifficulty("dahik");
        BoardManager.instance.blackAI.setDifficulty("dahik");
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void dhruv()
    {
        BoardManager.instance.whiteAI.setDifficulty("dhruv");
        BoardManager.instance.blackAI.setDifficulty("dhruv");
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void elisha()
    {
        BoardManager.instance.whiteAI.setDifficulty("elisha");
        BoardManager.instance.blackAI.setDifficulty("elisha");
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void rohan()
    {
        BoardManager.instance.whiteAI.setDifficulty("rohan");   
        BoardManager.instance.blackAI.setDifficulty("rohan");
        hide(startCanvasObject);
        show(whiteOrBlack);
    }
    public void whiteDiff()
    {
        if (BoardManager.instance != null)
        {
            BoardManager.instance.isBlackAI = true;
            BoardManager.instance.isWhiteAI = true;
        }
        hide(startCanvasObject);
        show(whiteDifficulty);
    }
    public void shadman1() { BoardManager.instance.whiteAI.setDifficulty("shadman");
        hide(whiteDifficulty);
        show(blackDifficulty);
    }
    public void dahik1() { BoardManager.instance.whiteAI.setDifficulty("dahik");
        hide(whiteDifficulty);
        show(blackDifficulty);
    }
    public void dhruv1() { BoardManager.instance.whiteAI.setDifficulty("dhruv");
        hide(whiteDifficulty);
        show(blackDifficulty);
    }
    public void elisha1() { BoardManager.instance.whiteAI.setDifficulty("elisha");
        hide(whiteDifficulty);
        show(blackDifficulty);
    }
    public void rohan1() {
        BoardManager.instance.whiteAI.setDifficulty("rohan");
        hide(whiteDifficulty);
        show(blackDifficulty);
    }
    public void shadman2()
    {
        BoardManager.instance.blackAI.setDifficulty("shadman");
        hide(blackDifficulty);
        show(playingGameUi);
    }
    public void dahik2()
    {
        BoardManager.instance.blackAI.setDifficulty("dahik");
        hide(blackDifficulty);
        show(playingGameUi);
    }
    public void dhruv2()
    {
        BoardManager.instance.blackAI.setDifficulty("dhruv");
        hide(blackDifficulty);
        show(playingGameUi);
    }
    public void elisha2()
    {
        BoardManager.instance.blackAI.setDifficulty("elisha");
        hide(blackDifficulty);
        show(playingGameUi);
    }
    public void rohan2()
    {
        BoardManager.instance.blackAI.setDifficulty("rohan");
        hide(blackDifficulty);
        show(playingGameUi);
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