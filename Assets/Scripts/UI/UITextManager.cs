using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private NumBlocksManager _numBlocksManager;
    [SerializeField] private TextMeshProUGUI _GameOverText;


    
    void Awake()
    {
        NumBlocksManager.scoreChanged += ScoreChanged;
        NumBlocksManager.gameOver += GameOverCalled;
        NumBlocksManager.gameStarted += GameStartCalled;
    }

    // Update is called once per frame
    void Update()
    {        
        
    }
    private void ScoreChanged(ulong score)
    {
        string scoreString = new string($"Score: \n{score}");
        _scoreText.SetText(scoreString);
    }
    private void GameOverCalled()
    {
        _GameOverText.gameObject.SetActive(true);
    }
    private void GameStartCalled()
    {
        _GameOverText.gameObject.SetActive(false);
    }
}
