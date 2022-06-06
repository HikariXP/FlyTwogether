using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeelingManager : MonoBehaviour
{
    //当FeelingManager的配合度足够可以复活玩家的时候，
    public delegate void canRebornAction();
    public event canRebornAction canRebornEvent;

    public delegate void CooperationChangeAction();
    public event CooperationChangeAction CooperationChangeEvent;

    public static FeelingManager Instance { get; private set; }

    //开启之后将随机两者的平静度
    public bool RandomMode;

    [Header("玩家对象")]
    public GameObject Player1;
    public GameObject Player2;
    public data1 dataOrigin;

    [Header("配合度")]
    public int cooperation = 10;
    public int maxCooperation = 10;
    public int minCooperation = 0;

    [Tooltip("当平静值和值大于它时，复活条开始增加")]
    public float CalmLine = 50;

    public float rebornValue;

    public bool isReborning;
    

    //数据刷新频率
    public float ReFreshFrequency = 10f;
    //配合度刷新频率
    private WaitForSeconds waitForReFresh;


    [Header("Player1")]

    [Tooltip("玩家1的平静度")]
    public int player1_Calm = 0;
    [Tooltip("玩家1的专注度")]
    public int player1_Force = 0;

    [Header("Player2")]

    [Tooltip("玩家2的平静度")]
    public int player2_Calm = 0;
    [Tooltip("玩家2的专注度")]
    public int player2_Force = 0;
    [Header("队列长度")]
    [Tooltip("使用最近MaxCount次平静值的差值")]
    //队列最大长度,也就是每次算最近多少次的差值平均值
    public int MaxCount = 10;

    public bool canCalculate = false;

    //队列，存储最近十次的专注值差值。
    public Queue<float> queueD_Value = new Queue<float>();

    public void Awake()
    {
        Instance = this;
        waitForReFresh = new WaitForSeconds(ReFreshFrequency);
    }

    public void Start()
    {

        //对游戏开始的事件进行订阅
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
        //正常状态则读取，随机状态则不读取data1
        if (!RandomMode)
        {
            RefreshData();
        }
        
    }

    //读取数据
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

    //枚举器一秒刷新一次状态。仅计算平静值差值并存在队列之中
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

    //配合度计算
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
                    //每次计算完总体配合度之后，如果配合度足以复活一名玩家，则会发出可以复活的广播，由GM订阅
                    //canRebornEvent?.Invoke();
                }
                GameManager.Instance.DifficultChange();
                //GameManager.instance.RebornPlayer();
                ////修改完配合度之后让GM修改虚拟鸟的清晰度
                //GameManager.instance.ChangeVirtualBirdAlpha(cooperation);
            }
            yield return waitForReFresh;
        }
    }

    //对GM的单体复活行为进行订阅，一旦复活单体则进入配合度惩罚
    public void OnRebornCountDown()        
    {
        StartCoroutine(FeelingUpdateStop());
    }

    //复活队友后一分钟的困难度锁定惩罚
    public IEnumerator FeelingUpdateStop()
    {
        GameManager.Instance.RebornNerf = true;
        yield return EnumeratorList.waitForOneMinute;
        GameManager.Instance.RebornNerf = false;
        canCalculate = true;
    }
}
