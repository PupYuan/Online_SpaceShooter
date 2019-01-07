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
        
        
        public Fix64 movingSpeed = (Fix64)8;
        public CtrlType ctrlType = CtrlType.player;
        public FixVector3 logicPosition = FixVector3.Zero;//逻辑位置，用于实际碰撞判断以及网络同步

        private void Awake()
        {
            logicPosition.x = (Fix64)transform.position.x;
            logicPosition.y = (Fix64)transform.position.y;
            logicPosition.z = (Fix64)transform.position.z;
        }

        public void ExecuteCommand(Command command)
        {
            FixVector3 movingDir = FixVector3.Zero;

            movingDir.x = command.input.x;
            movingDir.z = command.input.z;
            FixVector3 velocity = movingDir * movingSpeed;                                  //通过输入计算出速度
            logicPosition += velocity * (Fix64)Time.fixedDeltaTime;
            Debug.Log("(Fix64)Time.fixedDeltaTime :" + (Fix64)Time.fixedDeltaTime);
            command.result.position = logicPosition;                                //将结果保存到CommandResult中
        }
        //渲染位置与逻辑位置分离，每次渲染前移动渲染位置到逻辑位置
        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, logicPosition.ToVector3(), (float)movingSpeed * Time.deltaTime);
        }
    }
}



