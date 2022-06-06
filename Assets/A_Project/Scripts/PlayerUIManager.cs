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
    [Tooltip("��Ļ��ÿ����Լ�����")]
    public float BlackImagegTime = 2f;

    [Tooltip("��Ļ����֮������ÿ�ʼ��ʧ")]
    public float WaitTime = 3f;

    public GameObject GameInfo;
    public bool isShowInfo;

    public GameObject LOGO;

    public GameObject BGSC_LOGO;

    //���ڱ�������Э��ͬʱ���������
    bool canControl;

    public bool isPlayer1;
    //���������ת������ֵ

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

    //������Ļ�ķ���
    public void TurnOnBlackImage()
    {
        StartCoroutine(OnBlackImage());
    }

    //�رպ�Ļ�ķ����������ò���
    public void TrunOffBlackImage()
    {
        StartCoroutine(OffBlackImage());
    }

    //��Ļ�𽥲�͸����Э�̣���Ϊֱ��Ӱ��۸�����ʹ��ÿ֡ˢ��
    //Э�̽���֮����canControl����ΪTure
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
        //����Ϊ����ִ��
        OnBlackOnCompleteEvent?.Invoke();
        StartCoroutine(CloseBlackImageCountDown());
    }

    public void OnShowInfo()
    {
        isShowInfo = isShowInfo ? false : true;
        GameInfo.SetActive(isShowInfo);
        BGSC_LOGO.SetActive(!isShowInfo);
    }

    //��Ļ��͸����Э��
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
