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
	public int locationX, locationY;

    /* The tile sprite. */
    public SpriteRenderer sprite;

	/* GameObject holding this tile's text, and the corresponding text component. */
	public GameObject textObject;
	public Text text;

    /* GameObject holding the tile's particle effect */
    public ParticleSystem particles;

    /* Check wether the game has started. The tile sets its letter and position beforehand. */
    bool gameStarted = false;

    /* Used if moving = true, to get the tile to its destination. */
    float destinationX, destinationY;

    /* True if the tile is currently moving. */
    public bool moving = false;

    /* Speed multiplier to use while moving. */
    static float speed = 3f;
    /* Distance to the destination where it's small enough to just finish moving. */
    static float minimumDistance = 2f;

    //--------------------------------------------------------------------------------

    void Update() {

        if (!gameStarted) {

            /* Run this only once, on the first frame after the tile is created. */
            textObject = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
            text = textObject.GetComponent<Text>();
            text.text = letter.ToString();

            particles = transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();

            sprite = GetComponent<SpriteRenderer>();

            transform.position = new Vector3(TileToWorldSpace(locationX), TileToWorldSpace(locationY), 0);
            gameStarted = true;

        }

        if (moving) {

            /* Moves the tile toward its destination, if it is moving. 
             * Once the tile reaches its destination, report back to the board script
             * that this tile is finished. */
            float xDirection = destinationX - transform.position.x;
            float yDirection = destinationY - transform.position.y;
            Vector2 direction = new Vector2(xDirection, yDirection);
            if (direction.magnitude < minimumDistance) {
                moving = false;
                transform.position = new Vector3(destinationX, destinationY, 0);
            } else {
                transform.Translate(direction * speed * Time.deltaTime);
            }

        }

    }

    //--------------------------------------------------------------------------------

    /* Turns a coordinate like [3, 4] into a coordinate like [127.3, 224.8] */
    float TileToWorldSpace(int location) {
        return ((32 * location) - 12);
    }

    //--------------------------------------------------------------------------------

    /* Begin the process of moving this tile onto another square. */
    void Move(int[] destination) {
        locationX = destination[0];
        locationY = destination[1];
        destinationX = TileToWorldSpace(destination[0]);
        destinationY = TileToWorldSpace(destination[1]);
        moving = true;
    }

    //--------------------------------------------------------------------------------

    /* This function is called by the board script when it destroys the tile. */
    void DestroySelf() {
        sprite.enabled = false;
        textObject.SetActive(false);
        particles.Play();
        Destroy(gameObject, particles.main.duration);
    }

    //--------------------------------------------------------------------------------

}
