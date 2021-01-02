using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	//--------------------------------------------------------------------------------

	public GameObject PlayButton;
    public GameObject QuitEverythingButton;

	//--------------------------------------------------------------------------------
    
    public void Play() {
    	SceneManager.LoadScene(1);
    }

    public void QuitEverything() {
        Application.Quit();
    }

    //--------------------------------------------------------------------------------

}