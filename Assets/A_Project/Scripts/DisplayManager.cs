using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    //���ڿ����ڶ�����Ļ�Ŀռ�������еĻ�
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Screen.SetResolution(Display.displays[1].renderingWidth, Display.displays[1].renderingHeight, true);
        }
    }
}
