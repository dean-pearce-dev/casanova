using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicInput : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_resetInput;
    [SerializeField]
    private InputActionReference m_exitInput;
    [SerializeField]
    private InputActionReference m_pauseInput;

    void Start()
    {
        m_resetInput.action.Enable();
        m_exitInput.action.Enable();
        m_pauseInput.action.Enable();
    }

    void Update()
    {
        InputCheck();
    }

    private void InputCheck()
    {
        if (m_pauseInput.action.triggered)
        {
            CinematicEventManager.StartCameraPauseEvent();
        }
        if (m_resetInput.action.triggered)
        {
            CinematicEventManager.StartCameraResetEvent();
        }
        if (m_exitInput.action.triggered)
        {
            Application.Quit();
        }
    }
}
