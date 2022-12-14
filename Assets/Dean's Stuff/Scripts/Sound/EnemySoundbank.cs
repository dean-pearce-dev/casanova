using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EnemySoundbank
*
* Author: Dean Pearce
*
* Description: Class for holding audio files related to the enemies.
**************************************************************************************/
public class EnemySoundbank : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] m_footstepSFX;
    [SerializeField]
    private AudioClip[] m_dodgeSFX;
    [SerializeField]
    private AudioClip[] m_damageSFX;
    [SerializeField]
    private AudioClip[] m_attackGruntSFX;
    [SerializeField]
    private AudioClip[] m_normalAttackSFX;
    [SerializeField]
    private AudioClip[] m_normalCollisionSFX;
    [SerializeField]
    private AudioClip[] m_heavyAttackSFX;
    [SerializeField]
    private AudioClip[] m_heavyCollisionSFX;
    [SerializeField]
    private AudioClip[] m_quickAttackSFX;
    [SerializeField]
    private AudioClip[] m_quickCollisionSFX;
    [SerializeField]
    private AudioClip[] m_deathSFX;
    [SerializeField]
    private AudioClip[] m_deathFizzSFX;
    [SerializeField]
    private AudioClip m_wakeSFX;
    [SerializeField]
    private AudioClip[] m_tauntSFX;

    public ref AudioClip GetFootstepSFX()
    {
        return ref RandomiseSFX(ref m_footstepSFX);
    }

    public ref AudioClip GetFootstepSFX(int index)
    {
        return ref m_footstepSFX[index];
    }

    public ref AudioClip GetDodgeSFX()
    {
        return ref RandomiseSFX(ref m_dodgeSFX);
    }

    public ref AudioClip GetDodgeSFX(int index)
    {
        return ref m_dodgeSFX[index];
    }

    public ref AudioClip GetDamageSFX()
    {
        return ref RandomiseSFX(ref m_damageSFX);
    }

    public ref AudioClip GetDamageSFX(int index)
    {
        return ref m_damageSFX[index];
    }

    public ref AudioClip GetAttackGruntSFX()
    {
        return ref RandomiseSFX(ref m_attackGruntSFX);
    }

    public ref AudioClip GetAttackGruntSFX(int index)
    {
        return ref m_attackGruntSFX[index];
    }

    public ref AudioClip GetNormalAttackSFX()
    {
        return ref RandomiseSFX(ref m_normalAttackSFX);
    }

    public ref AudioClip GetNormalCollisionSFX()
    {
        return ref RandomiseSFX(ref m_normalCollisionSFX);
    }

    public ref AudioClip GetNormalCollisionSFX(int index)
    {
        return ref m_normalCollisionSFX[index];
    }

    public ref AudioClip GetHeavyAttackSFX()
    {
        return ref RandomiseSFX(ref m_heavyAttackSFX);
    }

    public ref AudioClip GetHeavyCollisionSFX()
    {
        return ref RandomiseSFX(ref m_heavyCollisionSFX);
    }

    public ref AudioClip GetHeavyCollisionSFX(int index)
    {
        return ref m_heavyCollisionSFX[index];
    }

    public ref AudioClip GetQuickAttackSFX()
    {
        return ref RandomiseSFX(ref m_quickAttackSFX);
    }

    public ref AudioClip GetQuickCollisionSFX()
    {
        return ref RandomiseSFX(ref m_quickCollisionSFX);
    }

    public ref AudioClip GetQuickCollisionSFX(int index)
    {
        return ref m_quickCollisionSFX[index];
    }

    public ref AudioClip GetDeathSFX()
    {
        return ref RandomiseSFX(ref m_deathSFX);
    }

    public ref AudioClip GetDeathSFX(int index)
    {
        return ref m_deathSFX[index];
    }

    public ref AudioClip GetDeathFizzSFX()
    {
        return ref RandomiseSFX(ref m_deathFizzSFX);
    }

    public ref AudioClip GetWakeSFX()
    {
        return ref m_wakeSFX;
    }

    public ref AudioClip GetTauntSFX()
    {
        return ref RandomiseSFX(ref m_tauntSFX);
    }

    public ref AudioClip GetTauntSFX(int index)
    {
        return ref m_tauntSFX[index];
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RandomiseSFX
    * Parameters: ref AudioClip[] clipArray
    * Return: ref AudioClip
    *
    * Author: Dean Pearce
    *
    * Description: Returns a AudioClip randomised from a specified array. Used to make sounds
    *              more varied and immersive.
    **************************************************************************************/
    private ref AudioClip RandomiseSFX(ref AudioClip[] clipArray)
    {
        int indexToGet = 0;

        if (clipArray.Length > 1)
        {
            indexToGet = Random.Range(0, clipArray.Length);
        }

        return ref clipArray[indexToGet];
    }
}
