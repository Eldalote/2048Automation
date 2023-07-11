using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumBlocksManager : MonoBehaviour
{

    [SerializeField] private int _gridScale;
    [SerializeField] private NumBlock _Numblock;

    [SerializeField] private Dictionary<Vector2Int, NumBlock> _allBlocks = new Dictionary<Vector2Int, NumBlock>();
    [SerializeField] private int _emptySpaces = 16;

    // Start is called before the first frame update
    void Start()
    {
        CreateNewNumBlock();
        CreateNewNumBlock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    [ContextMenu("Add Block")]
    public bool CreateNewNumBlock()
    {
        if (_emptySpaces == 0) { return false; } 
        int locationCount = Random.Range(1, _emptySpaces +1);        //Random from inclusive min to EXCLUSIVE max
        Vector2Int location = new Vector2Int(0,0); // start of location
        
        while(locationCount > 0)
        {
           if(!_allBlocks.ContainsKey(location))
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
                    Debug.Log("Array error in numBlockManager create new");
                    return false;
                }
            }
            else
            {
                location.x++;
            }            
        }
        
        int value = Random.Range(1, 5);
        Debug.Log(value);
        if (value == 4)
        {
            value = 2;
        }
        else
        {
            value = 1;
        }
        Vector3 spaceLocation = new Vector3Int(location.x * _gridScale, location.y * _gridScale,-1);
        NumBlock newBlock = Instantiate(_Numblock, spaceLocation, Quaternion.identity);
        newBlock.SetValue(value);
        newBlock.SetLocation(location);
        newBlock.SetGridScale(_gridScale);
        _allBlocks.Add(location, newBlock);
        _emptySpaces--;

        return true;

    }
}
