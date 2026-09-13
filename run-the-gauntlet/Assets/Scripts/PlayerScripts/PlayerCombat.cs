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

    // for hardware connection
    private ArduinoConnector arduinoConnector;

    void Start()
    {
        // initialize targeting
        targeting = GetComponent<PlayerTargeting>();

        // initialize inputs and set actions
        playerInput = GetComponent<PlayerInput>();

        // get hardware output
        arduinoConnector = GetComponent<ArduinoConnector>();
        arduinoConnector.PunchDetected += Punch;

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

    // attack the active enemy in attacking range
    void Punch(ArduinoConnector.Hand hand, ArduinoConnector.PunchType punchType)
    {
        // set target to the active enemy
        GameObject targetEnemy = targeting.ActiveEnemy;
        // return out if nothing found
        if (targetEnemy == null) return;
        // destroy target enemy
        Destroy(targetEnemy);
        // add to score
        gameManager.score += 100;
    }

    // in later iterations, also check for enemy armor and weaknesses in the below methods
    // for instance, for uppercuts, check to see if the enemy does not have armor on its underside

    void OnLeftJab(InputAction.CallbackContext context)
    {
        Debug.Log("Left Jab");
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Jab);
    }

    void OnLeftCross(InputAction.CallbackContext context)
    {
        Debug.Log("Left Cross");
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Cross);
    }

    void OnLeftHook(InputAction.CallbackContext context)
    {
        Debug.Log("Left Hook");
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Hook);
    }

    void OnLeftUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Left Uppercut");
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Uppercut);
    }

    void OnRightJab(InputAction.CallbackContext context)
    {
        Debug.Log("Right Jab");
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Jab);
    }

    void OnRightCross(InputAction.CallbackContext context)
    {
        Debug.Log("Right Cross");
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Cross);
    }

    void OnRightHook(InputAction.CallbackContext context)
    {
        Debug.Log("Right Hook");
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Hook);
    }

    void OnRightUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Right Uppercut");
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Uppercut);
    }

    void OnBlock(InputAction.CallbackContext context)
    {
        Debug.Log("Block");
    }
}
