using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/**************************************************************************************
* Type: Class
* 
* Name: PlayerController
*
* Author: Charlie Taylor
*
* Description: Managing the player's movement, jumping and locking them when they can't
*              move
**************************************************************************************/

public enum  MovementAnim
{
    Standing,
    Walking,
    Running
}

public class PlayerController : MonoBehaviour
{
    //Start in menu state
    private int an_menuState;

    //Input actions
    [Header( "Input Controls" )]
    [SerializeField, Tooltip( "Movement Input" )]
    private InputActionReference m_movementControl;
    [SerializeField, Tooltip( "Dodge Control Input" )]
    private InputActionReference m_dodgeControl;

    //Jump turned off
    [SerializeField]
    [Tooltip( "Jump Control Input" )]
    private InputActionReference m_jumpControl;



    //Player Components
    private MeleeController m_meleeController;
    private Animator m_animator;
    private CharacterController m_controller;
    private CharacterDamageManager m_playerHealth;

    //Player stats
    //Move Speed
    [Header( "Player Settings" )]
    [SerializeField, Range( 0.0f, 8.0f ), Tooltip( "How fast the player runs" )]
    private float m_playerSpeed = 5.0f;
    [SerializeField, Range( 1.0f, 15.0f ), Tooltip( "Player jump force" )]
    private float m_jumpForce = 1.0f;
    [SerializeField, Range( 0.0f, -15.0f ), Tooltip( "Gravity that affects the player as it does not use physics" )]
    private float m_gravityValue = -9.81f;
    [SerializeField, Range( 0.0f, -30.0f ), Tooltip( "Player's Terminal Velocity" )]
    private float m_terminalVelocity = -20.0f;
    [SerializeField, Range( 2, 5 ), Tooltip( "How fast the player rotates when moving in a new direction" )]
    private float m_rotationSpeed = 4f;
    [SerializeField, Range( 0.0f, 1.0f ), Tooltip( "How quickly the blend tree animations blend together on a change" )]
    private float m_dampTime;

    //Player's velocity (Only for Y, just easier as a vector 3)
    private Vector3 m_playerVelocity;
    //Is the player touching the ground
    private bool m_isGrounded;
    //Camera's transform position, used for directional movmement/attacking
    private Transform m_cameraMainTransform;

    //Values that allow player to move or fall or rotate
    private bool m_canMove = true;
    private bool m_canFall = true;
    private bool m_canRotate = true;
    private bool m_canDodge = true;

    //When regaining control from an attack, if menu was pressed inbetween, it could allow player to regain control. This stops that
    private bool m_menuLock = true;

    //Normalised float of how much player is moving. Used in 1D Blend Tree
    private float m_moveAmount;

    //to stop moving while standing up
    private bool m_stoodUp = false;

    //Animator Parameters
    private int an_movingSpeed;
    private int an_inAir;
    private int an_jumped;
    private int an_dodge;
    private int an_beganFalling;
    private int an_yVelocity;

    [Header( "Landing Raycast Values" )]
    [SerializeField, Range( 0, 1 ), Tooltip( "How big the sphere at the feet of the player should be to detect the floor" )]
    private float m_raycastDistance;
    [SerializeField, Tooltip( "Layers that allow the player to land and not stay in fall" )]
    private LayerMask m_landableLayers;

