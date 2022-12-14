using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: DespawnTableLeg
*
* Author: Charlie Taylor
*
* Description: After a set time, despawn the table leg weapon when dropped
**************************************************************************************/

public class DespawnTableLeg : MonoBehaviour
{
    //How long until weapon despawns
    private float despawnTime = 10f;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Despawn
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by WeaponSwap(), despawn after a set time
    **************************************************************************************/
    public void Despawn()
	{
        StartCoroutine(TimerDespawn());
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TimerDespawn
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Disable table leg object
    **************************************************************************************/
    private IEnumerator TimerDespawn()
	{
        yield return new WaitForSeconds( despawnTime );
        gameObject.SetActive( false ); 
    }
}
