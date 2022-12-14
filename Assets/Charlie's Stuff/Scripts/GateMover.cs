using System.Collections;
using UnityEngine;
/**************************************************************************************
* Type: Class
* 
* Name: GateMover
*
* Author: Charlie Taylor
*
* Description: Managuing opening and closing room gates
**************************************************************************************/
public class GateMover : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField, Range( 0.0f, 5.0f ), Tooltip("How long it takes to Open the gate")]
    float m_gateOpenTime = 3f;
    [SerializeField, Range( 0.0f, 5.0f ), Tooltip( "How long it takes to Close the gate" )]
    float m_gateCloseTime = 1f;
    [SerializeField, Tooltip("What Y value should the gate rise to")]
    private float m_openYTarget = 5.0f;

    [Header("Relevant Game Objects")]
    [SerializeField, Tooltip( "Gate Trigger object, should be a child of this Game Object" )]
    GameObject m_gateTrigger;

    [SerializeField, Tooltip( "Actual Gate Object" )]
    GameObject m_visualGate;

    //Y Value when closed
    private float m_closedY = 0.0f;

    //Bool, based on if the gate closer script should be active
    private bool m_gateTriggerActive;

    private ObjectSoundHandler m_soundHandler;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Populate gate trigger checker, to stop gates that shouldn't have active
    *              triggers
    **************************************************************************************/
    private void Start()
	{
        m_gateTriggerActive = m_gateTrigger.activeSelf;

        m_soundHandler = GetComponent<ObjectSoundHandler>();
    }
	/**************************************************************************************
    * Type: Function
    * 
    * Name: ResetGate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Used when Respawning, reset gate to be open and reactivate trigger
    **************************************************************************************/
	public void ResetGate()
	{
        OpenGate();
        if ( m_gateTriggerActive )
        {
            m_gateTrigger.SetActive( true );
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OpenGate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Open the gate 
    **************************************************************************************/
    public void OpenGate()
	{
        /* There are 2 Colliders involved in gate opening. 
         * There is the box collider attached to the physical gate that moves with it
         * and the same sized one attached to the PARENT that is IMMOVABLE
         * The moving one is active as it opens, so the player can get through when it is open enough to clear the players head
         * BUT the immovable one is active when it is CLOSING, so the player can't sneak through as it closes, as they'd get stuck
         */

        //Enable the MOVING collider
        m_visualGate.GetComponent<BoxCollider>().enabled = true;
        //Disable IMMOVABLE collider, we want to be able to get through just as it's tall enough
        GetComponent<Collider>().enabled = false;

        //Begin actually moving gate over time
        StartCoroutine( MoveGate( m_openYTarget, m_gateOpenTime ) );

        m_soundHandler.PlayGateOpenSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CloseGate
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Close the gate
    **************************************************************************************/
    public void CloseGate()
    {
        //Activate immovable collider, stops sneaking through
        GetComponent<BoxCollider>().enabled = true;
        //Disable moving collider, don't need both active at once for this
        m_visualGate.GetComponent<Collider>().enabled = false;

        //Move the gate
        StartCoroutine( MoveGate( m_closedY, m_gateCloseTime) );

        m_soundHandler.PlayGateCloseSFX();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MoveGate
    * Parameters: float yTarget, float overTime
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Mover the gate to a designated Y position (local) over a set time (sec)
    **************************************************************************************/
    private IEnumerator MoveGate( float yTarget, float overTime )
	{
        //Starting local Y pos
        Vector3 currentPosition = m_visualGate.transform.position;
        
        //Create a temp Vector3 so we can modify the Y easily
        Vector3 targetPosition = transform.position;
        targetPosition.y += yTarget;

        //Local time elapsed value.
        float timeElapsed = 0.0f;

        //While time isn't done
        while ( timeElapsed < overTime )
		{
            //Set pos to target y, based on time/max time
            currentPosition.y = Mathf.Lerp( m_visualGate.transform.position.y, targetPosition.y, timeElapsed / overTime );
            m_visualGate.transform.position = currentPosition;

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        //At the end of time, set to exact
        m_visualGate.transform.position = targetPosition;

    }
}
