using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�˽ű����ڵ��⸴��㣬ֻ��Ҫ������ҿ�ʼ��ʱ���λ�ü��ɣ�����������Ϸ��ʼ��ʱ���Զ�����
public class StartPointLocate : MonoBehaviour
{
    public GameObject Player;

    public void Start()
    {
        gameObject.transform.position = Player.transform.position;
    }
}
