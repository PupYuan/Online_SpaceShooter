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
	public void MsgSyncPlayerState(Player player, ProtocolBase protoBase)
	{
		//获取数值
		int start = 0;
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		string protoName = protocol.GetString (start, ref start);

        float pos_x = protocol.GetFloat(start, ref start);
        float pos_y = protocol.GetFloat(start, ref start);
        float pos_z = protocol.GetFloat(start, ref start);
        float rot_x = protocol.GetFloat(start, ref start);
        float rot_y = protocol.GetFloat(start, ref start);
        float rot_z = protocol.GetFloat(start, ref start);
        float vel_x = protocol.GetFloat(start, ref start);
        float vel_y = protocol.GetFloat(start, ref start);
        float vel_z = protocol.GetFloat(start, ref start);

        //Scene.instance.UpdateInfo (player.id, x, y, z, score);
		//广播
		ProtocolBytes protocolRet = new ProtocolBytes();
		protocolRet.AddString ("SyncPlayerState");
		protocolRet.AddString (player.id);
		protocolRet.AddFloat (pos_x);
		protocolRet.AddFloat (pos_y);
		protocolRet.AddFloat (pos_z);
        protocolRet.AddFloat(rot_x);
        protocolRet.AddFloat(rot_y);
        protocolRet.AddFloat(rot_z);
        protocolRet.AddFloat(vel_x);
        protocolRet.AddFloat(vel_y);
        protocolRet.AddFloat(vel_z);

        ServNet.instance.Broadcast (protocolRet);
	}

    //更新信息
    public void MsgSyncPlayerFire(Player player, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);

        float pos_x = protocol.GetFloat(start, ref start);
        float pos_y = protocol.GetFloat(start, ref start);
        float pos_z = protocol.GetFloat(start, ref start);
        float rot_x = protocol.GetFloat(start, ref start);
        float rot_y = protocol.GetFloat(start, ref start);
        float rot_z = protocol.GetFloat(start, ref start);

        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("SyncPlayerFire");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(pos_x);
        protocolRet.AddFloat(pos_y);
        protocolRet.AddFloat(pos_z);
        protocolRet.AddFloat(rot_x);
        protocolRet.AddFloat(rot_y);
        protocolRet.AddFloat(rot_z);

        ServNet.instance.Broadcast(protocolRet);
    }

    //更新信息
    public void MsgSyncPlayerDie(Player player, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("SyncPlayerDie");
        protocolRet.AddString(player.id);

        ServNet.instance.Broadcast(protocolRet);
    }
    
}