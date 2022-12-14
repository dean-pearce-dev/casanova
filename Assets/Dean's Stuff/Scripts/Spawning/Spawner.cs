using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: Spawner
*
* Author: Dean Pearce
*
* Description: Class to be used by the SpawnManager for spawning an enemy on a specific point.
**************************************************************************************/
public class Spawner : MonoBehaviour
{
    [SerializeField]
    private EnemyType m_typeToSpawn;
    [SerializeField]
    private AIState m_stateToSpawn;
    [SerializeField]
    private int m_spawnGroup;
    [SerializeField]
    private GameObject m_patrolRoute;
    [SerializeField]
    private bool m_isWaveEnemy = false;

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
    public void Spawn(GameObject enemyToSpawn)
    {
        EnemyAI enemyScript = enemyToSpawn.GetComponent<EnemyAI>();

        // Setting enemy position and rotation to match spawner
        enemyToSpawn.transform.position = transform.position;
        enemyToSpawn.transform.rotation = transform.rotation;

        // Reset the necessary values, then set the state and group num
        enemyScript.ResetToSpawn();

        if (m_patrolRoute != null)
        {
            enemyScript.SetPatrolRoute(m_patrolRoute);
        }

        // Enable enemy
        enemyToSpawn.SetActive(true);
        enemyScript.SetAIState(m_stateToSpawn);
        enemyScript.SetSpawnGroup(m_spawnGroup);
        enemyScript.SetWaveEnemy(m_isWaveEnemy);

    }

    // Start of getters & setters

    public EnemyType GetSpawnType()
    {
        return m_typeToSpawn;
    }

    public int GetSpawnGroup()
    {
        return m_spawnGroup;
    }
}
