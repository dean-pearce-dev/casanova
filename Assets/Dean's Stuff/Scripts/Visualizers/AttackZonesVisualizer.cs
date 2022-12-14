#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**************************************************************************************
* Type: Class
* 
* Name: AttackZonesVisualizer
*
* Author: Dean Pearce
*
* Description: Script for visualizing the available zones for AI attackers in the Editor
*              Logic from https://www.youtube.com/watch?v=rQG9aUWarwE
**************************************************************************************/
[CustomEditor(typeof(AIManager))]
public class AttackZonesVisualizer : Editor
{
    void OnSceneGUI()
    {
        // Setting object refs
        AIManager aiManager = GameObject.FindGameObjectWithTag(Settings.g_controllerTag).GetComponent<AIManager>();
        GameObject player = GameObject.FindGameObjectWithTag(Settings.g_playerTag);

        if (EditorApplication.isPlaying)
        {
            DrawSolidZones(aiManager, player);
        }

        // Drawing the initial zone arcs
        Handles.color = Color.red;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMinDist());
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMaxDist());

        // Finding the section half angle to use as an offset
        float sectionHalf = (360.0f / aiManager.GetAttackZonesNum()) * 0.5f;

        // For loop to figure out how many zones/zone lines need to be drawn
        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMinDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
        }

        // Changing color to blue for passive zones
        Handles.color = Color.blue;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetPassiveAttackerMaxDist());

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMaxDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: DrawSolidZones
	* Parameters: AIManager aiManager, GameObject player
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Colours in the AttackZones to help visualize their status.
	**************************************************************************************/
    private void DrawSolidZones( AIManager aiManager, GameObject player )
    {
        Color transRed = new Color(255, 0, 0, 0.1f);
        Color transGreen = new Color(0, 255, 0, 0.1f);
        Color transBlue = new Color(0, 0, 255, 0.5f);

        Handles.color = transGreen;

        float angle = aiManager.GetAttackZoneManager().GetAnglePerSection();

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            AttackZone passiveZone = aiManager.GetAttackZoneManager().GetAttackZoneByNum(i, ZoneType.Passive);
            AttackZone activeZone = aiManager.GetAttackZoneManager().GetAttackZoneByNum(i, ZoneType.Active);

            if (passiveZone.IsObstructed())
            {
                Handles.color = transRed;
            }
            else if (passiveZone.IsOccupied())
            {
                Handles.color = transBlue;
            }
            else
            {
                Handles.color = transGreen;
            }

            Handles.DrawSolidArc(player.transform.position, Vector3.up, DirFromAngle(passiveZone.GetAngleStart(), true, player), angle, passiveZone.GetEndDist());

            if (activeZone.IsObstructed())
            {
                Handles.color = transRed;
            }
            else if (activeZone.IsOccupied())
            {
                Handles.color = transBlue;
            }
            else
            {
                Handles.color = transGreen;
            }

            Handles.DrawSolidArc(player.transform.position, Vector3.up, DirFromAngle(activeZone.GetAngleStart(), true, player), angle, activeZone.GetEndDist());

        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DirFromAngle
    * Parameters: float angleInDegrees, bool angleIsGlobal, GameObject gameObject
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
}

#endif