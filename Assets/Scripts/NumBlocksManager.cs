using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;


public class NumBlocksManager : MonoBehaviour
{
    // Unity parts attached to the game object.
    [SerializeField] private NumBlock _numBlockPreFab;


    // Settings variables.
    [SerializeField] private int _gridScale = 10;    
    [SerializeField] private int _gridSize = 4;
    [SerializeField] private int _blockMoveSpeed = 25;
    [SerializeField] private int _blockScreenLayer = -1;

    // Internal use flag.
    private bool _spawnNewBlockFlag = false;
    private bool _gameOver = false;

    // Variables for use in functions.
    [SerializeField] private List<NumBlock> _allBlocksList = new List<NumBlock>();
    [SerializeField] private int _emptySpaces = 16;    
    private int _currentKey = 0;       
    private ulong[] _preMMHexBoard = new ulong[2];
    private MoveDirection _lastMoveDirection;
    private TimeSpan _totalTimeTestOne = TimeSpan.Zero;
    private TimeSpan _totalTimeTestTwo = TimeSpan.Zero;
    [SerializeField] private ulong _currentScore = 0;

    // Enum for returning status of the system to external scripts.
    public enum ResultCode
    { 
        Success,
        Failure,
        Busy,
        GameOver,
        Win2048
    }
    

    // Start is called before the first frame update
    void Start()
    {
        // The number of empty spaces depends on the grid size settings. 
        CalculateEmptySpaces();
        // The game starts with two randomly placed blocks.
        CreateNewNumBlock();
        CreateNewNumBlock();         
       
        
    }

