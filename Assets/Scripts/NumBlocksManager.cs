using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

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

    // Variables for use in functions.
    [SerializeField] private List<NumBlock> _allBlocksList = new List<NumBlock>();
    [SerializeField] private int _emptySpaces = 16;    
    private int _currentKey = 0;    
    
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
    public bool CreateNewNumBlock()
    {
        // Check if there is room for a new block. 
        if (CalculateEmptySpaces() == 0) { return false; }
        //Random from inclusive min to EXCLUSIVE max. This selects a random position for the block. The function shifts over one time less than the random number.
        int randomCount = Random.Range(1, _emptySpaces +1);
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
                    return false;
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
        return true;

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
        // Calculate the number of empty spaces, and if it is 0, check if the game is lost, if so return GameOver.
        if (CalculateEmptySpaces() == 0)
        {
            if (CheckGameOver()) { return ResultCode.GameOver; }
            
        }        
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
    // Function to check if the gamestate is game over. Not yet implemented.
    private bool CheckGameOver()
    {
        Debug.Log("Unimplemented game over check.");
        return false;
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
            if (!block.IsAtDestination())
            { return; }
        }
        // Create new block, and deassert the flag.
        CreateNewNumBlock();
        _spawnNewBlockFlag = false;
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
}
