using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//此脚本单纯用于控制角色的二维位移。
public class PlayerController : MonoBehaviour
{
    public delegate void OnBirdCrash();
    public event OnBirdCrash BirdCrashEvent;

    public delegate void OnPlayerReady();
    public event OnPlayerReady PlayerReadyEvent;

    public delegate void OnFlyDanceAction();
    public event OnFlyDanceAction OnFlyDanceEvent;

    public delegate void OnShowInfoAction();
    public event OnShowInfoAction OnShowInfoEvent;


    public delegate void OnHitAction();
    public event OnHitAction OnHitEvent;

    //public delegate float MovementAction(ref float movement);
    //public event MovementAction MovementEvent;
    

    public bool canControl;

    private bool tryRestartGame;

    public Rigidbody playerRigidbody;

    //[Header("摄像机")]

    //[Tooltip("放置玩家专用摄像机")]
    //public GameObject PlayerCamera;
    //public GameObject cameraTarget;

    public float targetMaxRadius = 5;
    public float birdMaxRadius = 3;
    public float targetDistance = 10;
    public float birdDistance = 7;

    public Vector3 rawInputMovement;
    public Vector3 smoothInputMovement;
    //平滑处理的速度，越大越快到达目标位置
    public float movementSmoothingSpeed = 3;

    public Vector3 BirdMoveVector;
    public Vector3 CameraMoveVector;

    public bool Flying;

    public void FlyControl(InputAction.CallbackContext context)
    {
        Vector2 V2Input = context.ReadValue<Vector2>();
        rawInputMovement = V2Input;
    }

    public void ShowInfo(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnShowInfoEvent?.Invoke();
        }
    }

    public void OnReady(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.CheckPlayerReadyStatus();
            PlayerReadyEvent?.Invoke();
        }
    }

    //广播触发飞行翻滚的动画，由Player订阅，解耦合
    public void OnFlyDance(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnFlyDanceEvent?.Invoke();
        }
    }

    //开启FeelingManager的随机数值变动
    public void OnRandomFreshFeelingManager(InputAction.CallbackContext context)
    {
        //会自动调整
        if (context.started)
        {
            FeelingManager.Instance.OnStartRandomMode();
        }
    }


    //广播触发移动，平面移动
    public void OnMoveControl(InputAction.CallbackContext context)
    {
        Vector2 V2Input = context.ReadValue<Vector2>();
        rawInputMovement = V2Input;
    }
    //广播触发小鸟展翅

    //广播触发小鸟啄地面
    public void OnHit()
    {
        OnHitEvent?.Invoke();
    }

    //重置游戏
    public void OnReStartGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //按住只触发一次
            Debug.Log("Started");
            tryRestartGame = true;
            StartCoroutine(ReStartCountDown());
        }
        if(context.canceled)
        {
            tryRestartGame = false;
            //松手会触发一次
            Debug.Log("Canceled");
        }
    }


    //撞墙的时候，如果撞的使Wall，则广播并且让鸟自然下坠
    public void OnTriggerEnter(Collider collision)

    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            BirdCrashEvent?.Invoke();
            playerRigidbody.useGravity = true;
        }
    }

    void Update()
    {
        //每帧都对输入进行平滑处理
        CalculateMovementInputSmoothing();
    }

    //开启重置游戏的协程，如果满3秒的话
    IEnumerator ReStartCountDown()
    {
        int tempTime = 0;
        while(tryRestartGame)
        {
            yield return EnumeratorList.waitForOneSecond;
            tempTime += 1;
            if (tempTime >= 3)
            {
                GameManager.Instance.ReStartGame();
            }
        }
        
    }

    



    //平滑处理实际移动和目标移动
    public void CalculateMovementInputSmoothing()
    {
        smoothInputMovement = Vector3.Lerp(smoothInputMovement, rawInputMovement, Time.deltaTime * movementSmoothingSpeed);
        BirdMoveVector = smoothInputMovement * birdMaxRadius;
        BirdMoveVector.z = birdDistance;
        //CameraMoveVector = smoothInputMovement * targetMaxRadius;
        //CameraMoveVector.y += 1.5f;
        //CameraMoveVector.z = targetDistance;
    }

    //这个BirdMovement,仅用于控制飞行中的鸟，陆地行走的鸟将会另作控制
    public void BirdMovement()
    {

        gameObject.transform.localPosition = BirdMoveVector;
        //cameraTarget.transform.localPosition = CameraMoveVector;
    }


    private void FixedUpdate()
    {
        //只有在可以操作的情况下摄像机才会追踪视角
        if (canControl)
        {
            BirdMovement();
            //PlayerCamera.transform.LookAt(cameraTarget.transform);
        }
        //CameraTargetMove();
    }
}
