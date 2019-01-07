using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeterministicLockstepDemo
{
    public class Command
    {
        public Command()
        {
            input = new CommandInput();
            result = new CommandResult();
        }
        public uint sequence;          //指令序号

        public CommandInput input;  //操作指令的输入

        public CommandResult result;  //操作指令执行后得到的结果
    }
    //本游戏的输入类型为，横轴值horizontal，竖轴值vertical，开火trigger
    public class CommandInput
    {
        public Fix64 x;
        public Fix64 z;
        public bool fire;
    }

    public class CommandResult
    {
        public FixVector3 position;
    }
}
