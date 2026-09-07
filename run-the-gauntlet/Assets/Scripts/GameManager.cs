using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float score = 0;
    public float timeRemaining = 60f;

    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject gameOverPanel;

    void Update()
    {
        // update score
        scoreText.text = score.ToString();
        // countdown
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            // end the game
            timeRemaining = 0;
            TimerEnded();
        }
        DisplayTime(timeRemaining);
    }

    // format to minutes:seconds
    void DisplayTime(float timeToDisplay)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeToDisplay);
        timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    // game over logic
    // display game over panel with Game Over text and final score
    void TimerEnded()
    {
        gameOverPanel.transform.Find("ScoreText").GetComponent<TMP_Text>().text = score.ToString();
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
