using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/**************************************************************************************
* Type: Class
* 
* Name: InteractionManager
*
* Author: Charlie Taylor
*
* Description: Attached to Interaction zones, when the player is in the area to interact
*              with an interactable, this script takes effect
**************************************************************************************/
public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Elements")]
    [SerializeField, Tooltip("Input for Interaction")]
    private InputActionReference m_interact;

    [SerializeField, Tooltip("What object, with an Interactable script, do we want this zone to affect")]
    private GameObject m_thingToAffect;

    [SerializeField, Tooltip("The text object that is displayed when entering the trigger")]
    TextMeshPro m_text;

    //Bool for checking if this object is interactble, so pressing interact outside of the range doesn't do it
    private bool m_isInteractive = false;
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Disable text at launch
    **************************************************************************************/
    private void Start()
    {
        m_text.enabled = false;
    }
   
    /**************************************************************************************
    * Type: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Call interact clicked check
    **************************************************************************************/
    private void Update()
	{
        CheckInteractonClicked();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckInteractonClicked
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if the interact key has been pressed
    **************************************************************************************/
    private void CheckInteractonClicked()
	{
        //This has to have a member bool saying this one is interactable or not, otherwise it
        //would trigger all interactables at once. Need to make sure it's only THIS one
        if ( m_isInteractive )
        {
            if ( !Settings.g_inMenu )
            {
                //Pressed
                if ( m_interact.action.triggered )
                {
                    //Activate
                    ActivateInteractable();
                    //Parent of this game object is the whole interaction prefab which is now redundant
                    transform.parent.gameObject.SetActive( false );
                }
            }
            
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnTriggerEnter
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: If player enters trigger zone, show text, and allow interaction
    **************************************************************************************/
    private void OnTriggerEnter( Collider other )
    {
        //action only needed here
        m_interact.action.Enable();
        if ( other.tag == Settings.g_playerTag )
        {
            m_text.enabled = true;
            m_isInteractive = true;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnTriggerExit
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: If player exits trigger zone, hide text, and stop interaction
    **************************************************************************************/
    private void OnTriggerExit( Collider other )
    {
        /* WARNING
         * MAKE SURE NO TRIGGER BOXES ARE NEAR EACHOTHER 
         * FOR THERE TO BE A POSSIBILITY THAT YOU CAN 
         * EXIT A BOX WHILE STILL BEING INSIDE ANOTHER AS 
         * THE INPUT WILL BE DISABLED*/
        m_interact.action.Disable();

        if ( other.tag == Settings.g_playerTag )
        {
            m_isInteractive = false;
            m_text.enabled = false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ActivateInteractable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Call interactable object's interact functionality
    **************************************************************************************/
    public void ActivateInteractable()
	{
        m_thingToAffect.GetComponent<Interactable>().Interact();
    }
}
