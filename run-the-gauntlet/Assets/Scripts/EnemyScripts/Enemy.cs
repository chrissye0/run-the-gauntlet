using System.Collections.Generic;
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

        for(int i = 0; i < randomCombo.Count; i++)
        {
            if (randomCombo[i] == ArduinoConnector.PunchType.Jab) punchText.text += "JAB";
            if (randomCombo[i] == ArduinoConnector.PunchType.Cross) punchText.text += "CROSS";
            if (randomCombo[i] == ArduinoConnector.PunchType.Hook) punchText.text += "HOOK";
            if (randomCombo[i] == ArduinoConnector.PunchType.Uppercut) punchText.text += "UPPERCUT";
            // if only one element OR this is the last element, return out
            if (randomCombo.Count == 1 || i == randomCombo.Count) return;
            // add a space
            punchText.text += "\n";
        }
        //foreach (ArduinoConnector.PunchType punch in randomCombo)
        //{
        //    if (punch == ArduinoConnector.PunchType.Jab) punchText.text += "JAB";
        //    if (punch == ArduinoConnector.PunchType.Cross) punchText.text += "CROSS";
        //    if (punch == ArduinoConnector.PunchType.Hook) punchText.text += "HOOK";
        //    if (punch == ArduinoConnector.PunchType.Uppercut) punchText.text += "UPPERCUT";
        //}
    }

    private void Update()
    {
        if (!this.gameObject || !image || !punchText) return; 
        if (this.gameObject == playerTargeting.ActiveEnemy)
        {
            image.SetActive(true);
            punchText.gameObject.SetActive(true);
            Debug.Log(randomCombo.ToString());
            //if (randomCombo[0] == ArduinoConnector.PunchType.Jab) punchText.text = "JAB";
            //if (randomCombo[0] == ArduinoConnector.PunchType.Cross) punchText.text = "CROSS";
            //if (randomCombo[0] == ArduinoConnector.PunchType.Hook) punchText.text = "HOOK";
            //if (randomCombo[0] == ArduinoConnector.PunchType.Uppercut) punchText.text = "UPPERCUT";
        } else
        {
            image.SetActive(false);
            punchText.gameObject.SetActive(false);
        }
    }

    public bool CheckPunch(ArduinoConnector.PunchType punchType)
    {
        Debug.Log(randomCombo[0]);
        // change this later when combos with multiple punches are integrated
        // if punch matches current needed punch OR player maxes out on tries
        if (randomCombo[0] == punchType || tries == 1)
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
