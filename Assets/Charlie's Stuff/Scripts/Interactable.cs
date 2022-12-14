using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: Interactable
*
* Author: Charlie Taylor
*
* Description: Parent class for objects that are interactable
**************************************************************************************/
public class Interactable : MonoBehaviour
{
	//Player game object, as every interactable will need a player ref
	private GameObject m_player;

	/**************************************************************************************
	* Type: Function
	* 
	* Name: GetPlayer
	* Parameters: n/a
	* Return: GameObject
	*
	* Author: Charlie Taylor
	*
	* Description: Return the player game object for use in the sword Interact child 
	**************************************************************************************/
	protected GameObject GetPlayer()
	{
		return m_player;
	}

	/**************************************************************************************
	* Type: Function
	* 
	* Name: GetPlayer
	* Parameters: n/a
	* Return: GameObject
	*
	* Author: Charlie Taylor
	*
	* Description: Every interact will have the player lose control and do the interact animation
	**************************************************************************************/
	public virtual void Interact()
	{
		m_player = GameObject.FindGameObjectWithTag( Settings.g_playerTag );
		m_player.GetComponent<PlayerController>().LoseControl();
		//String, but only place, so a var would be unnecessary
		m_player.GetComponent<Animator>().SetTrigger( "Interact" );
	}
}
