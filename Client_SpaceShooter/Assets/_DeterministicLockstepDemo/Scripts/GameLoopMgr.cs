using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeterministicLockstepDemo
{
    //GameLoopMgr负责游戏的游戏循环
    public class GameLoopMgr : MonoBehaviour
    {
        public static GameLoopMgr instance;
        public GameObject[] PlayerPrefab;
        //通过id来更新
        public Dictionary<string, PlayerController> m_playerControllerList = new Dictionary<string, PlayerController>();
        public Dictionary<string, Command> command_list = new Dictionary<string, Command>();

        public uint updateK1 = 0;
        public uint updateK2 = 0;
        private uint sequence = 0;//当前帧号
        public uint KeyFrameInterval = 5;//固定法：关键帧之前相差固定帧数
        public uint KeyFrameNumber = 0;//下一关键帧的帧号
        private void Awake()
        {
            instance = this;
            
        }
        //逻辑循环，目前直接使用FixUpdate
        private void FixedUpdate()
        {
            Simulate();
        }

        public void Simulate()
        {
            //若当前是关键帧
            if (sequence == KeyFrameNumber)
            {
                //检查是否有K1的UPDATE数据
                if (command_list.Count <= 0)
                {
                    Debug.Log("LockStep");
                    return;//LockStep
                }
                else
                {
                    KeyFrameNumber = GameLoopMgr.instance.updateK2;
                    SendCommand();
                }
            }
            else//若当前处于模拟帧（即输入采取上一次关键帧时的输入I进行模拟）
            {
                //遍历所有玩家的controller，并让他们执行指令
                foreach(var player in m_playerControllerList)
                {
                    if (command_list.ContainsKey(player.Key))
                        player.Value.ExecuteCommand(command_list[player.Key]);
                    else Debug.Log("No " + player.Key +" ' s Command");
                }
                sequence++;
            }
        }

        //和原本直接采用Input.GetKey得到的向量改变速度相比，我们采取离散的
        public CommandInput CollectCommandInput()
        {
            CommandInput input = new CommandInput();
            input.x = (Fix64)Input.GetAxis("Horizontal");
            input.z = (Fix64)Input.GetAxis("Vertical");
            return input;
        }

        public void SendCommand()
        {
            Command cmd = new Command();
            cmd.input = CollectCommandInput();      // 获取指令
            cmd.sequence = KeyFrameNumber;

            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("SyncCommand");
            proto.AddUint(cmd.sequence);
            proto.AddFix(cmd.input.x);
            proto.AddFix(cmd.input.z);

            NetMgr.srvConn.Send(proto);
        }


        private void Start()
        {
            SendCommand();//发送最开始的关键帧，帧号为0
            KeyFrameNumber += KeyFrameInterval;//开始五帧的模拟为空
            int Count = 0;//生成玩家时给予一个偏移量
            foreach (var player in GameMgr.instance.player_list)
            {
                GameObject player_obj = (GameObject)Instantiate(PlayerPrefab[0], new Vector3(1, 0, 0) * Count, Quaternion.identity);
                player_obj.name = player.id;//场景中生成的GameObject名字改成id
                player_obj.GetComponent<PlayerController>().player_id = player.id;//每个实体都知道自己的id
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
            updateK1 = protocol.GetUint(start, ref start);
            updateK2 = protocol.GetUint(start, ref start);
            //每次接受到的都是所有玩家的指令
            int player_num = protocol.GetInt(start, ref start);

            string player_id;
            Fix64 x, z;
            Command cmd;
            command_list.Clear();//清空上一帧的command
            for (int i = 0; i < player_num; i++)
            {
                player_id = protocol.GetString(start, ref start);
                x = protocol.GetFix(start, ref start);
                z = protocol.GetFix(start, ref start);
                cmd = new Command();
                cmd.input.x = x;
                cmd.input.z = z;
                cmd.input.fire = false;

                command_list.Add(player_id,cmd);//若不及时清理command_list，则会出现错误
                //m_playerControllerList[player_id].ExecuteCommand(cmd); //不是马上执行，而是加入到commandList中
            }
        }
    }
}

