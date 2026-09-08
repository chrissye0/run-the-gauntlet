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

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

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
    }

    // returns the nearest enemy in punching range
    GameObject ClosestEnemy()
    {
        // finds all existing game objects with tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // for storing closest enemy and shortest distance
        GameObject closestEnemy = null;
        float shortestDistance = 4;
        // loop through all found enemies
        foreach (GameObject enemy in enemies)
        {
            // make them all white by default
            enemy.GetComponent<Renderer>().material.color = Color.white;
            // calculate distance
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            // see if it is the shortest and if it is within range
            if (distance < shortestDistance && distance < 4)
            {
                // if so, set it to the closest enemy
                shortestDistance = distance;
                closestEnemy = enemy;
            }
        }
        // make the closest enemy red
        if (closestEnemy != null)
        {
            closestEnemy.GetComponent<Renderer>().material.color = Color.red;
        }
        return closestEnemy;
    }

    void Update()
    {
        // for updating the color
        // return out if no enemy in range
        if (ClosestEnemy() == null) return;
        ClosestEnemy();
    }

    // attack the nearest in bound enemy
    void Punch()
    {
        if (ClosestEnemy() != null)
        {
            Debug.Log("Punch!!!");
            Destroy(ClosestEnemy());
            gameManager.score += 100;
        }
    }

    // in later iterations, also check for enemy armor and weaknesses in the below methods
    // for instance, for uppercuts, check to see if the enemy does not have armor on its underside

    void OnLeftJab(InputAction.CallbackContext context)
    {
        Debug.Log("Left Jab");
        Punch();
    }

    void OnLeftCross(InputAction.CallbackContext context)
    {
        Debug.Log("Left Cross");
        Punch();
    }

    void OnLeftHook(InputAction.CallbackContext context)
    {
        Debug.Log("Left Hook");
        Punch();
    }

    void OnLeftUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Left Uppercut");
        Punch();
    }

    void OnRightJab(InputAction.CallbackContext context)
    {
        Debug.Log("Right Jab");
        Punch();
    }

    void OnRightCross(InputAction.CallbackContext context)
    {
        Debug.Log("Right Cross");
        Punch();
    }

    void OnRightHook(InputAction.CallbackContext context)
    {
        Debug.Log("Right Hook");
        Punch();
    }

    void OnRightUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Right Uppercut");
        Punch();
    }

    void OnBlock(InputAction.CallbackContext context)
    {
        Debug.Log("Block");
        Punch();
    }
}
