using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    //用于开启第二块屏幕的空间如果你有的话
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Screen.SetResolution(Display.displays[1].renderingWidth, Display.displays[1].renderingHeight, true);
        }
    }
}
