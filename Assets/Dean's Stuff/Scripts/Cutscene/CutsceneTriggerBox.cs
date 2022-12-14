using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: CutsceneTriggerBox
*
* Author: Dean Pearce
*
* Description: Class for attaching to a trigger box that on enter will trigger a cutscene.
**************************************************************************************/
public class CutsceneTriggerBox : MonoBehaviour
{
    [SerializeField]
    private GameObject m_cutsceneObj;
    private UIManager m_uiManager;

    private void Start()
    {
        m_uiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIManager>();
    }

    private void OnTriggerEnter( Collider other )
    {
        m_cutsceneObj.SetActive(true);
        m_uiManager.CutsceneHandover(m_cutsceneObj.GetComponent<Cutscene>());
        gameObject.SetActive(false);
    }
}
