using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//�˽ű��������ڿ��ƽ�ɫ�Ķ�άλ�ơ�
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

    //[Header("�����")]

    //[Tooltip("�������ר�������")]
    //public GameObject PlayerCamera;
    //public GameObject cameraTarget;

    public float targetMaxRadius = 5;
    public float birdMaxRadius = 3;
    public float targetDistance = 10;
    public float birdDistance = 7;

    public Vector3 rawInputMovement;
    public Vector3 smoothInputMovement;
    //ƽ��������ٶȣ�Խ��Խ�쵽��Ŀ��λ��
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

    //�㲥�������з����Ķ�������Player���ģ������
    public void OnFlyDance(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnFlyDanceEvent?.Invoke();
        }
    }

    //����FeelingManager�������ֵ�䶯
    public void OnRandomFreshFeelingManager(InputAction.CallbackContext context)
    {
        //���Զ�����
        if (context.started)
        {
            FeelingManager.Instance.OnStartRandomMode();
        }
    }


    //�㲥�����ƶ���ƽ���ƶ�
    public void OnMoveControl(InputAction.CallbackContext context)
    {
        Vector2 V2Input = context.ReadValue<Vector2>();
        rawInputMovement = V2Input;
    }
    //�㲥����С��չ��

    //�㲥����С���ĵ���
    public void OnHit()
    {
        OnHitEvent?.Invoke();
    }

    //������Ϸ
    public void OnReStartGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //��סֻ����һ��
            Debug.Log("Started");
            tryRestartGame = true;
            StartCoroutine(ReStartCountDown());
        }
        if(context.canceled)
        {
            tryRestartGame = false;
            //���ֻᴥ��һ��
            Debug.Log("Canceled");
        }
    }


    //ײǽ��ʱ�����ײ��ʹWall����㲥����������Ȼ��׹
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
        //ÿ֡�����������ƽ������
        CalculateMovementInputSmoothing();
    }

    //����������Ϸ��Э�̣������3��Ļ�
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

    



    //ƽ������ʵ���ƶ���Ŀ���ƶ�
    public void CalculateMovementInputSmoothing()
    {
        smoothInputMovement = Vector3.Lerp(smoothInputMovement, rawInputMovement, Time.deltaTime * movementSmoothingSpeed);
        BirdMoveVector = smoothInputMovement * birdMaxRadius;
        BirdMoveVector.z = birdDistance;
        //CameraMoveVector = smoothInputMovement * targetMaxRadius;
        //CameraMoveVector.y += 1.5f;
        //CameraMoveVector.z = targetDistance;
    }

    //���BirdMovement,�����ڿ��Ʒ����е���½�����ߵ��񽫻���������
    public void BirdMovement()
    {

        gameObject.transform.localPosition = BirdMoveVector;
        //cameraTarget.transform.localPosition = CameraMoveVector;
    }


    private void FixedUpdate()
    {
        //ֻ���ڿ��Բ����������������Ż�׷���ӽ�
        if (canControl)
        {
            BirdMovement();
            //PlayerCamera.transform.LookAt(cameraTarget.transform);
        }
        //CameraTargetMove();
    }
}
