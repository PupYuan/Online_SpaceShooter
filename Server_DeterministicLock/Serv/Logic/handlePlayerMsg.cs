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
        float x = protocol.GetFloat(start, ref start);
        float y = protocol.GetFloat(start, ref start);
        Command cmd = new Command();
        cmd.input.x = x;
        cmd.input.y = y;
        //使用Enqueue的话，我们假定了接收到的包都是顺序的，并且不会丢包；若出现这种情况怎么办？
        ServNet.instance.command_list[player.id].Enqueue(cmd);
        //在这里判断是否收到了所有玩家下一关键帧的控制信息
        foreach(var item in ServNet.instance.command_list)
        {
            //有玩家的关键帧还未送达
            if (item.Value.Count == 0)
                return;
        }
        //所有玩家的关键帧都送达，所有玩家的控制消息队列都出列一项，并广播给所有客户
		//广播
		ProtocolBytes protocolRet = new ProtocolBytes();
		protocolRet.AddString ("SyncCommand");
        protocolRet.AddInt(ServNet.instance.command_list.Count);
        foreach (var item in ServNet.instance.command_list)
        {
            protocolRet.AddString(item.Key);

            Command _cmd = item.Value.Dequeue();
            protocolRet.AddFloat(_cmd.input.x);
            protocolRet.AddFloat(_cmd.input.y);
        }
        ServNet.instance.Broadcast (protocolRet);
	}
}