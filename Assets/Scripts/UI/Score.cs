using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text _scoreText;
    public Text _numCollectedText;
    // Update is called once per frame
    void Start()
    {
        _scoreText.text = "Score: " + 0;
        _numCollectedText.text = "Gems: " + 0;
    }

    public void UpdateScore(int playerScore)
    {
        _scoreText.text = "Score: " + playerScore.ToString();
    }

    public void UpdateCollected(int playerCollected)
    {
        _numCollectedText.text = "Gems: " + playerCollected.ToString();
    }
}
