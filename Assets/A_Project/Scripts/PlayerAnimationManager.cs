using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此脚本负责管理玩家的所有动画，包括Camera以及bird的动画
public class PlayerAnimationManager : MonoBehaviour
{
    

    public delegate void CameraAnimationOverAction();
    public event CameraAnimationOverAction CameraAnimationOverEvent;

    //接受来自cameraEvent的广播
    public CameraEvent cameraEvent;

    public Animator CameraAnimator;

    public Animator BirdAnimator;
    public Animator V_BirdAnimator;


    public void Start()
    {
        GameManager.Instance.GameStartEvent += OnBirdStartToFly;
        cameraEvent.OnCameraActionOverEvent += OnCameraAnimationOver;
    }

    //类似于转播，告诉Player动画已经播完，PathFinder可以开始追踪复活点
    public void OnCameraAnimationOver()
    {
        CameraAnimationOverEvent?.Invoke();
        CameraAnimator.SetBool("PlayerDead",false);
    }

    //调用此方法，播放死亡之后抬起摄像机的操作
    public void PlayCameraDeadAnimation()
    {
        CameraAnimator.SetBool("PlayerDead",true);
    }

    public void OnBirdStartToFly()
    {
        BirdAnimator.SetTrigger("StartFly");
        V_BirdAnimator.SetTrigger("StartFly");
    }

    public void OnBirdFlyingDance()
    {
        BirdAnimator.SetTrigger("FlyDance");
        V_BirdAnimator.SetTrigger("FlyDance");
    }

    //在PathFinder到达FinalPoint
    public void OnBirdGetDown()
    {
        BirdAnimator.SetTrigger("StartGetDown");
        V_BirdAnimator.SetTrigger("StartGetDown");
    }

    public void OnBirdMovement(float length)
    {
        BirdAnimator.SetFloat("Movement", length);
        V_BirdAnimator.SetFloat("Movement", length);
    }

    public void OnBirdSayHello()
    {
        BirdAnimator.SetTrigger("SayHello");
        V_BirdAnimator.SetTrigger("SayHello");
    }

    public void OnRestart()
    {
        BirdAnimator.SetTrigger("Restart");
        V_BirdAnimator.SetTrigger("Restart");
    }

}
