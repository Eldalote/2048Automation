using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreTable : MonoBehaviour
{
    // Container for the entries and the template for them.
    [SerializeField] private Transform _entryContainer;
    [SerializeField] private HighScoreEntry _entryTemplate;

    [SerializeField] private InputManager _inputManager;

    // Variables.
    private HighScoreEntry _headerEntry;
    private List<HighScoreEntry> _highScoreEntryList = new List<HighScoreEntry>();
    private Highscores _highScores = new Highscores();
    private int _entryHeight = 50;
    private int _numberOfScoresKept = 15;
    private string _highscoreSaveLocation = "Saves/HighScores.dat";

   


    private void Awake()
    {
        // Subscribe to relevant numblock manager events.
        NumBlocksManager.gameStarted += NewCurrentScore;
        NumBlocksManager.gameOver += SaveGameEndScores;
        NumBlocksManager.scoreChanged += UpdateCurrentScore;

        // Set the template as the header.
        _headerEntry = _entryTemplate;
        _headerEntry.gameObject.name = "Highscore Header";
        _highScores = new Highscores();
        _highScores.ScoreList = new List<ScoreEntry>();

    }


    // Simple bubble sort for the highscores.
    private void SortHighScores(Highscores highscores)
    {
        for (int i = 0; i < highscores.ScoreList.Count; i++)
        {
            for (int j = i +1;  j < highscores.ScoreList.Count; j++)
            {
                if (highscores.ScoreList[j].Score > highscores.ScoreList[i].Score)
                {
                    // Swap
                    var temp = highscores.ScoreList[j];
                    highscores.ScoreList[j] = highscores.ScoreList[i];
                    highscores.ScoreList[i] = temp;
                }
            }
        }
    }
    //  Add new current score.
    private void NewCurrentScore()
    {
        string readFromFile = string.Empty;
        FileManager.LoadFromFile(_highscoreSaveLocation, out readFromFile);
        _highScores.ScoreList.Clear();
        _highScores = JsonUtility.FromJson<Highscores>(readFromFile);
        ScoreEntry  currentScore = new ScoreEntry {Score = 0, Name = "Current"};
        _highScores.ScoreList.Add(currentScore);
    }
    // Save the current score to the highscore.
    private void SaveGameEndScores()
    {
        // Start by renaming current to player or auto.
        ScoreEntry currentScore = _highScores.ScoreList.Find(x => x.Name == "Current");
        // If automated, save as Auto, else save as Player.
        if (_inputManager.GetAutomatedStatus())
        {
            currentScore.Name = "Auto";
        }
        else
        {
            currentScore.Name = "Player";
        }        
        // Then, if the number of entries is higher than the amount we should keep, drop the lowest.
        if (_highScores.ScoreList.Count > _numberOfScoresKept)
        {
            _highScores.ScoreList.Remove(_highScores.ScoreList[_numberOfScoresKept]);
        }
        // Then convert to JSON, and store in file.
        string JSON = JsonUtility.ToJson(_highScores);
        FileManager.WriteToFile(_highscoreSaveLocation, JSON);
    }
    private void UpdateCurrentScore(ulong score)
    {
        if(!_highScores.ScoreList.Exists(x => x.Name == "Current"))
        {
            return;
        }
        ScoreEntry currentScore = _highScores.ScoreList.Find(x => x.Name == "Current");
        currentScore.Score = score;
        SortHighScores(_highScores);
        DisplayScores();

    }
    private void DisplayScores()
    {
        foreach (var score in _highScoreEntryList)
        {
            Destroy(score.gameObject);
        }
        _highScoreEntryList.Clear();
        for ( int i = 0; i < _highScores.ScoreList.Count; i++)
        {
            // Instantiate new displayEntry, and get it's rectTransform, then set it do the correct height.
            HighScoreEntry displayEntry = Instantiate(_entryTemplate, _entryContainer);
            RectTransform entryRectTransform = displayEntry.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0f, -(i + 1) * _entryHeight);
            // Toggle background on/off depending on place in list, so we get alternating backgrounds.
            bool backgroundOn = (i % 2 == 1);
            // Store all the information in strings for more readable entry.
            int position = i + 1;
            string positionString = position.ToString();
            string scoreString = _highScores.ScoreList[i].Score.ToString();
            string nameString = _highScores.ScoreList[i].Name;
            displayEntry.SetEntry(backgroundOn, positionString, scoreString, nameString);
            _highScoreEntryList.Add(displayEntry);
        }
    }






    // Serializable entry class.
    [System.Serializable]
    private class ScoreEntry
    {
        public ulong Score;
        public string Name;
    }
    // Class that holds the list, for JSONification.
    private class Highscores
    {
        public List<ScoreEntry> ScoreList;
    }    
}
