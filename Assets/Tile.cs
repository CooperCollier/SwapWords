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

    /* True if the tile is currently moving. */
    public bool moving = false;

    /* Check wether the game has started. The tile sets its letter and position beforehand. */
    bool gameStarted = false;

    /* Used if moving = true, to get the tile to its destination. */
    float destinationX;
    float destinationY;
    /* Speed multiplier to use while moving. */
    float speed = 3f;

    //--------------------------------------------------------------------------------

    void Update() {

        if (!gameStarted) {
            textObject = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
            text = textObject.GetComponent<Text>();
            text.text = letter.ToString();
            transform.position = new Vector3(TileToWorldSpace(locationX), TileToWorldSpace(locationY), 0);
            gameStarted = true;
        }

        if (moving) {
            Vector2 direction = new Vector2(destinationX - transform.position.x,
                                            destinationY - transform.position.y);
            if (direction.magnitude < 2f) {
                moving = false;
                transform.position = new Vector3(destinationX, destinationY, 0);
            } else {
                transform.Translate(direction * speed * Time.deltaTime);
            }
        }

    }

    /* Turns a coordinate like [3, 4] into a coordinate like [127.3, 224.8] */
    float TileToWorldSpace(int location) {
        return ((32 * location) - 12);
    }

    //--------------------------------------------------------------------------------

    /* Moves this tile onto another square. */
    void Move(int[] destination) {
        locationX = destination[0];
        locationY = destination[1];
        destinationX = TileToWorldSpace(destination[0]);
        destinationY = TileToWorldSpace(destination[1]);
        moving = true;

        //Debug.Log(letter.ToString() + " " + destination[0].ToString() + destination[1].ToString());
    }

    //--------------------------------------------------------------------------------

}
