using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;
using System.Collections;

public class Board : MonoBehaviour {

    /* Gameplay loop:
     * 1: WaitForInput
     * The game is waiting for the player to complete a move.
     * 2: MoveTiles
     * The game is swapping the two tiles chosen by the player. 
     * The tile-swap animation plays, and then the locations of the two tiles are updated.
     * 3: CheckForWords
     * The game checks if there are any words on the board, and removes them.
     * If there are no words, return to state 1.
     * 4: DropTiles
     * The game checks if any tiles are 'floating', and drops them.
     * Return to state 3.
     */

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

    /* A hash table used to store all the dictionary words. */
    Hashtable hashTable;

    /* Records the current state of the board at each turn. */
    enum State { GetInput, TilesMoving, FindWords, DropTiles }
    State currentState;

    //--------------------------------------------------------------------------------

    void Start() {
        camera = Camera.main;
    	GenerateStartingBoard();
    	hashTable = new Hashtable();
    	string[] lines = System.IO.File.ReadAllLines("Assets/allWords.txt");
    	foreach (string line in lines) {
    		hashTable.Add(line, true);
    	}
    	StartingLoop();
        currentState = State.GetInput;
    }

    //--------------------------------------------------------------------------------

    void Update() {

    	if (Input.GetMouseButtonDown(0) && (currentState == State.GetInput)) {
    		GetInput();  
        }

        if (currentState == State.TilesMoving) {
        	TilesMoving();
        }
        
    	if (currentState == State.FindWords) {
        	if (FindWords()) {
        		currentState = State.DropTiles;
        	} else {
        		currentState = State.GetInput;
        	}
        }

        if (currentState == State.DropTiles) {
        	DropTiles();
        	currentState = State.TilesMoving;
        }

    }

    //--------------------------------------------------------------------------------

    void GetInput() {

    	Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shiftedPosition = new Vector2(position.x + 32, position.y + 32);
        int locationX = (int) (shiftedPosition.x / 32);
        int locationY = (int) (shiftedPosition.y / 32);

        if ((locationX >= 0 && locationX < 8) && (locationY >= 0 && locationY < 8)) {

            Tile selectedTile = tiles[locationX, locationY];
            if (selectedTile == null) { return; }

            if (isTileSelected) {

            	firstTile.GetComponent<SpriteRenderer>().color = Color.white;
                isTileSelected = false;
                secondTile = selectedTile;
                secondTileX = selectedTile.locationX;
                secondTileY = selectedTile.locationY;

                if ((System.Math.Abs(secondTileX - firstTileX) 
                    + System.Math.Abs(secondTileY - firstTileY)) == 1) {
                    firstTile.SendMessage("Move", new int[]{secondTileX, secondTileY});
                    secondTile.SendMessage("Move", new int[]{firstTileX, firstTileY});
                    tiles[firstTileX, firstTileY] = secondTile;
                    tiles[secondTileX, secondTileY] = firstTile;
                    currentState = State.TilesMoving;
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

    //--------------------------------------------------------------------------------

    void TilesMoving() {
    	for (int row = 0; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {
    			Tile tile = tiles[col, row];
    			if (tile != null) {
    				if (tile.moving) {
    					return;
    				}
    			}
    		}
    	}
    	currentState = State.FindWords;
    }

    //--------------------------------------------------------------------------------

    bool FindWords() {

    	bool returnValue = false;

    	bool[,] toDelete = new bool[8, 8];

    	string bestString = "";
        int bestStringLength = 0;
        string testString = "";
        int testStringLength = 0;

        // TODO: Repeat this, but going by columns instead.
        for (int row = 0; row < 8; row += 1) {

    		for (int startSquare = 0; startSquare < 6; startSquare += 1) {

    			// Move this outside to the previous for loop?
    			bestString = ""; 
        		bestStringLength = 0;
        		testString = "";
        		testStringLength = 0;

        		if (tiles[startSquare, row] == null) {
    				continue;
    			}

    			for (int nextSquare = startSquare; nextSquare < 8; nextSquare += 1) {

    				if (tiles[nextSquare, row] == null) {
    					break;
    				}

    				testString += tiles[nextSquare, row].letter.ToString();
    				testStringLength = testString.Length;

    				if (testStringLength > bestStringLength && testStringLength > 2) {

    					if (CheckForWord(testString)) {

    						bestString = testString;
    						bestStringLength = testStringLength;
    						for (int tile = startSquare; tile <= nextSquare; tile += 1) {
    							toDelete[tile, row] = true;
    							tiles[tile, row].GetComponent<SpriteRenderer>().color = Color.green;
    						}

    					}

    				}

    			}

    		}

    	}

    	for (int row = 0; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {
    			if (toDelete[col, row]) {
    				returnValue = true;
    				Tile tileToDelete = tiles[col, row];
    				Destroy(tileToDelete.gameObject);
    				tiles[col, row] = null;
    			}
    		}
    	}

    	return returnValue;
    }



    //--------------------------------------------------------------------------------

    void DropTiles() {
    	for (int row = 1; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {

    			if (tiles[col, row] != null && tiles[col, row-1] == null) {
    				Tile tileToMove = tiles[col, row];
    				int groundRow;
    				for (groundRow = row; groundRow >= 0; groundRow -= 1) {
    					if (groundRow == 0) { break; }
    					if (tiles[col, groundRow-1] != null) { break; }
    				}
    				tileToMove.SendMessage("Move", new int[]{col, groundRow});
    				tiles[col, row] = null;
    				tiles[col, groundRow] = tileToMove;
    			}

    		}
    	}
    }

    //--------------------------------------------------------------------------------

    /* Make sure there are no words at the start of the game. */
    void StartingLoop() {
    	while (FindWords()) {
    		for (int row = 0; row < 8; row += 1) {
    			for (int col = 0; col < 8; col += 1) {
    				if (tiles[col, row] == null) {
    					tiles[col, row] = GenerateRandomTile(col, row);
    				}
    			}
    		}
    	}
    }

    /* Randomly generate the board's starting state. */
    void GenerateStartingBoard() {
    	for (int row = 0; row < 8; row += 1) {
    		for (int col = 0; col < 8; col += 1) {
    			tiles[col, row] = GenerateRandomTile(col, row);
    		}
    	}
    }

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

    /* Returns wether or not the supplied string is an english word. */
    bool CheckForWord(string testString) {
    	if (hashTable.ContainsKey(testString)) {
    		return true;
    	} else {
    		return false;
    	}
    }

    //--------------------------------------------------------------------------------

    void PrintBoard() {

    	string message = "";

    	for (int row = 0; row < 8; row += 1) {
    		message += " [ ";
    		for (int col = 0; col < 8; col += 1) {

    			message += ",";

    			Tile tile = tiles[col, row];
    			if (tile != null) {
    				message += tile.letter.ToString();
    			} else {
    				message += "0";
    			}

    			message += ",";

    		}
    		message += " ] ";
    	}

    	Debug.Log(message);
    }

    //--------------------------------------------------------------------------------

}
