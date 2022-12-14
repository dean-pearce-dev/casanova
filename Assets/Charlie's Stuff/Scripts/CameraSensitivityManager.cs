using UnityEngine;
using Cinemachine;

/**************************************************************************************
* Type: Class
* 
* Name: CameraSensitivityManager
*
* Author: Charlie Taylor
*
* Description: Affect the sensitivity for the camera
**************************************************************************************/
public class CameraSensitivityManager : MonoBehaviour
{
    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetSensitivity
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Confim Settings in options menu
    **************************************************************************************/
    public void SetSensitivity()
    {
        //Temp Vector 2 based on the Option Manager's sensitivity
        Vector2 newSensitivity = GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<OptionsManager>().GetSensitivity();
        //set global sensitivity to vector values
        Settings.g_currentXCameraSensitiviy = newSensitivity.x;
        Settings.g_currentYCameraSensitiviy = newSensitivity.y;
        //Set actual camera values to sensitivity
        GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed = Settings.g_currentYCameraSensitiviy;
        GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = Settings.g_currentXCameraSensitiviy;


        GameObject.FindGameObjectWithTag( Settings.g_controllerTag ).GetComponent<UIManager>().SwapMenus("Main");
    }
}
