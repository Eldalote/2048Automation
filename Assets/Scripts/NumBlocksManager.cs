using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class NumBlocksManager : MonoBehaviour
{

    [SerializeField] private int _gridScale;
    [SerializeField] private NumBlock _Numblock;
    [SerializeField] private int _gridSize = 4;

    [SerializeField] private List<NumBlock> _allBlocksList = new List<NumBlock>();
    [SerializeField] private int _emptySpaces = 16;
    private bool _busyMoving = false;
    private int _currentKey = 0;
    [SerializeField] private List<int> _destroyOnArrival = new List<int>();

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
        CalculateEmptySpaces();
        CreateNewNumBlock();
        CreateNewNumBlock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    // Function for creating a new (properly random) number block. Returns true if succesful, false if not.
    public bool CreateNewNumBlock()
    {
        if (_emptySpaces == 0) { return false; }
        //Random from inclusive min to EXCLUSIVE max.
        int locationCount = Random.Range(1, _emptySpaces +1);
        // Start of location.
        Vector2Int location = new Vector2Int(0,0); 
        
        //Shift over x times, where x is the random location count. Do not count down when a block is already present.
        while(locationCount > 0)
        {
           if(!_allBlocksList.Exists(x => x.GetGridLocation() == location))
            {
                locationCount--;
            }
           if(locationCount == 0)
            {
                break;
            }
           if (location.x == 3)
            {
                location.x = 0;
                location.y++;
                if (location.y == 4)
                {
                    Debug.Log("Array error in numBlockManager create new.");
                    return false;
                }
            }
            else
            {
                location.x++;
            }            
        }
        // Set the value of the new block. The block should have a value of 1 (2^1) 75% of the time,
        // and 2 (2^2) 25% of the time. Remember that int random has an exclusive maximum.
        int value = Random.Range(1, 5);        
        if (value == 4)
        {
            value = 2;
        }
        else
        {
            value = 1;
        }
        // Create the new block, pass it's information to it, and remember to decrease the number of empty spaces by 1.
        Vector3 spaceLocation = new Vector3Int(location.x * _gridScale, location.y * _gridScale,-1);
        NumBlock newBlock = Instantiate(_Numblock, spaceLocation, Quaternion.identity);
        newBlock.SetValue(value);
        newBlock.SetLocation(location);
        newBlock.SetGridScale(_gridScale);
        newBlock.SetKey(_currentKey);        
        _allBlocksList.Add(newBlock);        
        _currentKey++;
        _emptySpaces--;

        return true;

    }

    public ResultCode GiveInput(MoveDirection direction)
    {
        
        // Check if all the blocks have stopped moving, if not return busy.
        foreach (var block in _allBlocksList)
        {
            if (!block.AtDestination)
            { return ResultCode.Busy; }
        }     
        // Calculate the number of empty spaces, and if it is 0, check if the game is lost, if so return GameOver.
        // If there is room, move before merging.
        if (CalculateEmptySpaces() == 0)
        {
            if (CheckGameOver())
            { return ResultCode.GameOver; }
        }
        else
        {
            MoveBlocks(direction);
        }
        // Merge in the direction. The merge also calls the follow up move, so we don't have to do that again.
        MergeBlocks(direction);
        // Check again if there are empty spaces. If there are none, something is wrong with the CheckGameOver code.
        if (CalculateEmptySpaces() == 0)
        {
            Debug.Log("Something wrong with Game over check. There should be space, but there is not.");            
        }
        else
        {
            // Since there is space, spawn a new block, and if there are now no open spaces, check for game over again.
            CreateNewNumBlock();
            if (_emptySpaces == 0)
            {
                if(CheckGameOver()) return ResultCode.GameOver;
            }
        }

        // Return Success 
        return ResultCode.Success;
    }

    private int CalculateEmptySpaces()
    {
        int EmptySpaces = _gridSize * _gridSize;
        foreach (var block in _allBlocksList)
        {
            EmptySpaces--;
        }
        _emptySpaces = EmptySpaces;
        return EmptySpaces;
    }
    private bool CheckGameOver()
    {
        return false;
    }

    // Move the blocks in the direction specified. Current implementation quite messy, maybe better option?
    private void MoveBlocks(MoveDirection direction)
    {
        // Get a vector that corresponds to the move direction
        Vector2Int MoveCalcVector = CalculateDirection(direction);
        bool doneMoving = false;
        int sanityCheck = 0;
        // We loop over each block, to check if it can move one over, and loop this untill no block can move anymore.
        while (!doneMoving)
        {
            // Set as done, any move sets it as not done.
            doneMoving = true;
            foreach (var block in _allBlocksList)
            {
                // Get the coordinate that the block wants to move to. Check if it is within the playing field.
                Vector2Int wantToMoveToLocation = block.GetGridLocation() + MoveCalcVector;
                if ((wantToMoveToLocation.x >= 0 && wantToMoveToLocation.x < _gridSize) && (wantToMoveToLocation.y >= 0 && wantToMoveToLocation.y < _gridSize))
                {
                    // If there is no block at the destination, move to that destination.                    
                    if (!_allBlocksList.Exists(x => x.GetGridLocation() == wantToMoveToLocation))
                    {
                        block.SetLocation(wantToMoveToLocation);
                        doneMoving = false;
                    }
                }
            }
            // Make sure we dont get stuck in a while loop (for debug). DEBUG REMOVE
            sanityCheck++;
            if (sanityCheck > 1000)
            {
                Debug.Log("While loop error in move blocks.");
                break;
            }

        }
        
    }
    private void MergeBlocks(MoveDirection direction)
    {
        
        bool hasMerged = false;
        // Get the oposite vector of the movement vector, we check from the furthest back.
        Vector2Int mergeDirectionVector = CalculateDirection(direction) * -1;
        Debug.Log($"MergeDirectionVector: {mergeDirectionVector}");
        // The current location starts at the far end of the line. The mergeGroupDirect determines if we work in columns or rows.
        Vector2Int mergeStartLocation;
        Vector2Int mergeGroupDirection;
        Vector2Int currentLocation;
        
        switch (direction)
        {
            case MoveDirection.Left:
                mergeStartLocation = new Vector2Int(0, 0);
                mergeGroupDirection = new Vector2Int(0, 1);
                break;
            case MoveDirection.Right:
                mergeStartLocation = new Vector2Int(_gridSize, 0);
                mergeGroupDirection = new Vector2Int(0, 1);
                break;
            case MoveDirection.Up:
                mergeStartLocation = new Vector2Int(0, _gridSize);
                mergeGroupDirection = new Vector2Int(1, 0);
                break;
            case MoveDirection.Down:
                mergeStartLocation = new Vector2Int(0, 0);
                mergeGroupDirection = new Vector2Int(1, 0);
                break;
            default:
                mergeStartLocation = new Vector2Int(0, 0);
                mergeGroupDirection = new Vector2Int(0, 0);
                Debug.Log("Switch default error");
                return;
                break;
        }
        for (int i = 0 ; i < _gridSize; i++)
        {
            // Reset the current location to the correct starting position (rows/columns may have been moved).
            currentLocation = mergeStartLocation;
            for (int j = 0 ; j < _gridSize; j++)
            {
                // Check if there is a block at the current location.
                if (_allBlocksList.Exists(x => x.GetGridLocation() == currentLocation))
                {
                    // Check if the block has a neighbour
                    Vector2Int neighbourLocation = currentLocation + mergeDirectionVector;
                    if (_allBlocksList.Exists(x => x.GetGridLocation() == neighbourLocation))
                    {
                        // If there is a block and a neighbour check if their values match. If they do, increase value of this block and delete neighbour.
                        if (_allBlocksList.Find(x => x.GetGridLocation() == currentLocation).GetValue() == _allBlocksList.Find(y => y.GetGridLocation() == neighbourLocation).GetValue())
                        {
                            // Nasty line of code says find value and add one.
                            _allBlocksList.Find(x => x.GetGridLocation() == currentLocation).SetValue(1 + _allBlocksList.Find(x => x.GetGridLocation() == currentLocation).GetValue());
                            DestroyBlockAtLocation(neighbourLocation);
                            //NumBlock neighbour = _allBlocksList.Find(x => x.GetGridLocation().Equals(neighbourLocation));
                            //_allBlocksList.Remove(neighbour);
                            //Destroy(neighbour);
                            hasMerged = true; 
                        }                              

                    }
                }
                currentLocation = currentLocation + mergeDirectionVector;
            }
            mergeStartLocation += mergeGroupDirection;
        }



        if (hasMerged) MoveBlocks(direction);

    }

    private Vector2Int CalculateDirection(MoveDirection direction)
    {
        Vector2Int MoveVector;
        switch (direction)
        {
            case MoveDirection.Left:
                MoveVector = new Vector2Int(-1, 0);
                break;
            case MoveDirection.Right:
                MoveVector = new Vector2Int(1, 0);
                break;
            case MoveDirection.Up:
                MoveVector = new Vector2Int(0, 1);
                break;
            case MoveDirection.Down:
                MoveVector = new Vector2Int(0, -1);
                break;
            default:
                MoveVector = new Vector2Int(0, 0);
                break;
        }

        return MoveVector;
    }
    private int GetBlockValue(Vector2Int location)
    {
        return _allBlocksList.Find(x => x.GetGridLocation().Equals(location)).GetValue();
    }
    private void SetBlockValue(Vector2Int location, int value)
    {
        _allBlocksList.Find(x => x.GetGridLocation().Equals(location)).SetValue(value);
    }
    private void DestroyBlockAtLocation(Vector2Int location)
    {
        //Destroy(_allBlocksList.Find(x => x.GetGridLocation().Equals(location)));
        NumBlock target = _allBlocksList.Find(x => x.GetGridLocation().Equals(location));
        target.DestroyThisBlock();
        //_allBlocksList.Find(x => x.GetGridLocation().Equals(location)).DestroyThisBlock();
        _allBlocksList.Remove(target);
        //_allBlocksList.RemoveAll(x => x == null);
    }
    private void DestroyBlockByKey(int key)
    {
        //Destroy(_allBlocksList.Find(x => x.GetKey().Equals(key)));
        _allBlocksList.Find(x => x.GetKey().Equals(key)).DestroyThisBlock();
        _allBlocksList.RemoveAll(x => x == null);
    }
    private void MoveAndMerge(MoveDirection direction)
    {
        Vector2Int startLocation;
        Vector2Int rowColumnDirection;
        Vector2Int inRowDirection;
        Vector2Int currentLocation;

        switch (direction)
        {
            case MoveDirection.Left:
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(0, 1);
                inRowDirection = new Vector2Int(1, 0);
                break;
            case MoveDirection.Right:
                startLocation = new Vector2Int(_gridSize, 0);
                rowColumnDirection = new Vector2Int(0, 1);
                inRowDirection = new Vector2Int(-1, 0);
                break;
            case MoveDirection.Up:
                startLocation = new Vector2Int(0, _gridSize);
                rowColumnDirection = new Vector2Int(1, 0);
                inRowDirection = new Vector2Int(0, -1);
                break;
            case MoveDirection.Down:
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(1, 0); 
                inRowDirection = new Vector2Int(0, -1);
                break;
            default:
                startLocation = new Vector2Int(0, 0);
                rowColumnDirection = new Vector2Int(0, 0);
                Debug.Log("Switch default error");
                return;
                break;
        }
    }
}
