
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private RiveWidget riveWidget;
    private ViewModelInstanceTriggerProperty startProperty;
    private bool isStarting;

    private void OnEnable()
    {
        isStarting = false;
        if (riveWidget == null) return;
        riveWidget.OnWidgetStatusChanged += HandleWidgetStatusChanged;
        HandleWidgetStatusChanged();
    }

    private void OnDisable()
    {
        if (riveWidget != null) riveWidget.OnWidgetStatusChanged -= HandleWidgetStatusChanged;
        startProperty = null;
    }


    private void HandleWidgetStatusChanged()
    {
        if (riveWidget == null || riveWidget.Status != WidgetStatus.Loaded) return;
        // get state machine and view model from rive file
        var stateMachine = riveWidget.StateMachine;
        var viewModel = stateMachine.ViewModelInstance;
        // get start trigger
        startProperty = viewModel.GetTriggerProperty("start");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        // play start animation on spacebar trigger
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartGameAnimation();
        }
    }

    public void StartGameAnimation()
    {
        if (isStarting || startProperty == null) return;
        isStarting = true;
        startProperty.Trigger();
        // for executing game after animation duration
        StartCoroutine(WaitAndLoadScene());
    }

    private IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene("Game");
    }
}
