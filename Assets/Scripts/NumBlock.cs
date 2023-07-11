using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class NumBlock : MonoBehaviour
{
    [SerializeField] private int _value;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Vector2Int _location;
    private int _gridScale;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValue(int value)
    {
        _value = value;
        SetColour();
    }

    private void SetColour()
    {
        if(_value == 1)
        {
            _spriteRenderer.color = Color.green;
        }
        else if(_value == 2)
        {
            _spriteRenderer.color = Color.red;
        }
        
    }  
    public void SetLocation(Vector2Int location)
    {
        _location = location;
    }
    public void SetGridScale(int gridScale)
    { 
        _gridScale = gridScale; 
    }

}
