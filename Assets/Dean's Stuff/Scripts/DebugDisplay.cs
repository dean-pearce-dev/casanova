using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum DebugType
{
    AI,
    Zone,
    None
}

/**************************************************************************************
* Type: Class
* 
* Name: DebugDisplay
*
* Author: Dean Pearce
*
* Description: Handles the UI element which is used for displaying debug information on a specific zone or AI.
**************************************************************************************/
public class DebugDisplay : MonoBehaviour
{
    private DebugType m_debugType = DebugType.None;
    private GameObject m_aiDebugHolder;
    private GameObject m_zoneDebugHolder;

    private List<GameObject> m_aiList = new List<GameObject>();
    private EnemyAI m_targetAI;

    private int m_currentAiNum = 0;

    private List<AttackZone> m_passiveAttackZones = new List<AttackZone>();
    private List<AttackZone> m_activeAttackZones = new List<AttackZone>();
    private AttackZone m_targetZone;
    private AttackingType m_targetZoneType = AttackingType.Passive;
    private AttackZoneManager m_attackZoneManager;

    private int m_currentAttackZoneNum = 0;

    private Text m_aiNameText;
    private Text m_aiStateText;
    private Text m_aiSubstateText;
    private Text m_playerDetectedText;
    private Text m_strafeAtDistText;
    private Text m_attackZoneText;
    private Text m_occupiedZoneText;

    private Text m_zoneNameText;
    private Text m_zoneOccupantText;
    private Text m_zoneObstructedText;
    private Text m_angleStartText;
    private Text m_angleEndText;

    [SerializeField]
    private InputActionReference m_f12Pressed;
    [SerializeField]
    private InputActionReference m_lArrow;    
    [SerializeField]
    private InputActionReference m_rArrow;
    [SerializeField]
    private InputActionReference m_downArrow;
    [SerializeField]
    private InputActionReference m_upArrow;

    private void OnEnable()
    {
        m_f12Pressed.action.Enable();
        m_lArrow.action.Enable();
        m_rArrow.action.Enable();
        m_downArrow.action.Enable();
        m_upArrow.action.Enable();
    }
    void Start()
    {
        m_aiDebugHolder = GameObject.Find("AIDebugHolder");
        m_zoneDebugHolder = GameObject.Find("ZoneDebugHolder");
        SetupAIDebugDisplay();
        SetupZoneDebugDisplay();

        SetDebugType(DebugType.None);
    }

