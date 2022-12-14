using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: Billboarding
*
* Author: Charlie Taylor
*
* Description: Make 2D objects face the screen at all times. Used on Candle Flames and
*              Enemy Health
**************************************************************************************/
public class Billboarding : MonoBehaviour
{
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: every frame, rotate to face camera
    **************************************************************************************/
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
