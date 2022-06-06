using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public delegate void OnFinalPointAction();
    public event OnFinalPointAction FinalPointEvent;

    public delegate void OnPathEndAction();
    public event OnPathEndAction PathEndEvent;

    [Header("选择赛道")]
    [Tooltip("这里放入赛道的父对象")]
    public GameObject targetPath;

    public GameObject Player;
    [Tooltip("为真时才能移动")]
    public bool canMove = false;
    [Tooltip("移动速度")]
    public float speed = 5;
    [Tooltip("旋转的速度，单位:度/秒")]
    public int rotateSpeed = 90;


    public Transform[] PathPoints;

    //距离目标距离多远的时候设置为下一个目标
    public float LeastDistanceBetweenTarget;

    public int index;

    public Transform target; //瞄准的目标
    public Vector3 V3speed; //炮弹本地坐标速度
    public Vector3 lastSpeed; //存储转向前炮弹的本地坐标速度
     
    Vector3 finalForward; //目标到自身连线的向量，最终朝向
    float angleOffset;  //自己的forward朝向和mFinalForward之间的夹角

    //当Follow为真时，canMove必为假，随后PathFinder将直接跟随于另一名存活玩家的复活位等待
    //这种跟随跟平时的跟随不一样，将完全对着复活点的旋转以及位置进行复刻，且跟随之前会由GM执行一次复刻
    public bool Follow;
    public Transform FollowTarget;

    // Use this for initialization

    void Awake()
    {
        V3speed = new Vector3(0, 0, speed);
        index = 0;
    }

    private void Start()
    {
        PathPoints = targetPath.GetComponent<CheckPointList>().PathPoints;
        target = PathPoints[0];

    }

    // Update is called once per frame

    void Update()
    {

    }



    void MoveTo()
    {
        if (index > PathPoints.Length - 1)
        {
            //return;
            index = 0;
        }
        //transform.Translate((PathPoints[index].position -
        //    transform.position).normalized * speed);
        ////transform.position = Vector3.Lerp(transform.position, checkPoints[index].position, speed);
        ////transform.position = Vector3.Slerp(transform.position, checkPoints[index].position, speed);
        //Player.transform.rotation = Quaternion.Slerp(Player.transform.rotation, PathPoints[index].rotation, rotateSpeed);
        
        UpdatePosition();
        UpdateRotation();
        //if (Vector3.Distance(PathPoints[index].position, transform.position) < LeastDistanceBetweenTarget)
        //{
        //}
    }

    void FollowTo()
    {
        gameObject.transform.position = FollowTarget.position;
        gameObject.transform.rotation = FollowTarget.rotation;
    }

    //用于检测PathFinder中途碰撞到的是路径点还是检测点
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PathPoint"))
        {
            ChangeToNextPathPoint(other);
        }
        if (other.gameObject.CompareTag("CheckPoint"))
        {
            ChangeToNextPathPoint(other);
            CallGameManagerRecordCheckPoint();
            if (GameManager.Instance.CheckPointIndex == 3)
            {
                MusicManager.Instance.OnToSnowMountain();
            }
        }
        //如果过了终点则触发游戏结束相关操作
        if (other.gameObject.CompareTag("FinalPoint"))
        {
            FinalPointEvent?.Invoke();
            ChangeToNextPathPoint(other);
            //暂且做法是到达终点之后还会往前走一个点，那个点就是小岛的位置，同时播放小鸟下降的动画
            //所以还会继续往前，但到达最后一个点之后就不会动
        }
        if (other.gameObject.CompareTag("PathEndPoint"))
        {
            //到这里则会真正意义上的停止，在Path的识别上会加入对最后一个点的Tag修改
            canMove = false;
            
            PathEndEvent?.Invoke();
            Debug.Log("PathEndPoint");
        }

        //ChangeNextPathPoint(other);
    }

    private void ChangeToNextPathPoint()
    {
        ////A.plan 顺着路线碰到区域点之后索引就+1
        //index++;
        //if (index != PathPoints.Length)
        //{
        //    target = PathPoints[index];
        //}
        //if (index == PathPoints.Length)
        //{
        //    //transform.position = PathPoints[index - 1].position;

        //    //如果是一次性路径，把下面这句注释取消掉
        //    //canMove = false;
        //}
        
    }
    private void ChangeToNextPathPoint(Collider collider)
    {
        //B.plan 碰到区域点之后以区域点的下一个为目标点
        for(int i = 0;i<PathPoints.Length-1;i++)
        {
            if (collider.transform == PathPoints[i].transform)
            {
                if (i <= PathPoints.Length - 2)
                {
                    index = i;
                    target = PathPoints[(i + 1)];
                }
                break;
            }
        }
    }

    public void RefreshTarget()
    {
        target = PathPoints[index];
    }

    //B.plan 碰到任何区域点则立刻追踪当前区域点的下一区域点
    //private void ChangeNextPathPoint(Collider other)
    //{
    //    foreach (Transform tr in PathPoints)
    //    {
            
    //    }
    //}


    //激活GameManager刷新CheckPoint记录
    private void CallGameManagerRecordCheckPoint()
    {
        if (GameManager.Instance.CheckPointIndex != index)
        {
            GameManager.Instance.checkSpeed = V3speed;
            GameManager.Instance.checkLeastSpeed = lastSpeed;
            GameManager.Instance.RotateQ = gameObject.transform.rotation;
            GameManager.Instance.CheckPointAreaNo += 1;
            GameManager.Instance.OnCallWeatherManagerChangeSky();
        }
        GameManager.Instance.OnPlayerToCheckPoint(index);

    }

    void UpdatePosition()
    {
        transform.position = transform.position + V3speed * Time.deltaTime;
    }

    //旋转，使其朝向目标点，要改变速度的方向
    void UpdateRotation()
    {
        //先将速度转为本地坐标，旋转之后再变为世界坐标
        lastSpeed = transform.InverseTransformDirection(V3speed);

        ChangeForward(rotateSpeed * Time.deltaTime);

        V3speed = transform.TransformDirection(lastSpeed);
    }

    void ChangeForward(float speed)
    {
        //获得目标点到自身的朝向
        finalForward = (target.position - transform.position).normalized;
        if (finalForward != transform.forward)
        {
            angleOffset = Vector3.Angle(transform.forward, finalForward);
            if (angleOffset > rotateSpeed)
            {
                angleOffset = rotateSpeed;
            }
            //将自身forward朝向慢慢转向最终朝向
            transform.forward = Vector3.Lerp(transform.forward, finalForward, speed / angleOffset);
        }
    }

    public void ChangeMoveStatus()
    {
        canMove = canMove == true ? false : true;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            Follow = false;
            MoveTo();
        }
        if (Follow)
        {
            canMove = false;
            FollowTo();
        }
    }
}
