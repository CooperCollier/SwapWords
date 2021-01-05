using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {

	/* UI Text displaying the score. */
	public GameObject scoreText;
	/* The actual score. */
	public int score;

	/* UI Text displaying the moves remaining. */
	public GameObject movesText;
	/* The actual moves remaining. */
	public int movesRemaining;

    /* UI Text displaying player's score multiplier. */
    public GameObject bonusText;
    /* The player's score multiplier. */
    public int bonus;

    /* UI Text appearing if the player made a bingo. */
    public GameObject bingoText;
    /* Check if there is a bingo. */
    public bool bingo;

	/* Check whether the game is finished. */
	public static bool finished;

    /* The board that reports score and moves remaining to this script. */
	[SerializeField]
	public Board board;

    /* UI Objects from the canvas. */
	public GameObject endCard;
	public GameObject endText;
	public GameObject menuButton;
	public GameObject menuButton2;
    public GameObject retryButton;

    public AudioSource Song;
    public AudioSource AudioClickButton;

    //--------------------------------------------------------------------------------

    void Start() {
    	endCard.SetActive(false);
        finished = false;
        Song = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        AudioClickButton = transform.GetChild(1).gameObject.GetComponent<AudioSource>();
        Song.Play();
    }

    void Update() {

        /* If the game is over, do nothing. */
    	if (finished) {return;}

        /* Get the score from the board script. */
    	score = board.ReportScore();
    	scoreText.GetComponent<Text>().text = "Score: " + score.ToString();

        /* Get the moves remaining from the board script. */
    	movesRemaining = board.ReportMoves();
    	movesText.GetComponent<Text>().text = "Moves: " + movesRemaining.ToString();

        /* Get the bonus value from the board script. */
        bonus = board.ReportBonus();
        if (bonus == 1) {
        	bonusText.GetComponent<Text>().text = "Double! + " + (bonus * 5).ToString();
        } else if (bonus == 2) {
            bonusText.GetComponent<Text>().text = "Triple! + " + (bonus * 5).ToString();
        } else if (bonus == 3) {
            bonusText.GetComponent<Text>().text = "Quadruple! + " + (bonus * 5).ToString();
        } else if (bonus == 4) {
            bonusText.GetComponent<Text>().text = "Quintuple! + " + (bonus * 5).ToString();
        } else if (bonus == 5) {
            bonusText.GetComponent<Text>().text = "Sextuple! + " + (bonus * 5).ToString();
        } else if (bonus == 6) {
            bonusText.GetComponent<Text>().text = "Septuple! + " + (bonus * 5).ToString();
        } else if (bonus == 7) {
            bonusText.GetComponent<Text>().text = "Octuple! + " + (bonus * 5).ToString();
        } else if (bonus > 7) {
            bonusText.GetComponent<Text>().text = "MEGA BONUS! + " + (bonus * 5).ToString();
        } else {
            bonusText.GetComponent<Text>().text = "";
        }

        /* Check the bingo value from the board script. */
        bingo = board.ReportBingo();
        if (bingo) {
            bingoText.GetComponent<Text>().text = "BINGO! +100";
        } else {
            bingoText.GetComponent<Text>().text = "";
        }

        /* Check if the game is finished. If it is, show the end-card. */
    	if (board.CheckIfGameFinished()) {
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

        int highScore = PlayerPrefs.GetInt("HighScore");
        if (score > highScore) {
            PlayerPrefs.SetInt("HighScore", score);
        }
        PlayerPrefs.Save();

    }

    void Menu() {
        AudioClickButton.Play();
    	Time.timeScale = 1f;
    	SceneManager.LoadScene(0);
    }

    void Retry() {
        AudioClickButton.Play();
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    //--------------------------------------------------------------------------------

}
