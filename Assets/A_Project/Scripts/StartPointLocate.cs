using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此脚本用于调衡复活点，只需要调整玩家开始的时候的位置即可，复活点会在游戏开始的时候自动贴上
public class StartPointLocate : MonoBehaviour
{
    public GameObject Player;

    public void Start()
    {
        gameObject.transform.position = Player.transform.position;
    }
}
