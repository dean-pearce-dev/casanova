using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: CharacterDamageManager
*
* Author: Charlie Taylor
*
* Description: Parent class for character damage management for player and enemies
**************************************************************************************/
public class CharacterDamageManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Max health of this Character")]
    private float m_maxHealth = 100.0f;
    //Health at any given time
    private float m_currentHealth;

    [SerializeField, Tooltip("How long after being hurt until is this character invulnerable to damage for")]
    private float m_invulnerableTime = 1f;
    private bool m_invulnerable = false;

    //The Animator Component of this character
    private Animator m_animator;

    [Header( "Required Game Objects" )]
    [SerializeField, Tooltip("The Fill of the health bar for this character")]
    private Image m_healthBarFill;

    //Triggers for animator
    private int an_getHitTrigger;
    private int an_deathTrigger;

    //Alive or not
    private bool m_alive = true;
    //Can be staggered or not
    private bool m_staggerable = true;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set up variables for health for characters
    **************************************************************************************/
    protected virtual void Start()
    {
        //Set health to the max health
        m_currentHealth = m_maxHealth;
        m_animator = GetComponent<Animator>();
        //Instead of using strings everywhere for animater parameters, this is safer
        an_getHitTrigger = Animator.StringToHash( "TakeHit" );
        an_deathTrigger = Animator.StringToHash( "Death" );

        //Make health bar what it should be (Sometimes it would default to half fill)
        UpdateHealthBar();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UpdateHealthBar
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Update the health bar with based on the health and max health (Percentage)
    **************************************************************************************/
    protected void UpdateHealthBar()
    {
        m_healthBarFill.fillAmount = GetHealth() / m_maxHealth;
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
    * Description: Take Damage from an opponent
    **************************************************************************************/
    public virtual void TakeDamage(Transform othersTransform, float damage )
    {
        //Nothing to be done if the character is currently on I-Frames
        if( !m_invulnerable )
        {
            //We're not invulnerable so, take damage
            m_currentHealth -= damage;
            //And now temporartily invulnerable
            m_invulnerable = true;
            //Update Health bar
            UpdateHealthBar();

            //If the damage done has killed you
            if ( m_currentHealth <= 0.0f ) // Die
            {
                Die();
            }
            else // Just get hurt
			{
                //If we can be staggered, rotate to face attacker and stumble
                if ( m_staggerable)
                {
                    //Set rotation to face who attacked this character
                    transform.rotation = Quaternion.Lerp( transform.rotation, RotateToFaceAttacker( othersTransform ), 1f );
                    m_animator.SetTrigger( an_getHitTrigger );
                }
                //Reset invulnerable after set time (We only do if not dead to stop dead enemies being not invulnerable)
                StartCoroutine( ResetInvulnerable( m_invulnerableTime ) );

                PlayDamageSFX();
            }

        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RotateToFaceAttacker
    * Parameters: Transform othersTransform
    * Return: Quaternion
    *
    * Author: Charlie Taylor
    *
    * Description: Based on the transform of the attacker, rotate towards them and return
    *              target angle to face
    **************************************************************************************/
    private Quaternion RotateToFaceAttacker( Transform othersTransform )
    {
        //We want to rotate to face the oponent and THEN stumble backwards, so get the rotation we should be at

        Vector3 startPosition = transform.position;
        //... to....
        Vector3 targetPosition = othersTransform.position;

        //This is now the angle (between -180 and 180) between origin and attacker
        float targetAngle = Mathf.Rad2Deg * ( Mathf.Atan2( targetPosition.x - startPosition.x, targetPosition.z - startPosition.z ) );

        return Quaternion.Euler( 0f, targetAngle, 0f );
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
    * Description: 
    **************************************************************************************/
    protected virtual void PlayDamageSFX()
    {}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayDeathSFX
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: 
    **************************************************************************************/
    protected virtual void PlayDeathSFX()
    {}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Die
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Kill this character
    **************************************************************************************/
    protected virtual void Die()
    {
        m_currentHealth = 0.0f;
        m_alive = false;
        m_animator.SetTrigger( an_deathTrigger );
        PlayDeathSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetIFrames
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set invulnerable and start timer to reset it. Seperate from SetInvulnerable
    *              so as to also call the coroutine, as SetInvulnerable won't always want
    *              to reset it (So not really a setter)
    **************************************************************************************/
    public void SetIFrames()
    {
        m_invulnerable = true;
        StartCoroutine( ResetInvulnerable( m_invulnerableTime ) );

    }

    //Getters/Setters
    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetStaggerAnimTrigger
    * Parameters: n/a
    * Return: int
    *
    * Author: Charlie Taylor
    *
    * Description: Returns the Stagger trigger trigger value. In use in children classes
    **************************************************************************************/
    protected int GetStaggerAnimTrigger()
    {
        return an_getHitTrigger;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetDieAnimTrigger
    * Parameters: n/a
    * Return: int
    *
    * Author: Charlie Taylor
    *
    * Description: Returns the Die trigger trigger value. In use in children classes
    **************************************************************************************/
    protected int GetDieAnimTrigger()
    {
        return an_deathTrigger;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetAnimator
    * Parameters: n/a
    * Return: Animator
    *
    * Author: Charlie Taylor
    *
    * Description: Get animator for this game object
    **************************************************************************************/
    protected Animator GetAnimator()
    {
        return m_animator;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetAlive
    * Parameters: bool alive
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set the value for alive member variable from children scripts
    **************************************************************************************/
    protected void SetAlive( bool alive )
	{
        m_alive = alive;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetAlive
    * Parameters: n/a
    * Return: bool
    *
    * Author: Charlie Taylor
    *
    * Description: Get the value for alive member variable
    **************************************************************************************/
    protected bool GetAlive()
	{
        return m_alive;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetHealth
    * Parameters: n/a
    * Return: float
    *
    * Author: Charlie Taylor
    *
    * Description: Return Health value
    **************************************************************************************/
    public float GetHealth()
	{
        return m_currentHealth;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetMaxHealth
    * Parameters: n/a
    * Return: float
    *
    * Author: Charlie Taylor
    *
    * Description: Return Max Health value
    **************************************************************************************/
    protected float GetMaxHealth()
	{
        return m_maxHealth;
	}


    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetHealth
    * Parameters: float health
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set current health
    **************************************************************************************/
    public void SetHealth( float health )
    {
        m_currentHealth = health;
        if ( m_currentHealth > m_maxHealth )
		{
            m_currentHealth = m_maxHealth;
		}
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetStaggerable
    * Parameters: bool shouldStagger
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set the staggerable bool, making this character able, or unable to stagger
    **************************************************************************************/
    public void SetStaggerable( bool shouldStagger )
    {
        m_staggerable = shouldStagger;
    }
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetStaggerable
    * Parameters: n/a
    * Return: bool
    *
    * Author: Charlie Taylor
    *
    * Description: Get the staggerable bool, making this character able, or unable to stagger
    **************************************************************************************/
    protected bool GetStaggerable()
	{
        return m_staggerable;

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetInvulnerable
    * Parameters: bool invulnerable
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set invulnerable 
    **************************************************************************************/
    public void SetInvulnerable( bool invulnerable )
    {
        m_invulnerable = invulnerable;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetInvulnerable
    * Parameters: n/a
    * Return: bool
    *
    * Author: Charlie Taylor
    *
    * Description: Get invulnerable 
    **************************************************************************************/
    public bool GetInvulnerable()
    {
        return m_invulnerable;
    }

    //Resetters
    /**************************************************************************************
    * Type: ResetInvulnerable
    * 
    * Name: SetHealth
    * Parameters: float timer
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: After a set time, reset invulnerable
    **************************************************************************************/
    public virtual IEnumerator ResetInvulnerable( float timer )
    {
        yield return new WaitForSeconds( timer );
        m_invulnerable = false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetHealth
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Reset health to the max value (Used on respawns, both player and enemy)
    **************************************************************************************/
    protected void ResetHealth()
    {
        m_currentHealth = m_maxHealth;
    }

}
