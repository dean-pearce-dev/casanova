using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: ZoneHandler
*
* Author: Dean Pearce
*
* Description: Class for handling zone information on behalf of the EnemyAI.
**************************************************************************************/
public class ZoneHandler
{
    private EnemyAI m_parentAI;
    private AttackZoneManager m_attackZoneManager;
    private AttackZone m_currentAttackZone;
    private AttackZone m_occupiedAttackZone;
    private AttackZone m_reservedZone;
    private bool m_reserveZone = false;
    private Vector3 m_reservedPos;
    private float m_targetAngle;
    private int m_closestZoneNum = 0;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Init
    * Parameters: ref EnemyAI enemyAI, ref AttackZoneManager attackZoneManager
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for initialising the required values for the ZoneHandler.
    **************************************************************************************/
    public void Init(ref EnemyAI enemyAI, ref AttackZoneManager attackZoneManager)
    {
        m_parentAI = enemyAI;
        m_attackZoneManager = attackZoneManager;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Custom update function for the EnemyAI to call since ZoneHandler 
    *              doesn't use MonoBehaviour.
    **************************************************************************************/
    public void Update()
    {
        m_currentAttackZone = m_attackZoneManager.FindAttackZone(m_parentAI);

        OccupiedZoneCheck();

        if (m_reserveZone)
        {
            UpdateReservedPos();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UpdateReservedPos
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to keep the reserved position of a zone updated since
    *              the zones move with the player.
    **************************************************************************************/
    private void UpdateReservedPos()
    {
        m_reservedPos = m_attackZoneManager.GetSpecifiedPos(m_targetAngle, m_parentAI.GetStrafeDist());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OccupiedZoneCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Check to make sure this object isn't occupying a zone when it shouldn't.
    **************************************************************************************/
    private void OccupiedZoneCheck()
    {
        if (!m_reserveZone && m_occupiedAttackZone != null && !IsInOccupiedZone())
        {
            ClearOccupiedZone();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ClearOccupiedZone
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clear the zone currently being occupied.
    **************************************************************************************/
    public void ClearOccupiedZone()
    {
        if (m_occupiedAttackZone != null)
        {
            m_occupiedAttackZone.EmptyZone();
            m_occupiedAttackZone = null;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ClearReservedZone
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clear the zone currently being reserved.
    **************************************************************************************/
    public void ClearReservedZone()
    {
        if (m_reservedZone != null)
        {
            m_reservedZone.EmptyZone();
            m_reservedZone = null;
        }

        m_reserveZone = false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OccupyCurrentZone
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to occupy the zone which the EnemyAI is currently inside.
    **************************************************************************************/
    public void OccupyCurrentZone()
    {
        if (m_currentAttackZone != null)
        {
            m_occupiedAttackZone = m_currentAttackZone;
            m_occupiedAttackZone.SetOccupant(m_parentAI);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TakeOverOccupiedZone
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to remove an AI from the zone the parent AI is in, and then occupy it.
    **************************************************************************************/
    public void TakeOverOccupiedZone()
    {
        EnemyAI currentOccupant = m_currentAttackZone.GetOccupant();

        currentOccupant.GetZoneHandler().ClearOccupiedZone();
        currentOccupant.SetCombatState(CombatState.StrafingToZone);
        OccupyCurrentZone();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindClosestZone
    * Parameters: n/a
    * Return: AttackZone
    *
    * Author: Dean Pearce
    *
    * Description: Finds and returns the closest zone to the EnemyAI.
    **************************************************************************************/
    private AttackZone FindClosestZone()
    {
        m_closestZoneNum = m_attackZoneManager.GetZoneNumByAngle(m_parentAI);

        if (m_parentAI.GetAttackingType() == AttackingType.Passive)
        {
            return m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Passive);
        }
        else
        {
            return m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Active);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindClosestAvailableZone
    * Parameters: n/a
    * Return: AttackZone
    *
    * Author: Dean Pearce
    *
    * Description: Finds and returns the closest available zone to the EnemyAI.
    **************************************************************************************/
    private AttackZone FindClosestAvailableZone()
    {
        m_closestZoneNum = m_attackZoneManager.GetZoneNumByAngle(m_parentAI);

        int dirToCheckFirst = Random.Range(0, 2);
        int zoneNumOffset = 1;

        AttackZone zoneToReturn = null;

        // Passive Attacker
        if (m_parentAI.GetAttackingType() == AttackingType.Passive)
        {
            // Closest Zone
            zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Passive);

            // While loop to loop through zones to find the next available zone
            while (!zoneToReturn.IsAvailable())
            {
                int leftNum = (m_closestZoneNum + zoneNumOffset) % m_attackZoneManager.GetTotalZonesNum();
                int rightNum = m_closestZoneNum - zoneNumOffset;

                if (rightNum < 0)
                {
                    rightNum = m_attackZoneManager.GetTotalZonesNum() - (zoneNumOffset % m_attackZoneManager.GetTotalZonesNum());
                }

                // Randomising which direction to check so AI don't bias a direction
                if (dirToCheckFirst == 0)
                {
                    zoneToReturn =  m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Passive);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Passive);
                    }
                }
                else
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Passive);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Passive);

                    }
                }

                zoneNumOffset++;
            }
        }

        // Active Attacker
        else
        {
            zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(m_closestZoneNum, ZoneType.Active);

            while (!zoneToReturn.IsAvailable())
            {
                int leftNum = (m_closestZoneNum + zoneNumOffset) % m_attackZoneManager.GetTotalZonesNum();
                int rightNum = m_closestZoneNum - zoneNumOffset;

                if (rightNum < 0)
                {
                    rightNum = m_attackZoneManager.GetTotalZonesNum() - (zoneNumOffset % m_attackZoneManager.GetTotalZonesNum());
                }

                if (dirToCheckFirst == 0)
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Active);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Active);
                    }
                }
                else
                {
                    zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(rightNum, ZoneType.Active);

                    if (!zoneToReturn.IsAvailable())
                    {
                        zoneToReturn = m_attackZoneManager.GetAttackZoneByNum(leftNum, ZoneType.Active);
                    }
                }

                zoneNumOffset++;
            }
        }

        return zoneToReturn;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReserveZone
    * Parameters: AttackZone zoneToReserve
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Reserves the specified zone for the AI.
    **************************************************************************************/
    public void ReserveZone(AttackZone zoneToReserve)
    {
        m_reserveZone = true;
        m_reservedZone = zoneToReserve;
        m_occupiedAttackZone = m_reservedZone;
        m_targetAngle = Random.Range(m_reservedZone.GetAngleStart(), m_reservedZone.GetAngleEnd());
        m_reservedZone.SetOccupant(m_parentAI);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReserveClosestZone
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Uses ReserveZone to reserve the closest available zone for the AI.
    **************************************************************************************/
    public void ReserveClosestZone()
    {
        ReserveZone(FindClosestAvailableZone());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsInOccupiedZone
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks whether the occupied zone is the one the AI is currently in. 
    **************************************************************************************/
    public bool IsInOccupiedZone()
    {
        if (IsInValidZone())
        {
            return m_occupiedAttackZone == m_currentAttackZone;
        }
        else
        {
            return false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsZoneAvailablel
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks if the zone the AI is currently in is available. 
    **************************************************************************************/
    public bool IsZoneAvailable()
    {
        if (IsInValidZone())
        {
            return m_currentAttackZone.IsAvailable();
        }
        else
        {
            return false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsInValidZone
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks if the enemy is in a valid AttackZone 
    **************************************************************************************/
    public bool IsInValidZone()
    {
        return m_currentAttackZone != null;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsInMatchingZone
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks if the enemy is in the correct zone type. 
    **************************************************************************************/
    public bool IsInMatchingZone()
    {
        if (IsInValidZone())
        {
            return m_currentAttackZone.GetZoneType() == m_parentAI.GetZoneTypeFromAttackType();
        }
        else
        {
            return false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AreZonesAvailable
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks from the AttackZoneManager if any zones are available. 
    **************************************************************************************/
    public bool AreZonesAvailable()
    {
        return m_attackZoneManager.AreZonesAvailable(m_parentAI.GetZoneTypeFromAttackType());
    }

    // Start of getters & setters

    public ref AttackZone GetCurrentAttackZone()
    {
        return ref m_currentAttackZone;
    }

    public void SetCurrentAttackZone( ref AttackZone zoneToSet )
    {
        m_currentAttackZone = zoneToSet;
    }

    public ref AttackZone GetOccupiedAttackZone()
    {
        return ref m_occupiedAttackZone;
    }

    public void SetOccupiedAttackZone( ref AttackZone zoneToSet )
    {
        m_occupiedAttackZone = zoneToSet;
    }

    public Vector3 GetReservedPos()
    {
        return m_reservedPos;
    }

    public void SetReserveFlag( bool shouldReserve )
    {
        m_reserveZone = shouldReserve;
    }

    public bool GetReserveFlag()
    {
        return m_reserveZone;
    }
}
