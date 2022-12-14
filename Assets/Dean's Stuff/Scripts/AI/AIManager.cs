using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: AIManager
*
* Author: Dean Pearce
*
* Description: Manages the AI enemy objects in the game.
**************************************************************************************/

public class AIManager : MonoBehaviour
{
    private GameObject m_player;

    private List<EnemyAI> m_enemyList = new List<EnemyAI>();
    private List<EnemyAI> m_activeAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_passiveAttackers = new List<EnemyAI>();
    private List<EnemyAI> m_unassignedAttackers = new List<EnemyAI>();
    private AttackZoneManager m_attackZoneManager;
    private SpawnManager m_spawnManager;

    private bool m_canAttack = true;
    [SerializeField]
    private bool m_useSpawnManager = true;

    [SerializeField]
    [Range(0, 30)]
    private int m_attackZonesNum = 10;
    [SerializeField]
    private float m_activeAttackerMinDist = 3.0f;
    [SerializeField]
    private float m_activeAttackerMaxDist = 5.0f;
    [SerializeField]
    private float m_passiveAttackerMaxDist = 10.0f;

    [SerializeField]
    private float m_zoneDistanceBuffer = 2.0f;

    [SerializeField]
    private int m_maxActiveAttackers = 3;
    [SerializeField]
    private int m_maxSimultaneousAttacks = 2;

    [SerializeField]
    private GameObject m_obsCheckDebug;

    private void Awake()
    {
        m_attackZoneManager = new AttackZoneManager(this);
    }

    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_spawnManager = GetComponent<SpawnManager>();

        EventManager.WakeEnemiesEvent += WakeGroup;
        EventManager.AlertEnemiesEvent += AlertGroup;

