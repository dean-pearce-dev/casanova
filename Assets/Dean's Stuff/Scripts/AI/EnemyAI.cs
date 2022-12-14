using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    Grunt,
    Guard
}

public enum AIState
{
    Idle,
    Sleeping,
    Waking,
    Patrolling,
    ReturningToPatrol,
    InCombat,
    Dead
}

public enum PatrolState
{
    Patrol,
    ReversePatrol,
    Waiting
}

public enum CombatState
{
    Pursuing,
    Strafing,
    StrafingToZone,
    RadialRunToZone,
    MaintainDist,
    ClosingDist,
    BackingUp,
    MovingToZone,
    MovingToAttack,
    Attacking,
    Dodging
}

public enum AttackingType
{
    Unassigned,
    Passive,
    Active
}

public enum AttackMode
{
    Normal,
    Quick,
    Heavy
}

public enum StrafeDir
{
    Left,
    Right
}

/**************************************************************************************
* Type: Class
* 
* Name: EnemyAI
*
* Author: Dean Pearce
*
* Description: Base EnemyAI class which handles navigation, behaviour, and animation.
**************************************************************************************/
public class EnemyAI : MonoBehaviour
{
    private AIManager m_aiManager;
    private EnemySoundHandler m_soundHandler;

    private NavMeshAgent m_navMeshAgent;
    private int m_spawnGroup = 0;
    [SerializeField]
    private EnemyType m_enemyType = EnemyType.Grunt;
    [SerializeField]
    [Tooltip("AI's Current State")]
    private AIState m_mainState = AIState.Idle;
    private CombatState m_combatState = CombatState.Strafing;
    [Header("Movement Values")]
    [SerializeField]
    [Tooltip("The walk speed of the AI")]
    private float m_walkSpeed = 1.5f;
    [SerializeField]
    [Tooltip("The run speed of the AI")]
    private float m_runSpeed = 3.0f;
    [SerializeField]
    [Tooltip("The speed the AI will rotate when attempting to look at a target")]
    private float m_turnSpeed = 75.0f;
    [SerializeField]
    [Tooltip("The difference from current rotation to target before the AI will lock rotation")]
    private float m_rotationBuffer = 5.0f;

    // Animation Relevant Variables
    private Animator m_animController;

    // Patrol Relevant Variables
    [Header("Patrol Values")]
    [SerializeField]
    [Tooltip("The GameObject which holds the position objects for patrolling")]
    private GameObject m_patrolRoute;
    private PatrolState m_patrolState = PatrolState.Patrol;
    private List<Transform> m_patrolRoutePoints = new List<Transform>();
    private Transform m_nextPatrolPoint;
    private Vector3 m_lastPointOnPatrol;
    private float m_patrolTimer = 0.0f;
    private float m_patrolWaitTime = 2.5f;
    private int m_patrolDestinationIndex = 1;
    [SerializeField]
    [Tooltip("The distance the AI will stop from the patrol points")]
    private float m_patrolStoppingDistance = 1.5f;

    // Player/Detection Relevant Variables
    private GameObject m_player;
    private PlayerController m_playerController;
    private Collider m_playerCollider;

