using DigitalRuby.RainMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Boxophobic;


public class WeatherManager : MonoBehaviour
{
    public List<Material> skiesBox;

    public RainScript rainManagerPlayer1;
    public RainScript rainManagerPlayer2;

    [Tooltip("太阳")]
    public GameObject SunLight;

    public Vector3 DefalutSunHeight;
    public Vector3 TargetSunHeight;
    public float SunRiseSpeed;

    public PolyverseSkies skies;
    public float SkyBoxChangeSpeed;

    [Tooltip("打雷的音效")]
    public AudioSource ThunderAudioSource;

    public Image Player1_ThunderImg;
    public Image Player2_ThunderImg;

    [Tooltip("每0.1秒改变多少")]
    public float WeatherChangeSpeed = 0.1f;
    public float TargetRainIntensity = 0.5f;

    [Tooltip("闪电每1秒改变多少")]
    public float ThunderChangeSpeed = 0.1f;
    public float ThunderIntensity = 1f;
    public float ThunderCD = 5f;
    [Tooltip("过了这个值就会闪电，满值100，最少0")]
    public float RandomThunderValue = 50;

    private bool canThunder = true;

    public static WeatherManager Instance { get; private set; }

    private bool isRaining = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.GameStartEvent += SunRise;
    }

    //游戏开始的时候日升
    public void SunRise()
    {
        StartCoroutine(SunRiseEnumerator());
        ChangeSky(0);
    }

    //重置游戏的时候太阳回到一开始的位置
    public void SunDown()
    {
        skies.timeOfDay = 0;
    }

    //启动这个协程以日升
    IEnumerator SunRiseEnumerator()
    {
        while (SunLight.transform.rotation.x < TargetSunHeight.x)
        {
            SunLight.transform.rotation = Quaternion.Lerp(SunLight.transform.rotation, Quaternion.Euler(TargetSunHeight), SunRiseSpeed*Time.deltaTime);
            yield return EnumeratorList.waitForEndOfFrame;
        }
    }

    IEnumerator SkyBoxEnumerator()
    {
        skies.timeOfDay = 0;
        float temp = 0;
        while (skies.timeOfDay < 1)
        {
            temp += Time.deltaTime * SkyBoxChangeSpeed;
            if (temp < 1)
            {
                skies.timeOfDay = temp;
            }
            else
            {
                skies.timeOfDay = 1;
            }
            yield return EnumeratorList.waitForEndOfFrame;
        }
    }

    public void ChangeSky(int index)
    {
        if (index > 3 || index < 0)
        {
            return;
        }
        else
        {
            skies.skyboxDay = skiesBox[index];
            skies.skyboxNight = skiesBox[index + 1];
            StartCoroutine(SkyBoxEnumerator());
        }
    }

    public void ChangeSky(int indexA,int indexB)
    {
        skies.skyboxDay = skiesBox[indexA];
        skies.skyboxNight = skiesBox[indexB];
        StartCoroutine(SkyBoxEnumerator());
    }


    //默认为雷雨，此为开启的方法
    public void OnStartRain()
    {
        //if (!isRaining)
        //{
        //    StartCoroutine(StartRain());
        //}
        //else return;
        rainManagerPlayer1.RainIntensity = 0.4f;
        SyncRainIntensity();
        isRaining = true;
        OnThunder();
    }
    
    //停止下雨的方法
    public void OnStopRain()
    {
        //if (isRaining)
        //{
        //    StartCoroutine(StopRain());
        //}
        //else return;
        rainManagerPlayer1.RainIntensity = 0.0f;
        SyncRainIntensity();
        isRaining = false;
    }

    //打雷的方法
    public void OnThunder()
    {
        if (canThunder)
        {
            ThunderAudioSource.Play();
            StartCoroutine(StartThunder());
        }
    }


    //用于逐渐开始下雨的协程
    IEnumerator StartRain()
    {
        StartCoroutine(RandomThunder());
        while (rainManagerPlayer1.RainIntensity < TargetRainIntensity)
        {
            rainManagerPlayer1.RainIntensity += WeatherChangeSpeed;
            SyncRainIntensity();
            yield return EnumeratorList.waitForOneMSecond;
        }
    }

    //用于逐渐停止下雨的协程
    IEnumerator StopRain()
    {
        isRaining = false;
        while (rainManagerPlayer1.RainIntensity > 0)
        {
            rainManagerPlayer1.RainIntensity -= WeatherChangeSpeed;
            SyncRainIntensity();
            yield return EnumeratorList.waitForOneMSecond;
        }
    }


    //直接改变的都是Player1的效果，让Player2的同步
    private void SyncRainIntensity()
    {
        if(rainManagerPlayer2 != null)
        rainManagerPlayer2.RainIntensity = rainManagerPlayer1.RainIntensity;
    }

    private void SyncThunderImgAlpha()
    {
        if(Player2_ThunderImg != null)
        Player2_ThunderImg.color = Player1_ThunderImg.color;
    }

    //开始打雷的协程
    IEnumerator StartThunder()
    {
        canThunder = false;
        float a = ThunderIntensity;
        Player1_ThunderImg.color = new Color(1,1,1,a);
        while (Player1_ThunderImg.color.a>0)
        {
            a -= ThunderChangeSpeed*Time.deltaTime;
            Player1_ThunderImg.color = new Color(1, 1, 1, a);
            SyncThunderImgAlpha();
            yield return EnumeratorList.waitForEndOfFrame;
        }
        StartCoroutine(ThunderCountDown());
    }

    //多少秒之后可以重新打雷，打雷的CD计算器.
    //一秒一刷
    //如果倒计时结束的时候还在下雨则会触发随机打雷
    IEnumerator ThunderCountDown()
    {
        float countDown = ThunderCD;
        while (countDown > 0)
        {
            countDown -= 1;
            yield return EnumeratorList.waitForOneSecond;
        }
        canThunder = true;
        if (isRaining)
        {
            StartCoroutine(RandomThunder());
        }
    }

    //随机打雷的协程,必须下雨才能触发打雷
    IEnumerator RandomThunder()
    {
        int temp;
        while (true)
        {
            if (isRaining)
            {
                temp = Random.Range(0, 100);
                if (temp > RandomThunderValue)
                {
                    OnThunder();
                    break;
                }
            }
            else break;
            yield return EnumeratorList.waitForOneSecond;
        }
        yield return EnumeratorList.waitForEndOfFrame;
    }
}
