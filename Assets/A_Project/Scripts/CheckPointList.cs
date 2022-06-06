using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointList : MonoBehaviour
{
    public Transform[] PathPoints;

    public List<Transform> CheckPoints;

    void Awake()
    {
        //��ȡ�Ӷ�������
        int count = transform.childCount;
        //�����б�
        PathPoints = new Transform[count];
        //����˳���Hierarchy���������¶�ȡ�Ӷ�����Ϊ·����ڵ�
        for (int i = 0; i < count; i++)
        {
            PathPoints[i] = transform.GetChild(i);
        }
        PathPoints[count - 2].gameObject.tag = "FinalPoint";
        PathPoints[count - 1].gameObject.tag = "PathEndPoint";
    }


}
