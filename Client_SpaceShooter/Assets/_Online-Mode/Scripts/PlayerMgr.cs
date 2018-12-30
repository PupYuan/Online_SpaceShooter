using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GamePlayController负责每局游戏的逻辑控制，使用NetMgr的网络接口
//SpaceShooter的游戏逻辑包括：初始化玩家
public class PlayerMgr : MonoBehaviour
{

    public GameObject[] PlayerPrefab;
    //通过id来更新
    private Dictionary<string, PlayerController> m_playerControllerList = new Dictionary<string, PlayerController>();
    private void Start()
    {
        int Count = 0;//生成玩家时给予一个偏移量
        foreach (var player in GameMgr.instance.player_list)
        {
            GameObject player_obj = (GameObject)Instantiate(PlayerPrefab[0], new Vector3(1, 0, 0) * Count, Quaternion.identity);
            player_obj.name = player.id;//场景中生成的GameObject名字改成id
            if (player.id != GameMgr.instance.local_player_ID)
            {
                player_obj.GetComponent<PlayerController>().ctrlType = CtrlType.net;//网络同步
                player_obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;//这里暂时不计入对网络玩家的碰撞
                //player_obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;//网络玩家的速度不会被限制
                m_playerControllerList.Add(player.id, player_obj.GetComponent<PlayerController>());//加入的是引用，而不会新建PlayerController
            }
            Count++;
        }
        NetMgr.srvConn.msgDist.AddListener("SyncMotionState", SyncMotionState);
        NetMgr.srvConn.msgDist.AddListener("SyncPlayerFire", SyncPlayerFire);
        NetMgr.srvConn.msgDist.AddListener("SyncPlayerDie", SyncPlayerDie);
    }
    private void OnDestroy()
    {
        NetMgr.srvConn.msgDist.DelListener("SyncMotionState", SyncMotionState);
        NetMgr.srvConn.msgDist.DelListener("SyncPlayerFire", SyncPlayerFire);
        NetMgr.srvConn.msgDist.DelListener("SyncPlayerDie", SyncPlayerDie);
    }
    public void SyncPlayerFire(ProtocolBase proto)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        Debug.Log("SyncPlayerFire ");
        if (protoName != "SyncPlayerFire")
            return;
        string player_id = protocol.GetString(start, ref start);

        float pos_x = protocol.GetFloat(start, ref start);
        float pos_y = protocol.GetFloat(start, ref start);
        float pos_z = protocol.GetFloat(start, ref start);
        float rot_x = protocol.GetFloat(start, ref start);
        float rot_y = protocol.GetFloat(start, ref start);
        float rot_z = protocol.GetFloat(start, ref start);

        Vector3 position = new Vector3(pos_x, pos_y, pos_z);
        Vector3 rotation = new Vector3(rot_x, rot_y, rot_z);

        //根据id去更新PlayerController的信息
        if (!m_playerControllerList.ContainsKey(player_id))
        {
            Debug.Log("SyncPlayerFire pc == null ");
            return;
        }
        PlayerController pc = m_playerControllerList[player_id];
        if (player_id == GameMgr.instance.local_player_ID)//本地玩家的同步信息省略
            return;
        pc.RecvFire(position, rotation);
    }

    public void SyncPlayerDie(ProtocolBase proto)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        Debug.Log("SyncPlayerDie ");
        if (protoName != "SyncPlayerDie")
            return;
        string player_id = protocol.GetString(start, ref start);
        //根据id去更新PlayerController的信息
        if (!m_playerControllerList.ContainsKey(player_id))
        {
            Debug.Log("SyncPlayerDie pc == null ");
            return;
        }
        PlayerController pc = m_playerControllerList[player_id];
        if (player_id == GameMgr.instance.local_player_ID)//本地玩家的同步信息省略
            return;
        pc.RecvDie();
    }

    //同步玩家信息，使用影子跟随算法
    //该函数调用大约以每秒5次的频率调用，在这里看看是否有别的优化方案
    public void SyncMotionState(ProtocolBase proto)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        if (protoName != "SyncMotionState")
            return;
        string player_id = protocol.GetString(start, ref start);

        float pos_x = protocol.GetFloat(start, ref start);
        float pos_y = protocol.GetFloat(start, ref start);
        float pos_z = protocol.GetFloat(start, ref start);
        float rot_x = protocol.GetFloat(start, ref start);
        float rot_y = protocol.GetFloat(start, ref start);
        float rot_z = protocol.GetFloat(start, ref start);
        float vel_x = protocol.GetFloat(start, ref start);
        float vel_y = protocol.GetFloat(start, ref start);
        float vel_z = protocol.GetFloat(start, ref start);

        //同步位置、转向
        Vector3 _position = new Vector3(pos_x, pos_y, pos_z);
        Vector3 _rotation = new Vector3(rot_x, rot_y, rot_z);
        Vector3 _velocity = new Vector3(vel_x, vel_y, vel_z);

        //根据id去更新PlayerController的信息
        Debug.Log("SyncMotionState " + player_id);
        if (!m_playerControllerList.ContainsKey(player_id))
        {
            Debug.Log("SyncMotionState pc == null ");
            return;
        }
        PlayerController pc = m_playerControllerList[player_id];
        if (player_id == GameMgr.instance.local_player_ID)//本地玩家的同步信息省略
            return;

        MotionState recv_state = new MotionState();
        recv_state.position = _position;
        recv_state.rotation = _rotation;
        recv_state.velocity = _velocity;

        pc.RecvMotionState(recv_state);
    }
}
