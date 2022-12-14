using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: Cutscene
*
* Author: Dean Pearce
*
* Description: Class for running a short cutscene which also sets up wave logic. 
*              Some of the logic has been re-used & adapted from my module 56 project.
**************************************************************************************/
public class Cutscene : MonoBehaviour
{
    private UIManager m_uiManager;
    [SerializeField]
    private Camera m_cutsceneCam;
    private Camera m_mainCamera;
    [SerializeField]
    [Tooltip("The speed of the camera.")]
    private float m_flySpeed = 0.5f;
    private float m_interpolateAmount = 0f;
    private int m_currentTrackNum = 0;
    [SerializeField]
    private Transform m_camLookTarget;
    private Transform[] m_dollyPoint;
    private bool m_isPlaying = false;
    [SerializeField]
    private bool m_combatOnCutsceneEnd = false;

    private PlayerController m_player;

    void Start()
    {
        m_uiManager = GameObject.FindGameObjectWithTag(Settings.g_controllerTag).GetComponent<UIManager>();
        m_cutsceneCam.enabled = false;
        m_dollyPoint = new Transform[transform.GetChild(0).childCount];
        m_player = GameObject.FindGameObjectWithTag(Settings.g_playerTag).GetComponent<PlayerController>();

        for (int i = 0; i < m_dollyPoint.Length; i++)
        {
            m_dollyPoint[i] = transform.GetChild(0).GetChild(i);
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (m_isPlaying)
        {
            MoveCamera();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MoveCamera
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Moves the cutscene camera along the track positions.
    **************************************************************************************/
    private void MoveCamera()
    {
        m_interpolateAmount += (m_flySpeed * Time.deltaTime);
        m_cutsceneCam.transform.position = TrackPositions(m_interpolateAmount, m_currentTrackNum);
        m_cutsceneCam.transform.LookAt(m_camLookTarget);

        if (m_interpolateAmount >= 1.0f && m_currentTrackNum < m_dollyPoint.Length - 1)
        {
            m_currentTrackNum = (m_currentTrackNum + 1) % m_dollyPoint.Length;
            m_interpolateAmount = 0f;
        }
        if (m_interpolateAmount >= 1.0f && m_currentTrackNum == m_dollyPoint.Length - 1)
        {
            m_uiManager.ReturnFromCutscene(this);
            m_isPlaying = false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: StartCutscene
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Takes control away from the player and starts the cutscene
    *              by enabling the cutscene camera. Also sets up the the wave enemies.
    **************************************************************************************/
    public void StartCutscene()
    {
        EventManager.StartWaveSetupEvent();
        EventManager.StartWakeEnemiesEvent(4);

        m_mainCamera = Camera.main;
        m_mainCamera.enabled = false;
        m_cutsceneCam.enabled = true;
        m_isPlaying = true;
        m_currentTrackNum = 0;
        m_interpolateAmount = 0.0f;
        m_cutsceneCam.transform.position = TrackPositions(m_interpolateAmount, m_currentTrackNum);
        m_player.SetMenuLock(true);
        m_player.LoseControl();

        EventManager.StartCutsceneBeginEvent();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EndCutscene
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Hands control back to the player, and disables the cutscene camera. Also
    *              begins wave spawning.
    **************************************************************************************/
    public void EndCutscene()
    {
        m_cutsceneCam.enabled = false;
        m_mainCamera.enabled = true;
        m_isPlaying = false;
        m_player.SetMenuLock(false);
        m_player.RegainControl();

        EventManager.StartAlertEnemiesEvent(4);
        EventManager.StartSpawnWaveEvent();
        EventManager.StartCutsceneEndEvent(m_combatOnCutsceneEnd);

        //Charlie: Set the respawn point to be the arena now.
        GameObject.FindGameObjectWithTag(Settings.g_controllerTag).GetComponent<RespawnManager>().SetRespawnPoint( Room.Arena );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TrackPositions
    * Parameters: float interpolateValue, int trackNum
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Returns a position on the track based on interpolation.
    *              Adapted module 56 code.
    **************************************************************************************/
    private Vector3 TrackPositions( float interpolateValue, int trackNum )
    {
        Vector3 trackPos = new Vector3(0, 0, 0);

        if (m_dollyPoint[trackNum].transform.childCount == 4)
        {
            trackPos = CubicLerp(m_dollyPoint[trackNum].GetChild(0).position, 
                                 m_dollyPoint[trackNum].GetChild(1).position, 
                                 m_dollyPoint[trackNum].GetChild(2).position, 
                                 m_dollyPoint[trackNum].GetChild(3).position, interpolateValue);
        }
        else if (m_dollyPoint[trackNum].transform.childCount == 2)
        {
            trackPos = Vector3.Lerp(m_dollyPoint[trackNum].GetChild(0).position, 
                                    m_dollyPoint[trackNum].GetChild(1).position, interpolateValue);
        }

        return trackPos;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: QuadraticLerp
    * Parameters: Vector3 pointA, Vector3 pointB, Vector3 pointC, float interpolateValue
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Function for lerping between 3 points based on an interpolation value.
    *              Original logic from https://www.youtube.com/watch?v=7j_BNf9s0jM
    **************************************************************************************/
    private Vector3 QuadraticLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB = Vector3.Lerp(pointA, pointB, interpolateValue);
        Vector3 pointBC = Vector3.Lerp(pointB, pointC, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB, pointBC, interpolateValue);

        return lerpedVector;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CubicLerp
    * Parameters: Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float interpolateValue
    * Return: Vector3
    *
    * Author: Dean Pearce
    *
    * Description: Function for lerping between 4 points based on an interpolation value.
    *              Original logic from https://www.youtube.com/watch?v=7j_BNf9s0jM
    **************************************************************************************/
    private Vector3 CubicLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB_BC = QuadraticLerp(pointA, pointB, pointC, interpolateValue);
        Vector3 pointBC_CD = QuadraticLerp(pointB, pointC, pointD, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB_BC, pointBC_CD, interpolateValue);

        return lerpedVector;
    }
}
