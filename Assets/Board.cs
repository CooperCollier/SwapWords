using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    /* Record how many words were found on the board in the past 2 turns. */
    int wordsFoundDuringThisTurn;
    int wordsFoundDuringPreviousTurn;

    /* Check if the player made any eight-letter words. */
    bool bingo = false;

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
        movesRemaining = 30;

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
    		wordsFoundDuringPreviousTurn = wordsFoundDuringThisTurn;
    		wordsFoundDuringThisTurn += FindWords();
        	if (wordsFoundDuringThisTurn > wordsFoundDuringPreviousTurn) {
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

    	wordsFoundDuringThisTurn = 0;
    	wordsFoundDuringPreviousTurn = 0;
    	bingo = false;

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
     * the ToDelete array and return how many were found. */
    int FindWords() {

        /* set the toDelete array to be all blank, and initialize the return value. */
    	toDelete = new bool[totalCols, totalCols];
    	int wordsFoundOnEntireBoard = 0;

    	/* Check if this string even forms a word at all. */
        string testString;

        /* Iterate through every row on the board. */
        for (int row = 0; row < totalCols; row += 1) {

        	/* Records all the words formed in this row, and their start and end tiles. */
            List<string> words_InThisRow = new List<string>();
            List<int> startSquares_InThisRow = new List<int>();
            List<int> endSquares_InThisRow = new List<int>();

        	/* For each row, iterate through all possible starting squares in order
        	 * to check every possible combination of tiles for a word. */
    		for (int startSquare = 0; startSquare < (totalCols-2); startSquare += 1) {

        		testString = "";

        		if (tiles[startSquare, row] == null) { continue; }

        		/* Continue building up squares and check if this is a word. */
    			for (int nextSquare = startSquare; nextSquare < totalCols; nextSquare += 1) {

    				if (tiles[nextSquare, row] == null) { break; }

    				testString += tiles[nextSquare, row].letter.ToString();

    				/* For each combination of tiles, check if it is a word. */
    				if (testString.Length > 2 && CheckForWord(testString)) {
    					words_InThisRow.Add(testString);
    					startSquares_InThisRow.Add(startSquare);
    					endSquares_InThisRow.Add(nextSquare);
    				}

    			}

    		}

    		/* If there were no words found, we can just check the next row. */
    		if (words_InThisRow.Count == 0) {
    			continue;
    		}

    		/* Records which tiles are still available in this row. The tiles are filled 
             * up by highest-scoring word first. */
    		bool[] tilesFreeInOneRow = new bool[] {true, true, true, true, true, true, true, true};

    		/* Sort the words that were found so the highest-scoring word appears first. */
    		int[] sortedOrder = SortWordsByScore(words_InThisRow);

    		/* Iterate through the whole list. */
    		for (int index = 0; index < sortedOrder.Length; index += 1) {

    			int trueIndex = sortedOrder[index];

    			string word = words_InThisRow[trueIndex];
    			int wordStart = startSquares_InThisRow[trueIndex];
    			int wordEnd = endSquares_InThisRow[trueIndex];

    			/* Make sure this word isn't overlapping with other words. */
    			if (!CheckIfWordFits(wordStart, wordEnd, tilesFreeInOneRow)) {
    				continue;
    			}

    			/* Now that we've definitely found a word, set it to be deleted. */
    			for (int tile = wordStart; tile <= wordEnd; tile += 1) {

    				/* Update the toDelete and tilesFreeInOneRow arrays. */
    				tilesFreeInOneRow[tile] = false;
    				toDelete[tile, row] = true;
    				tiles[tile, row].GetComponent<SpriteRenderer>().color = Color.green;

    				/* Check if a bingo appeared. */
    				if ((wordStart == 0) && (wordEnd == totalCols - 1)) {
    					bingo = true;
    				}

    			}

    			/* update the final return value. */
    			wordsFoundOnEntireBoard += 1;

    		}

    	}

    	return wordsFoundOnEntireBoard;

    }

    //--------------------------------------------------------------------------------

    /* Helper Functions for FindWords() */


    /* Helper function that takes the wordsFoundInOneRow dictionary and returns a
     * list where the first element is the index of the largest scoring word,
     * the second element is the index of the second-largest scoring word, etc.  */
    int[] SortWordsByScore(List<string> words_InThisRow) {
    	
    	int[] sortedOrder = new int[words_InThisRow.Count];

    	bool[] removed = new bool[words_InThisRow.Count];

    	for (int repetitions = 0; repetitions < words_InThisRow.Count; repetitions += 1) {

    		int maxScore = 0;
    		int indexOfMaxScoringWord = 0;

    		for (int index = 0; index < words_InThisRow.Count; index += 1) {

    			int score = GetScore(words_InThisRow[index]);
    			if (score > maxScore && !(removed[index])) {
    				indexOfMaxScoringWord = index;
    				maxScore = score;
    			}

    		}

    		sortedOrder[repetitions] = indexOfMaxScoringWord;
    		removed[indexOfMaxScoringWord] = true;
    	}

    	return sortedOrder;

    }

    /* Get the total score for a word. */
    int GetScore(string word) {
    	int score = 0;
    	char[] arr = word.ToCharArray();
    	for (int i = 0; i < arr.Length; i += 1) {
    		int positionInAplhabet = (int) char.ToUpper(arr[i]) - 64;
    		score += scores[positionInAplhabet - 1];
    	}
    	return score;
    }

    /* Checks if any squares between start and end are marked 'false' on
     * the tilesFreeInOneRow array. If none are, return true. */
    bool CheckIfWordFits(int start, int end, bool[] tilesFreeInOneRow) {
    	for (int tile = start; tile <= end; tile += 1) {
    		if (!tilesFreeInOneRow[tile]) {
    			return false;
    		}
    	}
    	return true;
    }

    /* Returns wether or not the supplied string
     * is an english word. */
    bool CheckForWord(string testString) {
    	if (hashTable.ContainsKey(testString)) {
    		return true;
    	} else {
    		return false;
    	}
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

    	score += (wordsFoundDuringThisTurn - 1) * 10;
    	if (bingo) {
    		score += 50;
    		bingo = false;
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

}
