using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SearcherConsole : MonoBehaviour
{
    [SerializeField] GameObject _consoleTextObject;
    private TextMeshProUGUI _consoleTextTMPUI;
    private string _consoleText = "";

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
        _consoleText = "Searcher:  " +  text + "\n\n";        
        if (lines.Length > 0)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                if (i < 10)
                {
                    _consoleText += "Searcher:"+ lines[i];
                }
            }
        }
        Debug.Log(_consoleText);
    }
}
