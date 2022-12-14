using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CinematicEventManager : MonoBehaviour
{
    public static event Action<int> CameraTrackEvent;
    public static event Action CameraTrackEndEvent;
    public static event Action CameraPauseEvent;
    public static event Action CameraResetEvent;

    public static void StartCameraTrackEvent( int nextTrack )
    {
        CameraTrackEvent?.Invoke(nextTrack);
    }

    public static void StartCameraTrackEndEvent()
    {
        CameraTrackEndEvent?.Invoke();
    }

    public static void StartCameraPauseEvent()
    {
        CameraPauseEvent?.Invoke();
    }

    public static void StartCameraResetEvent()
    {
        CameraResetEvent?.Invoke();
    }
}