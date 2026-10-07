using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float score = 0f;
    public float specialMeterValue, width, height;
    public float timeRemaining = 60f;

    public TMP_Text scoreText;
    public TMP_Text timerText;
    public RectTransform specialMeter;
    public GameObject gameOverPanel;

    // for replays
    private void Start()
    {
        Time.timeScale = 1f;
        score = 0;
        gameOverPanel.SetActive(false);
    }

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

    public void SetSpecialMeterValue(float value)
    {
        specialMeterValue += value;
        specialMeterValue = Mathf.Clamp(specialMeterValue, 0f, 100f);
        specialMeter.sizeDelta = new Vector2((specialMeterValue / 100f) * width, height);
        // do something here to indicate special
        if (specialMeterValue == 100) Debug.Log("special ready!");
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
