using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour {

	//--------------------------------------------------------------------------------

	/* The letter appearing on this tile. All letters are capital. */
	public char letter;
	/* The number of points gained by using this letter. */
	public int points;
	/* The (x, y) coordinate of this tile on the board. */
	public int locationX;
	public int locationY;

	/* GameObject holding this tile's text, and the corresponding text component. */
	public GameObject textObject;
	public Text text;

    /* Check wether the game has started. The tile sets its letter and position beforehand. */
    bool gameStarted = false;

    //--------------------------------------------------------------------------------

    void Start() {

        
    }

    //--------------------------------------------------------------------------------

    void Update() {

        if (!gameStarted) {
            textObject = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
            text = textObject.GetComponent<Text>();
            text.text = letter.ToString();
            MoveTo(locationX, locationY);
            gameStarted = true;
        }
        
    }

    void MoveTo(int destinationX, int destinationY) {
        transform.position = new Vector3((32 * destinationX - 12), (32 * destinationY - 12), 0);
    }

    //--------------------------------------------------------------------------------

    /* Moves this tile onto another square. Swap() has also been called on the tile
     * moving out of that square. */
    void Swap(int[] destination) {
        locationX = destination[0];
        locationY = destination[1];
        MoveTo(destination[0], destination[1]);
    }

    //--------------------------------------------------------------------------------

    /* Causes this tile to fall down and land on the highest tile beneath it. */
    void Fall(int destinationX, int destinationY) {

    }

    //--------------------------------------------------------------------------------

}
