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

    /* Audio sources to use. */
    public AudioSource AudioClickTile;
    public AudioSource AudioDestroyTile;
    public AudioSource AudioCollideTile;

    //--------------------------------------------------------------------------------

	/* A 2D array holding the tile GameObjects for the board. */
	public Tile[,] tiles = new Tile[totalCols, totalRows];

	/* Tile prefab to use for Instantiate() */
	public Tile tile;

    /* Three variables that need to be reported to the ButtonManager script. */
    public static int score;
    public static int movesRemaining;
    public static bool finished;

    /* Records the current state of the board at each turn. */
    enum State { GetInput, TilesMoving, FindWords, ClearWords, DropTiles }
    State currentState;

    /* Description of each state:
     * 1: GetInput
     * The board is waiting for the user to finish selecting two tiles to swap.
     * After this state, proceed to TilesMoving.
     *
     * 2: TilesMoving
     * The board has told some tiles to move to new locations, and it is waiting
     * for those tiles to report that they have finished moving. After this state,
     * proceed to FindWords.
     *
     * 3: FindWords
     * Check if there are any words formed on the board, and how many. If there are, 
     * record their locations and proceed to ClearWords. Otherwise, return to GetInput.
     * 
     * 4: ClearWords
     * Clear all tiles marked for deletion (tiles that are forming words). Record any
     * score multipliers here as well. After this state, proceed to DropTiles.
     * 
     * 5: DropTiles
     * Check which tiles have nothing underneath them, and compute where those tiles
     * should fall down to. Then, tell those tiles to fall down. After this state,
     * proceed to TilesMoving. 
     *
     */

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
    public int wordsFoundDuringThisTurn;
    public int wordsFoundDuringPreviousTurn;

    /* Check if the player made any eight-letter words. */
    public bool bingo = false;

    public ButtonManager buttonManager;

    public int hardMode;
    public int MINIMUM_WORD_LENGTH;

    //--------------------------------------------------------------------------------

    void Start() {

        /* Intialize the camera */
        camera = Camera.main;

        /* Set board location to the bottom center of the screen. */
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        float boardHeight = spriteRenderer.bounds.size.y;
        float boardWidth = spriteRenderer.bounds.size.x;
		camera.orthographicSize = (boardWidth * ((float) Screen.height / (float) Screen.width) * 0.5f);
        Vector3 screenCenter = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, camera.nearClipPlane));
        Vector3 screenBottom = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth / 2, 0, camera.nearClipPlane));
        float yOffset = (screenCenter.y - screenBottom.y) - (boardHeight / 2);
		camera.transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, camera.transform.position.z);

        /* Tell button manager where the board is located so that the
         * game over screen will appear in the center of the board. */
        Vector3 upperRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));
        Vector3 lowerLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        buttonManager.SendMessage("GetBoardTopLocation", boardHeight / (upperRight.y - lowerLeft.y));

        /* Check if the game is in hard mode or not. 
         * In hardmode, words must be at least 4 letters, but the player gets doubled moves. */
        int hardMode = PlayerPrefs.GetInt("HardMode");
        MINIMUM_WORD_LENGTH = 3;
        if (hardMode != 0) { MINIMUM_WORD_LENGTH = 4; }

        /* Set audio sources. */
        AudioClickTile = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        AudioDestroyTile = transform.GetChild(1).gameObject.GetComponent<AudioSource>();
        AudioCollideTile = transform.GetChild(2).gameObject.GetComponent<AudioSource>();

        /* Add all english words to the hashtable of words */
        hashTable = new Hashtable();
        string[] lines = System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/allWords.txt");
        foreach (string line in lines) { hashTable.Add(line, true);}

        /* Generate the board randomly, then remove any words that may have formed */
    	GenerateStartingBoard();
    	StartingLoop();

        /* Set the current state */
        currentState = State.GetInput;

        /* Set the score and movesRemaining variables to their initial values. */
        score = 0;
        movesRemaining = 30;
        if (hardMode != 0) { movesRemaining = 60; }
        finished = false;

    }

    //--------------------------------------------------------------------------------

    void Update() {

    	if (movesRemaining <= 0 && currentState == State.GetInput) {
           finished =  true;
        }
    	if (finished) { return; }

    	if (currentState == State.GetInput) {
    		wordsFoundDuringThisTurn = 0;
    		wordsFoundDuringPreviousTurn = 0;
    		bingo = false;
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
                Color newColor;
                ColorUtility.TryParseHtmlString("#fbf5ef", out newColor);
            	firstTile.GetComponent<SpriteRenderer>().color = newColor;
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
                    AudioClickTile.Play();

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
                Color newColor;
                ColorUtility.TryParseHtmlString("#f2d3ab", out newColor);
                firstTile.GetComponent<SpriteRenderer>().color = newColor;
                AudioClickTile.Play();
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
        AudioCollideTile.Play();
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
    				if (testString.Length >= MINIMUM_WORD_LENGTH && CheckForWord(testString)) {
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
                    Color newColor;
                    ColorUtility.TryParseHtmlString("#c69fa5", out newColor);
    				tiles[tile, row].GetComponent<SpriteRenderer>().color = newColor;

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

    	/* Instantiate the return value. */
    	int[] sortedOrder = new int[words_InThisRow.Count];

    	/* Use this to record what words we have already added to sortedOrder. */
    	bool[] removed = new bool[words_InThisRow.Count];

    	/* Run this loop for every word in words_InThisRow, so we don't miss any. */
    	for (int repetitions = 0; repetitions < words_InThisRow.Count; repetitions += 1) {

    		/* Walk through the loop, and keep checking for a word with a score
    		 * greater than our running maximum. If we find one, update the 
    		 * running maximum to reflect it. */
    		int maxScore = 0;
    		int indexOfMaxScoringWord = 0;
    		for (int index = 0; index < words_InThisRow.Count; index += 1) {
    			int score = GetScore(words_InThisRow[index]);
    			if (score > maxScore && !(removed[index])) {
    				indexOfMaxScoringWord = index;
    				maxScore = score;
    			}
    		}

    		/* Add the largest-scoring word to sortedOrder. */
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

    	/* Go through every tile on the board, and delete it if it is
    	 * marked for deletion. */
    	for (int row = 0; row < totalCols; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {

    			if (toDelete[col, row]) {
    				Tile tileToDelete = tiles[col, row];
                    score += tileToDelete.points;
                    tileToDelete.SendMessage("DestroySelf");
    				tiles[col, row] = null;
    			}

    		}
    	}

        AudioDestroyTile.Play();

    	/* Add score multipliers. */
    	score += (wordsFoundDuringThisTurn - 1) * 5;
    	if (bingo) {
    		score += 100;
    		bingo = false;
    	}

    	/* Reset toDelete so that it's all blank. */
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

    	/* Go through every square on the board. */
    	for (int row = 1; row < totalRows; row += 1) {
    		for (int col = 0; col < totalCols; col += 1) {

    			/* Check if this tile has nothing supporting it below. */
    			if (tiles[col, row] != null && tiles[col, row-1] == null) {

    				Tile tileToMove = tiles[col, row];
    				int groundRow;

    				/* Find the row that it will come to rest on after falling. */
    				for (groundRow = row; groundRow >= 0; groundRow -= 1) {
    					if (groundRow == 0) { break; }
    					if (tiles[col, groundRow-1] != null) { break; }
    				}

    				/* Tell the tile to move. */
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

    	/* Repeat this loop several times. */
        int repetitions = 8;
    	for (int i = 0; i < repetitions; i += 1) {

    		/* Check if there are any words on the board, and mark them
    		 * to be deleted. */
    		FindWords();

    		/* If there are any words to be deleted, change their letter to
    		 * something random. */
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

 	/* Functions that communicate with the ButtonManager script. */

    public int ReportScore() {
        return score;
    }

    public int ReportMoves() {
        return movesRemaining;
    }

    public int ReportBonus() {
    	return (wordsFoundDuringThisTurn - 1);
    }

    public bool ReportBingo() {
    	return bingo;
    }

    public bool CheckIfGameFinished() {
        return finished;
    }

    //--------------------------------------------------------------------------------

}
