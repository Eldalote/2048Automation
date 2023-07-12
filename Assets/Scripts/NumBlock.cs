using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;

public class NumBlock : MonoBehaviour
{
    [SerializeField] private int _value;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Vector2Int _gridLocationToBe;
    [SerializeField] private Vector3 _screenLocationToBe;
    [SerializeField] private TextMeshPro _textMeshPro;
    [SerializeField] private int _movespeed = 25;
    public bool AtDestination = true;
    public bool HasMergedThisRound = false;
    private int _gridScale;
    [SerializeField] private int _serialNumber;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLocation();
    }

    public void SetValue(int value)
    {
        _value = value;
        SetColour();
        SetText();
    }
    private void SetText()
    {
        if (_value < 12)
        {
            _textMeshPro.color = Color.black;
        }
        else
        {
            _textMeshPro.color = Color.white;
        }        
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
    public void SetLocation(Vector2Int location)
    {
        _gridLocationToBe = location;        
    }
    private void UpdateLocation()
    {
        _screenLocationToBe = new Vector3(_gridLocationToBe.x * _gridScale, _gridLocationToBe.y * _gridScale, -1);
        Vector3 differenceInLocation = _screenLocationToBe - transform.position;
        if ( (differenceInLocation.x < 1 && differenceInLocation.x > -1) && (differenceInLocation.y < 1 && differenceInLocation.y > -1) )
        {
            transform.position = _screenLocationToBe;
            AtDestination = true;
            
        }
        else
        {
            differenceInLocation.Normalize();
            transform.position = transform.position + (differenceInLocation * Time.deltaTime * _gridScale * _movespeed);
            AtDestination = false;
        }
        
    }
    public void SetGridScale(int gridScale)
    { 
        _gridScale = gridScale; 
    }

    public Vector2Int GetGridLocation()
    { return _gridLocationToBe; }

    public int GetValue()
    { return _value; }

    public int GetKey()
    {
        return _serialNumber;
    }

    public void SetKey(int key)
    {
        _serialNumber = key;
    }
    public void DestroyThisBlock()
    {
        Destroy(gameObject);
    }

}
