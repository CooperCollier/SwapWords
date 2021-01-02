using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;

public class Board : MonoBehaviour {

	//--------------------------------------------------------------------------------

    /* Instance of the main camera */
    public Camera camera;

    /* A hash table used to store all the dictionary words. */
    Hashtable hashTable;

    /* Two arrays describing the overall board dimensions, 8 wide and 24 tall. */
    static int totalRows = 24;
    static int totalCols = 8; 

    /* All the possible letters in the game, according to the scrabble distributions. */
    char[] letterDistribution = new char[] {
     'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A',
     'B', 'B',
     'C', 'C',
     'D', 'D', 'D', 'D',
     'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E',
     'F', 'F',
     'G', 'G', 'G',
     'H', 'H',
     'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I',
     'J', 'J',
     'K', 'K',
     'L', 'L', 'L', 'L',
     'M', 'M', 
     'N', 'N', 'N', 'N', 'N', 'N',
     'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O',
     'P', 'P',
     'Q',
     'R', 'R', 'R', 'R', 'R', 'R',
     'S', 'S', 'S', 'S',
     'T', 'T', 'T', 'T', 'T', 'T',
     'U', 'U', 'U', 'U',
     'V', 'V',
     'W', 'W',
     'X', 
     'Y', 'Y',
     'Z'
    };



    /* The score associated with each letter in the game. */
    int[] scores = new int[] {1, 3, 3, 2, 1, 4, 2, 4, 1, 8, 5, 1, 3,
                              1, 1, 3, 10, 1, 1, 1, 1, 4, 4, 8, 4, 10};

    //--------------------------------------------------------------------------------

	/* A 2D array holding the tile GameObjects for the board. */
	public Tile[,] tiles = new Tile[totalCols, totalRows];

	/* Tile prefab to use for Instantiate() */
	public Tile tile;

    /* Two variables that need to be reported to the ButtonManager script. */
    public static int score;
    public static int movesRemaining;

    /* Records the current state of the board at each turn. */
    enum State { GetInput, TilesMoving, FindWords, ClearWords, DropTiles }
    State currentState;

    //--------------------------------------------------------------------------------

    /* Two tiles to swap when swapping letters */
    public Tile firstTile;
    public int firstTileX;
    public int firstTileY;
    public Tile secondTile;
    public int secondTileX;
    public int secondTileY;

    /* Check wether the player has selected the first of 2 tiles to swap in GetInput(). */
    bool isTileSelected = false;

    /* An array that holds a boolean to check if we want to delete a tile on
     * this turn. There is one boolean for every tile on the board, but only
     * the visible tiles are actually checked. */
    bool[,] toDelete = new bool[totalCols, totalCols];

    /* Check if ClearWords() is currently running, so it doesn't run twice. */
    bool wordsAreBeingCleared = false;

    //--------------------------------------------------------------------------------

    void Start() {

        /* Intialize the camera */
        camera = Camera.main;

        /* Add all english words to the hashtable of words */
        hashTable = new Hashtable();
        string[] lines = System.IO.File.ReadAllLines("Assets/allWords.txt");
        foreach (string line in lines) { hashTable.Add(line, true);}

        /* Generate the board randomly, then remove any words that may have formed */
    	GenerateStartingBoard();
    	StartingLoop();

        /* Set the current state */
        currentState = State.GetInput;

        /* Set the score and movesRemaining variables to their initial values. */
        score = 0;
        movesRemaining = 40;

    }

    //--------------------------------------------------------------------------------

    void Update() {

    	if (currentState == State.GetInput) {
            if (Input.GetMouseButtonDown(0)) {
    		    GetInput();  
            }
        }

        if (currentState == State.TilesMoving) {
        	TilesMoving();
        }
        
    	if (currentState == State.FindWords) {
        	if (FindWords()) {
        		currentState = State.ClearWords;
        	} else {
        		currentState = State.GetInput;
        	}
        }

        if (currentState == State.ClearWords && !wordsAreBeingCleared) {
        	StartCoroutine(ClearWords());
        }

        if (currentState == State.DropTiles) {
        	DropTiles();
        }

    }

    //--------------------------------------------------------------------------------

