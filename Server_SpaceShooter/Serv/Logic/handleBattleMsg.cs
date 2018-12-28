using System;

public partial class HandlePlayerMsg
{
    //更新Hazard死亡
    public void MsgSyncHazardDie(Player player, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string hazardID = protocol.GetString(start, ref start);
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("SyncHazardDie");
        protocolRet.AddString(player.id);
        protocolRet.AddString(hazardID);

        ServNet.instance.Broadcast(protocolRet);
    }
}