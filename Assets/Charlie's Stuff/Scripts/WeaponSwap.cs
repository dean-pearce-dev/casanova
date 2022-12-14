using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: WeaponSwap
*
* Author: Charlie Taylor
*
* Description: Class on the player, to manage weapon swap
**************************************************************************************/
public class WeaponSwap : MonoBehaviour
{
    [SerializeField, Tooltip("The Table leg weapon in the player's hand")]
    private GameObject m_tableLeg;
    [SerializeField, Tooltip( "The Sword weapon in the player's hand" )]

    private GameObject m_swordInHand;
    private GameObject m_worldSword;

    private Rigidbody m_tableLegRB;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DropTableLeg
    * Parameters: GameObject worldSword 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Drop the table leg object in hand
    **************************************************************************************/
    public void DropTableLeg( GameObject worldSword )
    {
        m_worldSword = worldSword;
        GetComponent<PlayerController>().LoseControl();


        //Deactivate Trigger box used for sword collisions
        m_tableLeg.GetComponents<Collider>()[ 0 ].enabled = false;
        //Activate Collider work as the physics collider
        m_tableLeg.GetComponents<Collider>()[ 1 ].enabled = true;

        //Only really need to populate this here rather than start
        m_tableLegRB = m_tableLeg.GetComponent<Rigidbody>();
        //Set up Rigid body to be right
        m_tableLegRB.isKinematic = false;
        m_tableLegRB.mass = 1;
        m_tableLegRB.useGravity = true;
        m_tableLegRB.constraints = RigidbodyConstraints.None;

        //Call despawn, which calls a coroutine
        m_tableLeg.GetComponent<DespawnTableLeg>().Despawn();
        //Remove from player heirachy
        m_tableLeg.transform.parent = null;
        //Equip the sword after a second
        StartCoroutine(EquipSword());
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EquipSword
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Activate the sword in the player's hand
    **************************************************************************************/
    private IEnumerator EquipSword()
	{
        yield return new WaitForSeconds( 1f );
        m_swordInHand.SetActive( true );
        m_worldSword.SetActive( false );
        //GetComponent<PlayerController>().RegainControl();

        GetComponent<MeleeController>().SwapWeapon( m_swordInHand ) ;
        
    }

}
