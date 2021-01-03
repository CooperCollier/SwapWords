using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	//--------------------------------------------------------------------------------

	public GameObject PlayButton;
    public GameObject QuitEverythingButton;
    public GameObject highScoreText;
    public AudioSource AudioClickButton;

	//--------------------------------------------------------------------------------

    void Start() {
        AudioClickButton = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        int highScore = PlayerPrefs.GetInt("HighScore");
        highScoreText.GetComponent<Text>().text = "High Score: " + highScore.ToString();
    }
    
    public void Play() {
        AudioClickButton.Play();
    	SceneManager.LoadScene(1);
    }

    public void QuitEverything() {
        AudioClickButton.Play();
        Application.Quit();
    }

    //--------------------------------------------------------------------------------

}