    void Update()
    {
        ToggleDebugType();

        switch (m_debugType)
        {
            case DebugType.AI:
            {
                ChangeAITarget();
                AIDebugTextUpdate();

                break;
            }
            case DebugType.Zone:
            {
                ChangeZoneTarget();
                ZoneDebugUpdate();
                break;
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetupAIDebugDisplay
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Sets up all the relevant components for AI debug text.
	**************************************************************************************/
    private void SetupAIDebugDisplay()
    {
        m_aiNameText = GameObject.Find("AINameText").GetComponent<Text>();
        m_aiStateText = GameObject.Find("AIStateText").GetComponent<Text>();
        m_aiSubstateText = GameObject.Find("AISubstateText").GetComponent<Text>();
        m_playerDetectedText = GameObject.Find("PlayerDetectText").GetComponent<Text>();
        m_strafeAtDistText = GameObject.Find("StrafeDistText").GetComponent<Text>();
        m_attackZoneText = GameObject.Find("AttackZoneText").GetComponent<Text>();
        m_occupiedZoneText = GameObject.Find("OccupiedZoneText").GetComponent<Text>();

        m_aiList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        if (m_aiList.Count > 0)
        {
            m_targetAI = m_aiList[m_currentAiNum].GetComponent<EnemyAI>();
        }

        SetAIDebugTarget(m_currentAiNum);
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetupZoneDebugDisplay
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Sets up all the relevant components for Zone debug text.
	**************************************************************************************/
    private void SetupZoneDebugDisplay()
    {
        m_zoneNameText = GameObject.Find("ZoneNameText").GetComponent<Text>();
        m_zoneOccupantText = GameObject.Find("OccupiedByText").GetComponent<Text>();
        m_zoneObstructedText = GameObject.Find("ObstructedText").GetComponent<Text>();
        m_angleStartText = GameObject.Find("AngleStartText").GetComponent<Text>();
        m_angleEndText = GameObject.Find("AngleEndText").GetComponent<Text>();

        m_attackZoneManager = GameObject.Find("AIManager").GetComponent<AIManager>().GetAttackZoneManager();
        m_passiveAttackZones.AddRange(m_attackZoneManager.GetPassiveAttackZones());
        m_activeAttackZones.AddRange(m_attackZoneManager.GetActiveAttackZones());

        SetZoneTarget(m_currentAttackZoneNum);
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: AIDebugTextUpdate
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Updates the AI debug text components.
	**************************************************************************************/
    private void AIDebugTextUpdate()
    {
        AIState currentAIState = m_targetAI.GetState();

        m_aiStateText.text = "AI State: " + currentAIState;
        m_playerDetectedText.text = "Player Detected: " + m_targetAI.IsPlayerVisible();
        m_strafeAtDistText.text = "Strafe Distance: " + m_targetAI.GetStrafeDist();

        if (m_targetAI.GetZoneHandler().GetCurrentAttackZone() != null)
        {
            AttackZone attackZone = m_targetAI.GetZoneHandler().GetCurrentAttackZone();
            m_attackZoneText.text = "Attack Zone: " + attackZone.GetZoneType() + " " + attackZone.GetZoneNum();
        }
        else
        {
            m_attackZoneText.text = "Attack Zone: None";
        }

        if (m_targetAI.GetZoneHandler().GetOccupiedAttackZone() != null)
        {
            AttackZone attackZone = m_targetAI.GetZoneHandler().GetOccupiedAttackZone();
            m_occupiedZoneText.text = "Occupied Zone: " + attackZone.GetZoneType() + " " + attackZone.GetZoneNum();
        }
        else
        {
            m_occupiedZoneText.text = "Occupied Zone: None";
        }

        if (currentAIState == AIState.InCombat || currentAIState == AIState.Patrolling)
        {
            if (currentAIState == AIState.InCombat)
            {
                m_aiSubstateText.text = "Combat State: " + m_targetAI.GetCombatState();
            }
            else if (currentAIState == AIState.Patrolling)
            {
                m_aiSubstateText.text = "Patrol State: " + m_targetAI.GetPatrolState();
            }
        }
        else
        {
            m_aiSubstateText.text = "Substate: None Active";
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: ZoneDebugUpdate
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Updates the Zone debug text components.
	**************************************************************************************/
    private void ZoneDebugUpdate()
    {
        m_zoneNameText.text = "Zone: " + m_targetZoneType + " " + m_targetZone.GetZoneNum();
        m_zoneObstructedText.text = "Is Obstructed: " + m_targetZone.IsObstructed();
        m_angleStartText.text = "Angle Start: " + m_targetZone.GetAngleStart();
        m_angleEndText.text = "Angle End: " + m_targetZone.GetAngleEnd();

        if (m_targetZone.IsOccupied())
        {
            m_zoneOccupantText.text = "Occupied By: " + m_targetZone.GetOccupant().gameObject.name;
        }
        else
        {
            m_zoneOccupantText.text = "No Occupant";
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: ChangeZoneTarget
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Changes which zone's info is displayed based on input.
	**************************************************************************************/
    private void ChangeZoneTarget()
    {
        // Inputs for cycling debug target, could use a refactor
        if (m_lArrow.action.triggered)
        {
            if (m_currentAttackZoneNum != 0)
            {
                m_currentAttackZoneNum--;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
        if (m_rArrow.action.triggered)
        {
            if (m_currentAttackZoneNum != m_passiveAttackZones.Count - 1)
            {
                m_currentAttackZoneNum++;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
        if (m_upArrow.action.triggered || m_downArrow.action.triggered)
        {
            if (m_targetZoneType == AttackingType.Passive)
            {
                m_targetZoneType = AttackingType.Active;
                SetZoneTarget(m_currentAttackZoneNum);
            }
            else
            {
                m_targetZoneType = AttackingType.Passive;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetZoneTarget
	* Parameters: int targetNum
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Sets which zone's info is displayed by the specified number.
	**************************************************************************************/
    private void SetZoneTarget(int targetNum)
    {
        if (m_targetZoneType == AttackingType.Passive)
        {
            m_targetZone = m_passiveAttackZones[targetNum];
        }
        else
        {
            m_targetZone = m_activeAttackZones[targetNum];
        }

        m_zoneNameText.text = "Zone: " + m_targetZoneType + " " + m_targetZone.GetZoneNum();
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: ChangeAITarget
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Changes which AI's info is displayed based on input.
	**************************************************************************************/
    private void ChangeAITarget()
    {
        // Inputs for cycling debug target, could use a refactor
        if (m_lArrow.action.triggered)
        {
            if (m_currentAiNum != 0)
            {
                m_currentAiNum--;
                SetAIDebugTarget(m_currentAiNum);
            }
        }
        if (m_rArrow.action.triggered)
            {
            if (m_currentAiNum != m_aiList.Count - 1)
            {
                m_currentAiNum++;
                SetAIDebugTarget(m_currentAiNum);
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetAIDebugTarget
	* Parameters: int targetNum
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Sets which AI's info is displayed by the specified number.
	**************************************************************************************/
    private void SetAIDebugTarget(int targetNum)
    {
        m_targetAI = m_aiList[targetNum].GetComponent<EnemyAI>();
        m_aiNameText.text = "AI: " + m_aiList[targetNum].name;
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetDebugType
	* Parameters: DebugType typeToSet
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Sets which debug information is displayed.
	**************************************************************************************/
    private void SetDebugType(DebugType typeToSet)
    {
        switch (typeToSet)
        {
            case DebugType.AI:
            {
                m_zoneDebugHolder.SetActive(false);
                m_aiDebugHolder.SetActive(true);
                m_debugType = DebugType.AI;
                break;
            }
            case DebugType.Zone:
            {
                m_aiDebugHolder.SetActive(false);
                m_zoneDebugHolder.SetActive(true);
                m_debugType = DebugType.Zone;
                break;
            }
            case DebugType.None:
            {
                m_aiDebugHolder.SetActive(false);
                m_zoneDebugHolder.SetActive(false);
                m_debugType = DebugType.None;
                break;
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: ToggleDebugType
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Cycles through the debug info types based on input.
	**************************************************************************************/
    private void ToggleDebugType()
    {
        if (m_f12Pressed.action.triggered)
        {
            switch (m_debugType)
            {
                case DebugType.AI:
                {
                    SetDebugType(DebugType.Zone);
                    break;
                }
                case DebugType.Zone:
                {
                    SetDebugType(DebugType.None);
                    break;
                }
                case DebugType.None:
                {
                    SetDebugType(DebugType.AI);
                    break;
                }
            }
        }
    }
}
