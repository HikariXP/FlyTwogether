using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public delegate void OnBlackOnCompleteAction();
    public event OnBlackOnCompleteAction OnBlackOnCompleteEvent;

    public delegate void OnBlackOffCompleteAction();
    public event OnBlackOffCompleteAction OnBlackOffCompleteEvent;

    public GameObject RebornSlider;

    public GameObject PlayingUI;

    public float largestWidth = 400;
    public Image CalmValue1;
    public Image CalmValue2;

    public Image ForceValue1;
    public Image ForceValue2;

    public Image BlackImage;
    [Tooltip("黑幕多久开完以及关完")]
    public float BlackImagegTime = 2f;

    [Tooltip("黑幕开启之后间隔多久开始消失")]
    public float WaitTime = 3f;

    public GameObject GameInfo;
    public bool isShowInfo;

    public GameObject LOGO;

    public GameObject BGSC_LOGO;

    //用于避免两个协程同时开启的情况
    bool canControl;

    public bool isPlayer1;
    //如果不是则反转两个数值

    public void Start()
    {
        GameManager.Instance.PlayerAllDeadEvent += TurnOnBlackImage;
    }

    public void Update()
    {
        if (GameManager.Instance.isReborning)
        {
            RebornSlider.SetActive(true);
            RebornSlider.GetComponent<Slider>().value = FeelingManager.Instance.rebornValue;
        }
        else
        {
            RebornSlider.SetActive(false);
        }
        if (isPlayer1)
        {
            CalmValue1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player1_Calm / 10f));
            CalmValue2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player2_Calm / 10f));
            ForceValue1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player1_Force / 10f));
            ForceValue2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player1_Force / 10f));
        }
        else
        {
            CalmValue1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player2_Calm / 10f));
            CalmValue2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player1_Calm / 10f));

            ForceValue1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player2_Force / 10f));
            ForceValue2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largestWidth * (FeelingManager.Instance.player1_Force / 10f));
        }
    }

    //开启黑幕的方法
    public void TurnOnBlackImage()
    {
        StartCoroutine(OnBlackImage());
    }

    //关闭黑幕的方法，可能用不上
    public void TrunOffBlackImage()
    {
        StartCoroutine(OffBlackImage());
    }

    //黑幕逐渐不透明的协程，因为直接影响观感所以使用每帧刷新
    //协程结束之后会把canControl设置为Ture
    IEnumerator OnBlackImage()
    {
        canControl = false;
        float temp = 0;
        while (BlackImage.color.a < 1)
        {
            temp += (Time.deltaTime / BlackImagegTime);
            BlackImage.color = new Color(0, 0, 0, temp);
            yield return EnumeratorList.waitForEndOfFrame;
        }
        //若不为空则执行
        OnBlackOnCompleteEvent?.Invoke();
        StartCoroutine(CloseBlackImageCountDown());
    }

    public void OnShowInfo()
    {
        isShowInfo = isShowInfo ? false : true;
        GameInfo.SetActive(isShowInfo);
        BGSC_LOGO.SetActive(!isShowInfo);
    }

    //黑幕逐渐透明的协程
    IEnumerator OffBlackImage()
    {
        canControl = false;
        float temp = 1;
        while (BlackImage.color.a > 0)
        {
            temp -= (Time.deltaTime / BlackImagegTime);
            BlackImage.color = new Color(0, 0, 0, temp);
            yield return EnumeratorList.waitForEndOfFrame;
        }
        OnBlackOffCompleteEvent?.Invoke();
        //canControl = true;
    }

    IEnumerator CloseBlackImageCountDown()
    {
        canControl = false;
        float temp = WaitTime;
        while (temp > 0)
        {
            temp -= 1;
            yield return EnumeratorList.waitForOneSecond;
        }
        StartCoroutine(OffBlackImage());
    }

    public void OnLOGODisappear()
    {
        StartCoroutine(LOGODisappear());
    }

    IEnumerator LOGODisappear()
    {
        float a = 1;
        Color color;
        while (a > 0)
        {
            color = LOGO.GetComponent<Image>().color;
            a -= Time.deltaTime*0.5f;
            color.a = a;
            
            LOGO.GetComponent<Image>().color = color;
            yield return EnumeratorList.waitForEndOfFrame;
        }
        
    }
}
