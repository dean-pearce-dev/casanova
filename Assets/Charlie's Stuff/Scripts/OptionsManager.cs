using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: OptionsManager
*
* Author: Charlie Taylor
*
* Description: Manage Options in game
**************************************************************************************/
public class OptionsManager : MonoBehaviour
{
	[SerializeField, Tooltip("Slider for Sensitivity")]
	private Slider m_cameraSensitivitySlider;

	/**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set slider to the global settings for sensitivity
    **************************************************************************************/
	private void Start()
	{

		m_cameraSensitivitySlider.minValue = Settings.g_minCameraSensitiviy;
		m_cameraSensitivitySlider.maxValue = Settings.g_maxCameraSensitiviy;

		RefreshSliders();
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RefreshSliders
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Updates the value of the slider to match the sensitivity
    **************************************************************************************/
    public void RefreshSliders()
	{
		m_cameraSensitivitySlider.value = Settings.g_currentXCameraSensitiviy;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetSensitivity
    * Parameters: n/a
    * Return: Vector2
    *
    * Author: Charlie Taylor
    *
    * Description: Return the sensitivity you want as a Vector2
    **************************************************************************************/
    public Vector2 GetSensitivity()
    {
        //We just use X as X is 100-300, and Y is 1-3, so we just use the bigger one, and then set Y to X/100
        return new Vector2(m_cameraSensitivitySlider.value, m_cameraSensitivitySlider.value/100);
	}

}
