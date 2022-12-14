using System.Collections;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: RespawnManager
*
* Author: Charlie Taylor
*
* Description: Manage player Respawns
**************************************************************************************/
public class RespawnManager : MonoBehaviour
{
    [SerializeField, Tooltip( "The Respawn Point Parent for the player.\nTHE AMOUNT AND THE ORDER ARE IMPORTANT.\nThey must be ordered in the heirachy and have the same amount of entries as the \"Room\" enum! " )]
    private Transform m_respawnMarkersParent;
    private Transform[] m_respawnPoints;

    //The room you should respawn in if you were to die right now
    private Room m_currentRespawnPoint;

    //Reference to the player GO (To get all the different scripts easier)
    private GameObject m_player;

    //Game Manager Script
    private GameManager m_gameManager;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Populate member variables with the relevant objects
    **************************************************************************************/
    private void Start()
    {
        //Use global string for getting the player
        m_player = GameObject.FindGameObjectWithTag( Settings.g_playerTag );
        //Game Manager is on the same object as this script
        m_gameManager = gameObject.GetComponent<GameManager>();

        m_respawnPoints = new Transform[m_respawnMarkersParent.childCount];
        for (int i = 0; i < m_respawnPoints.Length; i++ )
		{
            m_respawnPoints[i] = m_respawnMarkersParent.GetChild(i).transform;
		}

}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetRespawnPoint
    * Parameters: Room newRespawnPoint
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Send in a room value to populate the Respawn point for the player
    **************************************************************************************/
    public void SetRespawnPoint( Room newRespawnPoint )
    {
        m_currentRespawnPoint = newRespawnPoint;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetRespawnPoint
    * Parameters: n/a
    * Return: Room
    *
    * Author: Charlie Taylor
    *
    * Description: Return the current Respawn Point room
    **************************************************************************************/
    public Room GetRespawnPoint()
	{
        return m_currentRespawnPoint;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Respawn
    * Parameters: float delay
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Wait a set time, then using the current respawn point Room value, cast 
    *              to an int, and use that int to get the GO of the respawn point from the 
    *              array of respawn point
    *              THIS IS WHY THE ORDER AND TOTAL NUMBER OF RESPAWN POINTS IS IMPORTANT
    **************************************************************************************/
    public IEnumerator Respawn( float delay )
    {
        yield return new WaitForSeconds( delay );

        //Pass the transform of the Respawn point game object to the player damage manager, based on the current respawn point room
        m_player.GetComponent<PlayerDamageManager>().Respawn( m_respawnPoints[ (int)m_currentRespawnPoint ].transform );

        //Game Manager manages most of the respawn stuff, due to it having access to so much stuff
        m_gameManager.ActuallyRespawn();
    }
}

