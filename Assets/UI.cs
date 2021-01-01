using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {

	/* UI Text displaying the score. */
	public GameObject scoreText;
	/* The actual score. */
	public int score;

	/* UI Text displaying the moves remaining. */
	public GameObject movesText;
	/* The actual moves remaining. */
	public int movesRemaining;

	/* Check whether the game is finished. */
	public static bool finished;

	[SerializeField]
	public Board board;

	public GameObject endCard;
	public GameObject endText;
	public GameObject menuButton;
	public GameObject menuButton2;

    //--------------------------------------------------------------------------------

    void Start() {
    	endCard.SetActive(false);
        finished = false;
    }

    void Update() {
    	if (finished) {return;}
    	score = board.reportScore();
    	bool lastMove = board.reportFinished();
    	scoreText.GetComponent<Text>().text = "Score: " + score.ToString();
    	movesRemaining = board.reportMoves();
    	movesText.GetComponent<Text>().text = "Moves: " + movesRemaining.ToString();
    	if (movesRemaining <= 0 && lastMove) {
    		finished = true;
    		StartCoroutine(ShowEndCard());
    	}
    }

    //--------------------------------------------------------------------------------

    IEnumerator ShowEndCard() {
    	yield return new WaitForSeconds(1);
    	endCard.SetActive(true);
    	menuButton2.SetActive(false);
    	endText.GetComponent<Text>().text = "You got " + score.ToString() + " points!";
    	Time.timeScale = 0f;
    }

    void Menu() {
    	Time.timeScale = 1f;
    	SceneManager.LoadScene(0);
    }

    //--------------------------------------------------------------------------------

}
