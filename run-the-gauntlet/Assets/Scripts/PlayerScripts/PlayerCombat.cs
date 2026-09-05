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

    void OnLeftJab(InputAction.CallbackContext context)
    {
        Debug.Log("Left Jab");
    }

    void OnLeftCross(InputAction.CallbackContext context)
    {
        Debug.Log("Left Cross");
    }

    void OnLeftHook(InputAction.CallbackContext context)
    {
        Debug.Log("Left Hook");
    }

    void OnLeftUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Left Uppercut");
    }

    void OnRightJab(InputAction.CallbackContext context)
    {
        Debug.Log("Right Jab");
    }

    void OnRightCross(InputAction.CallbackContext context)
    {
        Debug.Log("Right Cross");
    }

    void OnRightHook(InputAction.CallbackContext context)
    {
        Debug.Log("Right Hook");
    }

    void OnRightUppercut(InputAction.CallbackContext context)
    {
        Debug.Log("Right Uppercut");
    }

    void OnBlock(InputAction.CallbackContext context)
    {
        Debug.Log("Block");
    }
}