    // Dean Note: Adding sound handler here for player sound
    private PlayerSoundHandler m_soundHandler;
    private MovementAnim m_movementAnimState = MovementAnim.Standing;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Prep the player for the begining of the game
    **************************************************************************************/
    private void Start()
    {
        //Input actions need to be enabled
        m_movementControl.action.Enable();
        m_jumpControl.action.Enable();
        m_dodgeControl.action.Enable();
        //Asign stuff with GetComponent instead of in Inspector
        m_meleeController = GetComponent<MeleeController>();
        m_animator = GetComponent<Animator>();
        m_controller = gameObject.GetComponent<CharacterController>();
        m_playerHealth = gameObject.GetComponent<CharacterDamageManager>();
        m_soundHandler = GetComponent<PlayerSoundHandler>();

        //Animation String to hash parameters
        an_movingSpeed = Animator.StringToHash( "movingSpeed" );
        an_inAir = Animator.StringToHash( "inAir" );
        an_jumped = Animator.StringToHash( "jumped" );
        an_dodge = Animator.StringToHash( "dodge" );
        an_beganFalling = Animator.StringToHash( "beganFalling" );
        an_yVelocity = Animator.StringToHash( "yVelocity" );
        an_menuState = Animator.StringToHash( "MenuState" );

        //Get Main Camera
        m_cameraMainTransform = Camera.main.transform;

        //Start in menu state
        if ( Settings.g_inMenu )
        {
            m_animator.SetBool( an_menuState, true );
        }
        else
        {
            m_animator.SetBool( an_menuState, false );
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
    * Description: Update everything
    **************************************************************************************/
    void Update()
    {
        if ( !Settings.g_inMenu && m_stoodUp )
        {
            //Velocity gets modified in jump check and what not, but i need to know what it was last time
            float yVelocityLastFrame = m_playerVelocity.y;

            MovePlayer();
            JumpCheck();
            GroundedCheck();
            LandCheck( yVelocityLastFrame );
            DodgeCheck();

            m_animator.SetFloat( an_yVelocity, m_playerVelocity.y );
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetPlayerInput
    * Parameters: n/a
    * Return: Vector3
    *
    * Author: Charlie Taylor
    *
    * Description: Return the players input values as a Vector3 direction
    **************************************************************************************/
    private Vector3 GetPlayerInput()
    {
        /* The player inputs are a vector2, but everywhere I used it,
         * it's in relation to a Vector 3, and the Y value of the Vector2 
         * was always put in the Z and it was kind of confusing, so now it returns
         * a Vector3, with X and Z being the correct values.*/

        //Y always being 0 may be a waste, but it made it far more readable for me
        return new Vector3( m_movementControl.action.ReadValue<Vector2>().x, 0f, m_movementControl.action.ReadValue<Vector2>().y );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetMoveDirection
    * Parameters: n/a
    * Return: Vector3
    *
    * Author: Charlie Taylor
    *
    * Description: Using player inputs, and the camera direction, get the direction that 
    *               the player wants to go. Used in here and MeleeController
    **************************************************************************************/
    public Vector3 GetMoveDirection()
    {
        //Snap inputs 
        InputSnapping();

        //Move Direction based on the camera angle
        Vector3 moveDirection = m_cameraMainTransform.forward * GetPlayerInput().z + m_cameraMainTransform.right * GetPlayerInput().x;
        //No move via Y. That's a jumping thing
        moveDirection.y = 0f;
        moveDirection.Normalize();

        //moveDirection is now the direction I want to go
        return moveDirection;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: InputSnapping
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Snap speed and animations for moving to be 0, 0.5 or 1
    **************************************************************************************/
    private void InputSnapping()
    {
        //Only update value if you can move
        if ( m_canMove && m_isGrounded )
        {
            //Get the absolute values of movement inputs (0-1) for use in a 1d Blend tree animations
            m_moveAmount = Mathf.Clamp01( Mathf.Abs( GetPlayerInput().x ) + Mathf.Abs( GetPlayerInput().z ) );

            //Get the absolute (0 to 1) values and set to the 3 levels of 0.0, 0.5 and 1.0
            // This is how most games do it, rather than moving and animating at the 0.75 mark
            // Dean: I've added a set for m_movementAnimState in each of these conditions to be
            // used by the ControlledFootstep function for preventing double footstep sounds
            if ( m_moveAmount >= 0.0f && m_moveAmount <= 0.05f )
            {
                m_moveAmount = 0.0f;
                m_movementAnimState = MovementAnim.Standing;
            }
            else if ( m_moveAmount > 0.05f && m_moveAmount < 0.55f )
            {
                m_moveAmount = 0.5f;
                m_movementAnimState = MovementAnim.Walking;
            }
            else if ( m_moveAmount >= 0.55f )
            {
                m_moveAmount = 1.0f;
                m_movementAnimState = MovementAnim.Running;
            }
            //Temp float for updating JUST the animator's move amount
            float animatorCurrentMovingSpeed = m_animator.GetFloat( an_movingSpeed );

            //If animator value not already 0 (Dampening down), and no input and but the animator value is NEARLY 0
            if ( animatorCurrentMovingSpeed != 0 && m_moveAmount <= 0.05f && animatorCurrentMovingSpeed <= 0.01f )
            {
                //set exactly to 0
                m_animator.SetFloat( an_movingSpeed, 0.0f );
            }
            else // if not, just do normal damp time stuff
            {
                //Animator variable set to move amount
                m_animator.SetFloat( an_movingSpeed, m_moveAmount, m_dampTime, Time.deltaTime );
            }
        }
        else //We don't want to update that value, so can't move or in air
        {
            m_animator.SetFloat( an_movingSpeed, 0.0f );
        }

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Activate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Activate player after they stand up
    *              Called by Sit to Stand animation 
    **************************************************************************************/
    private void Activate()
    {
        m_stoodUp = true;
        Settings.g_inMenu = false;
        m_animator.SetBool( an_menuState, false );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MovePlayer
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Move player based on inputs and camera direction
    **************************************************************************************/
    private void MovePlayer()
	{
        //If you're even touching Inputs at all
        if ( GetMoveDirection() != Vector3.zero )
        {
            if ( m_canMove )
            {
                //We are touching inputs AND we can move so, move
                //Multiply the move direction by  (speed * move amount) rather than just speed, and do it all before delta time
                m_controller.Move( GetMoveDirection() * ( m_playerSpeed * m_moveAmount ) * Time.deltaTime );
            }

            //Rotate player when moving, not when Idle
            if ( m_canRotate )
            {
                //Get the angle where your inputs are, relative to camera
                float targetAngle = Mathf.Atan2( GetPlayerInput().x, GetPlayerInput().z ) * Mathf.Rad2Deg + m_cameraMainTransform.eulerAngles.y;
                //Pass that into a quaternion
                Quaternion targetRotation = Quaternion.Euler( 0f, targetAngle, 0f );

                //Rotate to it using rotation speed
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * m_rotationSpeed );
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: JumpCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if Jump pressed
    **************************************************************************************/
    private void JumpCheck()
	{
        //Jumping
        if ( m_jumpControl.action.triggered && m_isGrounded )
        {
            m_animator.SetTrigger( an_jumped );
            m_playerVelocity.y = m_jumpForce;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GroundedCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if you are jsut on the ground
    **************************************************************************************/
    private void GroundedCheck()
	{
        //Raycast informations
        RaycastHit hit;

        Vector3 rayDirection = Vector3.down;
        //raise the raycast a little bit up 
        Vector3 rayOrigin = transform.position - rayDirection * 0.1f;


        //If the raycast hits something below you AND going DOWN/or just Flat walkin ( So it don't trigger on a jump)
        if ( Physics.Raycast( rayOrigin, rayDirection, out hit, m_raycastDistance, m_landableLayers )
            && ( m_playerVelocity.y <= 0f ) )
        {
            //WE ARE CLOSE ENOUGH TO BE GROUNDED
            //Just like, GO DOWN and hit ground and stop. setting transform doesn't work
            
            transform.position = hit.point;

            //m_controller.Move( Vector3.down );

            m_isGrounded = true;
            m_canFall = false;
            m_canDodge = true;

            m_playerVelocity.y = 0;
            m_animator.SetBool( an_inAir, false );
        }
        else // If raycast does not hit ground or velocity is not DOWN
        {
            m_canFall = true;
            m_isGrounded = false;
            //Can't dodge in air
            m_canDodge = false;
            m_animator.SetBool( an_inAir, true );
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LandCheck
    * Parameters: float yVelocityLastFrame
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if JUST landed
    **************************************************************************************/
    private void LandCheck( float yVelocityLastFrame )
	{
        //If "CAN'T" fall
        if ( !m_canFall )
        {
            //Velocity is 0
            m_playerVelocity.y = 0f;
        }
        //If you are falling, but not at terminal velocity, 
        else if ( m_playerVelocity.y > m_terminalVelocity && m_canFall )
        {
            //Accelerate
            m_playerVelocity.y += m_gravityValue * Time.deltaTime;

            //if the addition goes UNDER -20, set it to it, and now you'll never come back into this section
            if ( m_playerVelocity.y < m_terminalVelocity )
            {
                m_playerVelocity.y = m_terminalVelocity;
            }
        }

        //Seperated from above as we wanna call this even if 2nd statement above is called
        //And if you can fall, move that way.
        if ( m_canFall )
        {
            //Velocity is only used for falling and jumping
            m_controller.Move( m_playerVelocity * Time.deltaTime );
        }

        //You were stationary or going up last frame (You can go from Positvie to negative (skipping 0) in a single frame)
        if ( ( yVelocityLastFrame >= 0f && m_playerVelocity.y < 0f ) )
        {
            BeginFalling();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DodgeCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if we clicked dodge
    **************************************************************************************/
    private void DodgeCheck()
    {
        if ( m_dodgeControl.action.triggered && m_canDodge )
        {
            Dodge();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ControlledFootstep
    * Parameters: string walkingType
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Kind of a hacky function to prevent double footstep sounds. Using string for
    *              animation event usage.
    **************************************************************************************/
    private void ControlledFootstep( string movingType )
    {
        switch (movingType)
        {
            case "Walking":
            {
                if (m_movementAnimState == MovementAnim.Walking)
                {
                    m_soundHandler.PlayFootstepSFX();
                }
                break;
            }
            case "Running":
            {
                if (m_movementAnimState == MovementAnim.Running)
                {
                    m_soundHandler.PlayFootstepSFX();
                }
                break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: BeginFalling
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Handles anything for the BEGINING of a fall
    **************************************************************************************/
    private void BeginFalling() 
    { 
        m_animator.SetTrigger( an_beganFalling );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Dodge
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Dodge, rolling out of the way of an attack
    **************************************************************************************/
    private void Dodge()
	{
        //Trigger will play the animation
        m_animator.SetTrigger( an_dodge );
        //Lock controls 
        LoseControl();
        m_canMove = false;
        m_canRotate = false;
        m_canDodge = false;
        m_playerHealth.SetIFrames();

        m_meleeController.CollisionsEnd();
        //Current Position value
        Vector3 startPosition = transform.position;

        //Add the direction to where you are to get the vector
        Vector3 targetPosition;
        if ( GetPlayerInput() == Vector3.zero )
        {
            //Add the direction to where you are to get the vector
            targetPosition = transform.position + m_cameraMainTransform.forward * -1;
        }
        else
        {
            //Add the direction to where you are to get the vector
            targetPosition = transform.position + GetMoveDirection();
        }
        //Get target angle in degrees
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        transform.rotation = Quaternion.Euler( new Vector3( 0f, targetAngle, 0f ) );
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetGrounded
    * Parameters: n/a
    * Return: bool
    *
    * Author: Charlie Taylor
    *
    * Description: Return if the player is grounded
    **************************************************************************************/
    public bool GetGrounded()
	{
        return m_isGrounded;
	}


    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetDodge
    * Parameters: n/a
    * Return: n/a
    * 
    * Author: Charlie Taylor
    *
    * Description: Regain control of the player after a dodge (Called in animation events)
    **************************************************************************************/
    private void ResetDodge()
	{
        //Just calls regain control (so just a dupe, but easier for readability in the animation events)
        RegainControl();
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LoseControl
    * Parameters: n/a
    * Return: n/a
    * 
    * Author: Charlie Taylor
    *
    * Description: Make the player lose control of the character, for various reasons
    **************************************************************************************/
    public void LoseControl()
    {
        m_canRotate = false;
        m_canMove = false;
        m_canDodge = false;
        m_canFall = false;
        m_meleeController.SetCanAttack( false );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Regain
    * Parameters: n/a
    * Return: n/a
    * 
    * Author: Charlie Taylor
    *
    * Description: Make the player regain control, as long as not menu locked
    *              See SetMenuLock()
    **************************************************************************************/
    public void RegainControl()
	{
        if ( !m_menuLock )
        {
            m_canRotate = true;
            m_canMove = true;
            m_canDodge = true;
            m_canFall = true;
            m_meleeController.SetCanAttack( true );
        }
    }


    //PUT ALL YOUR GETTERS HERE, LET'S GET CLEAN


    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetMenuLock
    * Parameters: bool locked
    * Return: n/a
    * 
    * Author: Charlie Taylor
    *
    * Description: Attacks have RegainControl built into anim events
    *              This means that if you were to start an attack, then pause, and click
    *              respawn, the respawn function would make the player lose control, but
    *              then the animation even will return it. This bool will stop that
    **************************************************************************************/
    public void SetMenuLock( bool locked )
    {
        m_menuLock = locked;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetCanDodge
    * Parameters: bool canDodge
    * Return: n/a
    * 
    * Author: Charlie Taylor
    *
    * Description: Sets the ability to dodge (Not making the player dodge, just letting them)
    **************************************************************************************/
    public void SetCanDodge( bool canDodge )
	{
        m_canDodge = canDodge;
	}


    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetSoundHandler
    * Parameters: n/a
    * Return: ref PlayerSoundHandler
    * 
    * Author: Dean Pearce
    *
    * Description: Returns a reference to the sound handler for the Player
    **************************************************************************************/
    public ref PlayerSoundHandler GetSoundHandler()
    {
        return ref m_soundHandler;
    }
}