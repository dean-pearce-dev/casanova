using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: PlayerDamageManager
*
* Author: Charlie Taylor
*
* Description: Child of CharacterDamageManager to affect just the Player object
**************************************************************************************/
public class PlayerDamageManager : CharacterDamageManager
{
    //Reference to the PlayerContrller script
    private PlayerController m_playerController;
    //Game Controller
    private GameObject m_gameController;

    //Can regen when not received damage for a bit
    private bool m_canRegen = false;

    [Header("Health Regen")]
    [SerializeField, Range( 5f, 10f ), Tooltip("How long without taking damage, until you start regening health")]
    private float m_regenTimerMax;
    //Actual timer
    private float m_regenCounter;

    [SerializeField, Range( 1f, 10f ), Tooltip( "How much Health per Second you regen" )]
    private float m_regenRate;
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Call base setup and then fill member vars with components and objects
    **************************************************************************************/
    protected override void Start()
    {
        base.Start();
        m_playerController = GetComponent<PlayerController>();
        m_gameController = GameObject.FindGameObjectWithTag( Settings.g_controllerTag );
    }







	private void Update()
	{
        //If you can't regen, but your health is not max
		if (!m_canRegen && (GetHealth() < GetMaxHealth() ))
        {
            Debug.Log( "You are hurt but we're waiting to regen" );
            m_regenCounter -= Time.deltaTime;
		}

        if ( !m_canRegen && m_regenCounter <= 0.0f )
        {
            Debug.Log( "You CAN now regen" );
            m_regenCounter = m_regenTimerMax;
            m_canRegen = true;
		}

        if ( GetHealth() >= GetMaxHealth() )
		{
            SetHealth( GetMaxHealth() );
            m_canRegen = false;
		}

        if ( m_canRegen )
		{
            Debug.Log( "You are now regenning" );
            SetHealth( GetHealth() + (m_regenRate * Time.deltaTime) );

            UpdateHealthBar();
        }
	}

	/**************************************************************************************
    * Type: Function
    * 
    * Name: TakeDamage
    * Parameters: Transform othersTransform, float damage
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Take damage from enemies, adding the player exclusive LoseControl()
    **************************************************************************************/
	public override void TakeDamage( Transform othersTransform, float damage )
    {
        //Only run if alive
        if ( GetAlive() )
        {
            //Only run if not invulnerable
            if ( !GetInvulnerable() )
            {
                m_regenCounter = m_regenTimerMax;
                m_canRegen = false;
                //Player does have staggerable functionality in base, but that is only utilised by enemies for now
                m_playerController.LoseControl();

                //Only bother calling if this is true as it checks the same stuff
                base.TakeDamage( othersTransform, damage );
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Respawn
    * Parameters: Transform spawnPos
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Respawn the player at the set spawn Position fed in by Respawn Manager
    **************************************************************************************/
    public void Respawn( Transform spawnPos )
	{
        //Move
        //This works as in the project settings > Physics, I enabled sync transforms
        transform.position = spawnPos.position;

        //Reset values
        SetAlive( true );
        ResetHealth();
        SetInvulnerable( false );
        UpdateHealthBar();
        //Only place this trigger is used.

        ResetAnimTriggers();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetAnimTriggers
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Reset animation triggers for player if any were active
    **************************************************************************************/
    private void ResetAnimTriggers()
	{
        GetAnimator().SetTrigger( "Respawn" );
        
        GetAnimator().ResetTrigger( "light" );
        GetAnimator().ResetTrigger( "heavy" );
        GetAnimator().ResetTrigger( "attacked" );
        GetAnimator().ResetTrigger( "dodge" );
        GetAnimator().ResetTrigger( GetStaggerAnimTrigger() );
        GetAnimator().ResetTrigger( GetDieAnimTrigger() );
        GetAnimator().SetBool( "comboActive", false );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayDamageSFX
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Play the player's damage SFX
    **************************************************************************************/
    protected override void PlayDamageSFX()
    {
        m_playerController.GetSoundHandler().PlayDamageSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayDeathSFX
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Play the player's death SFX
    **************************************************************************************/
    protected override void PlayDeathSFX()
    {
        m_playerController.GetSoundHandler().PlayDeathSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Die
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Player exclusive Die override.
    *              Call Game Manager's die, to prep for respawn
    **************************************************************************************/
    protected override void Die()
    {
        base.Die();
        m_playerController.LoseControl();

        m_gameController.GetComponent<GameManager>().Die();
        
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetInvulnerable
    * Parameters: float timer
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Wait a set time and then reset invulnerable
    **************************************************************************************/
    //Override exclusive to player
    public override IEnumerator ResetInvulnerable( float timer )
	{
        yield return new WaitForSeconds( timer );
        SetInvulnerable( false );
        
        m_playerController.RegainControl();

    }
}
