using UnityEngine;
using System.Collections;

[System.Serializable]
public class Done_Boundary
{
    public float xMin, xMax, zMin, zMax;
}
public enum CtrlType
{
    none,
    player,
    computer,
    net,
}
public enum SendStrategy
{
    timeInterval,
    deadReckoning
}
// TODO：要同步的变换信息
public struct SyncPlayerState
{
    public Vector3 position;
    public Vector3 rotation;
    public bool isStatic;//若静止则可以省略velocity，即节省24字节。
    public Vector3 velocity;//赋值给刚体的velocity
    public float lastSyncTime;
}

public class PlayerController : MonoBehaviour
{
    public SendStrategy send_strategy = SendStrategy.deadReckoning;
    public SyncPlayerState lastState;//根据同步过来的状态插值得到的新状态，影子跟随算法中的“影子”
    public CtrlType ctrlType = CtrlType.player;
    public float speed;
    public float tilt;
    public Done_Boundary boundary;
    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;
    //上次的同步时间
    [HideInInspector]
    public float lastSendTime = float.MinValue;
    [HideInInspector]
    public float syncDelta = 1;
    public float syncFrequency = 5.0f;//默认为5HZ
    public float deadReckoningThreshold = 1f;//位置差向量的模
    //上次发送的状态                                         
    private Vector3 drPostion = new Vector3(0, 0, 0);
    //网络驱动的位移速度
    private Vector3 velocity = Vector3.zero;
    private float nextFire;

    private void Start()
    {
        lastState.lastSyncTime = float.MinValue;
        SendPlayerState();
    }

    void Update()
    {
        //本地玩家根据Input：键盘输入、鼠标输入来驱动
        if (ctrlType == CtrlType.player)
        {
            if (Input.GetButton("Fire1") && Time.time > nextFire)
                Fire();
        }
        else if (ctrlType == CtrlType.net)
        {
            ////位置信息在这里修正
            //transform.position = Vector3.SmoothDamp(transform.position, m_syncPlayerState.position, ref velocity, syncDelta);
            ////transform.position = Vector3.Lerp(transform.position, m_syncPlayerState.position, syncDelta);
            //transform.rotation = Quaternion.Lerp(Quaternion.Euler(transform.eulerAngles),
            //                                  Quaternion.Euler(m_syncPlayerState.rotation), syncDelta);
            //客户端不使用速度进行模拟
            //GetComponent<Rigidbody>().velocity = Vector3.Lerp(GetComponent<Rigidbody>().velocity, m_syncPlayerState.velocity, syncDelta);
        }
    }
    //检测开火不放在Fire函数中，这样可以减少无效的函数调用，增加程序效率。
    public void Fire()
    {
        nextFire = Time.time + fireRate;
        Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        GetComponent<AudioSource>().Play();
    }
    void FixedUpdate()
    {
        //两种方式改变Rigidbody的velocity，一种是Input.GetAxis，另外一种是网络同步过来的玩家信息
        if (ctrlType == CtrlType.player)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            GetComponent<Rigidbody>().velocity = movement * speed;
        }
        //根据Rigidbody的位置和速度来更新
        GetComponent<Rigidbody>().position = new Vector3
        (
            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
        );
        //网络同步的信息的话，或许会受到直接同步的rotation和同步的velocity的双重影响
        GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);

        if (ctrlType == CtrlType.player)
        {
            if(send_strategy == SendStrategy.timeInterval)
            {
                //平均295 bps
                //每隔0.2秒同步一次，仅仅发送控制类型为玩家的状态信息
                if (Time.time - lastSendTime >= 1.0 / syncFrequency)
                {
                    lastSendTime = Time.time;
                    SendPlayerState();
                }
            }
            else if(send_strategy == SendStrategy.deadReckoning)
            {

                //航位推算，仅仅当航位推算的位置与上次发送的位置差距大于阈值时才发送
                drPostion = lastState.position + lastState.velocity * (Time.time - lastState.lastSyncTime);
                if ((drPostion - transform.position).sqrMagnitude >= deadReckoningThreshold)
                {
                    SendPlayerState();
                }
            }
        }
    }

    public void SendPlayerState()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("SyncPlayerState");
        //位置旋转
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        Vector3 vel = GetComponent<Rigidbody>().velocity;

        //插值：发送方也保留一份发送的状态
        lastState.position = pos;
        lastState.rotation = rot;
        lastState.velocity = vel;
        lastState.lastSyncTime = Time.time;

        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);
        proto.AddFloat(vel.x);
        proto.AddFloat(vel.y);
        proto.AddFloat(vel.z);

        NetMgr.srvConn.Send(proto);
    }
}
