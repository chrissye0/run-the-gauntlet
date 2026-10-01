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
    void Punch(ArduinoConnector.Hand hand, ArduinoConnector.PunchType punchType)
    {
        // set target to the active enemy
        GameObject activeEnemy = targeting.ActiveEnemy;
        // return out if nothing found
        if (activeEnemy == null) return;
        // increase special meter by a little
        gameManager.SetSpecialMeterValue(5f);
        // destroy target enemy
        Destroy(activeEnemy);
        // add to score
        gameManager.score += 100;
    }

    // in later iterations, also check for enemy armor and weaknesses in the below methods
    // for instance, for uppercuts, check to see if the enemy does not have armor on its underside

    void OnLeftJab(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Jab);
    }

    void OnLeftCross(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Cross);
    }

    void OnLeftHook(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Hook);
    }

    void OnLeftUppercut(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Left, ArduinoConnector.PunchType.Uppercut);
    }

    void OnRightJab(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Jab);
    }

    void OnRightCross(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Cross);
    }

    void OnRightHook(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Hook);
    }

    void OnRightUppercut(InputAction.CallbackContext context)
    {
        Punch(ArduinoConnector.Hand.Right, ArduinoConnector.PunchType.Uppercut);
    }

    void OnBlock(InputAction.CallbackContext context)
    {
        blocking = true;
    }

    void OnBlockReleased(InputAction.CallbackContext context)
    {
        blocking = false;
    }
}
