using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private TMP_Text punchText;
    [SerializeField] private GameObject image;
    private List<List<ArduinoConnector.PunchType>> combosList = new List<List<ArduinoConnector.PunchType>>();
    private List<ArduinoConnector.PunchType> randomCombo = new List<ArduinoConnector.PunchType>();
    private GameObject player;
    private PlayerTargeting playerTargeting;

    private int tries = 0;
    public int enemyHealth = 0;
    public int punchCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerTargeting = player.GetComponent<PlayerTargeting>();

        // 1 hit
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab }); // JAB
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Cross }); // CROSS
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Hook }); // HOOK
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Uppercut }); // UPPERCUT

        // 2 hits
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab, ArduinoConnector.PunchType.Jab }); // JAB, JAB
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab, ArduinoConnector.PunchType.Cross }); // JAB, CROSS
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Cross, ArduinoConnector.PunchType.Hook }); // CROSS, HOOK
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Hook, ArduinoConnector.PunchType.Hook }); // HOOK, HOOK
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Hook, ArduinoConnector.PunchType.Uppercut }); // HOOK, UPPERCUT

        // 3 hits
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Cross, ArduinoConnector.PunchType.Hook, ArduinoConnector.PunchType.Uppercut }); // CROSS, HOOK, UPPERCUT
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab, ArduinoConnector.PunchType.Jab, ArduinoConnector.PunchType.Cross }); // JAB, JAB, CROSS
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Hook, ArduinoConnector.PunchType.Uppercut, ArduinoConnector.PunchType.Hook }); // HOOK, UPPERCUT, HOOK

        // 4 hits
        combosList.Add(new List<ArduinoConnector.PunchType> { ArduinoConnector.PunchType.Jab, ArduinoConnector.PunchType.Cross, ArduinoConnector.PunchType.Hook, ArduinoConnector.PunchType.Uppercut }); // JAB, CROSS, HOOK, UPPERCUT

        // get random punch combination
        randomCombo = combosList[Random.Range(0, combosList.Count)];
        // have health equal the number of required punches
        enemyHealth = randomCombo.Count;
        // update punch text UI
        UpdatePunchText();
    }

    private void Update()
    {
        if (!this.gameObject || !image || !punchText) return;
        // make UI active on active enemy
        if (this.gameObject == playerTargeting.ActiveEnemy)
        {
            image.SetActive(true);
            punchText.gameObject.SetActive(true);
        }
        else
        {
            image.SetActive(false);
            punchText.gameObject.SetActive(false);
        }
    }

    public bool CheckPunch(ArduinoConnector.PunchType punchType)
    {
        // if punch matches current needed punch OR player maxes out on tries (max 2)
        if (randomCombo[punchCount] == punchType || tries == 1)
        {
            tries = 0;
            // add to index
            punchCount += 1;
            // deal damage to enemy
            enemyHealth -= 1;
            // update punch text UI to show completed punches in red
            UpdatePunchText();
            return true;
        }
        else
        {
            tries += 1;
            return false;
        }
    }

    private void UpdatePunchText()
    {
        punchText.text = "";
        for (int i = 0; i < randomCombo.Count; i++)
        {
            string punchName = randomCombo[i].ToString().ToUpper();
            // highlight completed punches in red
            if (i < punchCount)
            {
                punchText.text += $"<color=red>{punchName}</color>";
            }
            else
            {
                // remaining punches remain white
                punchText.text += punchName;
            }
            // add a new line in between punches
            if (i < randomCombo.Count - 1)
            {
                punchText.text += "\n";
            }
        }
    }
}
