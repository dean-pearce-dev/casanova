using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundHandler : SoundHandler
{
    [SerializeField]
    private AudioClip m_uiClickSFX;

    public void PlayUIClickSFX()
    {
        m_audioSource.PlayOneShot(m_uiClickSFX);
    }
}
