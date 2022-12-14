using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class SwapText : MonoBehaviour
{
    [SerializeField]
    string text1;
    [Header("this is where the icon goes")]
    [SerializeField]
    string text2;

    TextMeshPro TMPComponent;

    string interactionIcon;
    // Start is called before the first frame update
    void Start()
    {
        TMPComponent = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Settings.g_currentControlScheme == Settings.g_gamepadScheme )
		{
            interactionIcon = Settings.g_bIcon;

        }
        else
		{
            interactionIcon = Settings.g_eIcon;
		}

        TMPComponent.text = text1 + " " + interactionIcon + " " + text2;
    }
}
