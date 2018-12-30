using System;

namespace Serv
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            RoomMgr roomMgr = new RoomMgr();
            Scene scene = new Scene ();
            MatchingQueue maching_queue = new MatchingQueue();
            //DataMgr dataMgr = new DataMgr ();//管理数据的类，先暂时去除
            ServNet servNet = new ServNet();
			servNet.proto = new ProtocolBytes ();
			servNet.Start("127.0.0.1",1234);

			while(true)
			{
				string str = Console.ReadLine();
				switch(str)
				{
				case "quit":
					servNet.Close();
					return;
				case "print":
					servNet.Print();
					break;
				}
			}

		}
	}
}
