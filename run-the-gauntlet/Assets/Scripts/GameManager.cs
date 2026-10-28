using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float score = 0f;
    public float specialMeterValue;
    public float width, height;
    [SerializeField] private float timeRemaining = 60f;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private RectTransform specialMeter;
    [SerializeField] private GameObject gameOverPanel;

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
        if (specialMeterValue == 100)
        {
            specialMeter.GetComponent<Image>().color = Color.yellow;
            Debug.Log("special ready!");
        }
        else
        {
            specialMeter.GetComponent<Image>().color = Color.white;
        }
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

    public void GoHome()
    {
        Time.timeScale = 1f; // Reset pause state, if applicable
        SceneManager.LoadScene("RiveStart");
    }
}