    // Combat Relevant Variables
    private bool m_lookAtPlayer = false;
    [Header("Combat Values")]
    [SerializeField]
    [Tooltip("Normal Attack Damage")]
    private float m_normalAttackDmg = 10.0f;
    [SerializeField]
    [Tooltip("Quick Attack Damage")]
    private float m_quickAttackDmg = 5.0f;
    [SerializeField]
    [Tooltip("Heavy Attack Damage")]
    private float m_heavyAttackDmg = 15.0f;

    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop")]
    private float m_playerStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on normal attack")]
    private float m_normalAttkStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on quick attack")]
    private float m_quickAttkStoppingDistance = 1.75f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop on heavy attack")]
    private float m_heavyAttkStoppingDistance = 2.0f;
    private float m_delayBeforeStrafe = 0.0f;
    private float m_timeUntilStrafe = 0.0f;
    [SerializeField]
    [Tooltip("The minimum time the AI will stand still in combat before strafing")]
    private float m_minDelayBeforeStrafe = 6.0f;
    [SerializeField]
    [Tooltip("The maximum time the AI will stand still in combat before strafing")]
    private float m_maxDelayBeforeStrafe = 10.0f;
    private StrafeDir m_strafeDir = StrafeDir.Left;
    [SerializeField]
    [Tooltip("The strafing speed of the AI")]
    private float m_strafeSpeed = 1.5f;
    [SerializeField]
    [Tooltip("The distance the AI will check for other obstructing AI during combat")]
    private float m_checkForAIDist = 2.1f;
    [SerializeField]
    [Tooltip("The angles the AI will check for other obstructing AI during combat")]
    private float m_checkForAIAngles = 45.0f;
    [SerializeField]
    [Tooltip("The distance the AI will move away when attempting to avoid other AI")]
    private float m_AIAvoidanceDist = 1.5f;
    private float m_strafeDist;
    private float m_attackTimer;
    [SerializeField]
    [Tooltip("The chance that the AI will takeover a zone which is already occupied by another AI.")]
    private float m_zoneTakeoverChance = 25.0f;
    [SerializeField]
    [Tooltip("Whether the AI can attack. For debugging")]
    private bool m_attackEnabled = true;
    [SerializeField]
    [Tooltip("The minimum time that has to pass before an actively attacking AI can attack")]
    private float m_minAttackTime = 3.5f;
    [SerializeField]
    [Tooltip("The maximum time that can pass before an actively attacking AI will attack")]
    private float m_maxAttackTime = 7.5f;
    private float m_timeSinceLastAttack = 0.0f;
    [SerializeField]
    [Tooltip("Number of different attacks the AI will use")]
    private int m_attackNum = 3;
    [SerializeField]
    [Tooltip("The primary weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_primaryWeapon;
    private BoxCollider m_primaryWeaponCollider;
    [SerializeField]
    [Tooltip("The secondary weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_secondaryWeapon;
    private BoxCollider m_secondaryWeaponCollider;
    private AttackMode m_attackMode = AttackMode.Normal;
    private AttackingType m_currentAttackingType = AttackingType.Passive;
    private ZoneHandler m_zoneHandler = new ZoneHandler();
    private float m_zoneCheckInterval = 5.0f;
    private float m_zoneTimer = 0.0f;
    private float m_strafeCheckInterval = 2.0f;
    private float m_strafeTimer = 0.0f;
    private bool m_combatOnWake = false;
    private bool m_attackLocked = false;
    private bool m_isWaveEnemy = false;

    // Vision Detection Relevant Variables
    [Header("Player Detection Values")]
    [SerializeField]
    [Tooltip("Whether the AI can detect the player. For debugging")]
    private bool m_playerDetectionEnabled = true;
    [SerializeField]
    [Tooltip("The range that the AI can detect the player")]
    private float m_viewRadius = 7.5f;
    [SerializeField]
    [Range(0.0f, 360.0f)]
    [Tooltip("The angle that the AI can detect the player AKA field of view")]
    private float m_viewAngle = 145.0f;

    [Header("Animation Values")]
    [SerializeField]
    [Tooltip("Total number of sleep to wake animations")]
    private int m_sleepToWakeAnimNum = 2;
    private int[] an_sleepToWakeHashes;
    [SerializeField]
    [Tooltip("Total number of dodge animations")]
    private int m_dodgeAnimNum = 2;
    private int[] an_dodgeHashes;
    private int m_lastUsedAnimTrigger;

    [SerializeField]
    [Tooltip("The layer mask for obstacles")]
    private LayerMask m_obstacleMask;
    [SerializeField]
    [Tooltip("The layer mask for AI")]
    private LayerMask m_aiMask;

    // String to Hash stuff
    private int an_triggerNone = 0;
    private int an_walk;
    private int an_walkBack;
    private int an_strafeRight;
    private int an_strafeLeft;
    private int an_run;
    private int an_idle;
    private int an_combatIdle;
    private int an_attack;
    private int an_quickAttack;
    private int an_heavyAttack;
    private int an_sleep;
    private int an_death;
    private int an_takeHit;
    private int an_weaken;

    // Health Manager Component
    private EnemyDamageManager m_healthManager;

    // Mask Stuff added by Charlie
    [SerializeField]
    private GameObject m_masks;
    private GameObject[] m_masksArray;
    int maskEquipped;

    private void Awake()
    {
        // Randomising the mask to use
        m_masksArray = new GameObject[ m_masks.transform.childCount ];

        for (int i = 0; i< m_masksArray.Length; i++ )
		{
            m_masksArray[i] = m_masks.transform.GetChild(i).gameObject;
		}

        ResetMasks();

        // Setup the animation trigger hashes
        SetupStringToHashes();

        // Getting required components
        m_healthManager = GetComponent<EnemyDamageManager>();
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();
        m_soundHandler = GetComponent<EnemySoundHandler>();

        m_navMeshAgent.speed = m_walkSpeed;

        // Setup the patrol route if any
        SetupPatrolRoutes();

        // More required components
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerController = m_player.GetComponent<PlayerController>();
        m_playerCollider = m_player.GetComponent<Collider>();
        m_primaryWeaponCollider = m_primaryWeapon.GetComponent<BoxCollider>();
        m_secondaryWeaponCollider = m_secondaryWeapon.GetComponent<BoxCollider>();

        // Disable weapon collision
        DisableCollision();

        // Set AI state from inspector
        SetAIState(m_mainState);

        // Make sure last used trigger is clear at start
        m_lastUsedAnimTrigger = an_triggerNone;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetMasks
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Disables all masks, then picks a mask at random to enable
    **************************************************************************************/
    private void ResetMasks()
	{
        foreach ( GameObject mask in m_masksArray )
        {
            mask.SetActive( false );
        }
        maskEquipped = Random.Range( 0, m_masksArray.Length );
        m_masksArray[ maskEquipped ].SetActive( true );
    }

    private void Update()
    {
        // Should the AI be looking at the player
        if (m_lookAtPlayer)
        {
            TurnToLookAt(m_player.gameObject);
        }

        switch (m_mainState)
        {
            // Idle State
            case AIState.Idle:
            {
                if (IsPlayerVisible())
                {
                    SetAIState(AIState.InCombat);
                }
                break;
            }
            case AIState.Sleeping:
            {
                break;
            }
            case AIState.Waking:
            {
                break;
            }
            // Patrol Logic
            case AIState.Patrolling:
            {
                // Continue Patrol Update
                PatrolUpdate();

                // Start combat when AI sees player
                if (IsPlayerVisible())
                {
                    SetAIState(AIState.InCombat);
                }

                break;
            }
            case AIState.ReturningToPatrol:
            {
                // Carry on patrolling once previous point is reached
                if (HasReachedDestination())
                {
                    SetAIState(AIState.Patrolling);
                }
                break;
            }
            case AIState.InCombat:
            {
                CombatUpdate();
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PatrolUpdate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for handling logic during Patrol State
    **************************************************************************************/
    private void PatrolUpdate()
    {
        // If somehow reached here with no patrol route, just go back to Idle
        if (m_patrolRoute == null)
        {
            SetAIState(AIState.Idle);
            return;
        }

        switch (m_patrolState)
        {
            case PatrolState.Patrol:
            {
                if (HasReachedDestination())
                {
                    // If reached the end of the patrol points, go to wait state
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    // Move to next patrol point in sequence
                    else
                    {
                        m_patrolDestinationIndex++;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.ReversePatrol:
            {
                if (HasReachedDestination())
                {
                    // If reached the end of the patrol points, go to wait state
                    if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    // Move to next patrol point in sequence
                    else
                    {
                        m_patrolDestinationIndex--;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.Waiting:
            {
                m_patrolTimer += Time.deltaTime;

                // Using a timer to make the AI wait before patrolling the opposite direction
                if (m_patrolTimer >= m_patrolWaitTime)
                {
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.ReversePatrol);
                    }
                    else if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Patrol);
                    }
                }
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CombatUpdate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for handling logic during Combat State
    **************************************************************************************/
    private void CombatUpdate()
    {
        // Update zone handler to track status of zones
        m_zoneHandler.Update();

        // Condition to help space out attacks a bit more
        // If AIManager says can attack, and AI is not pursuing and IS an active attacker, then increments the attack timer
        if (m_aiManager.CanAttack() && m_combatState != CombatState.Pursuing && m_currentAttackingType == AttackingType.Active)
        {
            m_timeSinceLastAttack += Time.deltaTime;
        }

        switch (m_combatState)
        {
            // Chase after target/player
            case CombatState.Pursuing:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Checking if within the overall zone range, and if they've been assigned an attacking type yet
                if (DistanceSqrCheck(m_player, m_aiManager.GetPassiveAttackerMaxDist()) && m_currentAttackingType == AttackingType.Unassigned)
                {
                    SetupAttackingType();

                    RandomiseStrafeRange();
                }

                // Checking if they've reached the strafe range yet
                if (IsInStrafeRange() && m_currentAttackingType != AttackingType.Unassigned)
                {
                    // If there's no zones available, just maintain distance
                    if (!m_zoneHandler.AreZonesAvailable())
                    {
                        SetCombatState(CombatState.MaintainDist);
                        return;
                    }

                    // If the current zone exists and isn't occupied by another AI
                    if (m_zoneHandler.IsZoneAvailable())
                    {
                        // Set to maintain distance, then set this zone as the occupied zone
                        SetCombatState(CombatState.MaintainDist);

                        m_zoneHandler.OccupyCurrentZone();
                    }
                    // Start radial running to a zone
                    else
                    {
                        SetCombatState(CombatState.RadialRunToZone);
                    }
                }
                break;
            }
            // Strafe around the player
            case CombatState.Strafing:
            {
                // Timed check to determine whether the AI should try and occupy the zone they're currently in
                TimedZoneCheck();

                // Function to continue strafing
                Strafe();

                AiToPlayerRangeCheck();
                AttackCheck();
                break;
            }
            // Strafe with the intention of finding a zone to occupy
            case CombatState.StrafingToZone:
            {
                Strafe();

                AiToPlayerRangeCheck();

                // Radial obstruction check looks for other AI to avoid bumping into
                RadialObstructionCheck();
                StrafeZoneCheck();
                AttackCheck();
                break;
            }
            // Running in circle to find zone
            case CombatState.RadialRunToZone:
            {
                RadialRun();

                AiToPlayerRangeCheck();

                RadialObstructionCheck();
                RadialZoneCheck();
                AttackCheck();
                break;
            }
            // Maintain the current distance between AI and player
            case CombatState.MaintainDist:
            {
                TimedZoneCheck();             

                AiToPlayerRangeCheck();
                TimedBeginStrafeCheck();
                AttackCheck();
                break;
            }
            // Close the distance between AI and player
            case CombatState.ClosingDist:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                AiToPlayerRangeCheck();

                if (DistanceSqrCheck(m_player, m_strafeDist))
                {
                    StrafeOrMaintain();
                }

                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            // Increase the distance between AI and player
            case CombatState.BackingUp:
            {
                if (!DistanceSqrCheck(m_player, m_strafeDist))
                {
                    StrafeOrMaintain();
                    return;
                }

                BackUp();
                AiToPlayerRangeCheck();

                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            // Begin moving in to range for an attack
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Once in range, begin attack
                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.Attacking);
                }
                break;
            }
            // Moving to a specific zone
            case CombatState.MovingToZone:
            {
                m_navMeshAgent.destination = m_zoneHandler.GetReservedPos();

                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.MaintainDist);
                    m_zoneHandler.ClearReservedZone();
                    m_zoneHandler.OccupyCurrentZone();
                    //Debug.Log("AI: " + name + " reached destination.");
                }
                break;
            }
            // Currently in attack animation
            case CombatState.Attacking:
            {
                // Attack hits
                if (IsAttackCollidingWithPlayer())
                {
                    m_player.GetComponent<CharacterDamageManager>().TakeDamage(transform, GetCurrentAttackDamage());
                    m_soundHandler.PlayNormalCollisionSFX();
                    DisableCollision();
                }
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetAIState
    * Parameters: AIState stateToSet
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clean way of setting the AI's main state
    **************************************************************************************/
    public void SetAIState( AIState stateToSet )
    {
        // If changing FROM patrol state, store the last position in the patrol route
        if (m_mainState == AIState.Patrolling)
        {
            m_lastPointOnPatrol = gameObject.transform.position;
        }

        m_mainState = stateToSet;
        ResetLastUsedAnimTrigger();

        switch (stateToSet)
        {
            // Idle State
            case AIState.Idle:
            {
                StartIdleAnim();
                break;
            }
            // Sleep State
            case AIState.Sleeping:
            {
                SetToPlayDeadAnim();
                break;
            }
            // Patrol State
            case AIState.Patrolling:
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;

                if (m_patrolState == PatrolState.Patrol || m_patrolState == PatrolState.ReversePatrol)
                {
                    StartWalkAnim();
                }
                else if (m_patrolState == PatrolState.Waiting)
                {
                    StartIdleAnim();
                }
                break;
            }
            // Return to Patrol State
            case AIState.ReturningToPatrol:
            {
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
                m_navMeshAgent.autoBraking = false;
                StartWalkAnim();
                break;
            }
            // Combat State
            case AIState.InCombat:
            {
                // Registering the enemy as an attacker with the manager
                m_aiManager.RegisterAttacker(this);

                // Charlie: Added to only show health when they enter combat
                m_healthManager.ShowUI(true);

                SetupAttackingType();
                RandomiseStrafeRange();

                // If there's zones available, try to reserve one
                if (m_zoneHandler.AreZonesAvailable())
                {
                    m_zoneHandler.ReserveClosestZone();
                    SetCombatState(CombatState.MovingToZone);
                }
                // Otherwise just pursue
                else
                {
                    SetCombatState(CombatState.Pursuing);
                }

                ResetAttackTimer();

                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;

                break;
            }
            // Death State
            case AIState.Dead:
            {
                m_lookAtPlayer = false;
                StartDeathAnim();
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetPatrolState
    * Parameters: PatrolState stateToSet
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clean way of setting the AI's patrol state
    **************************************************************************************/
    private void SetPatrolState( PatrolState stateToSet )
    {
        m_patrolState = stateToSet;
        ResetLastUsedAnimTrigger();

        switch (stateToSet)
        {
            // Patrol in normal direction
            case PatrolState.Patrol:
            {
                StartWalkAnim();
                break;
            }
            // Patrol in reverse direction
            case PatrolState.ReversePatrol:
            {
                StartWalkAnim();
                break;
            }
            // Waiting to resume patrol
            case PatrolState.Waiting:
            {
                m_patrolTimer = 0.0f;
                StartIdleAnim();
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SSetCombatState
    * Parameters: CombatState stateToSet
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clean way of setting the AI's combat state
    **************************************************************************************/
    public void SetCombatState( CombatState stateToSet )
    {
        m_combatState = stateToSet;
        ResetLastUsedAnimTrigger();

        switch (stateToSet)
        {
            // Pursue Player
            case CombatState.Pursuing:
            {
                m_navMeshAgent.autoBraking = true;
                RandomiseStrafeRange();
                StartRunAnim();
                break;
            }
            // Strafe around player
            case CombatState.Strafing:
            {
                // Randomise the direction to strafe
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            // Strafe around player in search of empty zone
            case CombatState.StrafingToZone:
            {
                // Randomise the direction to strafe
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            // Radial run around player in search of zone
            case CombatState.RadialRunToZone:
            {
                // Randomise the direction to run
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartRunAnim();
                break;
            }
            // Maintain current distance to player
            case CombatState.MaintainDist:
            {
                // Randomise delay before moving again
                m_timeUntilStrafe = Random.Range(m_minDelayBeforeStrafe, m_maxDelayBeforeStrafe);
                m_zoneHandler.OccupyCurrentZone();
                StartCombatIdleAnim();
                break;
            }
            // Attack player
            case CombatState.Attacking:
            {
                Attack();
                break;
            }
            // Move directly to a specified zone
            case CombatState.MovingToZone:
            {
                StartRunAnim();
                break;
            }
            // Get in range of player for attack
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                // Randomise type of attack
                m_attackMode = (AttackMode)Random.Range(0, m_attackNum);

                switch(m_attackMode)
                {
                    case AttackMode.Normal:
                    {
                        m_navMeshAgent.stoppingDistance = m_normalAttkStoppingDistance;
                        break;
                    }
                    case AttackMode.Quick:
                    {
                        m_navMeshAgent.stoppingDistance = m_quickAttkStoppingDistance;
                        break;
                    }
                    case AttackMode.Heavy:
                    {
                        m_navMeshAgent.stoppingDistance = m_heavyAttkStoppingDistance;
                        break;
                    }
                }

                StartRunAnim();
                break;
            }
            // Close distance to player
            case CombatState.ClosingDist:
            {
                RandomiseStrafeRange();
                StartWalkAnim();
                break;
            }
            // Increase distance to player
            case CombatState.BackingUp:
            {
                RandomiseStrafeRange();
                StartWalkBackAnim();
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupStringToHashes
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Sets up variables for string to int hashes used by the animation controller
    **************************************************************************************/
    private void SetupStringToHashes()
    {
        an_dodgeHashes = new int[m_dodgeAnimNum];
        an_sleepToWakeHashes = new int[m_sleepToWakeAnimNum];

        for(int i = 0; i < an_dodgeHashes.Length; i++)
        {
            an_dodgeHashes[i] = Animator.StringToHash("Dodge" + i);
        }

        for (int i = 0; i < an_dodgeHashes.Length; i++)
        {
            an_sleepToWakeHashes[i] = Animator.StringToHash("SleepToWake" + i);
        }

        an_walk = Animator.StringToHash("Walk");
        an_walkBack = Animator.StringToHash("WalkBack");
        an_strafeRight = Animator.StringToHash("StrafeRight");
        an_strafeLeft = Animator.StringToHash("StrafeLeft");
        an_run = Animator.StringToHash("Run");
        an_idle = Animator.StringToHash("Idle");
        an_combatIdle = Animator.StringToHash("CombatIdle");
        an_attack = Animator.StringToHash("Attack");
        an_quickAttack = Animator.StringToHash("QuickAttack");
        an_heavyAttack = Animator.StringToHash("HeavyAttack");
        an_sleep = Animator.StringToHash("Sleep");
        an_death = Animator.StringToHash("Death");
        an_takeHit = Animator.StringToHash("TakeHit");
        an_weaken = Animator.StringToHash("Weaken");
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupAttackingType
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Sets the attacking type for this AI. If there's an active slot open,
    *              becomes active, otherwise becomes passive.
    **************************************************************************************/
    private void SetupAttackingType()
    {
        // If there's space for active attackers, become active
        if (m_aiManager.ActiveSlotsOpen())
        {
            m_aiManager.MakeActiveAttacker(this);
            m_currentAttackingType = AttackingType.Active;
        }
        // Else become passive
        else
        {
            m_aiManager.MakePassiveAttacker(this);
            m_currentAttackingType = AttackingType.Passive;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupPatrolRoutes
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for setting up the patrol routes. Clears at the start since this
    *              is also used in a reset function.
    **************************************************************************************/
    private void SetupPatrolRoutes()
    {
        // Clear to make sure we don't add to an outdated list
        if (m_patrolRoutePoints.Count > 0)
        {
            m_patrolRoutePoints.Clear();
        }

        // Adding patrol points to a list that the AI can use to follow
        if (m_patrolRoute != null)
        {
            for (int i = 0; i < m_patrolRoute.transform.childCount; i++)
            {
                m_patrolRoutePoints.Add(m_patrolRoute.transform.GetChild(i).gameObject.transform);
            }
        }

        // Checking patrol route points is valid, then setting next patrol point to the second entry
        if (m_patrolRoutePoints.Count >= 2)
        {
            m_nextPatrolPoint = m_patrolRoutePoints[1];
            m_lastPointOnPatrol = m_nextPatrolPoint.position;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupZoneHandler
    * Parameters: ref AttackZoneManager attackZoneManager
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Sets up the zone handler object. Hands through a reference to the attack zone manager.
    **************************************************************************************/
    public void SetupZoneHandler( ref AttackZoneManager attackZoneManager )
    {
        EnemyAI thisEnemy = this;
        m_zoneHandler.Init(ref thisEnemy, ref attackZoneManager);
    }

    public void StartNavMesh()
    {
        m_navMeshAgent.isStopped = false;
    }

    public void StopNavMesh()
    {
        m_navMeshAgent.isStopped = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TurnToLookAt
    * Parameters: GameObject targetObj
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for slowly rotating the enemy towards a target so they don't snap turn.
    **************************************************************************************/
    private void TurnToLookAt(GameObject targetObj)
    {
        // Getting dir from enemy to player
        Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
        float angleFrom = Vector3.SignedAngle(dirToPlayer, transform.forward, Vector3.down);

        // If the angle difference is greater than the buffer, slow turn
        if (Mathf.Abs(angleFrom) > m_rotationBuffer)
        {
            Vector3 currentEulerAngles = transform.eulerAngles;

            // Checking whether it's quicker to rotate clockwise or counter-clockwise
            if (angleFrom > 0)
            {
                currentEulerAngles.y += m_turnSpeed * Time.deltaTime;
            }
            else
            {
                currentEulerAngles.y -= m_turnSpeed * Time.deltaTime;
            }

            transform.eulerAngles = currentEulerAngles;
        }
        // If the angle difference is less than the buffer, lock into LookAt
        else
        {
            transform.LookAt(new Vector3(targetObj.transform.position.x, transform.position.y, targetObj.transform.position.z));
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RadialRun
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for running around the player.
    **************************************************************************************/
    private void RadialRun()
    {
        Vector3 offset;

        // Determining nav mesh target based on strafe dir
        if (m_strafeDir == StrafeDir.Left)
        {
            offset = m_player.transform.position - transform.position;
        }
        else
        {
            offset = transform.position - m_player.transform.position;
        }
        Vector3 dir = Vector3.Cross(offset, Vector3.up);
        m_navMeshAgent.SetDestination(transform.position + dir);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Strafe
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for strafing around the player.
    **************************************************************************************/
    private void Strafe()
    {
        Vector3 offset;
        // Determining nav mesh target based on strafe dir
        if (m_strafeDir == StrafeDir.Left)
        {
            offset = m_player.transform.position - transform.position;
        }
        else
        {
            offset = transform.position - m_player.transform.position;
        }
        Vector3 dir = Vector3.Cross(offset, Vector3.up);
        m_navMeshAgent.SetDestination(transform.position + dir);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: BackUp
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for backing away from the player.
    **************************************************************************************/
    private void BackUp()
    {
        Vector3 dir = (transform.position - m_player.transform.position).normalized;
        m_navMeshAgent.SetDestination(transform.position + (dir * 2.0f));
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Attack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for starting the AI's attack.
    **************************************************************************************/
    private void Attack()
    {
        switch (m_attackMode)
        {
            case AttackMode.Normal:
            {
                StartAttackAnim();
                break;
            }
            case AttackMode.Quick:
            {
                StartQuickAttackAnim();
                break;
            }
            case AttackMode.Heavy:
            {
                StartHeavyAttackAnim();
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EndAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for ending the AI's attack. Used in animation events.
    **************************************************************************************/
    private void EndAttack()
    {
        // Start backing up, then reset necessary values.
        SetCombatState(CombatState.BackingUp);
        ResetAttackTimer();
        SetStaggerable(true);
        m_attackLocked = false;

        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetAttackTimer
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Resets the attack timer so attacks can be spaced out.
    **************************************************************************************/
    private void ResetAttackTimer()
    {
        m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
        m_timeSinceLastAttack = 0.0f;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RecoverFromHit
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to recover after being staggered by the player. 
    *              Used in animation events.
    **************************************************************************************/
    private void RecoverFromHit()
    {
        SetCombatState(CombatState.Pursuing);
        SetStaggerable(true);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UnregisterAttacker
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for removing the AI from combat. Used on death.
    **************************************************************************************/
    public void UnregisterAttacker()
    {
        m_aiManager.UnregisterAttacker(this);
        m_zoneHandler.ClearOccupiedZone();
        m_zoneHandler.ClearReservedZone();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ChangeStateFromWake
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for changing from waking animation into a normal state.
    *              Used in animation events.
    **************************************************************************************/
    public void ChangeStateFromWake()
    {
        // If the AI has been set to combat on wake, will enter combat state
        if (m_combatOnWake)
        {
            SetAIState(AIState.InCombat);
        }
        else
        {
            if (m_patrolRoute != null)
            {
                SetAIState(AIState.Patrolling);        
            }
            else
            {
                SetAIState(AIState.Idle);
            }
        }

        // Had to put this setter here to force path recalculation, otherwise AI would attack immediately.
        m_navMeshAgent.SetDestination(m_player.transform.position);
        m_lookAtPlayer = false;

        m_healthManager.SetInvulnerable(false);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetLastUsedAnimTrigger
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clear the last anim trigger.
    **************************************************************************************/
    public void ResetLastUsedAnimTrigger()
    {
        if (m_lastUsedAnimTrigger != an_triggerNone)
        {
            m_animController.ResetTrigger(m_lastUsedAnimTrigger);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetAllAnimTriggers
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Clear all anim triggers
    **************************************************************************************/
    private void ResetAllAnimTriggers()
    {
        m_animController.ResetTrigger(an_walk);
        m_animController.ResetTrigger(an_idle);
        m_animController.ResetTrigger(an_attack);
        m_animController.ResetTrigger(an_quickAttack);
        m_animController.ResetTrigger(an_heavyAttack);
        m_animController.ResetTrigger(an_run);
        m_animController.ResetTrigger(an_sleep);
        m_animController.ResetTrigger(an_takeHit);
        m_animController.ResetTrigger(an_strafeLeft);
        m_animController.ResetTrigger(an_strafeRight);
        m_animController.ResetTrigger(an_combatIdle);
        m_animController.ResetTrigger(an_walkBack);
        m_animController.ResetTrigger(an_death);
        m_animController.ResetTrigger(an_weaken);

        foreach(int trigger in an_dodgeHashes)
        {
            m_animController.ResetTrigger(trigger);
        }

        foreach (int trigger in an_sleepToWakeHashes)
        {
            m_animController.ResetTrigger(trigger);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetToSpawn
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Used to reset necessary values on respawn.
    **************************************************************************************/
    public void ResetToSpawn()
    {
        m_patrolRoute = null;

        m_combatOnWake = false;
        m_isWaveEnemy = false;
        m_lastUsedAnimTrigger = an_triggerNone;
        m_navMeshAgent.speed = m_walkSpeed;
        m_healthManager.SetStaggerable(true);

        SetupPatrolRoutes();
        DisableCollision();
        ResetMasks();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: WakeUpAI
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Start waking the AI.
    **************************************************************************************/
    public void WakeUpAI()
    {
        SetAIState(AIState.Waking);
        StartSleepToWakeAnim();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LookAtPlayerOnWake
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to tell the enemy to look at the player during wake animation.
    *              Used in animation events.
    **************************************************************************************/
    public void LookAtPlayerOnWake()
    {
        m_lookAtPlayer = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DisableCollision
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Disables the weapon collision.
    **************************************************************************************/
    public void DisableCollision()
    {
        m_primaryWeaponCollider.enabled = false;
        m_secondaryWeaponCollider.enabled = false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EnableCollision
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Enables weapon collision. Used in animation events.
    **************************************************************************************/
    private void EnableCollision()
    {
        m_primaryWeaponCollider.enabled = true;
        m_secondaryWeaponCollider.enabled = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EnableCollision
    * Parameters: string colliderToEnable
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Enables weapon collision based on given string. Used in animation events.
    **************************************************************************************/
    private void EnableCollision( string colliderToEnable )
    {
        switch (colliderToEnable)
        {
            case "Primary":
            {
                m_primaryWeaponCollider.enabled = true;

                break;
            }
            case "Secondary":
            {
                m_secondaryWeaponCollider.enabled = true;

                break;
            }
            case "Both":
            {
                m_primaryWeaponCollider.enabled = true;
                m_secondaryWeaponCollider.enabled = true;

                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AiToPlayerRangeCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for checking the distance from the AI to the player in various
    *              situations.
    **************************************************************************************/
    private void AiToPlayerRangeCheck()
    {
        float maxStrafeRange = 0.0f;
        float minStrafeRange = 0.0f;

        // Passive range check
        if (m_currentAttackingType == AttackingType.Passive)
        {
            maxStrafeRange = m_aiManager.GetPassiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMaxDist();

            // If enemy is too close to the player, tell the AI manager to make this AI an active attacker, and swap the furthest active attacker to a passive attacker
            if ( DistanceSqrCheck( m_player, m_aiManager.GetActiveAttackerMinDist() ) )
            {
                m_aiManager.SwapPassiveWithActive(this);
            }
        }
        // Active range check
        else
        {
            maxStrafeRange = m_aiManager.GetActiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMinDist();
        }

        // AI is out of zone, empty zone, resume pursuit
        if (!DistanceSqrCheck(m_player, maxStrafeRange))
        {
            if (m_currentAttackingType == AttackingType.Passive)
            {
                m_aiManager.MakeUnasssignedAttacker(this);
                m_currentAttackingType = AttackingType.Unassigned;
            }
            SetCombatState(CombatState.Pursuing);
        }

        // Player moved closer than strafe range
        // Empty zone, then back up
        if (DistanceSqrCheck(m_player, minStrafeRange) && m_combatState != CombatState.BackingUp)
        {
            SetCombatState(CombatState.BackingUp);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AttackCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for deciding when to attack.
    **************************************************************************************/
    private void AttackCheck()
    {
        // Checking if it's been long enough to attack, if the current AI is an active attacker, and checking with the AI manager if attacking is allowed currently
        if (m_timeSinceLastAttack >= m_attackTimer && m_aiManager.CanAttack() && m_attackEnabled && m_currentAttackingType == AttackingType.Active)
        {
            SetCombatState(CombatState.MovingToAttack);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StrafeOrMaintain
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for deciding whether the AI should maintain distance, or strafe
    *              to a new zone.
    **************************************************************************************/
    private void StrafeOrMaintain()
    {
        // Decide whether to strafe or maintain distance based on whether the zone is the currently occupied zone
        if (m_zoneHandler.IsInValidZone() || !m_zoneHandler.AreZonesAvailable())
        {
            SetCombatState(CombatState.MaintainDist);
        }
        else
        {
            m_combatState = CombatState.RadialRunToZone;
            ResetLastUsedAnimTrigger();
            StartRunAnim();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RandomiseStrafeRange
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Randomise the range the AI should maintain from the player based
    *              on their attacker type.
    **************************************************************************************/
    private void RandomiseStrafeRange()
    {
        // Randomise the range for the AI to maintain based on attacker type
        if (m_currentAttackingType == AttackingType.Passive)
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist());
        }
        else
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist());
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: HasReachedDestination
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function to check if the NavMeshAgent has reached the destination.
    **************************************************************************************/
    private bool HasReachedDestination()
    {
        bool destinationReached = false;

        if (!m_navMeshAgent.pathPending)
        {
            if (m_navMeshAgent.remainingDistance < m_navMeshAgent.stoppingDistance)
            {
                destinationReached = true;
            }
        }

        return destinationReached;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsInStrafeRange
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function to check if the AI is in strafe range of the player.
    **************************************************************************************/
    private bool IsInStrafeRange()
    {
        return DistanceSqrCheck(m_player, m_strafeDist);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsAttackCollidingWithPlayer
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function to check if the AI's attack is colliding with the player.
    **************************************************************************************/
    public bool IsAttackCollidingWithPlayer()
    {
        bool isColliding = false;

        if (m_primaryWeaponCollider.enabled && m_primaryWeaponCollider.bounds.Intersects(m_playerCollider.bounds) ||
            m_secondaryWeaponCollider.enabled && m_secondaryWeaponCollider.bounds.Intersects(m_playerCollider.bounds))
        {
            isColliding = true;
        }

        return isColliding;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TimedZoneCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to check for an available zone based on a timer.
    **************************************************************************************/
    private void TimedZoneCheck()
    {
        // If no zones available, just return from the function
        if (!m_zoneHandler.AreZonesAvailable())
        {
            return;
        }

        m_zoneTimer += Time.deltaTime;

        if (m_zoneTimer >= m_zoneCheckInterval)
        {
            m_zoneTimer = 0.0f;

            if (!m_zoneHandler.IsInMatchingZone())
            {
                // If in the active zone, but not an active attacker, back up
                if (m_currentAttackingType != AttackingType.Active)
                {
                    SetCombatState(CombatState.BackingUp);
                    return;
                }
                // If in a passive zone, but an active attacker, close the distance
                else if (m_currentAttackingType == AttackingType.Active)
                {
                    SetCombatState(CombatState.ClosingDist);
                    return;
                }
            }
            // If zone is not occupied
            if (m_zoneHandler.IsZoneAvailable())
            {
                m_zoneHandler.OccupyCurrentZone();
            }
            else if (m_zoneHandler.GetCurrentAttackZone().IsOccupied())
            {
                // Code to randomise whether an AI can force another AI out of zone
                float takeoverRand = Random.Range(0.0f, 100.0f);
                if (takeoverRand < m_zoneTakeoverChance)
                {
                    m_zoneHandler.TakeOverOccupiedZone();
                }
                else
                {
                    SetCombatState(CombatState.StrafingToZone);
                }
            }
            else
            {
                SetCombatState(CombatState.StrafingToZone);
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TimedBeginStrafeCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to make the AI start strafing after a certain amount of time.
    **************************************************************************************/
    private void TimedBeginStrafeCheck()
    {
        if (m_zoneHandler.AreZonesAvailable())
        {
            m_delayBeforeStrafe += Time.deltaTime;

            if ( m_delayBeforeStrafe > m_timeUntilStrafe )
            {
                SetCombatState(CombatState.StrafingToZone);
                m_delayBeforeStrafe = 0.0f;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StrafeZoneCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to check for an open zone while strafing.
    **************************************************************************************/
    private void StrafeZoneCheck()
    {
        // If no zone available, just return from the function
        if (!m_zoneHandler.AreZonesAvailable())
        {
            return;
        }

        m_strafeTimer += Time.deltaTime;

        if (m_strafeTimer >= m_strafeCheckInterval)
        {
            m_strafeTimer = 0.0f;

            if (m_zoneHandler.IsZoneAvailable())               
            {
                m_zoneHandler.OccupyCurrentZone();
                SetCombatState(CombatState.MaintainDist);
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RadialObstructionCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to check if another AI is in the way whilst strafing.
    **************************************************************************************/
    private void RadialObstructionCheck()
    {
        Vector3 dir = transform.forward;
        Vector3 castFrom = transform.position;
        castFrom.y += m_navMeshAgent.height * 0.5f;

        // If strafing, setup directions to be from sides
        if (m_combatState == CombatState.StrafingToZone)
        {
            if (m_strafeDir == StrafeDir.Left)
            {
                dir = -transform.right;
            }
            else
            {
                dir = transform.right;
            }
        }

        GameObject enemyToCheck = FindClosestEnemy().gameObject;

        if( enemyToCheck != this )
        {
            if (DistanceSqrCheck(enemyToCheck, m_checkForAIDist))
            {
                Vector3 dirToCheck;

                if (m_strafeDir == StrafeDir.Right)
                {
                    dirToCheck = transform.right;
                }
                else
                {
                    dirToCheck = -transform.right;
                }

                Vector3 dirToEnemy = (enemyToCheck.transform.position - transform.position).normalized;
                if (Vector3.Angle(dirToCheck, dirToEnemy) < m_checkForAIAngles * 0.5f)
                {
                    float currentZoneHalfDist = 0.0f;

                    // Finding the distance to compare with the current strafe distance to determine whether the AI should move backwards or forwards
                    if (m_currentAttackingType == AttackingType.Passive)
                    {
                        currentZoneHalfDist = m_aiManager.GetActiveAttackerMaxDist() + ((m_aiManager.GetPassiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMaxDist()) * 0.5f);
                    }
                    else
                    {
                        currentZoneHalfDist = m_aiManager.GetActiveAttackerMinDist() + ((m_aiManager.GetActiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMinDist()) * 0.5f);
                    }

                    if (m_strafeDist > currentZoneHalfDist)
                    {
                        ResetLastUsedAnimTrigger();
                        StartWalkAnim();
                        m_strafeDist -= m_AIAvoidanceDist;
                        m_combatState = CombatState.ClosingDist;
                    }
                    else
                    {
                        ResetLastUsedAnimTrigger();
                        StartWalkBackAnim();
                        m_strafeDist += m_AIAvoidanceDist;
                        m_combatState = CombatState.BackingUp;
                    }
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RadialZoneCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function to check if zone is available whilst radial running.
    **************************************************************************************/
    private void RadialZoneCheck()
    {
        if (m_zoneHandler.IsZoneAvailable())
        {
            m_combatState = CombatState.StrafingToZone;
            ResetLastUsedAnimTrigger();
            StartStrafeAnim(m_strafeDir);
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FindClosestEnemy
    * Parameters: n/a
    * Return: EnemyAI
    *
    * Author: Dean Pearce
    *
    * Description: Function for finding the closest enemy.
    **************************************************************************************/
    private EnemyAI FindClosestEnemy()
    {
        EnemyAI closestEnemy = this;

        foreach (EnemyAI enemy in m_aiManager.GetEnemyList())
        {
            // If enemy is not this one, is active, and is in combat
            if (enemy != this && enemy.gameObject.activeSelf && enemy.GetState() == AIState.InCombat)
            {
                if (DistanceSqrValue(enemy.gameObject) < DistanceSqrValue(closestEnemy.gameObject))
                {
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    // DISCLAIMER:
    // DirFromAngle() and IsPlayerVisible() functions use logic from https://www.youtube.com/watch?v=rQG9aUWarwE

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DirFromAngle
    * Parameters: float angleInDegrees, bool angleIsGlobal
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Returns a direction from a given angle.
    **************************************************************************************/
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
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
    * Description: Overloaded DirFromAngle to allow getting the direction from
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

    /**************************************************************************************
    * Type: Function
    * 
    * Name: IsPlayerVisible
    * Parameters: n/a
    * Return: bool
    *
    * Author: Dean Pearce
    *
    * Description: Function for allowing the AI to determine whether the player is visible to them.
    **************************************************************************************/
    public bool IsPlayerVisible()
    {
        bool playerIsVisible = false;

        // If in combat, just return true since no point redoing detection
        // Will need changing if de-aggro functionality is implemented
        if (m_mainState == AIState.InCombat)
        {
            return true;
        }

        if (m_playerDetectionEnabled)
        {
            // Checking if player is in range
            if (DistanceSqrCheck(m_player, m_viewRadius))
            {
                // Once player is in range, getting the direction to the player and checking if it's within the AI's FOV
                Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToPlayer) < m_viewAngle * 0.5f)
                {
                    // Once player is in range and in FOV, using Raycast to check if any obstacles are in the way
                    float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);
                    if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, m_obstacleMask))
                    {
                        playerIsVisible = true;
                    }
                }
            }
        }

        return playerIsVisible;
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
    private bool DistanceSqrCheck( GameObject targetToCheck, float distanceToCheck )
    {
        bool isInRange = false;

        // Getting the distance between this and the target
        Vector3 distance = transform.position - targetToCheck.transform.position;

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
    * Name: DistanceSqrValue
    * Parameters: GameObject targetToCheck
    * Return: float
    *
    * Author: Dean Pearce
    *
    * Description: Function for returning the distance to an object.
    **************************************************************************************/
    private float DistanceSqrValue( GameObject targetToCheck )
    {
        return (transform.position - targetToCheck.transform.position).sqrMagnitude;
    }

    // Start of Anim functions
    // These functions are mostly the same and are used to trigger various animations

    private void StartWalkAnim()
    {
        m_navMeshAgent.isStopped = false;
        SetAnimTrigger(an_walk);
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;
    }

    private void StartStrafeAnim( StrafeDir dirToStrafe )
    {
        int animTrigger = an_triggerNone;

        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.speed = m_strafeSpeed;

        // Hacky bit of code, but added as an afterthought as the Guard has no functional strafe anim
        // So this makes the guard look where it's walking when strafing, whereas the grunt can just float
        if (m_enemyType == EnemyType.Grunt)
        {
            m_navMeshAgent.updateRotation = false;
            m_lookAtPlayer = true;
        }
        else
        {
            m_navMeshAgent.updateRotation = true;
            m_lookAtPlayer = false;
        }

        switch (dirToStrafe)
        {
            case StrafeDir.Left:
            {
                animTrigger = an_strafeLeft;

                SetAnimTrigger(animTrigger);
                break;
            }
            case StrafeDir.Right:
            {
                animTrigger = an_strafeRight;

                SetAnimTrigger(animTrigger);
                break;
            }
        }
    }

    private void StartWalkBackAnim()
    {
        m_navMeshAgent.isStopped = false;
        SetAnimTrigger(an_walkBack);
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;

    }

    private void StartRunAnim()
    {
        m_navMeshAgent.isStopped = false;
        SetAnimTrigger(an_run);
        m_navMeshAgent.speed = m_runSpeed;
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;

    }

    private void StartIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_idle);
        m_navMeshAgent.updateRotation = true;
        m_lookAtPlayer = false;
    }

    private void StartCombatIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_combatIdle);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    private void StartAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_attack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    private void StartQuickAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_quickAttack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    private void StartHeavyAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_heavyAttack);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    private void StartDodgeAnim()
    {
        int animNum = Random.Range(0, m_dodgeAnimNum);
        int animTrigger = an_dodgeHashes[animNum];

        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(animTrigger);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
    }

    private void StartSleepToWakeAnim()
    {
        int animNum = Random.Range(0, m_sleepToWakeAnimNum);
        int animTrigger = an_sleepToWakeHashes[animNum];

        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(animTrigger);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
    }

    private void SetToPlayDeadAnim()
    {
        m_navMeshAgent.isStopped = true;
        SetAnimTrigger(an_sleep);
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
    }

    private void StartDeathAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;

        // Commented out due to EnemyDamageManager effectively controlling it
        //m_animController.SetTrigger(an_death);
        m_lastUsedAnimTrigger = an_death;
    }

    private void SetAnimTrigger(int animTrigger)
    {
        if (m_mainState != AIState.Dead)
        {
            m_animController.SetTrigger(animTrigger);
            m_lastUsedAnimTrigger = animTrigger;
        }
    }

    // End of Anim functions

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LockAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for locking the enemy into an attack so they don't track the player
    *              as the attack lands. Used in animation events.
    **************************************************************************************/
    public void LockAttack()
    {
        m_navMeshAgent.isStopped = true;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = false;
        m_healthManager.SetStaggerable(false);
        m_attackLocked = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UnlockAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Function for undoing LockAttack. Used in animation events.
    **************************************************************************************/
    public void UnlockAttack()
    {
        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.updateRotation = false;
        m_lookAtPlayer = true;
        m_healthManager.SetStaggerable(true);
        m_attackLocked = false;
    }

    // Start of getters & setters

    public EnemyType GetEnemyType()
    {
        return m_enemyType;
    }

    public ZoneType GetZoneTypeFromAttackType()
    {
        if(m_currentAttackingType == AttackingType.Passive)
        {
            return ZoneType.Passive;
        }
        else if (m_currentAttackingType == AttackingType.Active)
        {
            return ZoneType.Active;
        }
        else
        {
            return ZoneType.None;
        }
    }

    public AIState GetState()
    {
        return m_mainState;
    }

    public CombatState GetCombatState()
    {
        return m_combatState;
    }

    public PatrolState GetPatrolState()
    {
        return m_patrolState;
    }

    public void SetPatrolRoute(GameObject patrolRoute)
    {
        m_patrolRoute = patrolRoute;
        m_patrolRoutePoints.Clear();

        SetupPatrolRoutes();
    }

    public float GetViewRadius()
    {
        return m_viewRadius;
    }

    public float GetViewAngle()
    {
        return m_viewAngle;
    }

    public float GetEulerAngles()
    {
        return transform.eulerAngles.y;
    }

    public float GetStrafeDist()
    {
        return m_strafeDist;
    }

    public float GetAIAngleCheck()
    {
        return m_checkForAIAngles;
    }

    public float GetAgentHeight()
    {
        return m_navMeshAgent.height;
    }

    public EnemySoundHandler GetSoundHandler()
    {
        return m_soundHandler;
    }

    public CharacterDamageManager GetHealthManager()
	{
        return m_healthManager;
	}

    public void SetLastUsedAnimTrigger( int trigger )
	{
        m_lastUsedAnimTrigger = trigger;
    }

    public void SetAIManagerRef( AIManager aiManagerRef )
    {
        m_aiManager = aiManagerRef;
    }

    public void SetStaggerable(bool isStaggerable)
    {
        m_healthManager.SetStaggerable(isStaggerable);
    }

    public void SetUnstaggerable()
    {
        m_healthManager.SetStaggerable(false);
    }

    public AttackingType GetAttackingType()
    {
        return m_currentAttackingType;
    }

    public AttackMode GetAttackMode()
    {
        return m_attackMode;
    }

    public float GetCurrentAttackDamage()
    {
        float attackDamage = m_normalAttackDmg;

        switch (m_attackMode)
        {
            case AttackMode.Normal:
            {
                attackDamage = m_normalAttackDmg;

                break;
            }
            case AttackMode.Quick:
            {
                attackDamage = m_quickAttackDmg;

                break;
            }
            case AttackMode.Heavy:
            {
                attackDamage = m_heavyAttackDmg;

                break;
            }
        }

        return attackDamage;
    }

    public bool IsAttackLocked()
    {
        return m_attackLocked;
    }

    public void SetAttackingType( AttackingType typeToSet )
    {
        m_currentAttackingType = typeToSet;
    }

    public void SetStrafeDist( float distance )
    {
        m_strafeDist = distance;
    }

    public StrafeDir GetStrafeDir()
    {
        return m_strafeDir;
    }

    public float GetAICheckDist()
    {
        return m_checkForAIDist;
    }

    public ZoneHandler GetZoneHandler()
    {
        return m_zoneHandler;
    }

    public void SetSpawnGroup(int groupToSet)
    {
        m_spawnGroup = groupToSet;
    }

    public int GetSpawnGroup()
    {
        return m_spawnGroup;
    }

    public void SetCombatOnWake(bool shouldCombatOnWake)
    {
        m_combatOnWake = shouldCombatOnWake;
    }

    public void SetWaveEnemy( bool isWaveEnemy )
    {
        m_isWaveEnemy = isWaveEnemy;
    }

    public bool IsWaveEnemy()
    {
        return m_isWaveEnemy;
    }
}