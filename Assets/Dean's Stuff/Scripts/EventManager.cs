using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**************************************************************************************
* Type: Class
* 
* Name: EventManager
*
* Author: Dean Pearce
*
* Description: EventManager class for triggering events.
**************************************************************************************/
public class EventManager : MonoBehaviour
{
    // Setting up events
    public static event Action<int> WakeEnemiesEvent;
    public static event Action<int> SpawnEnemiesEvent;
    public static event Action<int> AlertEnemiesEvent;
    public static event Action WaveSetupEvent;
    public static event Action SpawnWaveEvent;
    public static event Action CutsceneBeginEvent;
    public static event Action<bool> CutsceneEndEvent;
    public static event Action GameBeginEvent;
    public static event Action PauseGameEvent;
    public static event Action UnpauseGameEvent;

    // Event functions
    public static void StartWakeEnemiesEvent(int triggerGroup)
    {
        WakeEnemiesEvent?.Invoke(triggerGroup);
    }

    public static void StartSpawnEnemiesEvent(int triggerGroup)
    {
        SpawnEnemiesEvent?.Invoke(triggerGroup);
    }

    public static void StartAlertEnemiesEvent( int triggerGroup )
    {
        AlertEnemiesEvent?.Invoke(triggerGroup);
    }

    public static void StartWaveSetupEvent()
    {
        WaveSetupEvent?.Invoke();
    }

    public static void StartSpawnWaveEvent()
    {
        SpawnWaveEvent?.Invoke();
    }

    public static void StartCutsceneBeginEvent()
    {
        CutsceneBeginEvent?.Invoke();
    }

    public static void StartCutsceneEndEvent(bool enterCombat)
    {
        CutsceneEndEvent?.Invoke(enterCombat);
    }

    public static void StartGameBeginEvent()
    {
        GameBeginEvent?.Invoke();
    }

    public static void StartPauseGameEvent()
    {
        PauseGameEvent?.Invoke();
    }

    public static void StartUnpauseGameEvent()
    {
        UnpauseGameEvent?.Invoke();
    }
}
