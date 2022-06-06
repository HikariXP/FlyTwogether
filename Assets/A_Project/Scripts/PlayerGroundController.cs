using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerGroundController : MonoBehaviour
{
    public delegate void OnSayHelloAction();
    public event OnSayHelloAction OnSayHelloEvent;

    public Animator GroundAnimator;

    public Rigidbody rb;

    public Camera GroundCamera;

    public Vector3 rawInputMovement;
    public Vector3 smoothInputMovement;
    //平滑处理的速度，越大越快到达目标位置
    public float movementSmoothingSpeed = 3;

    public float moveSpeed = 5f;
    public float rotateSpeed = 1f;
    private bool tryRestartGame;

    public void MovementControl(InputAction.CallbackContext context)
    {
        Vector2 V2Input = context.ReadValue<Vector2>();
        rawInputMovement = V2Input;
    }

    public void OnSayHello(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnSayHello();
        }
    }


    public void OnHit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnHit();
        }
    }

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //按住只触发一次
            Debug.Log("Started");
            tryRestartGame = true;
            StartCoroutine(ReStartCountDown());
        }
        if (context.canceled)
        {
            tryRestartGame = false;
            //松手会触发一次
            Debug.Log("Canceled");
        }
    }

    #region Animator相关

    public void UpdateAnimatorMovement()
    {
        GroundAnimator.SetFloat("Movement",smoothInputMovement.magnitude);

    }

    public void OnSayHello()
    {
        GroundAnimator.SetTrigger("SayHello");
    }

    public void OnHit()
    {
        GroundAnimator.SetTrigger("Hit");
    }

    #endregion

    public void UpdateRotate()
    {
        if(smoothInputMovement.sqrMagnitude > 0.01f)
        {
            //Quaternion rotation = Quaternion.Slerp(rb.rotation,
            //                                     Quaternion.LookRotation(CameraDirection(smoothInputMovement)),
            //                                     rotateSpeed);

            //rb.MoveRotation(rotation);
            //rb.angularVelocity = gameObject.transform.up*smoothInputMovement.x;
            rb.angularVelocity = Vector3.zero;
            Vector3 tempV3 = gameObject.transform.rotation.eulerAngles;
            tempV3.y += Time.deltaTime * rawInputMovement.x* 20f;
            gameObject.transform.rotation = Quaternion.Euler(tempV3);
        }
    }

    public void CalculateMovementInputSmoothing()
    {
        smoothInputMovement = Vector3.Lerp(smoothInputMovement, rawInputMovement, Time.deltaTime * movementSmoothingSpeed);
    }

    public void Update()
    {
        CalculateMovementInputSmoothing();
        //更新Animator中移动的动画幅度
        UpdateAnimatorMovement();
    }

    public void MovePlayer()
    {
        Vector3 movement = CameraDirection(smoothInputMovement) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
    }

    public void FixedUpdate()
    {
        MovePlayer();
        UpdateRotate();
        if (smoothInputMovement.magnitude < 0.01f)
        {
            rb.velocity = Vector3.zero;
        }
    }

    

    Vector3 CameraDirection(Vector3 movementDirection)
    {
        var cameraForward = GroundCamera.transform.forward;
        var cameraRight = GroundCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        return cameraForward * movementDirection.y/* + cameraRight * movementDirection.x*/;
    }

    //开启重置游戏的协程，如果满3秒的话
    IEnumerator ReStartCountDown()
    {
        int tempTime = 0;
        while (tryRestartGame)
        {
            yield return EnumeratorList.waitForOneSecond;
            tempTime += 1;
            if (tempTime >= 3)
            {
                GameManager.Instance.ReStartGame();
            }
        }

    }
}
