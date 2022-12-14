using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject m_straightTrackPrefab;
    [SerializeField]
    private GameObject m_curvedTrackPrefab;
    [SerializeField]
    private GameObject m_lookTargetPrefab;

    [SerializeField]
    [Tooltip("The speed of the camera.")]
    private float m_flySpeed = 0.5f;
    private float m_interpolateAmount = 0f;
    private int m_currentTrackNum = 0;
    private Transform m_camLookTarget;
    private Transform[] m_dollyPoint;
    private Transform m_lookTargets;
    private bool m_isPlaying = false;
    private GameObject m_cam;
    [SerializeField]
    private bool m_fadeBetweenTransitions = false;
    private Image m_blackScreen;
    private bool m_isFading = false;
    [SerializeField]
    private float m_fadeSpeed = 0.5f;
    [SerializeField]
    [Tooltip("The point along the route to begin fading the screen. 0 is the start, and 1 is the end.")]
    private float m_pointToStartFading = 0.8f;

    void Start()
    {
        m_dollyPoint = new Transform[transform.GetChild(0).childCount];
        m_cam = Camera.main.gameObject;
        m_lookTargets = GameObject.Find("LookTargets").transform;
        m_camLookTarget = m_lookTargets.GetChild(0);
        m_blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        for (int i = 0; i < m_dollyPoint.Length; i++)
        {
            m_dollyPoint[i] = transform.GetChild(0).GetChild(i);
        }

        StartCutscene();

        //To make it fade in from black
        if (m_fadeBetweenTransitions)
        {
            m_blackScreen.color = Color.black;
            m_blackScreen.canvasRenderer.SetAlpha(1.0f);
            FadeFromBlack();
        }

        CinematicEventManager.CameraPauseEvent += PauseUnpauseCutscene;
        CinematicEventManager.CameraResetEvent += ResetCutscene;
    }

    void Update()
    {
        if (m_isPlaying)
        {
            MoveCamera();
        }
    }

    private void MoveCamera()
    {
        m_interpolateAmount += (m_flySpeed * Time.deltaTime);
        m_cam.transform.position = TrackPositions(m_interpolateAmount, m_currentTrackNum);
        m_cam.transform.LookAt(m_camLookTarget);


        if (m_fadeBetweenTransitions && m_interpolateAmount > m_pointToStartFading && !m_isFading)
        {
            m_isFading = true;
            FadeToBlack();
        }
        if (m_interpolateAmount >= 1.0f && m_currentTrackNum < m_dollyPoint.Length - 1)
        {
            m_currentTrackNum = (m_currentTrackNum + 1) % m_dollyPoint.Length;
            m_interpolateAmount = 0f;
            m_camLookTarget = m_lookTargets.GetChild(m_currentTrackNum);

            if (m_fadeBetweenTransitions)
            {
                m_isFading = false;
                FadeFromBlack();
            }

            CinematicEventManager.StartCameraTrackEvent(m_currentTrackNum);
        }
        if (m_interpolateAmount >= 1.0f && m_currentTrackNum == m_dollyPoint.Length - 1)
        {
            m_currentTrackNum = 0;
            m_interpolateAmount = 0f;
            m_camLookTarget = m_lookTargets.GetChild(m_currentTrackNum);

            if (m_fadeBetweenTransitions)
            {
                m_isFading = false;
                FadeFromBlack();
            }

            CinematicEventManager.StartCameraTrackEvent(0);
            CinematicEventManager.StartCameraTrackEndEvent();
        }
    }

    public void StartCutscene()
    {
        m_isPlaying = true;
        m_currentTrackNum = 0;
        m_interpolateAmount = 0.0f;
    }

    public void PauseUnpauseCutscene()
    {
        if (m_isPlaying)
        {
            m_isPlaying = false;
        }
        else
        {
            m_isPlaying = true;
        }
    }

    public void ResetCutscene()
    {
        m_isPlaying = true;
        m_currentTrackNum = 0;
        m_interpolateAmount = 0.0f;
    }

    public void FadeToBlack()
    {
        m_blackScreen.color = Color.black;
        m_blackScreen.canvasRenderer.SetAlpha(0.0f);
        m_blackScreen.CrossFadeAlpha(1.0f, m_fadeSpeed, false);
    }

    private void FadeFromBlack()
    {
        m_blackScreen.color = Color.black;
        m_blackScreen.canvasRenderer.SetAlpha(1.0f);
        m_blackScreen.CrossFadeAlpha(0.0f, m_fadeSpeed, false);
    }

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

    private Vector3 QuadraticLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB = Vector3.Lerp(pointA, pointB, interpolateValue);
        Vector3 pointBC = Vector3.Lerp(pointB, pointC, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB, pointBC, interpolateValue);

        return lerpedVector;
    }

    private Vector3 CubicLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB_BC = QuadraticLerp(pointA, pointB, pointC, interpolateValue);
        Vector3 pointBC_CD = QuadraticLerp(pointB, pointC, pointD, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB_BC, pointBC_CD, interpolateValue);

        return lerpedVector;
    }

    public GameObject GetStraightTrackPrefab()
    {
        return m_straightTrackPrefab;
    }

    public GameObject GetCurvedTrackPrefab()
    {
        return m_curvedTrackPrefab;
    }

    public GameObject GetLookTargetPrefab()
    {
        return m_lookTargetPrefab;
    }

    private void OnDestroy()
    {
        CinematicEventManager.CameraPauseEvent -= PauseUnpauseCutscene;
        CinematicEventManager.CameraResetEvent -= ResetCutscene;
    }
}
