using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Command
{
    public Command()
    {
        input = new CommandInput();
    }
    public uint sequence;          //指令序号

    public CommandInput input;  //操作指令的输入
}

//本游戏的输入类型为，横轴值horizontal，竖轴值vertical，开火trigger
public class CommandInput
{
    public float x;
    public float y;
    public bool fire;
}

