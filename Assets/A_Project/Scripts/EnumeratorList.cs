using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumeratorList : MonoBehaviour
{
    public static WaitForSeconds waitForOneMSecond = new WaitForSeconds(0.1f);
    public static WaitForSeconds waitForOneSecond = new WaitForSeconds(1f);
    public static WaitForSeconds waitForTwoSecond = new WaitForSeconds(2f);
    public static WaitForSeconds waitForFourSecond = new WaitForSeconds(4f);
    public static WaitForSeconds waitForSixSecond = new WaitForSeconds(6f);
    public static WaitForSeconds waitForEightSecond = new WaitForSeconds(8f);
    public static WaitForSeconds waitForTenSecond = new WaitForSeconds(10f);
    public static WaitForSeconds waitForOneMinute = new WaitForSeconds(60f);
    public static WaitForSecondsRealtime waitForOneSecondRealtime = new WaitForSecondsRealtime(1f);
    public static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
}
