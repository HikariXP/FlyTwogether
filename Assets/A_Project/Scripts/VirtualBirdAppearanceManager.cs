using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//VirtualBird的外观（透明度）统一用这个组件管理
public class VirtualBirdAppearanceManager : MonoBehaviour
{
    public static VirtualBirdAppearanceManager Instance { get; private set; }

    public GameObject VirtualBird1;
    public GameObject VirtualBird2;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        FeelingManager.Instance.CooperationChangeEvent += ChangeVirtualBirdAlpha;
        GameManager.Instance.PlayerAllRebornEvent += ChangeVirtualBirdAlpha;
    }

    public void ChangeVirtualBirdAlpha()
    {
        ChangeMaterialAlpha(VirtualBird1);
        ChangeMaterialAlpha(VirtualBird2);
    }

    private void ChangeMaterialAlpha(GameObject vBird)
    {
        float temp = 0;
        temp = FeelingManager.Instance.cooperation / 10f;
        for (int i = 0; i <= 4; i++)
        {
            vBird.gameObject.GetComponent<SkinnedMeshRenderer>().materials[i].color = new Color(1, 1, 1, temp);
        }
    }
}
