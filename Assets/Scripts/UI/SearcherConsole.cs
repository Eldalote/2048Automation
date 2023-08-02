using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SearcherConsole : MonoBehaviour
{
    [SerializeField] GameObject _consoleTextObject;
    private TextMeshProUGUI _consoleTextTMPUI;
    private string _consoleText = "";

    private int _maxConsoleLength = 12;

    // Start is called before the first frame update
    void Start()
    {
        _consoleTextTMPUI = _consoleTextObject.GetComponent<TextMeshProUGUI>();
        _consoleText = "Searcher output display";
        _consoleTextTMPUI.text = _consoleText;
        
    }

    // Update is called once per frame
    void Update()
    {
        _consoleTextTMPUI.text = _consoleText;
    }

    public void PrintToOnScreenConsole(string text)
    {
        //Debug.Log(text);
        string[] lines = _consoleText.Split("Searcher:");
        _consoleText = "";
        if (lines.Length > 0)
        {
            int cutOff = 0;
            if (lines.Length >= _maxConsoleLength) 
            {
                cutOff = 1 + lines.Length - _maxConsoleLength;
            }
            for (int i = 1 + cutOff; i < lines.Length; i++)
            {
                
                _consoleText += "Searcher:"+ lines[i];
                
            }
        }
        _consoleText += "Searcher:  " + text + "\n\n";
        Debug.Log(_consoleText);
    }
}
