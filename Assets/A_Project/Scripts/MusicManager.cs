using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioSource BGM;
    [Tooltip("拍打翅膀的声音")]
    public AudioSource FlyingSound;
    [Tooltip("撞墙的声音")]
    public AudioSource CrushSound;
    [Tooltip("")]
    public AudioSource Temp;
    public AudioSource RebornSound;

    private void Start()
    {
        Instance = this;
    }

    public void OnRestartBGM()
    {
        if (BGM != null)
        {
            BGM.Stop();
            BGM.Play();
        }
    }

    public void OnStopBGM()
    {
        if(BGM != null)
        {
            BGM.Stop();
        }
        else Debug.LogWarning("BGM is Null");
    }

    public void OnPlayFlyingSound()
    {
        if(FlyingSound != null)
        {
            FlyingSound.Play();
        }
        else Debug.LogWarning("FlyingSound is Null");
    }

    public void OnStopFlyingSound()
    {
        if (FlyingSound != null)
        {
            FlyingSound.Stop();
        }
        else Debug.LogWarning("FlyingSound is Null");
    }

    public void OnPlayCrushSound()
    {
        if (CrushSound != null)
        {
            CrushSound.Play();
        }
        else Debug.LogWarning("CrushSound is Null");
    }

    public void OnGameStart()
    {
        OnRestartBGM();
        OnPlayFlyingSound();
    }

    public void OnGameReset()
    {
        OnStopBGM();
        OnStopFlyingSound();
    }

    //进入雪山开风音效
    public void OnToSnowMountain()
    {
        Temp.Play();
    }

    //关闭风音效
    public void OnOutOfSea()
    {
        Temp.Stop();
    }

    public void OnPlayRebornSound()
    {
        RebornSound.Play();
    }
}
