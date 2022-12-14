using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
    //Components
    //Animator
    private Animator m_animator;
    //Player Controller Script
    private PlayerController m_playerController;

    [Header("Input Controls")]
    [SerializeField, Tooltip( "Light Attack Input" )]
    private InputActionReference m_lightAttackInput;
    [SerializeField, Tooltip( "Heavy Attack Input" )]
    private InputActionReference m_heavyAttackInput;


    [Header( "Relevant Objects" )]
    [SerializeField]
    private SwordCollisionManager m_currentWeaponManager;
    [SerializeField, Tooltip("Trail for the player weapon (Default as table leg)")]
    ParticleSystem m_swordTrail;
    private Collider m_weaponCollider;

    private bool m_canStartNextAttack = true;

    //For box collider rotations
    //Object with the collider that actually rotates

    //How fast the player rotates at begining of an attack
    [Header( "Settings" )]
    [SerializeField, Range(0f, 1f),Tooltip("How fast player rotates on an attack")]
    private float m_rotateSpeed = 0.2f;

    //String to Hashes for animator parameters
    private int an_attack;
    private int an_lightAttack;
    private int an_heavyAttack;
    private int an_whirlwindHeld;
    private int an_comboActive;

    //Deactivated Air Attacks
    //[SerializeField]
    //private Transform m_sphereColliderTransform;

    //[Header("Ground Pound")]
    //[SerializeField, Range(0f, 5f)]
    //float m_timeToGrow = 1f;
    //[SerializeField, Range(5f, 10f)]
    //float m_maxSphereSize = 7;



    //Attack Enum, based on 2 attack types, and a nothing value so as to be able to have no attack queued
    private enum Attack
    {
        Nothing,
        Light,
        Heavy,
    }
    //Member version of attack
    private Attack m_attackType;


    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnEnable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enable input actions
    **************************************************************************************/
    private void OnEnable()
	{
        //Input actions need to be enabled
        m_lightAttackInput.action.Enable();
        m_heavyAttackInput.action.Enable();
    }

	/**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Setup components and member vars
    **************************************************************************************/
	void Start()
    {
        //Populate component refs
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponent<Animator>();
        //Should be table leg at start
        m_weaponCollider = m_currentWeaponManager.gameObject.GetComponent<Collider>();
        //Stop sword trail playing at launch
        m_swordTrail.Stop();

        //Weapon doesn't collide yet
        m_weaponCollider.enabled = false;

        //populate string to hashes
        an_attack = Animator.StringToHash( "attacked" );
        an_lightAttack = Animator.StringToHash( "light" );
        an_heavyAttack = Animator.StringToHash( "heavy" );
        an_whirlwindHeld = Animator.StringToHash( "whirlwindHeld" );
        an_comboActive = Animator.StringToHash( "comboActive" );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckInputs
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Every frame check player inputs, and update m_attackType
    **************************************************************************************/
    private void CheckInputs()
	{
        //If the player is grounded (No air attacks, and don't want to queue them when in air)
        if ( m_playerController.GetGrounded() )
        {
            //Light Attack
            if ( m_lightAttackInput.action.triggered )
            {
                m_attackType = Attack.Light;
            }
            //Heavy Attack
            if ( m_heavyAttackInput.action.triggered )
            {
                m_attackType = Attack.Heavy;
            }

            //Heavy will have been triggered anyway, but is it stil pressed?
            if ( m_heavyAttackInput.action.IsPressed() )
            {
                m_animator.SetBool( an_whirlwindHeld, true );
            }
            else
            {
                m_animator.SetBool( an_whirlwindHeld, false );
            }
            //Add a (NON COROUTINE BASED) timer to check if it should be cancelled???? like 1 second

        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PerformAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Based on input and if you can, perform an attack
    **************************************************************************************/
    private void PerformAttack()
	{
        /* When you click an input, as above, you assign m_attackType to Light or Heavy etc
         * If you can start an attack (Be it, you going from idle or are available to do so
         * in a combo), we can now enter this statement
         */
        if ( m_canStartNextAttack )
        {
            m_playerController.LoseControl();
            //lose control makes you lose dodge, but I want to be able to
            m_playerController.SetCanDodge( true );

            //We have attacked, set the trigger and bool
            m_animator.SetTrigger( an_attack );
            m_animator.SetBool( an_comboActive, true );

            //What attack type?
            switch ( m_attackType )
            {
                case Attack.Light:
                    m_animator.SetTrigger( an_lightAttack );
                    break;

                case Attack.Heavy:
                    m_animator.SetTrigger( an_heavyAttack );
                    break;

                case Attack.Nothing:
                    Debug.Log( "You've reset the Attack type to nothing before executing the Switch. This should not happen" );
                    break;

            }
            //Next queued attack is now nothing, until we add one in next run of update
            m_attackType = Attack.Nothing;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Activate Mechanim Triggers and bools, allowing attacks to flow
    **************************************************************************************/
    void Update()
    {
        if ( !Settings.g_paused )
        {
            /* I wanted to do a version where check inputs returned the Attack value
             * with no m_attackType member var, but that would have just resulted in
             * a temporary one being made all the time in the update as I need to compare
             * the value with Attack.Nothing AND i'd have to pass it into PerformAttack
             * and then it's 2 function calls or a temp var every frame, so member seemed
             * better */

            //Update m_attackType in here
            CheckInputs();
            //Then compare it to nothing
            if ( m_attackType != Attack.Nothing )
			{
                //And if there is an attack queued up
                PerformAttack();
            }

        }

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetCanAttack
    * Parameters: bool canAttack
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set can attack bool, which will allow or dissalow combos to happen
    **************************************************************************************/
    public void SetCanAttack(bool canAttack)
	{
        m_canStartNextAttack = canAttack;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetAttackDamage
    * Parameters: float damage
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set the damage for this attack, called at the VERY first frame of attack
    *              in AttackBegin
    *              Modified by a multiplier in m_currentWeaponManager
    **************************************************************************************/
    private void SetAttackDamage( float damage )
	{
        //Damage is modified by a multiplier on actual collision
        m_currentWeaponManager.SetDamage( damage );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SwapWeapon
    * Parameters: GameObject newWeapon, SwordCollisionManager weaponManager
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: When picking up the sword, we need to swap the sword trail info, 
    *              what manager is used due to damage multipliers and also the collider
    **************************************************************************************/
    public void SwapWeapon( GameObject newWeapon )
	{
        m_swordTrail = newWeapon.GetComponentInChildren<ParticleSystem>();
        m_currentWeaponManager = newWeapon.GetComponent<SwordCollisionManager>();
        m_weaponCollider = newWeapon.GetComponent<Collider>();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CollisionsStart
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attacks collisions should begin
    **************************************************************************************/
    private  void CollisionsStart()
    {
        m_swordTrail.Play();
        //Set collider sweeper on
        m_weaponCollider.enabled =  true ;

        //You can dodge when the collisions are happening,
        //If you dodge it will turn off the collider.
        m_playerController.SetCanDodge( true );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CollisionsEnd
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attacks collisions should end, and
    *              by PlayerController.Dodge()
    **************************************************************************************/
    public void CollisionsEnd()
    {
        m_swordTrail.Stop();
        //Set collider sweeper off
        m_weaponCollider.enabled = false;

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CanStartNextAttack
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called when another Attack can begin
    **************************************************************************************/
    public void CanStartNextAttack()
    {
        //Begin attack again if you can
        m_canStartNextAttack = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AttackBegin
    * Parameters: float damage
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attack animation FIRST begins.
    *              Used mainly for rotating the player to the direction they're inputting
    *              and also setting Damage
    **************************************************************************************/
    private void AttackBegin( float damage )
	{
        //Set Damage for this attack
        SetAttackDamage( damage );

        //Prevent dodging so it can't blend and leave the collider on
        m_playerController.SetCanDodge( false );

        //Only bother if the there is some level of input, other wise it's a waste
        if ( m_playerController.GetMoveDirection() != Vector3.zero )
        {
            //Rotate player over a short time, to the place where the player is trying to move to
            StartCoroutine( RotatePlayer() );

        }

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EndCombo
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Animation Events when the attack combo ends, at the begining 
    *              of the leave anim, to reset variables that allow the player to move 
    *              or fall
    **************************************************************************************/
    private void EndCombo()
    {
        CanStartNextAttack();
        m_playerController.RegainControl();
        m_animator.SetBool( an_comboActive, false );
    }

    /**************************************************************************************
    * Type: IEnumerator
    * 
    * Name: RotatePlayer
    * Parameters: Quaternion targetRotation
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Over a set time, rotate the player to face the inputted direction
    **************************************************************************************/
    private IEnumerator RotatePlayer()
    {
        //Current Position value
        Vector3 startPosition = transform.position;

        //Add the direction to where you are to get the vector
        Vector3 targetPosition = transform.position + m_playerController.GetMoveDirection();

        //Get target angle in degrees
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        //Debug.Log( targetAngle ); 

        Quaternion targetRotation = Quaternion.Euler( new Vector3( 0f, targetAngle, 0f ) );

        //While time is still less than the rotate time speed thing, rotate to the position
        for ( var elapsedTime = 0f; elapsedTime < 1; elapsedTime += Time.deltaTime / m_rotateSpeed )
        {
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, elapsedTime );
            yield return null;
        }
	}

    //Turned off Air Attacks as they were incomplete, but wanted to leave it here for the future
 //   private void GroundpoundActivated()
 //   {
 //       //m_playerController.m_playerVelocity.y -= Mathf.Sqrt( m_jumpForce * -3.0f * m_gravityValue )
 //       m_playerController.m_playerVelocity.y = -15f;
 //       //Begin plumetting to the ground

 //       m_playerController.m_canFall = true;

 //       m_playerController.m_canDodge = false;
 //   }

 //   private void GrounpoundLanded()
	//{
 //       //AoE Attack
 //       Debug.Log( "Big AoE" );
 //       StartCoroutine( ExpandGroundpoundSphere() );
	//}


 //   IEnumerator ExpandGroundpoundSphere()
	//{
 //       float timer = 0;
 //       Vector3 MaxSize = new Vector3(m_maxSphereSize, m_maxSphereSize, m_maxSphereSize ); 


 //       while( timer < m_timeToGrow )
	//	{

 //           //Debug.Log( m_sphereColliderTransform.localScale );
 //           m_sphereColliderTransform.localScale = Vector3.Lerp( m_sphereColliderTransform.localScale, MaxSize, (timer/m_timeToGrow) );
 //           timer += Time.deltaTime;

 //           yield return null;
 //       }
        
 //       m_sphereColliderTransform.localScale = new Vector3( 0.5f, 0.5f, 0.5f );
 //   }
}
