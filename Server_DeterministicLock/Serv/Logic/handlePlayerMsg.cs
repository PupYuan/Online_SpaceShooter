using System;

public partial class HandlePlayerMsg
{
    //Matching 
    public void MsgMatching(Player player, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        //事件触发
        ServNet.instance.handlePlayerEvent.OnMatching(player);
    }

    //获取玩家列表
    public void MsgGetList(Player player, ProtocolBase protoBase)
	{
		Scene.instance.SendPlayerList (player);
	}
	
	//更新信息
	public void MsgSyncCommand(Player player, ProtocolBase protoBase)
	{
		//获取数值
		int start = 0;
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		string protoName = protocol.GetString (start, ref start);
        uint KeyFrameNumber = protocol.GetUint(start, ref start);
        Fix64 x = protocol.GetFix(start, ref start);
        Fix64 z = protocol.GetFix(start, ref start);
        Command cmd = new Command();
        cmd.input.x = x;
        cmd.input.z = z;
        //多线程处理消息，此时需要防止竞争
        lock (ServNet.instance.command_list)
        {
            //使用Enqueue的话，我们假定了接收到的包都是顺序的，并且不会丢包；若出现这种情况怎么办？
            ServNet.instance.command_list[player.id].Enqueue(cmd);
            //在这里判断是否收到了所有玩家下一关键帧的控制信息
            foreach (var item in ServNet.instance.command_list)
            {
                //有玩家的关键帧还未送达
                if (item.Value.Count == 0)
                    return;
            }
            //所有玩家的关键帧都送达，所有玩家的控制消息队列都出列一项，并广播给所有客户
            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("SyncCommand");
            protocolRet.AddUint(KeyFrameNumber + 5);//K1
            protocolRet.AddUint(KeyFrameNumber + 10);//K2
            protocolRet.AddInt(ServNet.instance.command_list.Count);
            foreach (var item in ServNet.instance.command_list)
            {
                protocolRet.AddString(item.Key);

                Console.WriteLine("item.Key " + item.Key);

                Command _cmd = item.Value.Dequeue();
                protocolRet.AddFix(_cmd.input.x);
                protocolRet.AddFix(_cmd.input.z);
            }
            ServNet.instance.Broadcast(protocolRet);
        }
	}
}