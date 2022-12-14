#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**************************************************************************************
* Type: Class
* 
* Name: ConeDetectionVisualizer
*
* Author: Dean Pearce
*
* Description: Script for visualizing the AI's FOV in the Editor
*              Logic from https://www.youtube.com/watch?v=rQG9aUWarwE
**************************************************************************************/
[CustomEditor(typeof(EnemyAI))]
public class ConeDetectionVisualizer : Editor
{
    void OnSceneGUI()
    {
        // Getting object refs
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        EnemyAI targetEnemy = (EnemyAI)target;

        DrawPlayerDetectionCone(player, targetEnemy);
        DrawAIDetectionCone(targetEnemy);
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: DrawPlayerDetectionCone
	* Parameters: GameObject player, EnemyAI targetEnemy
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Draws lines to help visualize where the EnemyAI can see.
	**************************************************************************************/
    private void DrawPlayerDetectionCone( GameObject player, EnemyAI targetEnemy )
    {
        // Setting line color, drawing the initial arc, then getting the angles for the fov lines with DirFromAngle
        Handles.color = Color.white;
        Handles.DrawWireArc(targetEnemy.transform.position, Vector3.up, Vector3.forward, 360.0f, targetEnemy.GetViewRadius());
        Vector3 viewAngleA = targetEnemy.DirFromAngle(-targetEnemy.GetViewAngle() * 0.5f, false);
        Vector3 viewAngleB = targetEnemy.DirFromAngle(targetEnemy.GetViewAngle() * 0.5f, false);

        // Drawing the fov lines
        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleA * targetEnemy.GetViewRadius());
        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleB * targetEnemy.GetViewRadius());

        // When the player is detected, draw a line to the player to display that no obstacles are blocking vision
        Handles.color = Color.red;
        if (EditorApplication.isPlaying && targetEnemy.IsPlayerVisible())
        {
            Handles.DrawLine(targetEnemy.transform.position, player.transform.position);
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: DrawAIDetectionCone
	* Parameters: EnemyAI targetEnemy
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Draws lines to help visualize where the EnemyAI is checking for other AI as obstructions.
	**************************************************************************************/
    private void DrawAIDetectionCone( EnemyAI targetEnemy )
    {
        // Setting line color, drawing the initial arc, then getting the angles for the fov lines with DirFromAngle
        Handles.color = Color.white;
        Handles.DrawWireArc(targetEnemy.transform.position, Vector3.up, Vector3.forward, 360.0f, targetEnemy.GetAICheckDist());

        // Getting the direction and position to draw from, setting the y at half the AI's height
        Vector3 dir = targetEnemy.transform.forward;

        // Checking what direction the enemy is strafing to determine the direction to draw the lines
        if (targetEnemy.GetStrafeDir() == StrafeDir.Left)
        {
            dir = -targetEnemy.transform.right;
        }
        else
        {
            dir = targetEnemy.transform.right;
        }

        Vector3 viewAngleA = (dir - targetEnemy.DirFromAngle(-targetEnemy.GetAIAngleCheck() * 0.5f, false)).normalized;
        Vector3 viewAngleB = (dir + targetEnemy.DirFromAngle(targetEnemy.GetAIAngleCheck() * 0.5f, false)).normalized;

        // Drawing the fov lines
        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleA * targetEnemy.GetAICheckDist());
        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleB * targetEnemy.GetAICheckDist());
    }
}

#endif