    void GetInput() {

        /* Determine where the mouse click happened */
    	Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shiftedPosition = new Vector2(position.x + 32, position.y + 32);
        int locationX = (int) (shiftedPosition.x / 32);
        int locationY = (int) (shiftedPosition.y / 32);

        /* Check if the mouse click was within the bounds of the board. */
        if ((locationX >= 0 && locationX < totalCols) && (locationY >= 0 && locationY < totalCols)) {

            /* Tell the script which tile the player clicked on */
            Tile selectedTile = tiles[locationX, locationY];
            if (selectedTile == null) { return; }

            /* This section runs of this it the second of two clicks. */
            if (isTileSelected) {

                /* Prepare to swap the tiles. */
            	firstTile.GetComponent<SpriteRenderer>().color = Color.white;
                isTileSelected = false;
                secondTile = selectedTile;
                secondTileX = selectedTile.locationX;
                secondTileY = selectedTile.locationY;

                /* Ensure the two tiles can be swapped, then swap them. */
                if ((System.Math.Abs(secondTileX - firstTileX) 
                    + System.Math.Abs(secondTileY - firstTileY)) == 1) {

                    /* Tell the two tiles to update their own positions. */
                    firstTile.SendMessage("Move", new int[]{secondTileX, secondTileY});
                    secondTile.SendMessage("Move", new int[]{firstTileX, firstTileY});

                    /* Update the positions on the tiles array. */
                    tiles[firstTileX, firstTileY] = secondTile;
                    tiles[secondTileX, secondTileY] = firstTile;

                    /* Decrement the moves remaining, and change the state to TilesMoving. */
                    movesRemaining -= 1;
                    currentState = State.TilesMoving;

                }

                /* Set the two tiles to null after the swap has completed. */
                firstTile = null;
                secondTile = null;

            /* This section runs if it is the first of two clicks. */
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

    /* Continually repeats this functions and waits until every tile reports that
     * it is finished moving. */
    void TilesMoving() {
    	for (int row = 0; row < totalRows; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {
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

    /* Check if there are any words formed on the board. If there are, record them in 
     * the ToDelete array and return "true". */
    bool FindWords() {

        /* set the toDelete array to be all blank, and initialize the return value. */
    	toDelete = new bool[totalCols, totalCols];
    	bool returnValue = false;

    	string bestString = "";
        int bestStringLength = 0;
        string testString = "";
        int testStringLength = 0;

        for (int row = 0; row < totalCols; row += 1) {

    		for (int startSquare = 0; startSquare < (totalCols-2); startSquare += 1) {

    			// Move this outside to the previous for loop?
    			bestString = ""; 
        		bestStringLength = 0;
        		testString = "";
        		testStringLength = 0;

        		if (tiles[startSquare, row] == null) {
    				continue;
    			}

    			for (int nextSquare = startSquare; nextSquare < totalCols; nextSquare += 1) {

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
    							returnValue = true;
    						}

    					}

    				}

    			}

    		}

    	}

    	return returnValue;

        // TODO: Add bonus multipliers for long words, or for eight-letter words.
        // TODO: Fix 'TWOW' detection?

    }

    //--------------------------------------------------------------------------------

    /* Delete tiles from the board that are forming words. This is a 
     * Coroutine because the board needs to wait for a second to show which
     * words are being cleared. */
    IEnumerator ClearWords() {

    	wordsAreBeingCleared = true;

    	yield return new WaitForSeconds(1);

    	for (int row = 0; row < totalCols; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {

    			if (toDelete[col, row]) {
    				Tile tileToDelete = tiles[col, row];
                    score += tileToDelete.points;
    				Destroy(tileToDelete.gameObject);
    				tiles[col, row] = null;
    			}

    		}
    	}

    	toDelete = new bool[totalCols, totalCols];
    	currentState = State.DropTiles;

    	wordsAreBeingCleared = false;

    }

    //--------------------------------------------------------------------------------

    /* Now that some tiles have been cleared from the board, there will be
     * a few tiles left 'hanging' in the air. 
     * This functions tells those hanging tiles to fall into the highest
     * tile below them. */
    void DropTiles() {

    	for (int row = 1; row < totalRows; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {

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

        currentState = State.TilesMoving;

    }

    //--------------------------------------------------------------------------------

    /* Make sure there are no words at the start of the game. */
    void StartingLoop() {

        int repetitions = 10;

    	for (int i = 0; i < repetitions; i += 1) {

    		FindWords();

    		for (int row = 0; row < totalCols; row += 1) {
    			for (int col = 0; col < totalCols; col += 1) {

    				if (toDelete[col, row]) {
    					Tile tileToDelete = tiles[col, row];
    					Destroy(tileToDelete.gameObject);
    					tiles[col, row] = GenerateRandomTile(col, row);
    				}

    			}
    		}

    	}

    }

    /* Randomly generate the board's starting state. */
    void GenerateStartingBoard() {
    	for (int row = 0; row < totalRows; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {
    			tiles[col, row] = GenerateRandomTile(col, row);
    		}
    	}
    }

    /* Pick a random letter tile. */
    Tile GenerateRandomTile(int locationX, int locationY) {
    	Tile newTile = (Tile) Instantiate(tile);
        int randomNumber = Random.Range(0, 100);
    	newTile.letter = letterDistribution[randomNumber];
        int positionInAplhabet = (int) newTile.letter - 64;
    	newTile.points = scores[positionInAplhabet - 1];
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

    public int ReportScore() {
        return score;
    }

    public int ReportMoves() {
        return movesRemaining;
    }

    public bool CheckIfGameFinished() {
        if (movesRemaining <= 0 && currentState == State.GetInput) {
            return true;
        } else {
            return false;
        }
    }

    //--------------------------------------------------------------------------------

    void PrintBoard() {

    	string message = "";

    	for (int row = 0; row < totalRows; row += 1) {
    		message += " [ ";
    		for (int col = 0; col < 8; col += 1) {

    			message += ",";

    			bool tile = toDelete[col, row];
    			
    			message += tile;

    			message += ",";

    		}
    		message += " ] ";
    	}

    	Debug.Log(message);
    }

    //--------------------------------------------------------------------------------

}
