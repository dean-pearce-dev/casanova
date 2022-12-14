using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.InputSystem;

public class QuitButtonHider : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_hideQuit;

    [SerializeField]
    private GameObject m_quitButton;

	// Start is called before the first frame update
	private void OnEnable()
	{
        m_hideQuit.action.Enable();
	}
    // Update is called once per frame
    void Update()
    {
        /*
        if ( m_hideQuit.action.triggered )
        {
            m_quitButton.SetActive( !m_quitButton.activeSelf );
            m_quitButton.transform.GetChild(0).GetComponent<Button>().interactable = m_quitButton.activeSelf;
		}*/
    }
}
