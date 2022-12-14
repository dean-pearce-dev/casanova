using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: 
*
* Author: Charlie Taylor
*
* Description: 
**************************************************************************************/
public class GateManager : MonoBehaviour
{
    [Header("Gate Objects")]
    [SerializeField, Tooltip( "Gate the exits the cell corridor and enters the armory" )]
    GateMover m_cellsExit;
    [SerializeField, Tooltip( "Gate the exits the armoury and enters the corridor toward the Guard Room" )]
    GateMover m_armoryExit;
    [SerializeField, Tooltip( "Gate the enters the guard room" )]
    GateMover m_guardRoomEntrance;
    [SerializeField, Tooltip( "Gate the enters the Arena room" )]
    GateMover m_guardRoomExit;
    [SerializeField, Tooltip( "Arena Exit Gate / Final exit gate" )]
    GateMover m_arenaExitGate;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetGate
    * Parameters: Room roomExit
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Reset a gate to open based on the respawn point
    **************************************************************************************/
    public void ResetGate(Room roomExit)
	{
        switch ( roomExit )
        {
            //Nothing to do with Cell Respawn

            case Room.Hall:
                m_cellsExit.ResetGate();
                break;

            //We respawn inside the armoury, trapped
            
            case Room.Armory2:
                m_armoryExit.ResetGate();
                m_guardRoomEntrance.ResetGate();
                break;

            case Room.GuardRoom:
                m_guardRoomExit.ResetGate();
                break;

           //We respawn inside the Arena, trapped
        }
	}
    //I know this is awful, but it just needed to be made to work and time was not my friend
    public void OpenCellHallExitGate()
    {
        m_cellsExit.OpenGate();
    }
    public void OpenArmoryExitGate()
    {
        m_armoryExit.OpenGate();
        m_guardRoomEntrance.OpenGate();
    }
    public void OpenGuardRoomExitGate()
	{
        m_guardRoomExit.OpenGate();

    }
    public void OpenArenaExitGate()
    {
        m_arenaExitGate.OpenGate();
    }
}
