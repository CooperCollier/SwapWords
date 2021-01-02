using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;
using System.Collections;

public class Board : MonoBehaviour {

	//--------------------------------------------------------------------------------

    static int totalRows = 24;
    static int totalCols = 8;

    /* Two arrays with the possible letters in the game and the score associated
     * with that letter. */
    char[] letters = new char[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                                 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                                 'U', 'V', 'W', 'X', 'Y', 'Z'};
    int[] scores = new int[] {1, 3, 3, 2, 1, 4, 2, 4, 1, 8, 
                              5, 1, 3, 1, 1, 3, 10, 1, 1, 1, 
                              1, 4, 4, 8, 4, 10};
    int[] amounts = new int[] {9, 2, 2, 4, 12, 2, 3, 2, 9, 1, 
                              1, 4, 2, 6, 8, 2, 1, 6, 4, 6, 
                              4, 2, 2, 1, 2, 1};               

	/* A 2D array holding the tile GameObjects for the board. The lower left corner
	 * is (0, 0) and the upper right corner is (7, 7). */
	public Tile[,] tiles = new Tile[totalCols, totalRows];

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

    /* Check wether to delete a tile because it is forming a word. */
    bool[,] toDelete = new bool[totalCols, totalCols];

    /* Check is ClearWords() is currently running. */
    bool coroutineStarted = false;

    /* A hash table used to store all the dictionary words. */
    Hashtable hashTable;

    /* Records the current state of the board at each turn. */
    enum State { GetInput, TilesMoving, FindWords, ClearWords, DropTiles }
    State currentState;

    public static int score;
    public static int movesRemaining;
    public static bool finished;

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

        movesRemaining = 40;
        finished = false;
        score = 0;
    }

    //--------------------------------------------------------------------------------

    void Update() {

        if (movesRemaining <= 0 && currentState == State.GetInput) {
            finished = true;
            return;
        }

    	if (Input.GetMouseButtonDown(0) && (currentState == State.GetInput)) {
    		GetInput();  
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

        if (currentState == State.ClearWords && !coroutineStarted) {
        	StartCoroutine(ClearWords());
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

        if ((locationX >= 0 && locationX < totalCols) && (locationY >= 0 && locationY < totalCols)) {

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
                    movesRemaining -= 1;
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

    bool FindWords() {

    	toDelete = new bool[totalCols, totalCols];

    	bool returnValue = false;

    	string bestString = "";
        int bestStringLength = 0;
        string testString = "";
        int testStringLength = 0;

        //TODO: Fix 'TWOW' detection?

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

    }

    //--------------------------------------------------------------------------------

    IEnumerator ClearWords() {
    	coroutineStarted = true;
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
    	coroutineStarted = false;
    }

    //--------------------------------------------------------------------------------

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
    }

    //--------------------------------------------------------------------------------

    /* Make sure there are no words at the start of the game. */
    void StartingLoop() {
    	for (int i = 0; i < (totalCols-3); i += 1) {
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
        int randomNumber1 = Random.Range(0, 26);
        int randomNumber2 = Random.Range(0, 26);
        int randomNumber;
        if (amounts[randomNumber2] > amounts[randomNumber1]) {
        	randomNumber = randomNumber2;
        } else {
        	randomNumber = randomNumber1;
        }
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

    public int reportScore() {
        return score;
    }

    public int reportMoves() {
        return movesRemaining;
    }

    public bool reportFinished() {
        return finished;
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
