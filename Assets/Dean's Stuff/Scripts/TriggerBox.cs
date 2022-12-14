using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    None,
    Spawn,
    Wake,
    Alert
}

/**************************************************************************************
* Type: Class
* 
* Name: TriggerBox
*
* Author: Dean Pearce
*
* Description: Class for attaching to a trigger box for the purpose of spawning, waking,
*              and alerting enemies via event manager calls.
**************************************************************************************/
public class TriggerBox : MonoBehaviour
{
    [SerializeField]
    private TriggerType m_triggerType = TriggerType.Spawn;
    [SerializeField]
    private int m_triggerGroup;
    [SerializeField]
    private bool m_spawnNextGroupOnAlert = false;

    [SerializeField]
    private Room m_roomEntered;

    private void OnTriggerEnter( Collider other )
    {
        // Using OnTriggerEnter to tell the event manager to send the relevant message to subscribers
        if (other.gameObject.tag == "Player")
        {
            switch (m_triggerType)
            {
                case TriggerType.Spawn:
                {
                    EventManager.StartSpawnEnemiesEvent(m_triggerGroup);
                    break;
                }
                case TriggerType.Wake:
                {
                    EventManager.StartWakeEnemiesEvent(m_triggerGroup);
                    break;
                }
                case TriggerType.Alert:
                {
                    if (m_spawnNextGroupOnAlert)
                    {
                        EventManager.StartSpawnEnemiesEvent(m_triggerGroup + 1);
                    }

                    EventManager.StartAlertEnemiesEvent(m_triggerGroup);
                    break;
                }
            }

            // Charlie: Update the game manager's current room
            GameObject.FindGameObjectWithTag(Settings.g_controllerTag).GetComponent<GameManager>().EnterRoom(m_roomEntered);
            

            gameObject.SetActive(false);
        }
    }

    public int GetGroup()
    {
        return m_triggerGroup;
    }
}
