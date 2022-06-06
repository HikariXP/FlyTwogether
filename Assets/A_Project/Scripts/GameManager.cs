using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerInput player1nput;
    public PlayerInput player2nput;
    //专门用于触发
    //玩家全体死亡的事件广播
    public delegate void PlayerAllDeadAction();
    public event PlayerAllDeadAction PlayerAllDeadEvent;
    //玩家全体复活的事件广播
    public delegate void PlayerAllRebornAction();
    public event PlayerAllRebornAction PlayerAllRebornEvent;

    //游戏开始的事件广播
    public delegate void GameStartAction();
    public event GameStartAction GameStartEvent;

    //GameManager的单例，提供一个全局的引用位置
    public static GameManager Instance { get; private set; }

    public Player player1;
    public Player player2;
    public GameObject Player1GO;
    public GameObject Player2GO;

    public GameObject StartPoint1;
    public GameObject StartPoint2;

    public GameObject GroundPoint1;
    public GameObject GroundPoint2;

    public GameObject Player1InteractiveGO;
    public GameObject Player2InteractiveGO;

    public GameObject BeforeGameStartCamera;
    public GameObject BeforeGameStartUI;

    public GameObject DeveloperInfo1;
    public GameObject DeveloperInfo2;

    [Header("最大/最小玩家操控响应速度")]
    public float maxControlSpeed = 5f;
    public float minControlSpeed = 1f;
    public float Level;


    //复活后启用，在开启期间内困难度最低为5级
    public bool RebornNerf;


    //是否所有玩家已死亡 ---- 准备全体复活至监测点
    public bool isAllPlayerDead = false;

    //是否所有玩家已准备 ---- 准备出发
    public bool isAllPlayerReady = false;

    public bool gameOver = false;

    public bool isReborning = false;

    //当前检查点
    ////public GameObject CurrentlyCheckPoint;

    //当前检测点的路径序号
    public int CheckPointIndex;
    public Vector3 checkSpeed;
    public Vector3 checkLeastSpeed;
    public Quaternion RotateQ;

    public int PathPointIndex;

    public int CheckPointAreaNo = 0;
    //岛，沙漠，城市，雪山，

    //游戏到达终点之后倒计时多少秒重置游戏
    public float GameEndTime = 30f;
    public bool GameEndCountDownStarted = false;
    Coroutine GameEndCountDownEnumerator = null;
    
    void Awake()
    {
        GM_Init();
        if (player1!=null)
        {
            PlayerGMInit(player1);
        }
        if (player2 != null)
        {
            PlayerGMInit(player2);
        }
    }

    private void GM_Init()
    {
        Instance = this;
        player1.playerUIManager.OnBlackOnCompleteEvent += OnReStartAllPlayerAtCheckPoint;
        FeelingManager.Instance.canRebornEvent += GMRebornPlayer;
    }


    //为单位玩家添加相关绑定
    public void PlayerGMInit(Player player)
    {
        //对玩家的死亡、准备、复活回应
        player.PlayerDeadEvent += PlayerDeadCallBack;
        player.PlayerReadyEvent += CheckPlayerReadyStatus;
        player.pathFinder.PathEndEvent += OnPlayerToPathEndPoint;
        //这一条对应玩家的黑幕加载完之后回档，由GM直接对P1实现
        //player.playerUIManager.OnBlackOnCompleteEvent += OnReStartAllPlayerAtCheckPoint;
    }

    // Update is called once per frame
    void Update()
    {
        //if need;
    }

    #region 已处理：玩家死亡判断，解决方法：直接对玩家死亡的动作订阅，玩家死的时候才触发一次判定
    ////每秒检测一次，如果两名玩家都死亡则触发返回检测点的方法
    //IEnumerator CheckIsAllPlayerDead()
    //{
    //    while (true)
    //    {

    //        yield return EnumeratorList.waitForOneSecond;
    //    }
    //}
    #endregion

    //触发这个方法会让全体玩家回到最近的检测点,会先复活玩家1，再让玩家2复活在玩家1的复活位
    //Attention:这个方法会直接复活，仅仅作为传送玩家的方法
    public void OnReStartAllPlayerAtCheckPoint()
    {
        PlayerAllRebornEvent?.Invoke();
        SyncPathFinderToCheckPoint(player1);
        GMRebornPlayer();

        player1.playerAppearanceManager.OnTeammateReborn();
        player1.playerAppearanceManager.ClosePlayerDead();
        player1.playerAppearanceManager.OnStopSmokeParticle();

        player2.playerAppearanceManager.OnTeammateReborn();
        player2.playerAppearanceManager.ClosePlayerDead();
        player2.playerAppearanceManager.OnStopSmokeParticle();
        //player1.playerUIManager.OnBlackOffCompleteEvent += ResetAllPlayerController;
        //player1.pathFinder.index = CheckPointIndex + 1;
        //player1.pathFinder.RefreshTarget();
        //player2.pathFinder.index = CheckPointIndex + 1;
        //player2.pathFinder.lastSpeed = player1.pathFinder.lastSpeed;
        //player2.pathFinder.RefreshTarget();
        WeatherManager.Instance.OnStopRain();
        //MusicManager.Instance.OnPlayFlyingSound();
    }

    public void OnPlayerToCheckPoint(int Index)
    {
        CheckPointIndex = Index;
    }



    //此方法与下方法不一样之处在于下方是可选，此方法直接由GM复活死亡的玩家并且同步PathFinder的Index，订阅了FeelingManager的可以复活的广播,会重置配合度
    public void GMRebornPlayer()
    {
        if (player1.isDead)
        {
            PlayerReborn(player1);
            //让幸存者的高亮显示删除
            player2.playerAppearanceManager.OnTeammateReborn();


            player1.pathFinder.index = player2.pathFinder.index;
            player1.pathFinder.V3speed = player2.pathFinder.V3speed;
            player1.pathFinder.lastSpeed = player2.pathFinder.lastSpeed;
            player1.pathFinder.RefreshTarget();
            FeelingManager.Instance.cooperation = 0;
            VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
        }
        if (player2.isDead)
        {
            PlayerReborn(player2);
            //让幸存者的高亮显示删除
            player1.playerAppearanceManager.OnTeammateReborn();

            
            player2.pathFinder.index = player1.pathFinder.index;
            player2.pathFinder.V3speed = player1.pathFinder.V3speed;
            player2.pathFinder.lastSpeed = player1.pathFinder.lastSpeed;
            player2.pathFinder.RefreshTarget();
            FeelingManager.Instance.cooperation = 0;
            VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
        }

    }

    //此方法直接用于复活玩家，复活至另外一个玩家身上携带的复活点
    public void PlayerReborn(Player player)
    {
        player.pathFinder.transform.position = player.RebornGameObject.transform.position;
        player.pathFinder.transform.rotation = player.RebornGameObject.transform.rotation;
        //死亡状态取消
        player.isDead = false;
        //为pathFinder恢复canMove
        player.pathFinder.canMove = true;
        //关闭重力影响
        player.playerController.GetComponent<Rigidbody>().useGravity = false;
        //清除惯性
        player.playerController.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        player.playerController.gameObject.transform.localPosition = new Vector3(0, 0, 7);
        //恢复操作
        player.playerController.canControl = true;
        WeatherManager.Instance.OnStopRain();
        CheckAllPlayerStatus();
        MusicManager.Instance.OnPlayRebornSound();

    }

    //传送玩家去检查点，因为会同步方向，所以检测点必须对准路径,注意这个方法是只有在全体玩家都死亡的时候才会传送Player1去制定的检测点
    //这也属于一种复活
    public void SyncPathFinderToCheckPoint(Player player)
    {
        player.pathFinder.transform.position = player.pathFinder.PathPoints[CheckPointIndex-1].position;
        //player.pathFinder.transform.rotation = player.pathFinder.PathPoints[CheckPointIndex].rotation;
        player.pathFinder.transform.rotation = RotateQ;
        //死亡状态取消
        player.isDead = false;
        //为pathFinder恢复canMove
        player.pathFinder.canMove = true;
        //关闭重力影响
        player.playerController.GetComponent<Rigidbody>().useGravity = false;
        //清除惯性
        player.playerController.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        player.playerController.gameObject.transform.localPosition = new Vector3(0, 0, 7);
        //player.pathFinder.lastSpeed = new Vector3(0, 0, 5);

        player.pathFinder.target = player.pathFinder.PathPoints[CheckPointIndex];

        player.playerAppearanceManager.OnTeammateReborn();

        player.pathFinder.V3speed = checkSpeed;
        player.pathFinder.lastSpeed = checkLeastSpeed;

        //恢复操作
        player.playerController.canControl = true;
        FeelingManager.Instance.cooperation = 0;
        VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
    }


    //当玩家死亡的时候会调用这个方法
    public void PlayerDeadCallBack()
    {
        WeatherManager.Instance.OnStartRain();
        CheckAllPlayerStatus();
    }

    //检查两位玩家的准备状态
    public void CheckPlayerReadyStatus()
    {
        if (player1.isReady && player2.isReady)
        {
            isAllPlayerReady = true;
            GameStart();
            player1.playerController.Flying = true;
            player2.playerController.Flying = true;
            
        }
    }

    //当两个玩家都准备就绪的时候就执行这个方法
    private void GameStart()
    {
        GameStartEvent?.Invoke();
        BeforeGameStartCamera.SetActive(false);
        BeforeGameStartUI.SetActive(false);
        player1nput.SwitchCurrentActionMap("FlyControl");
        player2nput.SwitchCurrentActionMap("FlyControl");

        MusicManager.Instance.OnGameStart();
        player1.playerUIManager.PlayingUI.SetActive(true);
        player2.playerUIManager.PlayingUI.SetActive(true);
        player1.playerUIManager.OnLOGODisappear();
        player2.playerUIManager.OnLOGODisappear();
        VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
        //MusicManager.Instance.OnRestartBGM();
        //MusicManager.Instance.OnPlayFlyingSound();
    }


    //游戏重启的方法
    public void ReStartGame()
    {
        if (GameEndCountDownEnumerator !=null)
        {
            StopCoroutine(GameEndCountDownEnumerator);
        }
        player1nput.SwitchCurrentActionMap("BeforeFlyControl");
        player2nput.SwitchCurrentActionMap("BeforeFlyControl");
        DeveloperInfo1.SetActive(false);
        DeveloperInfo2.SetActive(false);
        MusicManager.Instance.OnGameReset();
        BeforeGameStartCamera.SetActive(true);
        BeforeGameStartUI.SetActive(true);
        CheckPointAreaNo = 0;
        WeatherManager.Instance.skies.skyboxDay = WeatherManager.Instance.skiesBox[0];
        WeatherManager.Instance.skies.skyboxNight = WeatherManager.Instance.skiesBox[1];
        WeatherManager.Instance.skies.timeOfDay = 0;
        FeelingManager.Instance.cooperation = 10;
        FeelingManager.Instance.canCalculate = false;
        Debug.Log("GameRestart");
        Player1GO.SetActive(true);
        Player2GO.SetActive(true);
        Player1InteractiveGO.SetActive(false);
        Player2InteractiveGO.SetActive(false);
        player1.isReady = false;
        player2.isReady = false;
        player1.isDead = false;
        player2.isDead = false;
        WeatherManager.Instance.OnStopRain();
        player1.playerAppearanceManager.OnTeammateReborn();
        player2.playerAppearanceManager.OnTeammateReborn();
        player1.playerAppearanceManager.ClosePlayerDead();
        player2.playerAppearanceManager.ClosePlayerDead();
        player1.playerController.playerRigidbody.velocity = Vector3.zero;
        player2.playerController.playerRigidbody.velocity = Vector3.zero;
        player1.playerController.playerRigidbody.useGravity = false;
        player2.playerController.playerRigidbody.useGravity = false;
        player1.pathFinder.canMove = false;
        player2.pathFinder.canMove = false;
        player1.playerController.canControl = true;
        player2.playerController.canControl = true;
        
        //player1.playerController.playerInput.ActivateInput();
        //player2.playerController.playerInput.ActivateInput();
        //InputSystem.
        //InputSystem.SetDeviceUsage(player2Device, player2.playerController.playerInput.ToString());
        //InputSystem.AddDevice(player1Device);
        //InputSystem.AddDevice(player2Device);

        
        player1.playerAnimationManager.OnRestart();
        player2.playerAnimationManager.OnRestart();
        gameOver = false;
        CheckPointIndex = 0;
        player1.pathFinder.index = 0;
        player2.pathFinder.index = 0;
        player1.pathFinder.RefreshTarget();
        player2.pathFinder.RefreshTarget();
        player1.pathFinder.lastSpeed = new Vector3(0, 0, player1.pathFinder.speed);
        player2.pathFinder.lastSpeed = new Vector3(0, 0, player2.pathFinder.speed);
        player1.pathFinder.V3speed = new Vector3(0, 0, player1.pathFinder.speed);
        player2.pathFinder.V3speed = new Vector3(0, 0, player2.pathFinder.speed);
        Player1GO.transform.position = StartPoint1.transform.position;
        Player1GO.transform.rotation = StartPoint1.transform.rotation;
        Player2GO.transform.position = StartPoint2.transform.position;
        Player2GO.transform.rotation = StartPoint2.transform.rotation;
        CheckAllPlayerStatus();
        player1.playerUIManager.PlayingUI.SetActive(false);
        player2.playerUIManager.PlayingUI.SetActive(false);

    }

    //检查是否玩家都已死亡
    public void CheckAllPlayerStatus()
    {
        if (player1.isDead && player2.isDead)
        {
            isAllPlayerDead = true;
            //OnReStartAllPlayerAtCheckPoint();
            PlayerAllDeadEvent?.Invoke();
            //MusicManager.Instance.OnStopFlyingSound();
        }
        else isAllPlayerDead = false; 
        if (player1.isDead)
        {
            player2.OnSavingTeammate();
            FeelingManager.Instance.isReborning = true;
            isReborning = true;
        }
        else if (player2.isDead)
        {
            player1.OnSavingTeammate();
            FeelingManager.Instance.isReborning = true;
            isReborning = true;
        }
        if (!player1.isDead && !player2.isDead)
        {
            WeatherManager.Instance.OnStopRain();
            FeelingManager.Instance.isReborning = false;
            isReborning = false;
        }
    }

    //玩家到达终点之后触发GameOver方法,比如最终的展示
    public void OnGameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            Debug.Log("已有玩家到达终点,游戏已结束");
            //到达终点（倒数第二个点之后）玩家停止操作
            player1.playerController.canControl = false;
            player2.playerController.canControl = false;
            player1.playerUIManager.PlayingUI.SetActive(false);
            player2.playerUIManager.PlayingUI.SetActive(false);
        }
    }

    //切换至陆地模式，关闭飞行的鸟同时开启陆地行走的鸟取代
    public void OnPlayerToPathEndPoint()
    {
        DeveloperInfo1.SetActive(false);
        DeveloperInfo2.SetActive(false);
        Player1GO.SetActive(false);
        Player2GO.SetActive(false);
        Player1InteractiveGO.SetActive(true);
        Player2InteractiveGO.SetActive(true);
        Player1InteractiveGO.transform.position = GroundPoint1.transform.position;
        Player2InteractiveGO.transform.position = GroundPoint2.transform.position;
        player1nput.SwitchCurrentActionMap("Grounded");
        player2nput.SwitchCurrentActionMap("Grounded");

        //InputSystem.AddDevice(player1Device);
        //Player1InteractiveGO.GetComponent<PlayerGroundController>().playerInput.ActivateInput();
        ////InputSystem.AddDevice(player2Device);
        //Player2InteractiveGO.GetComponent<PlayerGroundController>().playerInput.ActivateInput();
        GameEndCountDownEnumerator = StartCoroutine("GameEndCountDown");
        MusicManager.Instance.OnStopFlyingSound();
        WeatherManager.Instance.OnStopRain();
        MusicManager.Instance.OnOutOfSea();
    }

    public void OnCallWeatherManagerChangeSky()
    {
        WeatherManager.Instance.ChangeSky(CheckPointAreaNo);
    }

    //用于控制两个Player的UI展示制作鸣谢
    public void OnShowDeveloperInfo()
    {
        DeveloperInfo1.SetActive(true);
        DeveloperInfo2.SetActive(true);
    }

    

    IEnumerator GameEndCountDown()
    {
        float CountDown = GameEndTime;
        while (CountDown > 0)
        {
            Debug.Log(CountDown);
            CountDown -= 1;
            if (CountDown <= 0)
            {
                OnShowDeveloperInfo();
                break;
            }
            yield return EnumeratorList.waitForOneSecond;
        }
    }


    public void DifficultChange()
    {
        Level = FeelingManager.Instance.cooperation;
        float difficultVal = 0;
        difficultVal = (float)FeelingManager.Instance.cooperation / 10f;
        
        if (RebornNerf)
        {
            difficultVal = difficultVal > 0.5f ? 0.5f : difficultVal;
        }
        float temp = difficultVal * (maxControlSpeed - minControlSpeed) + minControlSpeed;
        player1.playerController.movementSmoothingSpeed = temp;
        player2.playerController.movementSmoothingSpeed = temp;
    }

    //public void DifficultChange(float difficult)
    //{
    //    Level = difficult;
    //    float temp = (difficult / 10f) * (maxControlSpeed - minControlSpeed) + minControlSpeed;
    //    player1.playerController.movementSmoothingSpeed = temp;
    //    player2.playerController.movementSmoothingSpeed = temp;
    //}
}

