using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZoneType
{
    None,
    Passive,
    Active
}

/**************************************************************************************
* Type: Class
* 
* Name: AttackZone
*
* Author: Dean Pearce
*
* Description: AttackZone class which holds information for the attack zone.
**************************************************************************************/
public class AttackZone
{
    private bool m_isOccupied = false;
    private bool m_isObstructed = false;
    private ZoneType m_zoneType = ZoneType.Active;
    private EnemyAI m_occupant;
    private GameObject m_player;
    private int m_zoneNum;
    private float m_navMeshCheckDist = 0.3f;
    private float m_zoneAngleSize;
    private float m_zoneAngleStart;
    private float m_zoneAngleEnd;
    private float m_zoneDistStart;
    private float m_zoneDistEnd;
    private float m_obstructionDistBuffer = 0.7f;
    private float m_obstructionAngBuffer = 2.0f;

    private Vector3[] m_obstPointArray = new Vector3[5];

    public AttackZone( bool isOccupied, ZoneType zoneType, int zoneNum )
    {
        m_isOccupied = isOccupied;
        m_zoneType = zoneType;
        m_zoneNum = zoneNum;
        m_player = GameObject.FindGameObjectWithTag("Player");
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetBounds
    * Parameters: float angleStart, float angleEnd, float startDist, float endDist, float angleSize
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for setting the bounds of this attack zone.
    **************************************************************************************/
    public void SetBounds( float angleStart, float angleEnd, float startDist, float endDist, float angleSize )
    {
        m_zoneAngleStart = angleStart;
        m_zoneAngleEnd = angleEnd;
        m_zoneDistStart = startDist;
        m_zoneDistEnd = endDist;
        m_zoneAngleSize = angleSize;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckForObstruction
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Checks if this AttackZone is obstructed by the environment.
    **************************************************************************************/
    public void CheckForObstruction()
    {
        m_isObstructed = false;

        // Setting the points that should be checking the nav mesh
        // Setting them to the corners of the zone - the buffer values
        m_obstPointArray[0] = m_player.transform.position + DirFromAngle(m_zoneAngleStart + m_obstructionAngBuffer, true, m_player) * (m_zoneDistStart + m_obstructionDistBuffer);
        m_obstPointArray[1] = m_player.transform.position + DirFromAngle(m_zoneAngleStart + m_obstructionAngBuffer, true, m_player) * (m_zoneDistEnd - m_obstructionDistBuffer);
        m_obstPointArray[2] = m_player.transform.position + DirFromAngle(m_zoneAngleEnd - m_obstructionAngBuffer, true, m_player) * (m_zoneDistStart + m_obstructionDistBuffer);
        m_obstPointArray[3] = m_player.transform.position + DirFromAngle(m_zoneAngleEnd - m_obstructionAngBuffer, true, m_player) * (m_zoneDistEnd - m_obstructionDistBuffer);

        // Center point
        m_obstPointArray[4] = m_player.transform.position + DirFromAngle(m_zoneAngleEnd - (m_zoneAngleSize * 0.5f), true, m_player) * (m_zoneDistStart + ((m_zoneDistEnd - m_zoneDistStart) * 0.5f));

        for (int i = 0; i < m_obstPointArray.Length; i++)
        {
            // SamplePosition to check whether the position is currently on the navmesh
            if (!NavMesh.SamplePosition(m_obstPointArray[i], out NavMeshHit hit, m_navMeshCheckDist, NavMesh.AllAreas))
            {
                // If point is not valid on navmesh
                m_isObstructed = true;
                return;
            }
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
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject dirFromObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += dirFromObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Start of getters & setters

    public bool IsOccupied()
    {
        return m_isOccupied;
    }

    public bool IsObstructed()
    {
        return m_isObstructed;
    }

    public bool IsAvailable()
    {
        return !m_isOccupied && !m_isObstructed;
    }

    public void SetOccupied(bool isOccupied)
    {
        m_isOccupied = isOccupied;
    }

    public int GetZoneNum()
    {
        return m_zoneNum;
    }

    public void SetZoneNum(int zoneNum)
    {
        m_zoneNum = zoneNum;
    }

    public ZoneType GetZoneType()
    {
        return m_zoneType;
    }

    public void SetZoneType(ZoneType typeToSet)
    {
        m_zoneType = typeToSet;
    }

    public EnemyAI GetOccupant()
    {
        return m_occupant;
    }

    public void SetOccupant( EnemyAI occupantToSet )
    {
        m_occupant = occupantToSet;
        m_isOccupied = true;
    }

    public void EmptyZone()
    {
        m_occupant = null;
        m_isOccupied = false;
    }

    public float GetAngleStart()
    {
        return m_zoneAngleStart;
    }

    public float GetAngleEnd()
    {
        return m_zoneAngleEnd;
    }

    public float GetStartDist()
    {
        return m_zoneDistStart;
    }

    public float GetEndDist()
    {
        return m_zoneDistEnd;
    }
}
