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

    [Tooltip("̫��")]
    public GameObject SunLight;

    public Vector3 DefalutSunHeight;
    public Vector3 TargetSunHeight;
    public float SunRiseSpeed;

    public PolyverseSkies skies;
    public float SkyBoxChangeSpeed;

    [Tooltip("���׵���Ч")]
    public AudioSource ThunderAudioSource;

    public Image Player1_ThunderImg;
    public Image Player2_ThunderImg;

    [Tooltip("ÿ0.1��ı����")]
    public float WeatherChangeSpeed = 0.1f;
    public float TargetRainIntensity = 0.5f;

    [Tooltip("����ÿ1��ı����")]
    public float ThunderChangeSpeed = 0.1f;
    public float ThunderIntensity = 1f;
    public float ThunderCD = 5f;
    [Tooltip("�������ֵ�ͻ����磬��ֵ100������0")]
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

    //��Ϸ��ʼ��ʱ������
    public void SunRise()
    {
        StartCoroutine(SunRiseEnumerator());
        ChangeSky(0);
    }

    //������Ϸ��ʱ��̫���ص�һ��ʼ��λ��
    public void SunDown()
    {
        skies.timeOfDay = 0;
    }

    //�������Э��������
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


    //Ĭ��Ϊ���꣬��Ϊ�����ķ���
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
    
    //ֹͣ����ķ���
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

    //���׵ķ���
    public void OnThunder()
    {
        if (canThunder)
        {
            ThunderAudioSource.Play();
            StartCoroutine(StartThunder());
        }
    }


    //�����𽥿�ʼ�����Э��
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

    //������ֹͣ�����Э��
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


    //ֱ�Ӹı�Ķ���Player1��Ч������Player2��ͬ��
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

    //��ʼ���׵�Э��
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

    //������֮��������´��ף����׵�CD������.
    //һ��һˢ
    //�������ʱ������ʱ����������ᴥ���������
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

    //������׵�Э��,����������ܴ�������
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
