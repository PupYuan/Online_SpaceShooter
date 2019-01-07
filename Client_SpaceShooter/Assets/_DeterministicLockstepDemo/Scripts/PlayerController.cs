using UnityEngine;
using System.Collections;
namespace DeterministicLockstepDemo
{
    public enum CtrlType
    {
        none,
        player,
        computer,
        net,
    }
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector]
        public string player_id;
        private uint sequence = 0;//当前帧号
        public uint KeyFrameInterval = 5;//固定法：关键帧之前相差固定帧数
        public uint KeyFrameNumber = 0;//下一关键帧的帧号
        public Fix64 movingSpeed = (Fix64)8;
        public CtrlType ctrlType = CtrlType.player;
        public FixVector3 logicPosition = FixVector3.Zero;//逻辑位置，用于实际碰撞判断以及网络同步

        //发送最开始的关键帧，此时关键帧号为0
        private void Start()
        {
            SendCommand();//发送最开始的关键帧，帧号为0
            KeyFrameNumber += KeyFrameInterval;//开始五帧的模拟为空
            logicPosition.x = (Fix64)transform.position.x;
            logicPosition.y = (Fix64)transform.position.y;
            logicPosition.z = (Fix64)transform.position.z;
        }

        private void FixedUpdate()
        {
            Simulate();
        }
        //和原本直接采用Input.GetKey得到的向量改变速度相比，我们采取离散的
        public CommandInput CollectCommandInput()
        {
            CommandInput input = new CommandInput();
            input.x = (Fix64)Input.GetAxis("Horizontal");
            input.z = (Fix64)Input.GetAxis("Vertical");
            return input;
        }

        public void Simulate()
        {
            //若当前是关键帧
            if(sequence == KeyFrameNumber)
            {
                //检查是否有K1的UPDATE数据
                if (GameLoopMgr.instance.command_list.Count <= 0)
                {
                    return;//LockStep
                }
                else
                {
                    KeyFrameNumber = GameLoopMgr.instance.updateK2;
                    if(ctrlType == CtrlType.player)
                        SendCommand();
                }
            }
            else//若当前处于模拟帧（即输入采取上一次关键帧时的输入I进行模拟）
            {
                if(GameLoopMgr.instance.command_list.ContainsKey(player_id))
                    ExecuteCommand(GameLoopMgr.instance.command_list[player_id]);
                sequence++;
            }
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
            Debug.Log("cmd.input.x :" + cmd.input.x);
            Debug.Log("cmd.input.z :" + cmd.input.z);

            NetMgr.srvConn.Send(proto);
        }

        public void ExecuteCommand(Command command)
        {
            FixVector3 movingDir = FixVector3.Zero;

            movingDir.x = command.input.x;
            movingDir.z = command.input.z;

            Debug.Log("command.input.z :" + command.input.z);

            FixVector3 velocity = movingDir * movingSpeed;                                  //通过输入计算出速度

            
            logicPosition += velocity * (Fix64)Time.fixedDeltaTime;
            command.result.position = logicPosition;                                //将结果保存到CommandResult中
        }
        //渲染位置与逻辑位置分离，每次渲染前移动渲染位置到逻辑位置
        private void Update()
        {
            //transform.position = Vector3.MoveTowards(transform.position, logicPosition.ToVector3(), (float)movingSpeed * Time.deltaTime);
            transform.position = logicPosition.ToVector3();
            Debug.Log("logicPosition :" + logicPosition);
        }
    }
}



