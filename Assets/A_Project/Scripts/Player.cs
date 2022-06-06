using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//将玩家的操作，玩家外观变化，分别交由其他组件操作，本组件纯粹与GM互动
public class Player : MonoBehaviour
{
    //用于广播玩家死亡的信息
    #region 声明事件
    public delegate void PlayerDeadAction();
    public event PlayerDeadAction PlayerDeadEvent;

    public delegate void PlayerRebornAction();
    public event PlayerRebornAction PlayerRebornEvent;

    public delegate void PlayerReadyAction();
    public event PlayerReadyAction PlayerReadyEvent;

    public delegate void PathFinderToFinalPointAction();
    public event PathFinderToFinalPointAction PathFinderToFinalPointEvent;

    public delegate void PathFinderToPathEndAction();
    public event PathFinderToPathEndAction PathFinderToPathEndEvent;



    #endregion 

    public PlayerAppearanceManager playerAppearanceManager;
    public PlayerAnimationManager playerAnimationManager;
    public PlayerController playerController;
    public PlayerUIManager playerUIManager;
    public PathFinder pathFinder;

    [Tooltip("此处存放玩家特有的队友复活点")]
    public GameObject RebornGameObject;

    public bool isReady = false;
    public bool isDead = false;

    void Start()
    {
        //对GM的游戏开始方法进行订阅
        GameManager.Instance.GameStartEvent += GameStarted;
        //对玩家的撞墙事件订阅
        playerController.BirdCrashEvent += OnPlayerDead;
        //对玩家准备操作的订阅
        playerController.PlayerReadyEvent += OnPlayerReady;

        playerController.OnShowInfoEvent += OnShowInfo;

        //对玩家死亡的摄像机动画播放完的事件进行订阅
        playerAnimationManager.CameraAnimationOverEvent += OnStartWatingForReborn;
        playerAnimationManager.CameraAnimationOverEvent += OnCloseDeadAppearance;
        //对玩家飞行的旋转动画操作进行订阅
        playerController.OnFlyDanceEvent += OnPlayFlyDanceAnimation;
        //对玩家到达FinalPoint的事件进行订阅
        pathFinder.FinalPointEvent += OnToFinalPoint;
        pathFinder.PathEndEvent += OnToPathEndPoint;

        
    }

    //当两名玩家都准备就绪的时候会由系统调用这个方法，使pathFinder走起来,并且改变PlayerInput的ActionsMap
    public void GameStarted()
    {
        pathFinder.canMove = true;
        playerUIManager.isShowInfo = true;playerUIManager.OnShowInfo();

       
    }

    //监听了玩家操作的准备动作，当玩家按下时则触发
    public void OnPlayerReady()
    {
        isReady = true;
        PlayerReadyEvent?.Invoke();
    }

    //展示死亡的特效
    public void PlayerDeadAppearance()
    {
        playerAppearanceManager.OnPlayerDead();
    }
    //
    public void OnShowInfo()
    {
        playerUIManager.OnShowInfo();
    }

    //摄像机动画播放完之后关闭死亡特效
    public void OnCloseDeadAppearance()
    {
        playerAppearanceManager.ClosePlayerDead();
    }

    //展示复活队友的特效
    public void OnSavingTeammate()
    {
        playerAppearanceManager.OnTeammateDead();
    }

    //关闭复活队友的特效
    public void SavedTeammate()
    {
        playerAppearanceManager.OnTeammateReborn();
    }

    public void UpdateMovementAnimation(float length)
    {
        playerAnimationManager.OnBirdMovement(length);
    }

    public void FixedUpdate()
    {
        UpdateMovementAnimation(playerController.smoothInputMovement.magnitude);
    }


    public void OnPlaySayHelloAnimation()
    {
        playerAnimationManager.OnBirdSayHello();
    }

    //由此命令动画控制器播放在空中翻滚的动画
    public void OnPlayFlyDanceAnimation()
    {
        playerAnimationManager.OnBirdFlyingDance();
    }

    //已经对玩家的撞墙订阅，并且绑定了OnPlayerDead的方法
    //在玩家撞墙的时候会自动触发
    public void OnPlayerDead()
    {
        //撞墙之后把玩家设置为死亡状态
        isDead = true;
        //关闭玩家的操作
        playerController.canControl = false;
        //如果有人订阅玩家死亡事件的话则广播
        PlayerDeadEvent?.Invoke();
        //PathFinder设置为停止移动
        pathFinder.canMove = false;

        //playerAnimatonManager播放摄像机抬头的动画
        playerAnimationManager.PlayCameraDeadAnimation();
        //playerAppearanceManager让鸟消失并且触发对应的粒子效果，队友的则变亮且有粒子环绕
        playerAppearanceManager.OnPlayerDead();
        //
        MusicManager.Instance.OnPlayCrushSound();
        FeelingManager.Instance.canCalculate = false;
        FeelingManager.Instance.cooperation = 10;
        VirtualBirdAppearanceManager.Instance.ChangeVirtualBirdAlpha();
        //FeelingManager.Instance.isReborning = true;
    }

    //由GM调用恢复死亡前的状态
    public void OnPlayerReborn()
    {
        //死亡状态取消
        isDead = false;
        //为pathFinder恢复canMove
        pathFinder.canMove = true;
        //清除惯性
        playerController.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        //关闭重力影响
        playerController.GetComponent<Rigidbody>().useGravity = false;
        //恢复操作
        playerController.canControl = true;
    }

    public void OnStartWatingForReborn()
    {
        pathFinder.Follow = true;
    }

    //对PathFinder的降落到达订阅，开启互动事件
    public void OnToPathEndPoint()
    {
        GameManager.Instance.OnPlayerToPathEndPoint();
    }

    //对PathFinder到达终点的事件进行订阅，播放降落动画
    public void OnToFinalPoint()
    {
        playerAnimationManager.OnBirdGetDown();
    }


    //重启游戏会重设参数
    public void OnGameReset()
    {
        
    }


}
