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

    //--------------------------------------------------------------------------------

    void Start() {
        finished = false;
    }

    void Update() {
    	score = board.reportScore();
    	scoreText.GetComponent<Text>().text = "Score: " + score.ToString();
    	movesRemaining = board.reportMoves();
    	movesText.GetComponent<Text>().text = "Moves: " + movesRemaining.ToString();
    	if (movesRemaining <= 0) {
    		finished = true;
    		ShowEndCard();
    	}
    }

    //--------------------------------------------------------------------------------

    void ShowEndCard() {
    	Time.timeScale = 0f;

    	// Show End Card...
        
    }

    //--------------------------------------------------------------------------------

}
