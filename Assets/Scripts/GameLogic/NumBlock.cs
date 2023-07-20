using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class NumBlock : MonoBehaviour
{
    // Unity parts attached to the NumBlock.
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _textMeshPro;
    
    // Variables.
    [SerializeField] private int _value;    
    [SerializeField] private Vector2Int _gridLocation;        
    [SerializeField] private int _moveSpeed = 25;
    private int _gridScale;
    [SerializeField] private int _key;
    private int _screenLayer = -1;

    // Status flags.
    private bool _atDestination = true;    
    private bool _hasMergedThisRound = false;   
    private bool _mergeOnArrival = false;

    // References to other objects.
    private NumBlock _mergeTarget;
    private NumBlocksManager _managerParent;

    // Awake function. Attached SpriteRenderer and TextMeshPro are referenced here.
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();       
    }
    // Since the constructor cannot be used due to unity thingies, I use the Initialize function as a sort of constructor.
    public void Initialize(int value, Vector2Int gridlocation, int key, int gridscale, int movespeed, int screenLayer, NumBlocksManager parent)
    {
        _value = value;
        _gridLocation = gridlocation;
        _key = key;
        _gridScale = gridscale;
        _moveSpeed = movespeed;
        _screenLayer = screenLayer;
        _managerParent = parent;
        // Call functions to display correct colour and text.
        SetColour();
        SetText();
        // Name the gameobject for clarity in the unity editor.
        gameObject.name = $"Block {gridlocation.x}, {gridlocation.y}";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScreenLocation();
    }

    // Updates value of block, and sets the text and colour acordingly.
    private void UpdateValue(int value)
    {
        _value = value;
        SetColour();
        SetText();
    }
    // Set the text of the block. With fitting font size and colour.
    private void SetText()
    {
        if (_value < 12)
        {
            _textMeshPro.color = Color.black;
        }
        else
        {
            // Block colour at this value is black.
            _textMeshPro.color = Color.white;
        }        
        // This effectively does 2^value.
        int displayValue = 1 << _value;
        _textMeshPro.text = displayValue.ToString();
        if (_value < 4)
        {
            _textMeshPro.fontSize = 60;
        }
        else if (_value < 7)
        {
            _textMeshPro.fontSize = 50;
        }
        else if (_value < 10)
        {
            _textMeshPro.fontSize = 40;
        }
        else if (_value < 14)
        {
            _textMeshPro.fontSize = 30;
        }
        else
        {
            _textMeshPro.fontSize = 20;
        }
    }
    // Set the colour of the block, depending on the value.
    private void SetColour()
    {
        // The colour of the block depends on it's value. After a certain value it just turns Black.
        switch (_value)
        {
            case 1:
                // 2
                // Beige: #F5F5DC -> (0.96f, 0.96f, 0.86f) 
                _spriteRenderer.color = new Color(0.96f, 0.96f, 0.86f);
                break;
            case 2:
                // 4
                // Wheat: #F5DEB3 -> (0.96f, 0.87f, 0.7f)
                _spriteRenderer.color = new Color(0.96f, 0.87f, 0.7f);
                break;
            case 3:
                // 8
                // Light orange: #FFB500 -> (1, 0.71f, 0)
                _spriteRenderer.color = new Color(1, 0.55f, 0);
                break;
            case 4:
                // 16
                // Dark orange: #FF8C00 -> (1, 0.55f, 0)
                _spriteRenderer.color = new Color(1, 0.55f, 0);
                break;
            case 5:
                // 32
                // Orange red: #FF4500 -> (1, 0.27f, 0)
                _spriteRenderer.color = new Color(1, 0.27f, 0);
                break;
            case 6:
                // 64
                // Red
                _spriteRenderer.color = Color.red;
                break;
            case 7:
                // 128
                // Light Yellow: #EDCF73 -> (0.93f, 0.81f, 0.45f)
                _spriteRenderer.color = new Color(0.93f, 0.81f, 0.45f);
                break;
            case 8:
                // 256
                // Light Yellow, slightly darker: #EDCC62 -> (0.93f, 0.8f, 0.38f)
                _spriteRenderer.color = new Color(0.93f, 0.8f, 0.38f);
                break;
            case 9:
                // 512
                // Light Yellow, slightly darker again: #EDC53F (0.93f, 0.77f, 0.25f)
                _spriteRenderer.color = new Color(0.93f, 0.77f, 0.25f);
                break;
            case 10:
                // 1024
                // Yellow
                _spriteRenderer.color = Color.yellow;
                break;
            case 11:
                // 2048
                // Yellow
                _spriteRenderer.color = Color.yellow;
                break;
            default:
                _spriteRenderer.color = Color.black;
                break;
        }



    }  
    // Sets the gridlocation of the block, and toggles the is at location flag if appropiate.
    public void SetGridLocation(Vector2Int location)
    {
        if (_gridLocation != location) { _atDestination = false; }
        _gridLocation = location;
        // Name object for clarity in the unity editor.
        gameObject.name = $"Block {location.x}, {location.y}";
    }
    // Function to update the location of the block on the screen. It moves the block to the correct grid location. This function is called in the update method.
    private void UpdateScreenLocation()
    {
        // Calculate the screen location the block must move to by multiplying the grid location by the gridscale. 
        Vector3 screenLocationToBe = new Vector3(_gridLocation.x * _gridScale, _gridLocation.y * _gridScale, _screenLayer);
        // Calculate the direction the block must move to by calculating the difference between location it should be at, and where it is.
        Vector3 differenceInLocation = screenLocationToBe - transform.position;
        // If the difference in location is very small, set it as being at the correct location. Skipping this step will result in oscilation through overshoot.
        if ( (differenceInLocation.x < 1 && differenceInLocation.x > -1) && (differenceInLocation.y < 1 && differenceInLocation.y > -1) )
        {
            transform.position = screenLocationToBe;
            // This also sets the flag that the block has reached it's destination. (and is not currenly moving)
            _atDestination = true;
            // If merge flag is set, do the merge.
            if (_mergeOnArrival) 
            {
                // Call the pop animation before the merge.
                DeathAnimation();
                FinalMergeCall(); 
            }            
        }
        else
        {
            // If we are not very close yet, first normalize the movement vector. This sets the length to 1. If we don't do this the move speed would
            // decrease as we get closer to the destination.
            Vector3 movementVector = differenceInLocation;
            movementVector.Normalize();
            // Multiply the movementvector by the moveSpeed, the gridScale, and the frame time. Multiplying by the frametime makes the movespeed independant of FPS.
            movementVector = movementVector * Time.deltaTime * _gridScale * _moveSpeed;
            // Check if the length of the movement vector is greater than the length of the difference vector. If it is, that will result in overshoot,
            // so to stop overshoot the location will be set to the destination instead.
            if (movementVector.sqrMagnitude > differenceInLocation.sqrMagnitude )
            {
                transform.position = screenLocationToBe;
            }
            // If there will be no overshoot, move in the direction of the destination.
            else
            {
                transform.position = transform.position + movementVector;
            }            
            // Unsure if this is still needed, but since we are moving, set the flag to false.
            _atDestination = false;
        }        
    }
    // Gridlocation get.
    public Vector2Int GetGridLocation()  { return _gridLocation; }
    // Value get.
    public int GetValue() { return _value; }
    // Key get.
    public int GetKey() { return _key; }
    // Is at destination flag get.
    public bool IsAtDestination() { return _atDestination; }
    // Function to destroy the game object holding this script. Public so that the manager can call for it's destruction.
    public void DestroyThisBlock() { Destroy(gameObject); }    
    // Function called by the manager to initiate a block merge.
    public void MergeWithBlock(NumBlock mergeTarget)
    {
        // Set merged flag for self and the merge target, set the grid location to the location of the merge target and then set the merge on arrival flag.
        // Stores the merge target reference so it can be used at the final merge call.      
        SetHasMergedThisRound();
        _mergeTarget = mergeTarget;
        _mergeTarget.SetHasMergedThisRound();
        SetGridLocation(mergeTarget.GetGridLocation());
        _mergeOnArrival = true;        
    }
    // Function called my the update screen location function if the merge flag has been set. This means that the merge only happens when the blocks have properly
    // moved to the correct location on screen.
    private void FinalMergeCall()
    {        
        // Increase the value of the merge target by one.
        _mergeTarget.IncreaseValueByOne();
        // Increase the score.
        _managerParent.IncreaseScore(_value + 1);
        // Give parent instruction to distroy this block (the parent needs the call to remove it from it's list).
        _managerParent.DestroyBlockByKey(_key);

    }
    // Public set function for the value. Value only ever needs to be increased by one.
    public void IncreaseValueByOne() { UpdateValue(_value + 1); }
    // Set (clear) function for the has merged flag.
    public void ClearMergeRound() { _hasMergedThisRound = false; }
    // Has merged flag get.
    public bool GetHasMergedThisRound() { return _hasMergedThisRound; }
    // Has merged flag set. This is used when another block merges with this one.
    public void SetHasMergedThisRound() { _hasMergedThisRound = true; }
    // Death animation function. Called right before destruction.
    private void DeathAnimation()
    {
        // Needs work, this is not really visible (framerate probably too high to be seen)
        transform.localScale = Vector3.one * 1.3f;
    }
}

