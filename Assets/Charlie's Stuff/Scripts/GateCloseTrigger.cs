using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: GateCloseTrigger
*
* Author: Charlie Taylor
*
* Description: For use on Gates colliders to close behind player
**************************************************************************************/
public class GateCloseTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The Parent Game Object for the Gate this is attached to")]
    private GameObject m_gateToClose;
	
	/**************************************************************************************
	* Type: Function
	* 
	* Name: OnTriggerEnter
	* Parameters: Collider
	* Return: n/a
	*
	* Author: Charlie Taylor
	*
	* Description: When the player enters the trigger box, close
	**************************************************************************************/
	private void OnTriggerEnter( Collider other )
	{
		if ( other.tag == "Player" )
		{
			//Close gate
			m_gateToClose.GetComponent<GateMover>().CloseGate();
		}
	}
}
