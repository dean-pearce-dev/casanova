using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: SpawnManager
*
* Author: Dean Pearce
*
* Description: Class which handles spawning of enemies.
**************************************************************************************/
public class SpawnManager : MonoBehaviour
{
    private AIManager m_aiManager;
    [SerializeField]
    private GameObject m_gruntPrefab;
    [SerializeField]
    private GameObject m_guardPrefab;
    private List<GameObject> m_gruntPool;
    private List<GameObject> m_availableGrunts;
    private List<GameObject> m_guardPool;
    private List<GameObject> m_availableGuards;
    private List<Spawner> m_spawnerList;
    private int m_maxGrunts;
    private int m_maxGuards;
    private GameObject m_initialSpawnPoint;
    [SerializeField]
    private int[] m_gruntWaves;
    [SerializeField]
    private int[] m_guardWaves;
    private int m_currentWave = 0;

    private WaveSpawnerHolder m_waveSpawnerHolder;


    void Start()
    {
        // Getting the AI Manager, and creating the lists
        m_aiManager = gameObject.GetComponent<AIManager>();
        m_spawnerList = new List<Spawner>();
        m_gruntPool = new List<GameObject>();
        m_availableGrunts = new List<GameObject>();
        m_guardPool = new List<GameObject>();
        m_availableGuards = new List<GameObject>();
        m_initialSpawnPoint = GameObject.FindGameObjectWithTag("InitialSpawnPoint");
        m_waveSpawnerHolder = GameObject.FindGameObjectWithTag("WaveSpawnerHolder").GetComponent<WaveSpawnerHolder>();

        // Enemy setup
        SetupSpawnerList();
        CalculateEnemiesNeeded();
        SetupEnemies();

        // Subscribing to events
        EventManager.SpawnEnemiesEvent += SpawnGroup;
        EventManager.WaveSetupEvent += SetupWaveEnemies;
        EventManager.SpawnWaveEvent += SpawnNextWave;

    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: CalculateEnemiesNeeded
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Figures out the most enemies which will be needed at once, and instantiates
    *              them based on that.
    **************************************************************************************/
    private void CalculateEnemiesNeeded()
    {
        int totalGroups = 0;

        // Figuring out how many groups exist
        foreach(Spawner spawner in m_spawnerList)
        {
            if( totalGroups < spawner.GetSpawnGroup())
            {
                totalGroups = spawner.GetSpawnGroup();
            }
        }

        // Looping through each group to find out which one uses the most enemies and how many
        for (int i = 0; i <= totalGroups; i++)
        {
            int totalGruntsNeeded = 0;
            int totalGuardsNeeded = 0;

            foreach(Spawner spawner in m_spawnerList)
            {
                if (spawner.GetSpawnType() == EnemyType.Grunt)
                {
                    if (spawner.GetSpawnGroup() == i || spawner.GetSpawnGroup() == i + 1)
                    {
                        totalGruntsNeeded++;
                    }
                }
                if (spawner.GetSpawnType() == EnemyType.Guard)
                {
                    if (spawner.GetSpawnGroup() == i || spawner.GetSpawnGroup() == i + 1)
                    {
                        totalGuardsNeeded++;
                    }
                }
            }

            // Setting the max enemies num if there is more
            if (totalGruntsNeeded > m_maxGrunts)
            {
                m_maxGrunts = totalGruntsNeeded;
            }
            if (totalGuardsNeeded > m_maxGuards)
            {
                m_maxGuards = totalGuardsNeeded;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupSpawnerList
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Finds all the spawner objects, and adds their scripts to a list.
    **************************************************************************************/
    private void SetupSpawnerList()
    {
        // Adding all the spawner objects to a list
        foreach(GameObject spawnerObj in GameObject.FindGameObjectsWithTag("Spawner"))
        {
            m_spawnerList.Add(spawnerObj.GetComponent<Spawner>());
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupEnemies
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Instantiates the enemies and registers them with the AIManager
    **************************************************************************************/
    private void SetupEnemies()
    {
        // Instantiating enemies based on the max amount needed
        for(int i = 0; i < m_maxGrunts; i++)
        {
            GameObject newEnemy = Instantiate(m_gruntPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_gruntPool.Add(newEnemy);
            m_availableGrunts.Add(newEnemy);
            m_aiManager.RegisterEnemy(newEnemy);
            newEnemy.SetActive(false);
        }
        for (int i = 0; i < m_maxGuards; i++)
        {
            GameObject newEnemy = Instantiate(m_guardPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_guardPool.Add(newEnemy);
            m_availableGuards.Add(newEnemy);
            m_aiManager.RegisterEnemy(newEnemy);
            newEnemy.SetActive(false);
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SetupWaveEnemies
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Instantiates wave enemies based on the max that will be needed at once.
	**************************************************************************************/
    private void SetupWaveEnemies()
    {
        int gruntsNeeded = m_gruntWaves[m_gruntWaves.Length - 1] - m_availableGrunts.Count;
        int guardsNeeded = m_guardWaves[m_guardWaves.Length - 1] - m_availableGuards.Count;

        // Instantiating enemies based on the max amount needed
        for (int i = 0; i < gruntsNeeded; i++)
        {
            GameObject newEnemy = Instantiate(m_gruntPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_gruntPool.Add(newEnemy);
            m_availableGrunts.Add(newEnemy);
            m_aiManager.RegisterEnemy(newEnemy);
            newEnemy.SetActive(false);
        }
        for (int i = 0; i < guardsNeeded; i++)
        {
            GameObject newEnemy = Instantiate(m_guardPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_guardPool.Add(newEnemy);
            m_availableGuards.Add(newEnemy);
            m_aiManager.RegisterEnemy(newEnemy);
            newEnemy.SetActive(false);
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SpawnNextWave
	* Parameters: n/a
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Spawns the next wave of enemies.
	**************************************************************************************/
    private void SpawnNextWave()
    {
        if (m_currentWave > m_gruntWaves.Length)
        {
            Debug.Log("Tried to spawn too many waves.");
            return;
        }

        // Spawning Grunts
        for (int i = 0; i < m_gruntWaves[m_currentWave]; i++)
        {
            GameObject enemyToSpawn = GetAvailableEnemy(EnemyType.Grunt);

            if (enemyToSpawn == null)
            {
                Debug.Log("ATTEMPTED SPAWN: No Available Enemy Found");
            }

            m_waveSpawnerHolder.ReturnSpawnPoint().Spawn(enemyToSpawn);
            RemoveFromAvailable(enemyToSpawn.GetComponent<EnemyAI>());
        }

        // Spawning Guards
        for (int i = 0; i < m_guardWaves[m_currentWave]; i++)
        {
            GameObject enemyToSpawn = GetAvailableEnemy(EnemyType.Guard);

            if (enemyToSpawn == null)
            {
                Debug.Log("ATTEMPTED SPAWN: No Available Enemy Found");
            }

            m_waveSpawnerHolder.ReturnSpawnPoint().Spawn(enemyToSpawn);
            RemoveFromAvailable(enemyToSpawn.GetComponent<EnemyAI>());
        }

        m_currentWave++;
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: SpawnGroup
	* Parameters: int groupNum
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Spawns a group of enemies based on the specified group num.
	**************************************************************************************/
    private void SpawnGroup(int groupNum)
    {
        // Spawn enemies in matching group number
        foreach(Spawner spawner in m_spawnerList)
        {
            if (groupNum == spawner.GetSpawnGroup())
            {
                GameObject enemyToSpawn = GetAvailableEnemy(spawner.GetSpawnType());

                if (enemyToSpawn == null)
                {
                    Debug.Log("ATTEMPTED SPAWN: No Available Enemy Found");
                }

                spawner.Spawn(enemyToSpawn);
                RemoveFromAvailable(enemyToSpawn.GetComponent<EnemyAI>());
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: GetAvailableEnemy
	* Parameters: EnemyType typeToGet
	* Return: GameObject
	*
	* Author: Dean Pearce
	*
	* Description: Returns an enemy from the available enemy pools based on the specified type.
	**************************************************************************************/
    private GameObject GetAvailableEnemy(EnemyType typeToGet)
    {
        GameObject enemyToReturn = null;

        if (typeToGet == EnemyType.Grunt && m_availableGrunts.Count > 0)
        {
            enemyToReturn = m_availableGrunts[0];
        }
        else if (typeToGet == EnemyType.Guard && m_availableGuards.Count > 0)
        {
            enemyToReturn = m_availableGuards[0];
        }

        return enemyToReturn;
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: AddToAvailable
	* Parameters: EnemyAI enemyToAdd
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Adds a specified enemy into the available pool.
	**************************************************************************************/
    public void AddToAvailable( EnemyAI enemyToAdd )
    {
        if (enemyToAdd.GetEnemyType() == EnemyType.Grunt)
        {
            if (!m_availableGrunts.Contains(enemyToAdd.gameObject))
            {
                m_availableGrunts.Add(enemyToAdd.gameObject);
            }
        }
        else if (enemyToAdd.GetEnemyType() == EnemyType.Guard)
        {
            if (!m_availableGuards.Contains(enemyToAdd.gameObject))
            {
                m_availableGuards.Add(enemyToAdd.gameObject);
            }
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: RemoveFromAvailable
	* Parameters: EnemyAI enemyToRemove
	* Return: n/a
	*
	* Author: Dean Pearce
	*
	* Description: Removes the specified enemy from the available enemy pools.
	**************************************************************************************/
    public void RemoveFromAvailable(EnemyAI enemyToRemove)
    {
        if (enemyToRemove.GetEnemyType() == EnemyType.Grunt)
        {
            if (m_availableGrunts.Contains(enemyToRemove.gameObject))
            {
                m_availableGrunts.Remove(enemyToRemove.gameObject);
            }
        }
        else if (enemyToRemove.GetEnemyType() == EnemyType.Guard)
        {
            if (m_availableGuards.Contains(enemyToRemove.gameObject))
            {
                m_availableGuards.Remove(enemyToRemove.gameObject);
            }
        }
    }

    // Start of getters & setters
    public int GetCurrentWave()
    {
        return m_currentWave;
    }

    public int GetTotalWaves()
    {
        return m_gruntWaves.Length;
    }

    // Unsubscribe from events on destroy
	private void OnDestroy()
	{
        EventManager.SpawnEnemiesEvent -= SpawnGroup;
        EventManager.WaveSetupEvent -= SetupWaveEnemies;
        EventManager.SpawnWaveEvent -= SpawnNextWave;
    }
}
