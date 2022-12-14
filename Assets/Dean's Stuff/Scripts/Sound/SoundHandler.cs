using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: SoundHandler
*
* Author: Dean Pearce
*
* Description: Base class for objects which make use of sound.
**************************************************************************************/
public class SoundHandler : MonoBehaviour
{
    protected AudioSource m_audioSource;
    protected virtual void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }
}
