using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: SwordCollisionManager
*
* Author: Charlie Taylor
*         Dean Pearce
*
* Description: Manages collisions for the weapon (Sword and table leg)
**************************************************************************************/
public class SwordCollisionManager : MonoBehaviour
{
    //The damage that the wepaon will do at any given time
    private float m_damage;

    [SerializeField, Range(0.0f, 2.0f), Tooltip("Animation events will provide a base damage, but should this weapon do more or less than the base?")]
    private float m_damageMultiplier = 1.0f;

    // Dean Note: Adding a reference to the sound handler in here for collision SFX
    private PlayerSoundHandler m_soundHandler;
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: fill sound handler object reference
    **************************************************************************************/
    private void Start()
    {
        // Dean Note: I know this is a hideous line, but I can't think of a better way at this moment
        m_soundHandler = GameObject.FindGameObjectWithTag(Settings.g_playerTag).GetComponent<PlayerController>().GetSoundHandler();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Dean Pearce
    *         Charlie Taylor
    *
    * Description: Sword Collision with Enemy
    **************************************************************************************/
    private void OnTriggerEnter( Collider other )
    {
        //We've collided, but is it with an enemy?
        if ( other.gameObject.tag == "Enemy" )
        {
            //Yes? Okay, get the enemy
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            //Then hurt them
            if (enemy.GetState() != AIState.Dead && enemy.GetState() != AIState.Sleeping )
			{
                enemy.GetHealthManager().TakeDamage( transform, m_damage * m_damageMultiplier );

                m_soundHandler.PlayNormalCollisionSFX();
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetDamage
    * Parameters: float damage
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set the damage from very begining of attack, so this damage will be
    *              used in collision with enemy and sent to them
    **************************************************************************************/
    public void SetDamage( float damage )
	{
        m_damage = damage;
        //Swap to make 1 hit for speed
        //m_damage = 300f;
	}
}
