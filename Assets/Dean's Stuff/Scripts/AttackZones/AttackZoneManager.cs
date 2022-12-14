using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: AttackZoneManager
*
* Author: Dean Pearce
*
* Description: Class for managing the attack zone objects which hold information about the attack zones
*              around the player.
**************************************************************************************/
public class AttackZoneManager
{
    private AIManager m_aiManager;
    private GameObject m_player;
    private List<AttackZone> m_activeAttackZones = new List<AttackZone>();
    private List<AttackZone> m_passiveAttackZones = new List<AttackZone>();
    private int m_attackZonesNum;

    private float m_sectionHalfAngle;
    private float m_anglePerSection;

    private int m_currentZoneNumToCheck = 0;

    public AttackZoneManager(AIManager aiManager)
    {
        m_aiManager = aiManager;
        m_player = GameObject.FindGameObjectWithTag(Settings.g_playerTag);

        // Could possibly refactor this to use it's own variable, but for now it's set by the AIManager because it can be set in inspector
        m_attackZonesNum = m_aiManager.GetAttackZonesNum();

        SetupAttackZones();
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
    * Description: Custom update to be used by the AIManager since this class doesn't
    *              make use of MonoBehaviour.
    **************************************************************************************/
    public void Update()
    {
        ObsCheckUpdate();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ObsCheckUpdate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Update function to sequentially check for obstructions on attack zones.
    **************************************************************************************/
    private void ObsCheckUpdate()
    {
        m_activeAttackZones[m_currentZoneNumToCheck].CheckForObstruction();
        m_passiveAttackZones[m_currentZoneNumToCheck].CheckForObstruction();

        m_currentZoneNumToCheck++;
        if (m_currentZoneNumToCheck >= m_aiManager.GetAttackZonesNum())
        {
            m_currentZoneNumToCheck = 0;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupAttackZones
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Setup the attack zone objects and give them the necessary values.
    **************************************************************************************/
    private void SetupAttackZones()
    {
        // Setting up attack zone objects, and giving them their initial data
        m_anglePerSection = 360.0f / m_attackZonesNum;
        m_sectionHalfAngle = m_anglePerSection * 0.5f;

        for (int i = 0; i < m_attackZonesNum; i++)
        {
            m_activeAttackZones.Add(new AttackZone(false, ZoneType.Active, i));
            m_activeAttackZones[i].SetBounds(m_anglePerSection * i - m_sectionHalfAngle, m_anglePerSection * (i + 1) - m_sectionHalfAngle, m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist(), m_anglePerSection);
            m_passiveAttackZones.Add(new AttackZone(false, ZoneType.Passive, i));
            m_passiveAttackZones[i].SetBounds(m_anglePerSection * i - m_sectionHalfAngle, m_anglePerSection * (i + 1) - m_sectionHalfAngle, m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist(), m_anglePerSection);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindAttackZone
    * Parameters: EnemyAI enemyToCheck
    * Return: AttackZone
    *
    * Author: Dean Pearce
    *
    * Description: Function for finding and returning the AttackZone a specified enemy is in.
    **************************************************************************************/
    public AttackZone FindAttackZone( EnemyAI enemyToCheck )
    {
        AttackZone returnZone = null;

        // Getting relevant positions
        Vector3 enemyPos = enemyToCheck.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        // Zeroing y co-ords to prevent affecting angle output
        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        // Getting dir between enemy and player
        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Adding half angle to account for the offset
        angle += m_sectionHalfAngle;

        // Wrapping angle back to 360
        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        // If within the zone bounds
        if (DistanceSqrCheck(enemyToCheck.gameObject, m_player, m_aiManager.GetPassiveAttackerMaxDist()))
        {
            // Within active zone bounds
            if (DistanceSqrCheck(enemyToCheck.gameObject, m_player, m_aiManager.GetActiveAttackerMaxDist()))
            {
                returnZone = m_activeAttackZones[(int)(angle / m_anglePerSection)];
            }
            // Within passive zone bounds
            else
            {
                returnZone = m_passiveAttackZones[(int)(angle / m_anglePerSection)];
            }
        }

        return returnZone;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetSpecifiedPos
    * Parameters: float angle, float dist
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Return a position from a given angle and distance.
    **************************************************************************************/
    public Vector3 GetSpecifiedPos( float angle, float dist )
    {
        Vector3 dirToPos = DirFromAngle(angle - m_sectionHalfAngle, true, m_player);

        return m_player.transform.position + (dirToPos * dist);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetZoneNumByAngle
    * Parameters: EnemyAI enemy
    * Return: int
    *
    * Author: Dean Pearce
    *
    * Description: Return the number of zone a specified enemy is in.
    **************************************************************************************/
    public int GetZoneNumByAngle( EnemyAI enemy )
    {
        // Getting positions
        Vector3 enemyPos = enemy.gameObject.transform.position;
        Vector3 playerPos = m_player.transform.position;

        // Zero y to prevent affecting angles
        enemyPos.y = 0.0f;
        playerPos.y = 0.0f;

        // Getting dir from player to enemy
        Vector3 dirFromPlayer = (enemyPos - playerPos).normalized;
        float angle = Vector3.SignedAngle(dirFromPlayer, Vector3.forward, Vector3.down);

        // Add half angle to account for offset
        angle += m_sectionHalfAngle;

        // If angle less than 0, wrap back around
        if (angle < 0.0f)
        {
            angle = 360.0f - angle * -1.0f;
        }

        return (int)(angle / m_anglePerSection);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DistanceSqrCheck
    * Parameters: GameObject targetToCheck, float distanceToCheck
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function for checking if an object is within a specified distance.
    *              More optimal than using Vector3.Distance
    **************************************************************************************/
    private bool DistanceSqrCheck( GameObject firstTarget, GameObject secondTarget, float distanceToCheck )
    {
        bool isInRange = false;

        // Getting the distance between this and the target
        Vector3 distance = firstTarget.transform.position - secondTarget.transform.position;

        // Checking if sqrMagnitude is less than the distance squared
        if (distance.sqrMagnitude <= distanceToCheck * distanceToCheck)
        {
            isInRange = true;
        }

        return isInRange;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AreZonesAvailable
    * Parameters: ZoneType typeToCheck
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Checks if any zones of the specified zone type are available.
    **************************************************************************************/
    public bool AreZonesAvailable(ZoneType typeToCheck)
    {
        if( typeToCheck == ZoneType.Active)
        {
            foreach (AttackZone zone in m_activeAttackZones)
            {
                if (zone.IsAvailable())
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (AttackZone zone in m_passiveAttackZones)
            {
                if (zone.IsAvailable())
                {
                    return true;
                }
            }
        }

        return false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ClearZones
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clears all zones of any EnemyAI's that might be occupying them.
    **************************************************************************************/
    public void ClearZones()
    {
        foreach(AttackZone attackZone in m_activeAttackZones)
        {
            attackZone.EmptyZone();
        }
        foreach (AttackZone attackZone in m_passiveAttackZones)
        {
            attackZone.EmptyZone();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DirFromAngle
    * Parameters: float angleInDegrees, bool angleIsGlobal, GameObject dirFromObject
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Function to allow getting the direction from
    *              a specified object's position.
    **************************************************************************************/
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject gameObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += gameObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Start of getters & setters

    public AttackZone GetAttackZoneByNum( int num, ZoneType zoneType )
    {
        if (zoneType == ZoneType.Passive)
        {
            return m_passiveAttackZones[num];
        }
        else
        {
            return m_activeAttackZones[num];
        }
    }

    public List<AttackZone> GetPassiveAttackZones()
    {
        return m_passiveAttackZones;
    }

    public List<AttackZone> GetActiveAttackZones()
    {
        return m_activeAttackZones;
    }

    public int GetTotalZonesNum()
    {
        return m_attackZonesNum;
    }

    public float GetAnglePerSection()
    {
        return m_anglePerSection;
    }
}
