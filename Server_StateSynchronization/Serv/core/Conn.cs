using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

public class Conn
{
	//常量
	public const int BUFFER_SIZE = 1024;
	//Socket
	public Socket socket;
	//是否使用
	public bool isUse = false;
	//Buff
	public byte[] readBuff = new byte[BUFFER_SIZE];
	public int buffCount = 0;
	//沾包分包
	public byte[] lenBytes = new byte[sizeof(UInt32)];
	public Int32 msgLength = 0;
	//心跳时间
	public long lastTickTime = long.MinValue;
	//对应的Player
	public Player player;
	
	//构造函数
	public Conn()
	{
		readBuff = new byte[BUFFER_SIZE];
		
	}
	//初始化
	public void Init(Socket socket)
	{
		this.socket = socket;
		isUse = true;
		buffCount = 0;
		//心跳处理，稍后实现GetTimeStamp方法
		lastTickTime = Sys.GetTimeStamp ();
	}
	//剩余的Buff
	public int BuffRemain()
	{
		return BUFFER_SIZE - buffCount;
	}
	//获取客户端地址
	public string GetAdress()
	{
		if (!isUse)
			return "无法获取地址";
		return socket.RemoteEndPoint.ToString();
	}
	//关闭
	public void Close()
	{
		if (!isUse)
			return;
		if (player != null)
		{
			//玩家退出处理，稍后实现
			player.Logout ();
			return;
		}
		Console.WriteLine("[断开链接]" + GetAdress());
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		isUse = false;
	}
	
	//发送协议，相关内容稍后实现
	public void Send(ProtocolBase protocol)
	{
		ServNet.instance.Send (this, protocol);
	}
}