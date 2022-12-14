using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EndGame
*
* Author: Charlie Taylor
*
* Description: End the Game
**************************************************************************************/
public class EndGame : MonoBehaviour
{
    [Header( "Cursor Stuff" )]
    [SerializeField]
    private GameObject m_cursorController;
    [SerializeField]
    private GameObject m_cursorSprite;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnTriggerEnter
    * Parameters: Collider other
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: When the playe enters this object, end the game
    **************************************************************************************/
    private void OnTriggerEnter( Collider other )
	{
		if (other.tag == Settings.g_playerTag )
		{
			//Lock player from moving
			other.gameObject.GetComponent<PlayerController>().SetMenuLock( true );
			other.gameObject.GetComponent<PlayerController>().LoseControl();
			//Call complete game script
			CompleteGame();
		}
	}

	/**************************************************************************************
    * Type: Function
    * 
    * Name: CompleteGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Display "you win" UI and set game settings for ending game
    **************************************************************************************/
	private void CompleteGame()
    {
        Settings.g_inMenu = true;
        Settings.g_canPause = false;
        Cursor.visible = false;
        m_cursorController.SetActive( true );
        m_cursorSprite.SetActive( true );
        GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<UIManager>().CompleteGame();
	}
}