    // Update is called once per frame
    void Update()
    {
        // If the flag is set, try spawning a new block. It is in the update because it will not spawn as long as blocks are moving, so it gets checked every time
        // to see if all the blocks have stopped yet.
        if (_spawnNewBlockFlag) { SpawnNewBlockWhenReady(); }
        
    }

    
    // Function for creating a new (properly random) number block. Returns true if succesful, false if not.
    public int[] CreateNewNumBlock()
    {
        // Check if there is room for a new block. 
        if (CalculateEmptySpaces() == 0) { return new int[] { 0,0 }; }
        //Random from inclusive min to EXCLUSIVE max. This selects a random position for the block.
        int randomCount = Random.Range(0, _emptySpaces);
        int passRandomCount = randomCount;
        // The function shifts over one time less than the random number, so increase random by 1.
        randomCount++;
        // Start the placement at 0,0.
        Vector2Int placementLocation = new Vector2Int(0,0); 
        
        // Shift over x times, where x is the random count -1. Do not count down when a block is already present. start at 1 and end at random count -1 because
        // the function needs to check at least once if the first location is already occupied.
        while(randomCount > 0)
        {
            // Check if a block is present at the current location. If there is not, count the randomCount down one.
            if(!_allBlocksList.Exists(x => x.GetGridLocation().Equals(placementLocation))) { randomCount--; }            
            // If the randomCount is now 0, that means we are done looking, and we can break from the while loop.
            if(randomCount == 0) { break; } 
            // If the location reaches the end of the row, increase column one and restart the row.
            if (placementLocation.x == _gridSize -1)
            {
                placementLocation.x = 0;
                placementLocation.y++;
                // Sanity check, this should never trigger. If the column is now beyond the grid, something went wrong.
                if (placementLocation.y == _gridSize)
                {
                    Debug.Log("Array error in numBlockManager create new.");
                    return new int[] { 0, 0 };
                }
            }
            // If it is not yet at the end of the row, hop one over.
            else
            {
                placementLocation.x++;
            }            
        }
        // Randomly determine the value of the new block. The block should have a value of 1 (2 in game) 75% of the time,
        // and 2 (4 in game) 25% of the time. Remember that int random has an exclusive maximum.
        // The value stored in the block is the power of two that the game value is. (2^1, 2^2, 2^3, etc).
        int value = Random.Range(1, 5);        
        if (value == 4)
        {
            value = 2;
        }
        else
        {
            value = 1;
        }
        
        // Create the new block, pass it's information to it, and add it to the block list. And decrease the number of empty spaces just to be sure.
        Vector3 spaceLocation = new Vector3Int(placementLocation.x * _gridScale, placementLocation.y * _gridScale,-1);
        NumBlock newBlock = Instantiate(_numBlockPreFab, spaceLocation, Quaternion.identity);
        newBlock.Initialize(value, placementLocation, _currentKey, _gridScale, _blockMoveSpeed, _blockScreenLayer, this);       
        _allBlocksList.Add(newBlock);        
        _currentKey++;
        _emptySpaces--;
        // Block created succesfully.
        return new int[] { passRandomCount, value };

    }
    // Public function to call by other script to give a move input. Returns a resultcode depending on result.
    public ResultCode GiveMoveInput(MoveDirection direction)
    {        
        // First check if a new block must still be spawned. If it does, return busy.
        if (_spawnNewBlockFlag) { return ResultCode.Busy; }
        // Check if all the blocks have stopped moving, if not return busy. (I think this is redundant)
        foreach (var block in _allBlocksList)
        {
            if (!block.IsAtDestination())  { return ResultCode.Busy; }
        }
        // Execute the move and merge function.
        MoveAndMerge(direction);         
        // Return Success 
        return ResultCode.Success;
    }
    // Function to calculate how many empty spaces for blocks are left.
    private int CalculateEmptySpaces()
    {
        // The number of empty space is the size of the grid minus the number of blocks in the block list.
        int emptySpaces = (_gridSize * _gridSize) - _allBlocksList.Count;
        // Set the class wide variable and return the count.
        _emptySpaces = emptySpaces;       
        return emptySpaces;
    }
    // Function to check if the gamestate is game over.
    private bool CheckGameOver()
    {
        
        // This function is only called when the board is full, so we only have to check direct neighbours,
        // no further blocks.
        // Check each block.
        foreach (var block in _allBlocksList)
        {
            // First create a list of blocks that have the same value as this block.
            List<NumBlock> matchingBlocksList = _allBlocksList.FindAll(x => x.GetValue().Equals(block.GetValue()));
            // If the list is longer than 1, that means there is another block with the same value.
            if (matchingBlocksList.Count > 1 )
            {
                // Loop over the matching blocks, and check how far away it is. If the distance is exactly 1, they are able to merge
                // and we can return false; the game has a chance to continue. We can use the faster sqrMagnitude function, because the 
                // square of 1 is conviniently 1.
                foreach (var matchingBlock in  matchingBlocksList)
                {
                    Vector2Int differenceVector = matchingBlock.GetGridLocation() - block.GetGridLocation();
                    int distance = differenceVector.sqrMagnitude;                    
                    if (distance == 1) { return false; }
                }
            }
            // If there are no other blocks with the same value, it can't merge and free up space.
            
        }
        // If we made it through the entire loop, and found no matching neighbours, the game is over, return true.
        _gameOver = true;
        return true;
    }    
    // Function to destroy the block identified by the key, and remove it from the list.
    public void DestroyBlockByKey(int key)
    {   
        // Find the target block in the list, give it command to destroy itself, and remove it from the list.
        NumBlock target = _allBlocksList.Find(x => x.GetKey().Equals(key));
        target.DestroyThisBlock();
        _allBlocksList.Remove(target);
    }
    // Functions that spawns a new block if all the blocks are not moving, and does nothing when it's not ready yet.
    private void SpawnNewBlockWhenReady()
    {
        // Check if there are any moving blocks. Simply returns the function if it finds a moving block.
        foreach (var block in _allBlocksList)
        {
            if (!block.IsAtDestination()) { return; }
        }
        // Create new block, and deassert the flag. Store random values for use in tests.
        int[] randomValues = CreateNewNumBlock();
        _spawnNewBlockFlag = false;
        // Then, if the board is full, check if the game is over.
        if (CalculateEmptySpaces() == 0) { CheckGameOver(); }  
        // TESTING: Now that the new state of the board is complete, run tests of the new systems.
        QuickMoveMergeTester(_preMMHexBoard, randomValues[0], randomValues[1], _lastMoveDirection);
        
    }
    // Function that does the whole move and merge in one. Is compatible with smooth block movement. It also sets the spawn new block flag.
    private void MoveAndMerge(MoveDirection direction)
    {
        Vector2Int startLocation;
        Vector2Int rowColumnDirection;
        Vector2Int inRowDirection;
        Vector2Int currentlyCheckingLocation;
        Vector2Int moveToLocation;        

        NumBlock staticBlock;
        NumBlock movingBlock;

        bool hasMovedOrMerged = false;

        // TESTING: Hexboard before move merge, and direction.
        _preMMHexBoard = GetCompactGamestate();
        _lastMoveDirection = direction;

        // Set the location where we start looking, and in which direction we look. If the blocks are to be moved up,
        // start top left, then go down in the "row", and after that right to the next "row". I call it "row" with ""
        // because it might be rows or columns depending on the direction, but since that changes, this is easier.
        switch (direction)
        {
            case MoveDirection.Left:
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(0, 1);
                inRowDirection = new Vector2Int(1, 0);
                break;
            case MoveDirection.Right:
                startLocation = new Vector2Int(_gridSize -1, 0);
                rowColumnDirection = new Vector2Int(0, 1);
                inRowDirection = new Vector2Int(-1, 0);
                break;
            case MoveDirection.Up:
                startLocation = new Vector2Int(0, _gridSize -1);
                rowColumnDirection = new Vector2Int(1, 0);
                inRowDirection = new Vector2Int(0, -1);
                break;
            case MoveDirection.Down:
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(1, 0); 
                inRowDirection = new Vector2Int(0, 1);
                break;
            default:
                // We should never get to the default.
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(0, 0);
                Debug.Log("Switch default error");
                return;                
        }
        // Set all the blocks merge status to not merged yet.
        foreach (NumBlock block in _allBlocksList) { block.ClearMergeRound(); }        

        // Outer loop, One loop for each "row".
        for (int i = 0;  i < _gridSize; i++)
        {
            // Inner loop, looping through the positions in the "row". Up to gridSize -1, because we won't ever move 
            // to the last position. Set the moveToLocation to the startLocation for each loop.
            moveToLocation = startLocation;
            for (int j = 0; j < _gridSize -1; j++)
            {
                currentlyCheckingLocation = moveToLocation;
                // We are looping over each possible target location in this loop, for each find if there are blocks that could move to that location.
                // This loop looks up to the last square. This looks less far, the further we are into the "row".
                for (int k = 0; k < _gridSize - j -1; k++)
                {
                    // Move over one square, then check if there is a block there. 
                    currentlyCheckingLocation += inRowDirection;                    
                    if (_allBlocksList.Exists(x => x.GetGridLocation().Equals(currentlyCheckingLocation)))
                    {
                        // If there is a block there, copy it over to a more easily handled variable.
                        // This block will potentially be either moving or merging, so it is called the movingblock.
                        movingBlock = _allBlocksList.Find(x => x.GetGridLocation().Equals(currentlyCheckingLocation));
                        // Then check if the destination is occupied or not.
                        if (_allBlocksList.Exists(x => x.GetGridLocation().Equals(moveToLocation)))
                        {
                            // If it is, check if we can merge or not. Values must be equal, and neither must have merged yet.
                            // Copy over for more readably checking. This block will for sure not move.
                            staticBlock = _allBlocksList.Find(x => x.GetGridLocation().Equals(moveToLocation));
                            if ((staticBlock.GetValue() == movingBlock.GetValue()) && !staticBlock.GetHasMergedThisRound() && !movingBlock.GetHasMergedThisRound())
                            {
                                // If it can merge, give to command.
                                movingBlock.MergeWithBlock(staticBlock);
                                hasMovedOrMerged = true;
                            }
                            // Whether we can merge or not, we stop the loop looking for blocks to move to this location.
                            break;

                        }
                        else
                        {
                            // If it is not occupied, the block can move here.
                            movingBlock.SetGridLocation(moveToLocation);
                            hasMovedOrMerged = true;
                            // We don't break the loop yet, since another block might merge with this one.
                        }
                    }
                    // If there is no block there, move on to the next square.
                }
                // Next square in the row.
                moveToLocation += inRowDirection;
            }
            // After each "row" check, move the "row" one over.
            startLocation += rowColumnDirection;
        }

        // Once done, set the spawn new block when ready flag IF a move or merge happened.
        if (hasMovedOrMerged) { _spawnNewBlockFlag = true; }             
    }
    // Game over status get.
    public bool GetGameOverStatus() { return _gameOver;  }
    // Function to check if the game is "won" aka, if there is a value of 2048 or higher
    public bool IsGameWon()
    {
        // If the game is lost, it isn't won.
        if (_gameOver) { return false; }
        // Then check if there exists a block that has a value of at least 11 (2^11 is 2048).
        return _allBlocksList.Exists(x => x.GetValue() > 10);

    }
    // Function that returns 2 64bit numbers, that together represent the boardstate.
    // The least significant hex is grid 0,0 and the most significant is 3,3
    public ulong[] GetCompactGamestate()
    {
        // First of all, this only works with gridsize 4, so check if that is right.
        if (_gridSize != 4) { return new ulong[] { 0, 0 }; }
        // Shifting constants.
        const int shiftOneRight = 4;
        const int shiftOneUp = 16;        
        // The board consists of 16 spaces (4x4). Using a 64 bit unsigned int, that gives room for 4 bits per space.
        // The primary Hexboard holds the values up to 15 (0xF) for each space. The secondary Hexboard holds all the 
        // overflow data. Combined that gives a full byte of data per value per space. The max value of a byte is 255,
        // and given that the "real value" of a block is 2^value, 2^255 is a unrealistically large value. A single byte
        // per space should be more than sufficient.
        ulong primaryHexBoard = 0;
        ulong secondaryHexBoard = 0;
        ulong primaryMask;
        ulong secondaryMask;
        // Loop over all the blocks, and place it's value in the HexBoards.
        foreach (var block in _allBlocksList)
        {
            // If the value of the block is bigger than a full byte, it won't fit in this system. That is extremely unlikely though.
            if (block.GetValue() > 0xFF) 
            {
                Debug.Log("Value of block is higher than 255. Wow, that is impressive.");
                return new ulong[] {0,0};
            }
            // Put the value in a ulong.
            ulong value = Convert.ToUInt64(block.GetValue());
            // Split the value up into the primary and secondary parts.
            primaryMask = 0xF;
            secondaryMask = 0xF;
            // This grabs the lower 4 bits.
            primaryMask = primaryMask & value;
            // Shift over 4 bits results in the upper 4 bits of the value (we checked earlier if the value had more than 8 bits).
            secondaryMask = value >> 4;
            // Determine the number of places the values need to be shifted over to get to the right space.
            int shiftSize = (block.GetGridLocation().x * shiftOneRight) + (block.GetGridLocation().y * shiftOneUp);
            // Then shift both masks by that much, and add them to the hexboards.
            primaryMask = primaryMask << shiftSize;
            secondaryMask = secondaryMask << shiftSize;
            primaryHexBoard += primaryMask;
            secondaryHexBoard += secondaryMask;         
            
        }
        // This should complete the Hexboards.        
        return new ulong[] { primaryHexBoard, secondaryHexBoard };
    }
    
