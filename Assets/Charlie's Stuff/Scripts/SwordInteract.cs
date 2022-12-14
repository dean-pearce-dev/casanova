using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: SwordInteract
*
* Author: Charlie Taylor
*
* Description: Interactable child, for swapping swords
**************************************************************************************/
public class SwordInteract : Interactable
{
	[SerializeField, Tooltip("The Light that highlights the sword")]
	GameObject m_light;

	[SerializeField, Tooltip("Number of the Group of enemies that should wake up when you pick up the sword")]
	private int m_enemyGroupToAlert;
	[SerializeField, Tooltip("What Room should be \"Entered\" (To set up respawns) when the player picks up sword")]
	Room m_roomToActivate;

	/**************************************************************************************
    * Type: Function
    * 
    * Name: Interact
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Override for base interact. Alerts the enemies, spawns the next group,
    *			   and swaps the swords
    **************************************************************************************/
	public override void Interact()
	{
		base.Interact();

		//Alert and spawn right groups
		EventManager.StartAlertEnemiesEvent( m_enemyGroupToAlert );
		EventManager.StartSpawnEnemiesEvent( m_enemyGroupToAlert + 1 );
		//"Enter" a room
		GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<GameManager>().EnterRoom( m_roomToActivate );
		//A gross global bool so as to not mess up respawning if you die with the sword
		Settings.g_pickedUpSword = true;
		//Turn off light
		m_light.SetActive(false);
		//Drop table leg on player
		GetPlayer().GetComponent<WeaponSwap>().DropTableLeg(gameObject);
	}
}
