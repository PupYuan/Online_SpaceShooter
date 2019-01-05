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
        public CtrlType ctrlType = CtrlType.player;

        private void FixedUpdate()
        {
            Simulate();
        }
        //和原本直接采用Input.GetKey得到的向量改变速度相比，我们采取离散的
        public CommandInput CollectCommandInput()
        {
            CommandInput input = new CommandInput();
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
            return input;
        }

        public void Simulate()
        {
            //每次模拟都把采集的指令发给服务器
            if(ctrlType == CtrlType.player)
            {
                Command cmd = new Command();
                cmd.input = CollectCommandInput();      // 获取指令
                //ExecuteCommand(cmd);                    // 执行指令
                SendCommand(cmd);
            }
        }

        public void SendCommand(Command cmd)
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("SyncCommand");
            proto.AddFloat(cmd.input.x);
            proto.AddFloat(cmd.input.y);

            NetMgr.srvConn.Send(proto);
        }

        public void ExecuteCommand(Command command)
        {
            float movingSpeed = 4;
            Vector3 movingDir = Vector3.zero;

            movingDir.x = command.input.x;
            movingDir.z = command.input.y;

            Vector3 velocity = movingDir * movingSpeed;                                  //通过输入计算出速度

            transform.position = transform.position + velocity * Time.fixedDeltaTime;    //立即计算出结果

            command.result.position = transform.position;                                //将结果保存到CommandResult中
        }
    }
}



