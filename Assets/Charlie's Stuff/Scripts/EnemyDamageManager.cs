using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EnemyDamageManager
*
* Author: Charlie Taylor
*
* Description: Child class of CharacterDamageManager for Enemies
**************************************************************************************/
public class EnemyDamageManager : CharacterDamageManager
{
    //Reference to the enemy AI script on this enemy
    private EnemyAI m_enemyAI;
    //List of all the materials on this enemy, so as to play the despawn shader
    private List<Material> m_materialList = new List<Material>();
    //Spawn Manager, so as to reset enemy back into the spawn pool
    private SpawnManager m_spawnManager;

    //strings for referencing values in the despawn shader
    private string m_shaderStartTime = "_FadeStartTime";
    private string m_shaderForceVisible = "_ForceVisible";

    [Header( "Enemy Excluive" )]
    [SerializeField, Tooltip( "The Enemy UI Game Object, to be shown or hidden based on death or combat" )]
    GameObject m_enemyUI;

    [SerializeField, Range(1.0f, 20.0f), Tooltip( "How long this enemy can not get staggered after being damaged" )]
    private float m_staggerFreeTime = 8.0f;

    [SerializeField, Range( 2.0f, 4.0f ), Tooltip( "How long after death blow does the enemy dissolve" )]
    private float m_dissolveTime = 3.0f;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Awake
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Added awake for the purpose of re-ordering the ShowUI(false) call. Being
    *              called on start messed with health bars not showing up on newly instantiated
    *              enemies during the wave phase.
    **************************************************************************************/
    private void Awake()
    {
        //Disable UI at spawn
        ShowUI(false);
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
    * Description: Set up materials and reference objects
    **************************************************************************************/
    protected override void Start()
    {
        base.Start();

        m_spawnManager = GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<SpawnManager>();

        m_enemyAI = GetComponent<EnemyAI>();

        //Make a list of materials, and while we're there, set start time and stuff
        int iteration = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach ( Renderer child in renderers )
        {
            m_materialList.Add( renderers[ iteration ].material );
            m_materialList[ iteration ] = renderers[ iteration ].material;
            m_materialList[ iteration ].SetFloat( m_shaderStartTime, float.MaxValue );
            m_materialList[ iteration ].SetInt( m_shaderForceVisible, 0 );
            iteration++;
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
    * Description: Called from SwordCollisionManager, damage the enemy
    **************************************************************************************/
    public override void TakeDamage( Transform othersTransform, float damage )
    {
        //Still have to check invulnerable
        if ( !GetInvulnerable() )
		{
			//Check base stuff after as that is where it checks for death, where as above, overwrites with get hurt
			base.TakeDamage( othersTransform, damage );

            //Death and stuff will have happened in the base

			//If you are alive 
			if ( GetAlive() )
			{
                if ( GetStaggerable() )
                {
                    //stop them being able to hurt you, incase they were just active
                    m_enemyAI.DisableCollision();
                    //Reset triggers so as to not mess with the any state > animation stuff in enemy animator
                    m_enemyAI.ResetLastUsedAnimTrigger();
                    //Set to not stagger
                    SetStaggerable( false );
                    //Reset it in set time
                    ResetStagerable( m_staggerFreeTime );
                    //Set last used trigger in ai to be stagger
                    m_enemyAI.SetLastUsedAnimTrigger( GetStaggerAnimTrigger() );
                    //Temporarily stop using the nav mesh
                    m_enemyAI.StopNavMesh();
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetStagerable
    * Parameters: float time
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: after set time, reset staggerable to be true
    **************************************************************************************/
    private IEnumerator ResetStagerable( float time )
	{
        yield return new WaitForSeconds( time );
        SetStaggerable( true );
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
    * Description: Play the "Get Hurt" SFX
    **************************************************************************************/
    protected override void PlayDamageSFX()
    {
        m_enemyAI.GetSoundHandler().PlayDamageSFX();
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
    * Description: Play the "Die" SFX
    **************************************************************************************/
    protected override void PlayDeathSFX()
    {
        m_enemyAI.GetSoundHandler().PlayDeathSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetAllColliders
    * Parameters: bool reset
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Turn on or off all colliders on the enemy, even the ones on the skeleton used 
    *              for cloth physics, so as to not be collidable when dead
    **************************************************************************************/
    private void SetAllColliders(bool reset)
    {
        //This is the main one attached to the parent, a trigger for collisions with sword
        GetComponent<Collider>().enabled = reset;

        //Make an array of all colliders attached to children...
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach ( Collider collider in allColliders )
        {
            //... And turn them off
            collider.enabled = false;
        }
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
    * Description: Change AI State info, and dissolve the enemy after they land on the floor
    **************************************************************************************/
    protected override void Die()
    {
        m_enemyAI.SetAIState( AIState.Dead );
        //Unregister attacker so as to inform AI Manager the group this enemy is attached to is now -= 1
        m_enemyAI.UnregisterAttacker();

        //Turn off colliders
        SetAllColliders( false );

        //After set time, dissolve
        StartCoroutine( DissolveEnemy( m_dissolveTime ) );

        ShowUI( false );
        //Do the base die at the end
        base.Die();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DissolveEnemy
    * Parameters: float time
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Loop through the materials and set the shader to dissolve, making the
    *              enemy dissapear
    **************************************************************************************/
    private IEnumerator DissolveEnemy( float time )
	{
        //Wait a short delay
        yield return new WaitForSeconds( time );

        //Loop throught the materials and set them to start
        foreach ( Material mat in m_materialList )
        {
            mat.SetFloat( m_shaderStartTime, Time.time );
        }

        m_enemyAI.GetSoundHandler().PlayDeathFizzSFX();

        StartCoroutine(ResetEnemy());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetShader
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Loop through the materials and set the shader to be visible
    **************************************************************************************/
    private void ResetShader()
    {
        foreach ( Material mat in m_materialList )
        {
            mat.SetFloat( m_shaderStartTime, float.MaxValue );
            //Need to force them visible again which is weird but ah well
            mat.SetInt( m_shaderForceVisible, 0 );
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetEnemy
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: After a set time, disable the enemy and prep for respawning in the pool
    **************************************************************************************/
    public IEnumerator ResetEnemy()
	{
        yield return new WaitForSeconds( 1f );
        //Get em gone
        gameObject.SetActive( false );
        //Prep for respawn, so
        //Set colliders back on
        SetAllColliders( true );
        //Health back to max
        ResetHealth(); 
        //Shader back on
        ResetShader();
        //Return to available spawn group
        m_spawnManager.AddToAvailable(gameObject.GetComponent<EnemyAI>());
        //Make them not invulnerable
        SetInvulnerable(false);
        //They are alive, just not active
        SetAlive(true);
        //Base collider enabled
        gameObject.GetComponent<Collider>().enabled = true;
        //Health bar refreshed to max
        UpdateHealthBar();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ShowUI
    * Parameters: bool show 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Show or hide the Enemy's UI
    **************************************************************************************/
    public void ShowUI( bool show )
	{
        m_enemyUI.SetActive( show );

    }

}
