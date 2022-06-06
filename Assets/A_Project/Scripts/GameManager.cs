using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerInput player1nput;
    public PlayerInput player2nput;
    //ר�����ڴ���
    //���ȫ���������¼��㲥
    public delegate void PlayerAllDeadAction();
    public event PlayerAllDeadAction PlayerAllDeadEvent;
    //���ȫ�帴����¼��㲥
    public delegate void PlayerAllRebornAction();
    public event PlayerAllRebornAction PlayerAllRebornEvent;

    //��Ϸ��ʼ���¼��㲥
    public delegate void GameStartAction();
    public event GameStartAction GameStartEvent;

    //GameManager�ĵ������ṩһ��ȫ�ֵ�����λ��
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

    [Header("���/��С��Ҳٿ���Ӧ�ٶ�")]
    public float maxControlSpeed = 5f;
    public float minControlSpeed = 1f;
    public float Level;


    //��������ã��ڿ����ڼ������Ѷ����Ϊ5��
    public bool RebornNerf;


    //�Ƿ�������������� ---- ׼��ȫ�帴��������
    public bool isAllPlayerDead = false;

    //�Ƿ����������׼�� ---- ׼������
    public bool isAllPlayerReady = false;

    public bool gameOver = false;

    public bool isReborning = false;

    //��ǰ����
    ////public GameObject CurrentlyCheckPoint;

    //��ǰ�����·�����
    public int CheckPointIndex;
    public Vector3 checkSpeed;
    public Vector3 checkLeastSpeed;
    public Quaternion RotateQ;

    public int PathPointIndex;

    public int CheckPointAreaNo = 0;
    //����ɳĮ�����У�ѩɽ��

    //��Ϸ�����յ�֮�󵹼�ʱ������������Ϸ
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


    //Ϊ��λ��������ذ�
    public void PlayerGMInit(Player player)
    {
        //����ҵ�������׼���������Ӧ
        player.PlayerDeadEvent += PlayerDeadCallBack;
        player.PlayerReadyEvent += CheckPlayerReadyStatus;
        player.pathFinder.PathEndEvent += OnPlayerToPathEndPoint;
        //��һ����Ӧ��ҵĺ�Ļ������֮��ص�����GMֱ�Ӷ�P1ʵ��
        //player.playerUIManager.OnBlackOnCompleteEvent += OnReStartAllPlayerAtCheckPoint;
    }

    // Update is called once per frame
    void Update()
    {
        //if need;
    }

    #region �Ѵ�����������жϣ����������ֱ�Ӷ���������Ķ������ģ��������ʱ��Ŵ���һ���ж�
    ////ÿ����һ�Σ����������Ҷ������򴥷����ؼ���ķ���
    //IEnumerator CheckIsAllPlayerDead()
    //{
    //    while (true)
    //    {

    //        yield return EnumeratorList.waitForOneSecond;
    //    }
    //}
    #endregion

    //���������������ȫ����һص�����ļ���,���ȸ������1���������2���������1�ĸ���λ
    //Attention:���������ֱ�Ӹ��������Ϊ������ҵķ���
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



    //�˷������·�����һ��֮�������·��ǿ�ѡ���˷���ֱ����GM������������Ҳ���ͬ��PathFinder��Index��������FeelingManager�Ŀ��Ը���Ĺ㲥,��������϶�
    public void GMRebornPlayer()
    {
        if (player1.isDead)
        {
            PlayerReborn(player1);
            //���Ҵ��ߵĸ�����ʾɾ��
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
            //���Ҵ��ߵĸ�����ʾɾ��
            player1.playerAppearanceManager.OnTeammateReborn();

            
            player2.pathFinder.index = player1.pathFinder.index;
            player2.pathFinder.V3speed = player1.pathFinder.V3speed;
            player2.pathFinder.lastSpeed = player1.pathFinder.lastSpeed;
            player2.pathFinder.RefreshTarget();
            FeelingManager.Instance.cooperation = 0;
            VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
        }

    }

    //�˷���ֱ�����ڸ�����ң�����������һ���������Я���ĸ����
    public void PlayerReborn(Player player)
    {
        player.pathFinder.transform.position = player.RebornGameObject.transform.position;
        player.pathFinder.transform.rotation = player.RebornGameObject.transform.rotation;
        //����״̬ȡ��
        player.isDead = false;
        //ΪpathFinder�ָ�canMove
        player.pathFinder.canMove = true;
        //�ر�����Ӱ��
        player.playerController.GetComponent<Rigidbody>().useGravity = false;
        //�������
        player.playerController.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        player.playerController.gameObject.transform.localPosition = new Vector3(0, 0, 7);
        //�ָ�����
        player.playerController.canControl = true;
        WeatherManager.Instance.OnStopRain();
        CheckAllPlayerStatus();
        MusicManager.Instance.OnPlayRebornSound();

    }

    //�������ȥ���㣬��Ϊ��ͬ���������Լ�������׼·��,ע�����������ֻ����ȫ����Ҷ�������ʱ��Żᴫ��Player1ȥ�ƶ��ļ���
    //��Ҳ����һ�ָ���
    public void SyncPathFinderToCheckPoint(Player player)
    {
        player.pathFinder.transform.position = player.pathFinder.PathPoints[CheckPointIndex-1].position;
        //player.pathFinder.transform.rotation = player.pathFinder.PathPoints[CheckPointIndex].rotation;
        player.pathFinder.transform.rotation = RotateQ;
        //����״̬ȡ��
        player.isDead = false;
        //ΪpathFinder�ָ�canMove
        player.pathFinder.canMove = true;
        //�ر�����Ӱ��
        player.playerController.GetComponent<Rigidbody>().useGravity = false;
        //�������
        player.playerController.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        player.playerController.gameObject.transform.localPosition = new Vector3(0, 0, 7);
        //player.pathFinder.lastSpeed = new Vector3(0, 0, 5);

        player.pathFinder.target = player.pathFinder.PathPoints[CheckPointIndex];

        player.playerAppearanceManager.OnTeammateReborn();

        player.pathFinder.V3speed = checkSpeed;
        player.pathFinder.lastSpeed = checkLeastSpeed;

        //�ָ�����
        player.playerController.canControl = true;
        FeelingManager.Instance.cooperation = 0;
        VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
    }


    //�����������ʱ�������������
    public void PlayerDeadCallBack()
    {
        WeatherManager.Instance.OnStartRain();
        CheckAllPlayerStatus();
    }

    //�����λ��ҵ�׼��״̬
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

    //��������Ҷ�׼��������ʱ���ִ���������
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


    //��Ϸ�����ķ���
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

    //����Ƿ���Ҷ�������
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

    //��ҵ����յ�֮�󴥷�GameOver����,�������յ�չʾ
    public void OnGameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            Debug.Log("������ҵ����յ�,��Ϸ�ѽ���");
            //�����յ㣨�����ڶ�����֮�����ֹͣ����
            player1.playerController.canControl = false;
            player2.playerController.canControl = false;
            player1.playerUIManager.PlayingUI.SetActive(false);
            player2.playerUIManager.PlayingUI.SetActive(false);
        }
    }

    //�л���½��ģʽ���رշ��е���ͬʱ����½�����ߵ���ȡ��
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

    //���ڿ�������Player��UIչʾ������л
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

