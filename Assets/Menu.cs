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
    public GameObject hardModeText;
    public AudioSource AudioClickButton;

	//--------------------------------------------------------------------------------

    void Start() {
        AudioClickButton = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        
        int highScoreNormal = PlayerPrefs.GetInt("HighScoreNormal");
        int highScoreHard = PlayerPrefs.GetInt("HighScoreHard");

        highScoreText.GetComponent<Text>().text = "High Score: " + highScoreNormal.ToString() + " (Normal), " + highScoreHard.ToString() + " (Hard)";

        int hardMode = PlayerPrefs.GetInt("HardMode");
    	if (hardMode == 0) {
        	hardModeText.GetComponent<Text>().text = "Hard Mode Off\n(Words need at least 3 letters)";
        } else {
        	hardModeText.GetComponent<Text>().text = "Hard Mode On\n(Words need at least 4 letters)";
        }
    }
    
    public void Play() {
        AudioClickButton.Play();
    	SceneManager.LoadScene(1);
    }

    public void QuitEverything() {
        AudioClickButton.Play();
        Application.Quit();
    }

    public void HardModeToggle() {
        AudioClickButton.Play();
    	int hardMode = PlayerPrefs.GetInt("HardMode");
    	if (hardMode == 0) {
    		PlayerPrefs.SetInt("HardMode", 1);
    		hardModeText.GetComponent<Text>().text = "Hard Mode On\n(Words need at least 4 letters)";
    	} else {
    		PlayerPrefs.SetInt("HardMode", 0);
    		hardModeText.GetComponent<Text>().text = "Hard Mode Off\n(Words need at least 3 letters)";
    	}
        PlayerPrefs.Save();
    }

    //--------------------------------------------------------------------------------

}