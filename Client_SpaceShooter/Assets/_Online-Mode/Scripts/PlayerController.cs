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
    public float lastSyncTime;
}

public enum SendStrategy
{
    timeInterval,
    deadReckoning
}

//位置修正方法 
public enum PositionFix
{
    DirectSetPosition,
    LinearInterpolation,
    CubicInterpolation
}

public class PlayerController : MonoBehaviour
{
    public SendStrategy send_strategy = SendStrategy.deadReckoning;
    public PositionFix position_fix = PositionFix.DirectSetPosition;

    public SyncPlayerState lastState;//根据同步过来的状态插值得到的新状态，影子跟随算法中的“影子”
    public CtrlType ctrlType = CtrlType.player;
    public float speed;
    public float tilt;
    public Done_Boundary boundary;
    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;
    //上次发起同步的时间
    [HideInInspector]
    public float lastSendTime = float.MinValue;
    public float syncFrequency = 5.0f;//默认为5HZ
    public float deadReckoningThreshold = 1f;//位置差向量的模
    //航位预测的位置                                       
    private Vector3 drPostion = new Vector3(0, 0, 0);
    //网络驱动的位移速度
    private Vector3 velocity = Vector3.zero;
    private float nextFire;

    //采用线性插值平滑修正位置时所需的参数
    Vector3 forcastPosition = new Vector3(0, 0, 0);
    Vector3 startPosition = new Vector3(0, 0, 0);
    [HideInInspector]
    public float syncDelta = 1;
    float smoothTick = float.MinValue;//用于平滑状态的计数器
    Vector3 _originVeclocity;//用于记录进入平滑状态前的速度
    bool hasSetVelocity = false;//只设置一次速度
    //采用立方体插值平滑修正位置时所需的额外参数（相较于线性插值）
    float A, B, C, D, E, F, G, H;


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
        SendPlayerFire();
        nextFire = Time.time + fireRate;
        GameObject bulletObj = Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        bulletObj.GetComponent<Bullet>().attackerID = GameMgr.instance.local_player_ID;
        GetComponent<AudioSource>().Play();
    }

    //死亡，并且同步信息到其他玩家实体上
    public void Die()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("SyncPlayerDie");

        NetMgr.srvConn.Send(proto);
        RecvDie();
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
        else if(ctrlType == CtrlType.net)
        {
            if (position_fix == PositionFix.LinearInterpolation)
            {
                //当处于平滑状态的时候
                if (smoothTick > 0)
                {
                    transform.position = startPosition + (forcastPosition - startPosition) * (1 - smoothTick / syncDelta);
                    smoothTick -= Time.deltaTime;
                }
                else
                {
                    transform.position += velocity * Time.deltaTime;
                }
                //else if (!hasSetVelocity)//设置一次速度
                //{
                //    hasSetVelocity = true;
                //    //GetComponent<Rigidbody>().velocity = _originVeclocity;
                //}

            }
            else if (position_fix == PositionFix.CubicInterpolation)
            {
                //当处于平滑状态的时候
                if (smoothTick > 0)
                {
                    float dt = (1 - smoothTick / syncDelta);
                    Vector3 cur_position = new Vector3();//三次样条插值的位置
                    cur_position.x = A * dt * dt * dt + B * dt * dt + C * dt + D;
                    cur_position.z = E * dt * dt * dt + F * dt * dt + G * dt + H;
                    transform.position = cur_position;
                    smoothTick -= Time.deltaTime;
                }
                else
                {
                    transform.position += velocity*Time.deltaTime;
                }

                //else if (!hasSetVelocity)//设置一次速度
                //{
                //    hasSetVelocity = true;
                //    //GetComponent<Rigidbody>().velocity = _originVeclocity;
                //}
            }
        }
    }

    public void SendPlayerFire()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("SyncPlayerFire");
        proto.AddFloat(shotSpawn.position.x);
        proto.AddFloat(shotSpawn.position.y);
        proto.AddFloat(shotSpawn.position.z);
        proto.AddFloat(shotSpawn.rotation.x);
        proto.AddFloat(shotSpawn.rotation.y);
        proto.AddFloat(shotSpawn.rotation.z);

        NetMgr.srvConn.Send(proto);
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

    public void RecvPlayerState(SyncPlayerState recv_state)
    {
        if (position_fix == PositionFix.DirectSetPosition)
        {
            //修正位置
            transform.position = recv_state.position;
            transform.eulerAngles = recv_state.rotation;
        }
        else if(position_fix == PositionFix.LinearInterpolation)
        {
            syncDelta = Time.time - lastState.lastSyncTime;
            lastState.lastSyncTime = Time.time;
            smoothTick = syncDelta;

            forcastPosition = recv_state.position + recv_state.velocity * syncDelta;
            startPosition = transform.position;

            velocity = recv_state.velocity;

            _originVeclocity = recv_state.velocity;
            //hasSetVelocity = false;
        }
        else if (position_fix == PositionFix.CubicInterpolation)
        {
            syncDelta = Time.time - lastState.lastSyncTime;
            lastState.lastSyncTime = Time.time;
            smoothTick = syncDelta;

            Vector3 pos1 = transform.position;
            Vector3 pos4 = recv_state.position + recv_state.velocity * syncDelta;//这里用线性的速度预测
            Vector3 pos2 = pos1 + GetComponent<Rigidbody>().velocity * 0.1f;
            Vector3 pos3 = pos4 - recv_state.velocity* 0.1f;
            A = pos4.x - 3 * pos3.x + 3 * pos2.x - pos1.x;
            B = 3 * pos3.x - 6 * pos2.x + 3 * pos1.x;
            C = 3 * pos2.x - 3 * pos1.x;
            D = pos1.x;
            E = pos4.z - 3 * pos3.z + 3 * pos2.z - pos1.z;
            F = 3 * pos3.z - 6 * pos2.z + 3 * pos1.z;
            G = 3 * pos2.z - 3 * pos1.z;
            H = pos1.z;

            velocity = recv_state.velocity; 

            _originVeclocity = recv_state.velocity;
            //清空物理速度
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            //hasSetVelocity = false;
        }
    }

    public void RecvFire(Vector3 _position, Vector3 _rotation)
    {
        Instantiate(shot, _position, Quaternion.EulerAngles(_rotation));
        GetComponent<AudioSource>().Play();
    }

    public void RecvDie()
    {
        gameObject.SetActive(false);
        GameController.instance.PlayerDie();
    }
}
