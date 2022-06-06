using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointList : MonoBehaviour
{
    public Transform[] PathPoints;

    public List<Transform> CheckPoints;

    void Awake()
    {
        //获取子对象数量
        int count = transform.childCount;
        //声明列表
        PathPoints = new Transform[count];
        //按着顺序从Hierarchy面板从上往下读取子对象作为路径点节点
        for (int i = 0; i < count; i++)
        {
            PathPoints[i] = transform.GetChild(i);
        }
        PathPoints[count - 2].gameObject.tag = "FinalPoint";
        PathPoints[count - 1].gameObject.tag = "PathEndPoint";
    }


}
