#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CinematicCamera))]
public class CamTrackEditor : Editor
{
    private CamRouteVisualizer m_routeVisualizer;
    public override void OnInspectorGUI()
    {
        m_routeVisualizer = GameObject.Find("DollyTracks").GetComponent<CamRouteVisualizer>();

        DrawDefaultInspector();

        CinematicCamera myTarget = (CinematicCamera)target;

        if (GUILayout.Button("Create Straight Track"))
        {
            GameObject newTrack = Instantiate(myTarget.GetStraightTrackPrefab(), myTarget.transform.position, Quaternion.identity, myTarget.transform.GetChild(0));
            GameObject newLookTarget = Instantiate(myTarget.GetLookTargetPrefab(), myTarget.transform.position, Quaternion.identity, myTarget.transform.GetChild(1));

            newTrack.name = "Track" + myTarget.transform.GetChild(0).transform.childCount;
            newLookTarget.name = "Target" + myTarget.transform.GetChild(1).transform.childCount;

            m_routeVisualizer.RouteLanePointSetup();
        }

        if (GUILayout.Button("Create Curved Track"))
        {
            GameObject newTrack = Instantiate(myTarget.GetCurvedTrackPrefab(), myTarget.transform.position, Quaternion.identity, myTarget.transform.GetChild(0));
            GameObject newLookTarget = Instantiate(myTarget.GetLookTargetPrefab(), myTarget.transform.position, Quaternion.identity, myTarget.transform.GetChild(1));

            newTrack.name = "Track" + myTarget.transform.GetChild(0).transform.childCount;
            newLookTarget.name = "Target" + myTarget.transform.GetChild(1).transform.childCount;

            m_routeVisualizer.RouteLanePointSetup();
        }
    }
}

#endif
