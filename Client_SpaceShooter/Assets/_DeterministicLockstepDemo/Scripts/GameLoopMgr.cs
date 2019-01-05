using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeterministicLockstepDemo
{
    //GameLoopMgr负责游戏的游戏循环
    public class GameLoopMgr : MonoBehaviour
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

                }
                m_playerControllerList.Add(player.id, player_obj.GetComponent<PlayerController>());//加入的是引用，而不会新建PlayerController
                Count++;
            }
            NetMgr.srvConn.msgDist.AddListener("SyncCommand", SyncCommand);
        }


        private void OnDestroy()
        {
            NetMgr.srvConn.msgDist.DelListener("SyncCommand", SyncCommand);
        }

        public void SyncCommand(ProtocolBase proto)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)proto;
            string protoName = protocol.GetString(start, ref start);
            if (protoName != "SyncCommand")
                return;
            //每次接受到的都是所有玩家的指令
            int player_num = protocol.GetInt(start, ref start);

            string player_id;
            float x, y;
            Command cmd;
            for (int i = 0; i < player_num; i++)
            {
                player_id = protocol.GetString(start, ref start);
                x = protocol.GetFloat(start, ref start);
                y = protocol.GetFloat(start, ref start);
                cmd = new Command();
                cmd.input.x = x;
                cmd.input.y = y;
                cmd.input.fire = false;
                Debug.Log("player_id :" + player_id);
                m_playerControllerList[player_id].ExecuteCommand(cmd);
            }
        }
    }
}

