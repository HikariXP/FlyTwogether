using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeelingManager : MonoBehaviour
{
    //��FeelingManager����϶��㹻���Ը�����ҵ�ʱ��
    public delegate void canRebornAction();
    public event canRebornAction canRebornEvent;

    public delegate void CooperationChangeAction();
    public event CooperationChangeAction CooperationChangeEvent;

    public static FeelingManager Instance { get; private set; }

    //����֮��������ߵ�ƽ����
    public bool RandomMode;

    [Header("��Ҷ���")]
    public GameObject Player1;
    public GameObject Player2;
    public data1 dataOrigin;

    [Header("��϶�")]
    public int cooperation = 10;
    public int maxCooperation = 10;
    public int minCooperation = 0;

    [Tooltip("��ƽ��ֵ��ֵ������ʱ����������ʼ����")]
    public float CalmLine = 50;

    public float rebornValue;

    public bool isReborning;
    

    //����ˢ��Ƶ��
    public float ReFreshFrequency = 10f;
    //��϶�ˢ��Ƶ��
    private WaitForSeconds waitForReFresh;


    [Header("Player1")]

    [Tooltip("���1��ƽ����")]
    public int player1_Calm = 0;
    [Tooltip("���1��רע��")]
    public int player1_Force = 0;

    [Header("Player2")]

    [Tooltip("���2��ƽ����")]
    public int player2_Calm = 0;
    [Tooltip("���2��רע��")]
    public int player2_Force = 0;
    [Header("���г���")]
    [Tooltip("ʹ�����MaxCount��ƽ��ֵ�Ĳ�ֵ")]
    //������󳤶�,Ҳ����ÿ����������ٴεĲ�ֵƽ��ֵ
    public int MaxCount = 10;

    public bool canCalculate = false;

    //���У��洢���ʮ�ε�רעֵ��ֵ��
    public Queue<float> queueD_Value = new Queue<float>();

    public void Awake()
    {
        Instance = this;
        waitForReFresh = new WaitForSeconds(ReFreshFrequency);
    }

    public void Start()
    {

        //����Ϸ��ʼ���¼����ж���
        GameManager.Instance.GameStartEvent += OnStartCalculate;
        cooperation = 10;
        CooperationChangeEvent?.Invoke();
        StartCoroutine(FeelingUpdate());
        StartCoroutine(FeelingUpdateLate());

    }

    public void OnStartRandomMode()
    {
        RandomMode = RandomMode ? false : true;
        StartCoroutine(RandomMachine());
    }

    public void Update()
    {
        //����״̬���ȡ�����״̬�򲻶�ȡdata1
        if (!RandomMode)
        {
            RefreshData();
        }
        
    }

    //��ȡ����
    public void RefreshData()
    {
        player1_Calm = dataOrigin.Meditation1;
        player1_Force = dataOrigin.Attention1;
        player2_Calm = dataOrigin.Meditation2;
        player2_Force = dataOrigin.Attention2;
    }





    public void OnStartCalculate()
    {
        canCalculate = true;
    }

    IEnumerator RandomMachine()
    {
        while(RandomMode)
        {
            player1_Calm = Random.Range(0, 10);
            player2_Calm = Random.Range(0, 10);
            player1_Force = Random.Range(0, 10);
            player2_Force = Random.Range(0, 10);
            yield return EnumeratorList.waitForOneSecond;
        }
    }

    //ö����һ��ˢ��һ��״̬��������ƽ��ֵ��ֵ�����ڶ���֮��
    IEnumerator FeelingUpdate()
    {
        while (true)
        {
            if (queueD_Value.Count > MaxCount)
            {
                queueD_Value.Dequeue();
            }
            int temp = Mathf.Abs(player1_Force - player2_Force);
            queueD_Value.Enqueue(temp);
            
            if (isReborning)
            {
                rebornValue += (player1_Calm + player2_Calm) > CalmLine ? 0.1f : -0.1f;
                rebornValue = rebornValue < 0 ? 0 : rebornValue >= 1 ? 1 : rebornValue;
            }
            else rebornValue = 0;
            if (rebornValue >= 1)
            {
                canRebornEvent?.Invoke();
            }

            yield return EnumeratorList.waitForOneSecond;
        }
    }

    //��϶ȼ���
    IEnumerator FeelingUpdateLate()
    {
        //GameManager.instance.ChangeVirtualBirdAlpha(cooperation);
        while (true)
        {
            if (canCalculate)
            {
                float temp = 0f;
                foreach (int i in queueD_Value)
                {
                    temp += i;
                }
                temp /= queueD_Value.Count;
                cooperation = (int)temp;
                cooperation = cooperation > 10 ? 10 : cooperation < 0 ? 0 : cooperation;
                CooperationChangeEvent?.Invoke();
                if (cooperation >= 10)
                {
                    //ÿ�μ�����������϶�֮�������϶����Ը���һ����ң���ᷢ�����Ը���Ĺ㲥����GM����
                    //canRebornEvent?.Invoke();
                }
                GameManager.Instance.DifficultChange();
                //GameManager.instance.RebornPlayer();
                ////�޸�����϶�֮����GM�޸��������������
                //GameManager.instance.ChangeVirtualBirdAlpha(cooperation);
            }
            yield return waitForReFresh;
        }
    }

    //��GM�ĵ��帴����Ϊ���ж��ģ�һ��������������϶ȳͷ�
    public void OnRebornCountDown()        
    {
        StartCoroutine(FeelingUpdateStop());
    }

    //������Ѻ�һ���ӵ����Ѷ������ͷ�
    public IEnumerator FeelingUpdateStop()
    {
        GameManager.Instance.RebornNerf = true;
        yield return EnumeratorList.waitForOneMinute;
        GameManager.Instance.RebornNerf = false;
        canCalculate = true;
    }
}