        if (!m_useSpawnManager)
        {
            RegisterEnemies();
        }
    }

    void Update()
    {
        m_attackZoneManager.Update();
        ActiveAttackerCount();

        // Check to make sure too many enemies can't attack at once
        m_canAttack = TotalEnemiesAttacking() < m_maxSimultaneousAttacks;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RegisterEnemies
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Register any enemies in the scene into the AIManager.
    **************************************************************************************/
    public void RegisterEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();

            if (enemyScript != null)
            {
                // Adding the enemy into the list
                if (!m_enemyList.Contains(enemyScript))
                {
                    m_enemyList.Add(enemyScript);
                }

                // Giving the enemy a reference to the managers
                enemyScript.SetAIManagerRef(this);
                enemyScript.SetupZoneHandler(ref m_attackZoneManager);
            }
            else
            {
                // Notifying user that an enemy has failed to register with the manager
                Debug.Log("AIManager: Failed to add EnemyAI script of Enemy: " + enemy.name);
            }

        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RegisterEnemy
    * Parameters: GameObject enemy
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Register a specific enemy into the AIManager.
    **************************************************************************************/
    public void RegisterEnemy(GameObject enemy)
    {
        EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();

        if (enemyScript != null)
        {
            // Adding the enemy into the list
            m_enemyList.Add(enemyScript);

            // Giving the enemy a reference to the managers
            enemyScript.SetAIManagerRef(this);
            enemyScript.SetupZoneHandler(ref m_attackZoneManager);
        }
        else
        {
            // Notifying user that an enemy has failed to register with the manager
            Debug.Log("AIManager: Failed to add EnemyAI script of Enemy: " + enemy.name);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RegisterAttacker
    * Parameters: EnemyAI enemyToRegister
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function which the EnemyAI uses upon entering combat. Allows the AIManager
    *              to track and manage enemies in combat.
    **************************************************************************************/
    public void RegisterAttacker(EnemyAI enemyToRegister)
    {
        if (!m_unassignedAttackers.Contains(enemyToRegister))
        {
            m_unassignedAttackers.Add(enemyToRegister);
            enemyToRegister.SetAttackingType(AttackingType.Unassigned);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UnregisterAttacker
    * Parameters: EnemyAI enemyToUnregister
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to unregister the enemy from the attacker lists so they can be
    *              properly removed from combat.
    **************************************************************************************/
    public void UnregisterAttacker(EnemyAI enemyToUnregister)
    {
        if (m_activeAttackers.Contains(enemyToUnregister))
        {
            m_activeAttackers.Remove(enemyToUnregister);
        }
        if (m_passiveAttackers.Contains(enemyToUnregister))
        {
            m_passiveAttackers.Remove(enemyToUnregister);
        }
        if (m_unassignedAttackers.Contains(enemyToUnregister))
        {
            m_unassignedAttackers.Remove(enemyToUnregister);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MakeActiveAttacker
    * Parameters: EnemyAI enemy
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to switch the specified enemy to an active attacker.
    **************************************************************************************/
    public void MakeActiveAttacker(EnemyAI enemy)
    {
        if (m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Remove(enemy);
        }
        if (m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Remove(enemy);
        }
        if (!m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Add(enemy);
        }

        enemy.SetAttackingType(AttackingType.Active);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MakePassiveAttacker
    * Parameters: EnemyAI enemy
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to switch the specified enemy to an passive attacker.
    **************************************************************************************/
    public void MakePassiveAttacker(EnemyAI enemy)
    {
        if (m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Remove(enemy);
        }
        if (m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Remove(enemy);
        }
        if (!m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Add(enemy);
        }
        
        enemy.SetAttackingType(AttackingType.Passive);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MakeUnassignedAttacker
    * Parameters: EnemyAI enemy
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to switch the specified enemy to an unassigned attacker.
    **************************************************************************************/
    public void MakeUnasssignedAttacker( EnemyAI enemy )
    {
        if (m_activeAttackers.Contains(enemy))
        {
            m_activeAttackers.Remove(enemy);
        }
        if (m_passiveAttackers.Contains(enemy))
        {
            m_passiveAttackers.Remove(enemy);
        }
        if (!m_unassignedAttackers.Contains(enemy))
        {
            m_unassignedAttackers.Add(enemy);
        }
        
        enemy.SetAttackingType(AttackingType.Unassigned);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DeactivateActiveEnemies
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Deactivates all active enemies in the scene. Useful for respawn logic.
    **************************************************************************************/
    public void DeactivateActiveEnemies()
    {
        foreach (EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf)
            {
                // Unregister from attackers, add to the available enemy pool,
                // then deactivate and reset the health manager
                UnregisterAttacker(enemy);
                //m_spawnManager.AddToAvailable( enemy );
                enemy.gameObject.GetComponent<EnemyDamageManager>().ResetEnemy();
            }
        }

        // Clear any of the attack zones that might not have been cleared
        m_attackZoneManager.ClearZones();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ActiveAttackerCount
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for making sure active attackers don't go over the limit, and assigning passive
    *              attackers to active when possible
    **************************************************************************************/
    private void ActiveAttackerCount()
    {
        if (m_activeAttackers.Count > m_maxActiveAttackers)
        {
            MakePassiveAttacker(FindFurthestActiveAttacker());
        }
        else if (m_activeAttackers.Count < m_maxActiveAttackers && m_passiveAttackers.Count > 0)
        {
            MakeActiveAttacker(FindClosestPassiveAttacker());
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: WakeGroup
    * Parameters: int groupToWake
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to wake up a specified group of enemies. Used via the EventManager
    *              for the purpose of progression.
    **************************************************************************************/
    private void WakeGroup(int groupToWake)
    {
        foreach(EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf && enemy.GetSpawnGroup() == groupToWake)
            {
                enemy.WakeUpAI();
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AlertGroup
    * Parameters: int groupToWake
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to alert a specified group of enemies. Used via the EventManager
    *              for the purpose of progression.
    **************************************************************************************/
    private void AlertGroup(int groupToAlert)
    {
        foreach (EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf && enemy.GetSpawnGroup() == groupToAlert)
            {
                if (enemy.GetState() == AIState.Sleeping)
                {
                    enemy.SetCombatOnWake(true);
                    enemy.WakeUpAI();
                }
                else
                {
                    enemy.SetAIState(AIState.InCombat);
                }
                
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindFurthestActiveAttacker
    * Parameters: n/a
    * Return: EnemyAI
    *
    * Author: Dean Pearce
    *
    * Description: Function to find the furthest active attacker from the player.
    *              Useful for figuring out which enemy should be turned passive.
    **************************************************************************************/
    private EnemyAI FindFurthestActiveAttacker()
    {
        // Setting the first index as the default
        EnemyAI furthestEnemy = m_activeAttackers[0];

        for (int i = 0; i < m_activeAttackers.Count; i++)
        {
            // Looping through the list to compare distances
            if (GetSqrDistance(m_activeAttackers[i].gameObject, m_player) > GetSqrDistance(furthestEnemy.gameObject, m_player))
            {
                furthestEnemy = m_activeAttackers[i];
            }
        }

        return furthestEnemy;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindClosestPassiveAttacker
    * Parameters: n/a
    * Return: EnemyAI
    *
    * Author: Dean Pearce
    *
    * Description: Function to find the closest passive attacker from the player.
    *              Useful for figuring out which enemy should be turned active.
    **************************************************************************************/
    private EnemyAI FindClosestPassiveAttacker()
    {
        // Setting the first index as the default
        EnemyAI closestEnemy = m_passiveAttackers[0];

        for (int i = 0; i < m_passiveAttackers.Count; i++)
        {
            // Looping through the list to compare distances
            if (GetSqrDistance(m_passiveAttackers[i].gameObject, m_player) < GetSqrDistance(closestEnemy.gameObject, m_player))
            {
                closestEnemy = m_passiveAttackers[i];
            }
        }

        return closestEnemy;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ActiveSlotsOpen
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function to quickly check if the active attackers is at the max limit.
    **************************************************************************************/
    public bool ActiveSlotsOpen()
    {
        return m_activeAttackers.Count < m_maxActiveAttackers;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetSqrDistance
    * Parameters: GameObject firstTarget, GameObject secondTarget
    * Return: float
    *
    * Author: Dean Pearce
    *
    * Description: Function to return the distance between two GameObjects as a float.
    *              Quicker than using Vector3.Distance
    **************************************************************************************/
    private float GetSqrDistance( GameObject firstTarget, GameObject secondTarget )
    {
        return (firstTarget.transform.position - secondTarget.transform.position).sqrMagnitude;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SwapPassiveWithActive
    * Parameters: EnemyAI enemyToSwap
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to swap a specified passive enemy with an active enemy based on the
    *              furthest active attacker.
    **************************************************************************************/
    public void SwapPassiveWithActive( EnemyAI enemyToSwap )
    {
        EnemyAI furthestActive = FindFurthestActiveAttacker();
        MakeActiveAttacker(enemyToSwap);
        MakePassiveAttacker(furthestActive);

        // If attacking, let them finish attack, otherwise start backing up
        if (furthestActive.GetCombatState() != CombatState.Attacking)
        {
            furthestActive.SetCombatState(CombatState.BackingUp);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TotalEnemiesAttacking
    * Parameters: n/a
    * Return: int
    *
    * Author: Dean Pearce
    *
    * Description: Function which returns the amount of enemies currently making an attack.
    *              Useful for limiting max simultaneous attacks.
    **************************************************************************************/
    public int TotalEnemiesAttacking()
    {
        int total = 0;

        // Only check active attackers since they're the only ones which should be attacking
        foreach (EnemyAI enemy in m_activeAttackers)
        {
            if (enemy.GetCombatState() == CombatState.Attacking || enemy.GetCombatState() == CombatState.MovingToAttack)
            {
                total++;
            }
        }

        return total;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RemainingEnemiesInGroup
    * Parameters: int groupNum
    * Return: int
    *
    * Author: Dean Pearce
    *
    * Description: Function for returning the remaining active enemies in the specified group.
    **************************************************************************************/
    public int RemainingEnemiesInGroup(int groupNum)
    {
        int enemiesRemaining = 0;

        for (int i = 0; i < m_enemyList.Count; i++)
        {
            if (m_enemyList[i].GetSpawnGroup() == groupNum && m_enemyList[i].gameObject.activeSelf)
            {
                enemiesRemaining++;
            }
        }

        return enemiesRemaining;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RemainingWaveEnemies
    * Parameters: n/a
    * Return: int
    *
    * Author: Dean Pearce
    *
    * Description: Function for returning the remaining active wave enemies.
    *              Useful for the final arena where wave logic is used.
    **************************************************************************************/
    public int RemainingWaveEnemies()
    {
        int enemiesRemaining = 0;

        for (int i = 0; i < m_enemyList.Count; i++)
        {
            if (m_enemyList[i].IsWaveEnemy() && m_enemyList[i].gameObject.activeSelf)
            {
                enemiesRemaining++;
            }
        }

        return enemiesRemaining;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsCombatActive
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function for checking if any active enemies are currently in combat.
    **************************************************************************************/
    public bool IsCombatActive()
    {
        foreach (EnemyAI enemy in m_enemyList)
        {
            if (enemy.gameObject.activeSelf && enemy.GetState() == AIState.InCombat)
            {
                return true;
            }
        }

        return false;
    }

    // Start of getters & setters

    public AttackZoneManager GetAttackZoneManager()
    {
        return m_attackZoneManager;
    }

    public int GetAttackZonesNum()
    {
        return m_attackZonesNum;
    }

    public float GetActiveAttackerMinDist()
    {
        return m_activeAttackerMinDist;
    }

    public float GetActiveAttackerMaxDist()
    {
        return m_activeAttackerMaxDist;
    }

    public float GetPassiveAttackerMaxDist()
    {
        return m_passiveAttackerMaxDist;
    }

    public float GetZoneDistanceBuffer()
    {
        return m_zoneDistanceBuffer;
    }

    public bool CanAttack()
    {
        return m_canAttack;
    }

    public void SetCanAttack(bool canAttack)
    {
        m_canAttack = canAttack;
    }

    public GameObject GetObsCheckDebug()
    {
        return m_obsCheckDebug;
    }

    public List<EnemyAI> GetEnemyList()
    {
        return m_enemyList;
    }

    // Unsubscribe from the EventManager on destroy
	private void OnDestroy()
	{
        EventManager.WakeEnemiesEvent -= WakeGroup;
        EventManager.AlertEnemiesEvent -= AlertGroup;
    }
}
