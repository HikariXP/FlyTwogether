using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此组件专门负责鸟的外观,以及一些相关的粒子效果,不包括Virtual的外观
public class PlayerAppearanceManager : MonoBehaviour
{
    public GameObject BirdModel;

    public GameObject AnotherBirdStatus;

    public Material NormalMaterial;
    public Material SavingTeammateMaterial;


    public ParticleSystem playerDeadParticle;
    public ParticleSystem playerDeadSmoke;

    public ParticleSystem teammateDeadParticle;

    public Color ColorOn;

    public void Start()
    {

    }

    //这里专门用于开启队友死亡后的特效，包括：模型变亮，粒子聚拢
    public void OnTeammateDead()
    {
        for (int i = 0; i <= 4; i++)
        {
            //BirdModel.GetComponent<SkinnedMeshRenderer>().materials[i].CopyPropertiesFromMaterial(SavingTeammateMaterial);
            BirdModel.GetComponent<SkinnedMeshRenderer>().materials[i].SetColor("_EmissionColor", ColorOn);
            BirdModel.GetComponent<SkinnedMeshRenderer>().materials[i].EnableKeyword("_EMISSION");
        }
        teammateDeadParticle.Play();
    }
    //当队友复活之后，关闭所有特效恢复至平常状态
    public void OnTeammateReborn()
    {
        for (int i = 0; i <= 4; i++)
        {
            //BirdModel.GetComponent<SkinnedMeshRenderer>().materials[i].CopyPropertiesFromMaterial(NormalMaterial);
            BirdModel.GetComponent<SkinnedMeshRenderer>().materials[i].DisableKeyword("_EMISSION");
        }
        teammateDeadParticle.Stop();
    }

    IEnumerator Disappear()
    {
        while (BirdModel.GetComponent<SkinnedMeshRenderer>().material.color.a > 0)
        {
            Color temp = BirdModel.GetComponent<SkinnedMeshRenderer>().material.color;
            temp.a -= 0.1f;
            BirdModel.GetComponent<SkinnedMeshRenderer>().material.color = temp;
            yield return EnumeratorList.waitForOneMSecond;
        }
        
    }

    public void OnPlayerDead()
    {
        OnPlayDeadParticle();
        OnPlaySmokeParticle();
    }

    public void ClosePlayerDead()
    {
        OnStopDeadParticle();
        OnStopSmokeParticle();
    }


    public void OnPlayDeadParticle()
    {
        playerDeadParticle.Play();
    }

    public void OnStopDeadParticle()
    {
        playerDeadParticle.Stop();
    }

    public void OnPlaySmokeParticle()
    {
        playerDeadSmoke.Play();
    }

    public void OnStopSmokeParticle()
    {
        playerDeadSmoke.Stop();
    }


    public void OnRestart()
    { 
        
    }
}
    