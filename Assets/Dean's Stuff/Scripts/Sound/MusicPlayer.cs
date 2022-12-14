using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/**************************************************************************************
* Type: Class
* 
* Name: MusicPlayer
*
* Author: Dean Pearce
*
* Description: Class for handling the music of the game.
**************************************************************************************/
public class MusicPlayer : MonoBehaviour
{
    private AIManager m_aiManager;
    [SerializeField]
    private AudioClip m_menuMusic;
    [SerializeField]
    private AudioClip m_bgMusic;
    [SerializeField]
    private AudioClip m_combatMusic;
    [SerializeField]
    private AudioClip m_cutsceneMusic;
    private AudioSource m_audioSource;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_menuMusicVolume = 1.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_bgMusicVolume = 1.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_combatMusicVolume = 1.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_cutsceneMusicVolume = 1.0f;
    private AudioMixer m_audioMixer;
    private string m_musicVolString = "MusicFaderVol";
    [SerializeField]
    private float m_fadeTime = 1.5f;
    private float m_minMixerVol = -80.0f;
    private bool m_fadeFinished = false;


    private bool m_prevInCombat = false;

    void Start()
    {
        m_aiManager = GameObject.FindGameObjectWithTag(Settings.g_controllerTag).GetComponent<AIManager>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioMixer = m_audioSource.outputAudioMixerGroup.audioMixer;

        m_audioSource.clip = m_menuMusic;
        m_audioSource.volume = m_menuMusicVolume;
        m_audioSource.Play();

        EventManager.CutsceneBeginEvent += StartCutsceneMusic;
        EventManager.CutsceneEndEvent += EndCutsceneMusic;
        EventManager.GameBeginEvent += StartGameMusic;
        EventManager.PauseGameEvent += OnPauseMenu;
        EventManager.UnpauseGameEvent += OnUnpause;
    }

    void Update()
    {
        MusicUpdate();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MusicUpdate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for tracking and swapping music tracks based on what's happening in game.
    **************************************************************************************/
    private void MusicUpdate()
    {
        bool currentlyInCombat = m_aiManager.IsCombatActive();

        if (m_prevInCombat != currentlyInCombat)
        {
            if (currentlyInCombat == true)
            {
                // Entered combat
                StartCoroutine(SwitchTrack(m_combatMusic, m_combatMusicVolume, false));
            }
            else
            {
                // Exited combat
                StartCoroutine(SwitchTrack(m_bgMusic, m_bgMusicVolume, true));

            }
        }

        m_prevInCombat = currentlyInCombat;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StartCutsceneMusic
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Starts the cutscene music. Called by EventManager.
    **************************************************************************************/
    private void StartCutsceneMusic()
    {
        StartCoroutine(SwitchTrack(m_cutsceneMusic, m_cutsceneMusicVolume, false));
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EndCutsceneMusic
    * Parameters: bool enterCombat
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Ends the cutscene music and starts playing game music. Called by EventManager.
    **************************************************************************************/
    private void EndCutsceneMusic(bool enterCombat)
    {
        if (enterCombat)
        {
            StartCoroutine(SwitchTrack(m_combatMusic, m_combatMusicVolume, true));
        }
        else
        {
            StartCoroutine(SwitchTrack(m_bgMusic, m_bgMusicVolume, true));
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StartGameMusic
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Starts the game music. Called by EventManager.
    **************************************************************************************/
    private void StartGameMusic()
    {
        StartCoroutine(SwitchTrack(m_bgMusic, m_bgMusicVolume, true));
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnPauseMenu
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Pauses the music when the pause menu is opened. Called by EventManager.
    **************************************************************************************/
    private void OnPauseMenu()
    {
        m_audioSource.Pause();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnUnpause
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Resumes the music when the pause menu is closed. Called by EventManager.
    **************************************************************************************/
    private void OnUnpause()
    {
        m_audioSource.Play();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SwitchTrack
    * Parameters: AudioClip trackToSwitchTo, float vol, bool shouldFade
    * Return: IEnumerator
    *
    * Author: Dean Pearce
    *
    * Description: Function for switching music tracks
    **************************************************************************************/
    private IEnumerator SwitchTrack(AudioClip trackToSwitchTo, float vol, bool shouldFade)
    {
        if (m_audioSource.clip != trackToSwitchTo)
        {
            if (shouldFade)
            {
                // Start the fade coroutine, then wait until it's done
                StartCoroutine(StartFade(m_audioMixer, m_musicVolString, m_fadeTime, m_minMixerVol));
                yield return new WaitUntil(() => m_fadeFinished);
            }

            // Switching the tracks
            m_audioSource.clip = trackToSwitchTo;
            m_audioSource.volume = vol;
            m_audioSource.Play();

            // Reset the mixer volume
            m_audioMixer.SetFloat(m_musicVolString, 0.0f);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StartFade
    * Parameters: AudioMixer audioMixer, string exposedParam, float duration, float targetVolume
    * Return: IEnumerator
    *
    * Author: Dean Pearce
    *
    * Description: Function for fading audio to make music switching seem more natural.
    *              Logic from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
    **************************************************************************************/
    private IEnumerator StartFade( AudioMixer audioMixer, string exposedParam, float duration, float targetVolume )
    {
        m_fadeFinished = false;

        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }

        m_fadeFinished = true;

        yield break;
    }

    private void OnDestroy()
    {
        EventManager.CutsceneBeginEvent -= StartCutsceneMusic;
        EventManager.CutsceneEndEvent -= EndCutsceneMusic;
        EventManager.GameBeginEvent -= StartGameMusic;
        EventManager.PauseGameEvent -= OnPauseMenu;
        EventManager.UnpauseGameEvent -= OnUnpause;
    }
}
