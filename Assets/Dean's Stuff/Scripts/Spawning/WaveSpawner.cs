using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: WaveSpawner
*
* Author: Dean Pearce
*
* Description: Class to be used by the SpawnManager for spawning an enemy on a specific point.
**************************************************************************************/
public class WaveSpawner : MonoBehaviour
{
    /**************************************************************************************
	* Type: Function
	* 
	* Name: Spawn
	* Parameters: GameObject enemyToSpawn
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Spawns the specified enemy on the attached object's position.
	**************************************************************************************/
    public void Spawn( GameObject enemyToSpawn )
    {
        EnemyAI enemyScript = enemyToSpawn.GetComponent<EnemyAI>();

        // Setting enemy position and rotation to match spawner
        enemyToSpawn.transform.position = transform.position;
        enemyToSpawn.transform.rotation = transform.rotation;

        // Reset the necessary values, then set the state
        enemyScript.ResetToSpawn();

        // Enable enemy
        enemyToSpawn.SetActive(true);
        enemyScript.SetAIState(AIState.InCombat);
        enemyScript.SetWaveEnemy(true);
    }
}
