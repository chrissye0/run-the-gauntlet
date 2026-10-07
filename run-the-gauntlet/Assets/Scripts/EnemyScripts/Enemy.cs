using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private TMP_Text punchText;
    private List<List<ArduinoConnector.PunchType>> combosList = new List<List<ArduinoConnector.PunchType>>();
    private List<ArduinoConnector.PunchType> randomCombo = new List<ArduinoConnector.PunchType>();
    private GameObject player;
    private PlayerTargeting playerTargeting;

    private int tries = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        punchText = GetComponentInChildren<TMP_Text>(true);
        player = GameObject.FindWithTag("Player");
        playerTargeting = player.GetComponent<PlayerTargeting>();
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab });
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Cross });
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Hook });
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Uppercut });
        // get random punch combination
        randomCombo = combosList[Random.Range(0, combosList.Count)];
    }

    private void Update()
    {
        if (this.gameObject == playerTargeting.ActiveEnemy)
        {
            punchText.gameObject.SetActive(true);
            if (randomCombo[0] == ArduinoConnector.PunchType.Jab) punchText.text = "JAB";
            if (randomCombo[0] == ArduinoConnector.PunchType.Cross) punchText.text = "CROSS";
            if (randomCombo[0] == ArduinoConnector.PunchType.Hook) punchText.text = "HOOK";
            if (randomCombo[0] == ArduinoConnector.PunchType.Uppercut) punchText.text = "UPPERCUT";
        } else
        {
            punchText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    public bool CheckPunch(ArduinoConnector.PunchType punchType)
    {
        Debug.Log(randomCombo[0]);
        // change this later when combos with multiple punches are integrated
        // if punch matches current needed punch OR player maxes out on tries
        if (randomCombo[0] == punchType || tries == 2)
        {
            tries = 0;
            return true;
        }
        else
        {
            tries += 1;
            return false;
        }
    }
}
