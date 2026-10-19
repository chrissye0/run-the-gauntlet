using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCombat : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction leftJabAction;
    InputAction leftCrossAction;
    InputAction leftHookAction;
    InputAction leftUppercutAction;
    InputAction rightJabAction;
    InputAction rightCrossAction;
    InputAction rightHookAction;
    InputAction rightUppercutAction;
    InputAction blockAction;
    public GameManager gameManager;
    private PlayerTargeting targeting;
    // to see if actively blocking
    public bool blocking;
    [SerializeField] float specialRange = 5f;

    [SerializeField] Animator gauntletAnimator;

    // for hardware connection
    private ArduinoConnector arduinoConnector;

    void Start()
    {
        // initialize targeting
        targeting = GetComponent<PlayerTargeting>();

        // initialize inputs and set actions
        playerInput = GetComponent<PlayerInput>();

        // initialize blocking to false
        blocking = false;

        // get hardware output
        arduinoConnector = GetComponent<ArduinoConnector>();
        arduinoConnector.PunchDetected += Punch;
        arduinoConnector.BlockDetected += Block;
        arduinoConnector.UnblockDetected += Unblock;

        leftJabAction = playerInput.actions.FindAction("Left Jab");
        leftCrossAction = playerInput.actions.FindAction("Left Cross");
        leftHookAction = playerInput.actions.FindAction("Left Hook");
        leftUppercutAction = playerInput.actions.FindAction("Left Uppercut");

        rightJabAction = playerInput.actions.FindAction("Right Jab");
        rightCrossAction = playerInput.actions.FindAction("Right Cross");
        rightHookAction = playerInput.actions.FindAction("Right Hook");
        rightUppercutAction = playerInput.actions.FindAction("Right Uppercut");

        blockAction = playerInput.actions.FindAction("Block");

        leftJabAction.performed += OnLeftJab;
        leftCrossAction.performed += OnLeftCross;
        leftHookAction.performed += OnLeftHook;
        leftUppercutAction.performed += OnLeftUppercut;

        rightJabAction.performed += OnRightJab;
        rightCrossAction.performed += OnRightCross;
        rightHookAction.performed += OnRightHook;
        rightUppercutAction.performed += OnRightUppercut;

        blockAction.performed += OnBlock;
        blockAction.canceled += OnBlockReleased;
    }

    // attack the active enemy in attacking range
    void Punch(ArduinoConnector.Hand hand, ArduinoConnector.PunchType punchType, string animation)
    {
        // set target to the active enemy
        GameObject activeEnemy = targeting.ActiveEnemy;
        // return out if nothing found
        if (activeEnemy == null) return;
        Enemy enemyInfo = activeEnemy.GetComponent<Enemy>();
        Animator enemyAnimator = activeEnemy.transform.Find("Y Bot").GetComponent<Animator>();
        // trigger animation
        gauntletAnimator.SetTrigger(animation);

        // if punch matches
        if (enemyInfo.CheckPunch(punchType))
        {
            // increase special meter by a little
            gameManager.SetSpecialMeterValue(5f);
            // add to score
            gameManager.score += 50;
            // if defeated
            if (enemyInfo.enemyHealth == 0)
            {
                // reset punch count (used for punch indexing)
                enemyInfo.punchCount = 0;
                // increase special meter more
                gameManager.SetSpecialMeterValue(15f);
                // add more to score
                gameManager.score += 100;
                // destroy target enemy (after death animation plays)
                StartCoroutine(DestroyAfterDeath(activeEnemy));
            }
        }
    }

    // for invoking from ArduinoConnector
    void Block()
    {
        blocking = true;
        gauntletAnimator.SetBool("isBlocking", true);
    }

    void Unblock()
    {
        blocking = false;
        gauntletAnimator.SetBool("isBlocking", false);
        if (gameManager.specialMeterValue == 100) Special();
    }

    // Destroy all enemies within a certain range
    void Special()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> inRangeEnemies = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            // get distance between player and enemy
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < specialRange)
            {
                inRangeEnemies.Add(enemy);
            }
        }
        foreach (GameObject enemy in inRangeEnemies)
        {
            StartCoroutine(DestroyAfterDeath(enemy));
            gameManager.score += 100;
        }
        gameManager.SetSpecialMeterValue(-100f);
        Debug.Log("SPECIAL ATTACK!");
    }

    private IEnumerator DestroyAfterDeath(GameObject enemy)
    {
        // get animator from model
        Animator enemyAnimator = enemy.transform.Find("Y Bot").GetComponent<Animator>();
        if (enemyAnimator == null)
        {
            Destroy(enemy);
            yield break;
        }
        // trigger death animation
        enemyAnimator.SetTrigger("die");
        // wait until death animation begins
        while (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dying Backwards"))
        {
            yield return null;
        }
        // wait until it finishes
        while (enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dying Backwards") && enemyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        // destroy enemy game object
        Destroy(enemy);
    }

    void OnLeftJab(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Jab, "JabL");
        Debug.Log("left jab");
    }

    void OnLeftCross(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Cross, "CrossL");
        Debug.Log("left cross");
    }

    void OnLeftHook(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Hook, "HookL");
        Debug.Log("left hook");
    }

    void OnLeftUppercut(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Uppercut, "UppercutL");
        Debug.Log("left uppercut");
    }

    void OnRightJab(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Jab, "JabR");
        Debug.Log("right jab");
    }

    void OnRightCross(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Cross, "CrossR");
        Debug.Log("right cross");
    }

    void OnRightHook(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Hook, "HookR");
        Debug.Log("right hook");
    }

    void OnRightUppercut(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Uppercut, "UppercutR");
        Debug.Log("right uppercut");
    }

    void OnBlock(InputAction.CallbackContext context)
    {
        Block();
    }

    void OnBlockReleased(InputAction.CallbackContext context)
    {
        Unblock();
    }
}
