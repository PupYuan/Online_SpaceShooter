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
// TODO：要同步的变换信息
public struct SyncPlayerState
{
    public Vector3 position;
    public Vector3 rotation;
    public bool isStatic;//若静止则可以省略velocity，即节省24字节。
    public Vector3 velocity;//赋值给刚体的velocity
}

public class PlayerController : MonoBehaviour
{
    public SyncPlayerState m_syncPlayerState;//根据同步过来的状态插值得到的新状态，影子跟随算法中的“影子”
    public CtrlType ctrlType = CtrlType.player;
    public float speed;
    public float tilt;
    public Done_Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;
    private float nextFire;
    //上次的同步时间
    [HideInInspector]
    public float lastSyncTime = float.MinValue;
    public float lastSendTime = float.MinValue;
    [HideInInspector]
    public float syncDelta = 1;
    public float syncFrequency = 5.0f;//默认为5HZ
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
            //位置信息在这里修正
            transform.position = Vector3.Lerp(transform.position, m_syncPlayerState.position, syncDelta);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(transform.eulerAngles),
                                              Quaternion.Euler(m_syncPlayerState.rotation), syncDelta);
            //客户端不使用速度进行模拟
            GetComponent<Rigidbody>().velocity = Vector3.Lerp(GetComponent<Rigidbody>().velocity, m_syncPlayerState.velocity, syncDelta);
        }
    }
    //检测是否可以开火不放在Fire函数中，这样可以减少无效的函数调用，增加程序效率。
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
            //每隔0.2秒同步一次，仅仅发送控制类型为玩家的状态信息
            if(Time.time - lastSendTime >= 1.0/syncFrequency){
                lastSendTime = Time.time;
                SendPlayerState();
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