    // Function to compare 2 (or later maybe more) different functions with the same goal. It displays the time it took each function.
    private void QuickMoveMergeTester(ulong[] originalHexBoard, int newBlockRandomLocation, int newBlockValue, MoveDirection direction)
    {
        // How many times each function is called to check runtime.
        const int loopCount = 10000;
        // The stopwatch to measure time
        Stopwatch stopwatch = new Stopwatch();
        // Run the first function once, something to do with .NET JITing. Store the results for comparing later.
        FastGameActionsPrototypeOne prototypeOne = new FastGameActionsPrototypeOne();
        ulong[] resultsOne = prototypeOne.MoveMerge(originalHexBoard, newBlockRandomLocation, newBlockValue, direction);
        // Then start the stopwatch, run the function loopCount times, and then stop the stopwatch.
        stopwatch.Start();
        for (int i = 0; i < loopCount; i++)
        {
            prototypeOne.MoveMerge(originalHexBoard, newBlockRandomLocation, newBlockValue, direction);
        }
        stopwatch.Stop();
        TimeSpan loopTimeTestOne = stopwatch.Elapsed;
        _totalTimeTestOne += loopTimeTestOne;
        // Do the same with the second function
        FastGameActionsPrototypeTwo prototypeTwo = new FastGameActionsPrototypeTwo();
        ulong[] resultsTwo = prototypeTwo.MoveMerge(originalHexBoard, newBlockRandomLocation, newBlockValue, direction);
        stopwatch.Reset();
        stopwatch.Start();
        for (int i = 0; i <= loopCount; i++)
        {
            prototypeTwo.MoveMerge(originalHexBoard, newBlockRandomLocation, newBlockValue, direction);
        }
        stopwatch.Stop();
        TimeSpan loopTimeTestTwo = stopwatch.Elapsed;
        _totalTimeTestTwo += loopTimeTestTwo;
        // Check if the results of the tests are correct.
        ulong[] checkHexBoard = GetCompactGamestate();
        bool testOneCorrect = ((checkHexBoard[0] == resultsOne[0]) && (checkHexBoard[1] == resultsOne[1]));
        bool testTwoCorrect = ((checkHexBoard[0] == resultsTwo[0]) && (checkHexBoard[1] == resultsTwo[1]));
        
        // Report findings.        
        if (testOneCorrect && testTwoCorrect)
        { }
        else
        {
            Debug.Log($"ERROR, INCORRECT RESULT One: {testOneCorrect}, two: {testTwoCorrect} \n\n\n ERROR \n\n ERROR");
        }
        Debug.Log($"Total Times so far: One: {_totalTimeTestOne.ToString()}, Two: {_totalTimeTestTwo.ToString()}");
        //Debug.Log($"Test complete. Test ran {loopCount} times. First function restult correct: {testOneCorrect}, Elapsed time: {loopTimeTestOne.Seconds} s, {loopTimeTestOne.Milliseconds} ms. Second function result correct: {testTwoCorrect}, Elapsed time: {loopTimeTestTwo.Seconds} s, {loopTimeTestTwo.Milliseconds} ms.");        
    }    
    // Function blocks can call when they merge to increase the game score
    public void IncreaseScore(int mergeValue)
    {
        _currentScore += ((ulong) 1 << mergeValue);
    }    
    // Score Get
    public ulong GetScore()
    {
        return _currentScore;
    }
    

}
