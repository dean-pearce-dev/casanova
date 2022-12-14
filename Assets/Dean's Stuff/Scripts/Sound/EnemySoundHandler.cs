using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EnemySoundHandler
*
* Author: Dean Pearce
*
* Description: Derives from CharacterSoundHandler. Uses a soundbank to get references to AudioClips.
**************************************************************************************/
public class EnemySoundHandler : CharacterSoundHandler
{
    private EnemySoundbank m_soundbank;

    protected override void Awake()
    {
        base.Awake();

        // Determining which soundbank to use
        if (gameObject.GetComponent<EnemyAI>().GetEnemyType() == EnemyType.Grunt)
        {
            m_soundbank = GameObject.FindGameObjectWithTag("GruntSoundbank").GetComponent<EnemySoundbank>();
        }
        else
        {
            m_soundbank = GameObject.FindGameObjectWithTag("GuardSoundbank").GetComponent<EnemySoundbank>();
        }
    }

    public override void PlayFootstepSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetFootstepSFX());
    }

    public override void PlayDodgeSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDodgeSFX());
    }

    public override void PlayDamageSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDamageSFX());
    }

    public override void PlayAttackGruntSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetAttackGruntSFX());
    }

    public override void PlayNormalAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetNormalAttackSFX());
    }

    public override void PlayNormalCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetNormalCollisionSFX());
    }

    public override void PlayHeavyAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetHeavyAttackSFX());
    }

    public override void PlayHeavyCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetHeavyCollisionSFX());
    }

    public void PlayQuickAttackSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetQuickAttackSFX());
    }

    public void PlayQuickCollisionSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetQuickCollisionSFX());
    }

    public override void PlayDeathSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDeathSFX());
    }

    public virtual void PlayDeathFizzSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetDeathFizzSFX());
    }

    public void PlayWakeSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetWakeSFX());
    }

    public void PlayTauntSFX()
    {
        m_audioSource.PlayOneShot(m_soundbank.GetTauntSFX());
    }
}
