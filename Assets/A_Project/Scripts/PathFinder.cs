using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public delegate void OnFinalPointAction();
    public event OnFinalPointAction FinalPointEvent;

    public delegate void OnPathEndAction();
    public event OnPathEndAction PathEndEvent;

    [Header("ѡ������")]
    [Tooltip("������������ĸ�����")]
    public GameObject targetPath;

    public GameObject Player;
    [Tooltip("Ϊ��ʱ�����ƶ�")]
    public bool canMove = false;
    [Tooltip("�ƶ��ٶ�")]
    public float speed = 5;
    [Tooltip("��ת���ٶȣ���λ:��/��")]
    public int rotateSpeed = 90;


    public Transform[] PathPoints;

    //����Ŀ������Զ��ʱ������Ϊ��һ��Ŀ��
    public float LeastDistanceBetweenTarget;

    public int index;

    public Transform target; //��׼��Ŀ��
    public Vector3 V3speed; //�ڵ����������ٶ�
    public Vector3 lastSpeed; //�洢ת��ǰ�ڵ��ı��������ٶ�
     
    Vector3 finalForward; //Ŀ�굽�������ߵ����������ճ���
    float angleOffset;  //�Լ���forward�����mFinalForward֮��ļн�

    //��FollowΪ��ʱ��canMove��Ϊ�٣����PathFinder��ֱ�Ӹ�������һ�������ҵĸ���λ�ȴ�
    //���ָ����ƽʱ�ĸ��治һ��������ȫ���Ÿ�������ת�Լ�λ�ý��и��̣��Ҹ���֮ǰ����GMִ��һ�θ���
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

    //���ڼ��PathFinder��;��ײ������·���㻹�Ǽ���
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
        //��������յ��򴥷���Ϸ������ز���
        if (other.gameObject.CompareTag("FinalPoint"))
        {
            FinalPointEvent?.Invoke();
            ChangeToNextPathPoint(other);
            //���������ǵ����յ�֮�󻹻���ǰ��һ���㣬�Ǹ������С����λ�ã�ͬʱ����С���½��Ķ���
            //���Ի��������ǰ�����������һ����֮��Ͳ��ᶯ
        }
        if (other.gameObject.CompareTag("PathEndPoint"))
        {
            //������������������ϵ�ֹͣ����Path��ʶ���ϻ��������һ�����Tag�޸�
            canMove = false;
            
            PathEndEvent?.Invoke();
            Debug.Log("PathEndPoint");
        }

        //ChangeNextPathPoint(other);
    }

    private void ChangeToNextPathPoint()
    {
        ////A.plan ˳��·�����������֮��������+1
        //index++;
        //if (index != PathPoints.Length)
        //{
        //    target = PathPoints[index];
        //}
        //if (index == PathPoints.Length)
        //{
        //    //transform.position = PathPoints[index - 1].position;

        //    //�����һ����·�������������ע��ȡ����
        //    //canMove = false;
        //}
        
    }
    private void ChangeToNextPathPoint(Collider collider)
    {
        //B.plan ���������֮������������һ��ΪĿ���
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

    //B.plan �����κ������������׷�ٵ�ǰ��������һ�����
    //private void ChangeNextPathPoint(Collider other)
    //{
    //    foreach (Transform tr in PathPoints)
    //    {
            
    //    }
    //}


    //����GameManagerˢ��CheckPoint��¼
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

    //��ת��ʹ�䳯��Ŀ��㣬Ҫ�ı��ٶȵķ���
    void UpdateRotation()
    {
        //�Ƚ��ٶ�תΪ�������꣬��ת֮���ٱ�Ϊ��������
        lastSpeed = transform.InverseTransformDirection(V3speed);

        ChangeForward(rotateSpeed * Time.deltaTime);

        V3speed = transform.TransformDirection(lastSpeed);
    }

    void ChangeForward(float speed)
    {
        //���Ŀ��㵽����ĳ���
        finalForward = (target.position - transform.position).normalized;
        if (finalForward != transform.forward)
        {
            angleOffset = Vector3.Angle(transform.forward, finalForward);
            if (angleOffset > rotateSpeed)
            {
                angleOffset = rotateSpeed;
            }
            //������forward��������ת�����ճ���
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
