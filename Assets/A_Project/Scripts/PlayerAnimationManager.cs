using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�˽ű����������ҵ����ж���������Camera�Լ�bird�Ķ���
public class PlayerAnimationManager : MonoBehaviour
{
    

    public delegate void CameraAnimationOverAction();
    public event CameraAnimationOverAction CameraAnimationOverEvent;

    //��������cameraEvent�Ĺ㲥
    public CameraEvent cameraEvent;

    public Animator CameraAnimator;

    public Animator BirdAnimator;
    public Animator V_BirdAnimator;


    public void Start()
    {
        GameManager.Instance.GameStartEvent += OnBirdStartToFly;
        cameraEvent.OnCameraActionOverEvent += OnCameraAnimationOver;
    }

    //������ת��������Player�����Ѿ����꣬PathFinder���Կ�ʼ׷�ٸ����
    public void OnCameraAnimationOver()
    {
        CameraAnimationOverEvent?.Invoke();
        CameraAnimator.SetBool("PlayerDead",false);
    }

    //���ô˷�������������֮��̧��������Ĳ���
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

    //��PathFinder����FinalPoint
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
