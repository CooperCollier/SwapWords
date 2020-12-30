using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;

public class Board : MonoBehaviour {

	//--------------------------------------------------------------------------------

    /* Two arrays with the possible letters in the game and the score associated
     * with that letter. */
    char[] letters = new char[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                                 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                                 'U', 'V', 'W', 'X', 'Y', 'Z'};
    int[] scores = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 
                              11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 
                              21, 22, 23, 24, 25, 26};

	/* A 2D array holding the tile GameObjects for the board. The lower left corner
	 * is (0, 0) and the upper right corner is (7, 7). */
	public Tile[,] tiles = new Tile[8, 8];

	/* Tile prefab to use for Instantiate() */
	public Tile tile;

    /* Instance of the main camera */
    public Camera camera;

    /* Two tiles to swap when swapping letters */
    public Tile firstTile;
    public int firstTileX;
    public int firstTileY;
    public Tile secondTile;
    public int secondTileX;
    public int secondTileY;

    /* Check wether the player has selected the first of 2 tiles to swap. */
    bool isTileSelected = false;

    //--------------------------------------------------------------------------------

    void Start() {
        camera = Camera.main;
    	GenerateStartingBoard();
    }

    //--------------------------------------------------------------------------------

    void Update() {

    	if (Input.GetMouseButtonDown(0)) {

            Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shiftedPosition = new Vector2(position.x + 32, position.y + 32);
            int locationX = (int) (shiftedPosition.x / 32);
            int locationY = (int) (shiftedPosition.y / 32);

            if ((locationX >= 0 && locationX < 8) && (locationY >= 0 && locationY < 8)) {

                Tile selectedTile = tiles[locationX, locationY];

                if (isTileSelected) {

                	firstTile.GetComponent<SpriteRenderer>().color = Color.white;
                    isTileSelected = false;
                    secondTile = selectedTile;
                    secondTileX = selectedTile.locationX;
                    secondTileY = selectedTile.locationY;

                    Debug.Log(firstTileX);
                    Debug.Log(firstTileY);
                    Debug.Log(secondTileX);
                    Debug.Log(secondTileY);

                    if ((System.Math.Abs(secondTileX - firstTileX) 
                    	+ System.Math.Abs(secondTileY - firstTileY)) == 1) {
                        firstTile.SendMessage("Swap", new int[]{secondTileX, secondTileY});
                        secondTile.SendMessage("Swap", new int[]{firstTileX, firstTileY});
                        tiles[firstTileX, firstTileY] = secondTile;
                        tiles[secondTileX, secondTileY] = firstTile;
                    }

                    firstTile = null;
                    secondTile = null;

                } else {
                    isTileSelected = true;
                    firstTile = selectedTile;
                    firstTileX = selectedTile.locationX;
                    firstTileY = selectedTile.locationY;
                    firstTile.GetComponent<SpriteRenderer>().color = Color.cyan;
                }

            }
        }

        string bestString = "";
        int bestStringLength = 0;
        string testString = "";
        int testStringLength = 0;

        Tile[] toDelete = {};

        for (int row = 0; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {
    			testString = tiles[col, row].letter.ToString();
    			for (int square = 7; square > col + 2; square -= 1) {
    				for (int nextTile = col; nextTile <= square; nextTile += 1) {

    					testString += tiles[nextTile, row].letter.ToString();
    					testStringLength = testString.Length;
    					if ((testStringLength >= 4) && (testStringLength > bestStringLength)) {
    						if (CheckForWord(testString)) {
    							bestString = testString;
    							bestStringLength = testStringLength;
    						}
    					}

    				}
    			}
    			for (int square = 0; square > row + 2; square += 1) {
    				for (int nextTile = row; nextTile >= 0; nextTile -= 1) {
    					testString += tiles[row, nextTile].letter.ToString();



    				}
    			}
    		}
    	}
        
    }

    //--------------------------------------------------------------------------------

    /* Randomly generate the board's starting state. */
    void GenerateStartingBoard() {
    	for (int row = 0; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {
    			tiles[col, row] = GenerateRandomTile(col, row);
    		}
    	}
    }

    //--------------------------------------------------------------------------------

    /* Pick a random letter tile. */
    Tile GenerateRandomTile(int locationX, int locationY) {

    	Tile newTile = (Tile) Instantiate(tile);

        int randomNumber = Random.Range(0, 26);
    	newTile.letter = letters[randomNumber];
    	newTile.points = scores[randomNumber];

    	newTile.locationX = locationX;
    	newTile.locationY = locationY;

    	return newTile;

    }

    //--------------------------------------------------------------------------------

    /* Returns wether or not the supplied string is an english word. */
    bool CheckForWord(string testString) {
    	return false;
    }

    //--------------------------------------------------------------------------------

}
