using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HighScoreEntry : MonoBehaviour
{
    [SerializeField] private Transform _backgroundTransform;
    [SerializeField] private GameObject _positionTextObject;
    [SerializeField] private GameObject _scoreTextObject;
    [SerializeField] private GameObject _nameTextObject;
    private TextMeshProUGUI _positionText;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _nameText;
    private Color _defaultColour = Color.white;
    private Color _currentColour = Color.green;

    private void Awake()
    {
        _positionText = _positionTextObject.GetComponent<TextMeshProUGUI>();
        _scoreText = _scoreTextObject.GetComponent<TextMeshProUGUI>();
        _nameText = _nameTextObject.GetComponent<TextMeshProUGUI>();

        

    }
    // Set the values of the entry
    public void SetEntry(bool backgroundOn, string positionText, string scoreText, string nameText)
    {
        _backgroundTransform.gameObject.SetActive(backgroundOn);
        _positionText.text = positionText;
        _scoreText.text = scoreText;
        _nameText.text = nameText;
        // Set the color of the text to currentcolour if the entry is current, to default if not.
        if (nameText == "Current" )
        {
            _positionText.color = _currentColour;
            _scoreText.color = _currentColour;
            _nameText.color = _currentColour;
        }
        else
        {
            _positionText.color = _defaultColour;
            _scoreText.color = _defaultColour;
            _nameText.color = _defaultColour;
        }
    }
}