using System;

public partial class HandleConnMsg
{
	//心跳
	//协议参数：无
	public void MsgHeatBeat(Conn conn, ProtocolBase protoBase)
	{
		conn.lastTickTime = Sys.GetTimeStamp();
		Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
	}

    //登录
    //协议参数：str用户名,str密码
    //返回协议：-1表示失败 0表示成功
    public void MsgLogin(Conn conn, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        string strFormat = "[收到请求匹配协议]" + conn.GetAdress();
        Console.WriteLine(strFormat + " 用户名：" + id);
        //构建返回协议
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Login");
        //是否已经请求匹配
        ProtocolBytes protocolLogout = new ProtocolBytes();
        protocolLogout.AddString("Logout");
        if (!Player.KickOff(id, protocolLogout))
        {
            protocolRet.AddInt(-1);
            conn.Send(protocolRet);
            return;
        }
        conn.player = new Player(id, conn);
        //返回
        protocolRet.AddInt(0);
        conn.Send(protocolRet);
        return;
    }
    //下线
    //协议参数：
    //返回协议：0-正常下线
    public void MsgLogout(Conn conn, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        protocol.AddInt(0);
        if (conn.player == null)
        {
            conn.Send(protocol);
            conn.Close();
        }
        else
        {
            conn.Send(protocol);
            conn.player.Logout();
        }
    }
}