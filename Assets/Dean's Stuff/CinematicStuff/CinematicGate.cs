using System.Collections;
using UnityEngine;

public class CinematicGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField, Range(0.0f, 5.0f), Tooltip("How long it takes to Open the gate")]
    float m_gateOpenTime = 3f;
    [SerializeField, Tooltip("What Y value should the gate rise to")]
    private float m_openYTarget = 5.0f;

    [SerializeField]
    private float m_distanceFromCamToOpen = 5.0f;
    [SerializeField]
    private int m_camTrackToOpenOn = 0;

    private bool m_isOpen = false;

    private int m_currentCamTrack = 0;

    private Vector3 m_defaultPos;

    private void Start()
    {
        CinematicEventManager.CameraTrackEndEvent += CloseGate;
        CinematicEventManager.CameraTrackEvent += SetCamTrack;

        m_defaultPos = transform.position;
    }

    private void Update()
    {
        CamDistanceCheck();
    }

    public void OpenGate()
    {
        StartCoroutine(MoveGate(m_openYTarget, m_gateOpenTime));
        m_isOpen = true;
    }

    public void CloseGate()
    {
        transform.position = m_defaultPos;
        m_isOpen = false;
    }

    private void CamDistanceCheck()
    {
        if (DistanceSqrCheck(Camera.main.gameObject, m_distanceFromCamToOpen) && !m_isOpen && m_currentCamTrack == m_camTrackToOpenOn)
        {
            OpenGate();
        }
    }

    private void SetCamTrack( int trackNum )
    {
        m_currentCamTrack = trackNum;
    }

    private IEnumerator MoveGate( float yTarget, float overTime )
    {
        //Starting local Y pos
        Vector3 currentPosition = transform.position;

        //Create a temp Vector3 so we can modify the Y easily
        Vector3 targetPosition = transform.position;
        targetPosition.y += yTarget;

        //Local time elapsed value.
        float timeElapsed = 0.0f;

        //While time isn't done
        while (timeElapsed < overTime)
        {
            //Set pos to target y, based on time/max time
            currentPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, timeElapsed / overTime);
            transform.position = currentPosition;

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        //At the end of time, set to exact
        transform.position = targetPosition;
    }

    private bool DistanceSqrCheck( GameObject targetToCheck, float distanceToCheck )
    {
        bool isInRange = false;

        // Getting the distance between this and the target
        Vector3 distance = transform.position - targetToCheck.transform.position;

        // Checking if sqrMagnitude is less than the distance squared
        if (distance.sqrMagnitude <= distanceToCheck * distanceToCheck)
        {
            isInRange = true;
        }

        return isInRange;
    }

    private void OnDestroy()
    {
        CinematicEventManager.CameraTrackEndEvent -= CloseGate;
        CinematicEventManager.CameraTrackEvent -= SetCamTrack;
    }
}