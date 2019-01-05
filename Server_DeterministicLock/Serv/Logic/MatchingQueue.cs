using System;
using System.Collections.Generic;

public class MatchingQueue
{
	//单例
	public static MatchingQueue instance;
	public MatchingQueue()
	{
        instance = this;
	}
	
	List<Player> list = new List<Player>();
	
	//根据名字获取Player
	private Player GetPlayer(string id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].id == id)
				return list[i];
		}
		return null;
	}
	
	//添加玩家并看是否可以匹配
	public void AddPlayer(Player p)
	{
		lock (list)
		{
            //如果list中有玩家在匹配，则直接让他们匹配成功
            if (list.Count > 0)
            {
                RoomMgr.instance.CreateRoom(list[0],p);
                //RoomMgr.instance.CreateRoom(temp, p);
                list.RemoveAt(0);
            }
            else//否则将p添加进list
            {
                list.Add(p);
            }
		}
	}

    //删除玩家
    public void DelPlayer(string id)
    {
        lock (list)
        {
            Player p = GetPlayer(id);
            if (p != null)
                list.Remove(p);
        }
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("PlayerLeave");
        protocol.AddString(id);
        ServNet.instance.Broadcast(protocol);
    }
}