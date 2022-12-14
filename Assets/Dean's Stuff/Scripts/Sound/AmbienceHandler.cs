using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: AmbienceHandler
*
* Author: Dean Pearce
*
* Description: Class for pausing the ambience sounds when necessary.
**************************************************************************************/
public class AmbienceHandler : MonoBehaviour
{
    private AudioSource m_audioSource;

    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        EventManager.PauseGameEvent += PauseAmbience;
        EventManager.UnpauseGameEvent += ResumeAmbience;
    }

    private void PauseAmbience()
    {
        m_audioSource.Pause();
    }

    private void ResumeAmbience()
    {
        m_audioSource.Play();
    }

    void OnDestroy()
    {
        EventManager.PauseGameEvent -= PauseAmbience;
        EventManager.UnpauseGameEvent -= ResumeAmbience;
    }
}
