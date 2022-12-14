using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: OpenDoor
*
* Author: Charlie Taylor
*
* Description: A child of Interactable, this is for opening the cell door, but likely
*              could be reutilised for other doors if they were added
**************************************************************************************/
public class OpenDoor : Interactable
{
    [Header("Door Settings")]
    [SerializeField,Range(0.0f, 270.0f), Tooltip("The Y angle that the door should rotate to")]
    private float m_targetAngle;
    [SerializeField, Range(0.0f, 2.0f), Tooltip("How long to rotate")]
    private float m_rotationTime;
    
    //Sound for this door opening
    private ObjectSoundHandler m_soundHandler;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Populate sound handler object with component attached
    **************************************************************************************/
    void Start()
    {
        m_soundHandler = GetComponent<ObjectSoundHandler>();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Interact
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Do the base interact to affect the player, and rotate the door
    **************************************************************************************/
    public override void Interact()
	{
        base.Interact();
        StartCoroutine(OpenCellDoor());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OpenCellDoor
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Rotate door over time
    **************************************************************************************/
    private IEnumerator OpenCellDoor()
	{
        yield return new WaitForSeconds( 1.0f );

        Quaternion targetRotation = Quaternion.Euler( 0f, m_targetAngle, 0f );

        //Play door noise
        m_soundHandler.PlayCellDoorOpenSFX();

        for ( var elapsedTime = 0f; elapsedTime < 1; elapsedTime += Time.deltaTime / m_rotationTime )
		{
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime * m_rotationTime );
            yield return null;
        }
    }
}
