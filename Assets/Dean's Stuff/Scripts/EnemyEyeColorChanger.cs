using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EnemyEyeColorChanger
*
* Author: Dean Pearce
*
* Description: Class to change the eye color of the enemy based on their attack state.
**************************************************************************************/
public class EnemyEyeColorChanger : MonoBehaviour
{
    private EnemyAI m_parentAI;

    [SerializeField]
    private GameObject m_eye1;
    [SerializeField]
    private GameObject m_eye2;
    [SerializeField]
    private GameObject m_trail1;
    [SerializeField]
    private GameObject m_trail2;
    [SerializeField]
    private Color m_neutralColorMin;
    [SerializeField]
    private Color m_neutralColorMax;
    [SerializeField]
    private Color m_preAttackColorMin;
    [SerializeField]
    private Color m_preAttackColorMax;
    [SerializeField]
    private Color m_attackColorMin;
    [SerializeField]
    private Color m_attackColorMax;

    private ParticleSystem m_particleEye1;
    private ParticleSystem m_particleEye2;
    private ParticleSystem m_particleTrail1;
    private ParticleSystem m_particleTrail2;
    private CombatState m_prevAIState;
    private bool m_prevAttackLock = false;

    void Start()
    {
        m_parentAI = GetComponent<EnemyAI>();
        m_particleEye1 = m_eye1.GetComponent<ParticleSystem>();
        m_particleEye2 = m_eye2.GetComponent<ParticleSystem>();
        m_particleTrail1 = m_trail1.GetComponent<ParticleSystem>();
        m_particleTrail2 = m_trail2.GetComponent<ParticleSystem>();

        m_prevAIState = m_parentAI.GetCombatState();
    }

    void Update()
    {
        UpdateEyes();
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: ChangeEyeColor
	* Parameters: Color min, Color max
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Changes the eye colors based on the colors specified in the parameter.
	**************************************************************************************/
    private void ChangeEyeColor(Color min, Color max)
    {
        //ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(min, max);
        ParticleSystem.ColorOverLifetimeModule eyeCol1 = m_particleEye1.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule eyeCol2 = m_particleEye2.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule trailCol1 = m_particleTrail1.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule trailCol2 = m_particleTrail2.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(min, 0.0f), new GradientColorKey(max, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        eyeCol1.color = grad;
        eyeCol2.color = grad;
        trailCol1.color = grad;
        trailCol2.color = grad;
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: UpdateEyes
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Update logic to track when the eyes should change color based on attack state.
	**************************************************************************************/
    private void UpdateEyes()
    {
        CombatState currentState = m_parentAI.GetCombatState();
        bool isAttackLocked = m_parentAI.IsAttackLocked();

        if (m_prevAIState != currentState)
        {
            if (currentState == CombatState.Attacking)
            {
                ChangeEyeColor(m_preAttackColorMin, m_preAttackColorMax);
            }
            else if (m_prevAIState == CombatState.Attacking)
            {
                ChangeEyeColor(m_neutralColorMin, m_neutralColorMax);
            }
        }
        if (currentState == CombatState.Attacking && m_prevAttackLock != isAttackLocked)
        {
            if (isAttackLocked)
            {
                ChangeEyeColor(m_attackColorMin, m_attackColorMax);
            }
            else
            {
                ChangeEyeColor(m_preAttackColorMin, m_preAttackColorMax);
            }
        }

        m_prevAIState = currentState;
        m_prevAttackLock = isAttackLocked;
    }
}
