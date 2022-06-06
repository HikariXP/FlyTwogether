using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEvent : MonoBehaviour
{
    public delegate void OnAnimationOverAction();
    public event OnAnimationOverAction OnCameraActionOverEvent;

    public void OnCameraActionOver(string test)
    {
        OnCameraActionOverEvent?.Invoke();
    }
}
