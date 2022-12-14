using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: GuardHandTrails
*
* Author: Dean Pearce
*
* Description: Class specifically for the Guard to enable/disable the hand trail effects
*              during attacks.
**************************************************************************************/
public class GuardHandTrails : MonoBehaviour
{
    EnemyAI m_parentAI;
    [SerializeField]
    private GameObject m_lHand;
    [SerializeField]
    private GameObject m_rHand;
    private List<ParticleSystem> m_handParticleList = new List<ParticleSystem>();
    private CombatState m_prevAIState;

    void Start()
    {
        m_parentAI = GetComponent<EnemyAI>();

        foreach (Transform child in m_lHand.transform)
        {
            m_handParticleList.Add(child.gameObject.GetComponent<ParticleSystem>());
        }
        foreach (Transform child in m_rHand.transform)
        {
            m_handParticleList.Add(child.gameObject.GetComponent<ParticleSystem>());
        }

        m_prevAIState = m_parentAI.GetCombatState();

        TrailsEnabled(false);
    }

    void Update()
    {
        UpdateTrails();
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: UpdateTrails
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Update function to determine when to enable/disable hand trails.
	**************************************************************************************/
    private void UpdateTrails()
    {
        CombatState currentState = m_parentAI.GetCombatState();

        if (m_prevAIState != currentState)
        {
            if (currentState == CombatState.Attacking)
            {
                TrailsEnabled(true);
            }
            else if (m_prevAIState == CombatState.Attacking)
            {
                TrailsEnabled(false);
            }
        }

        m_prevAIState = currentState;
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: TrailsEnabled
	* Parameters: bool shouldEnable
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Function to enable/disable hand trails based on the bool parameter.
	**************************************************************************************/
    private void TrailsEnabled( bool shouldEnable)
    {
        foreach (ParticleSystem handTrail in m_handParticleList)
        {
            if (shouldEnable)
            {
                handTrail.Play();
            }
            else
            {
                handTrail.Clear();
                handTrail.Pause();
            }
        }
    }
}
