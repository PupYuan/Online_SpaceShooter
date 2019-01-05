using System;
using System.Collections.Generic;
using System.Linq;

//房间
public class Room
{
    //状态
    public enum Status
    {
        Prepare = 1,//大家还在加载，还未统一准备完成，需要保证所有玩家尽量统一开始游戏
        Fight = 2,
    }
    public Status status = Status.Prepare;
    //玩家
    public int maxPlayers = 2;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();


    //添加玩家
    public bool AddPlayer(Player player)
    {
        lock (list)
        {
            if (list.Count >= maxPlayers)
                return false;
            PlayerTempData tempData = player.tempData;
            tempData.room = this;
            tempData.status = PlayerTempData.Status.Room;

            if (list.Count == 0)
                tempData.isOwner = true;
            string id = player.id;
            list.Add(id, player);
        }
        return true;
    }

    //删除玩家
    public void DelPlayer(string id)
    {
        lock (list)
        {
            if (!list.ContainsKey(id))
                return;
            bool isOwner = list[id].tempData.isOwner;
            list[id].tempData.status = PlayerTempData.Status.None;
            list.Remove(id);
            if (isOwner)
                UpdateOwner();
        }
    }

    //更换房主
    public void UpdateOwner()
    {
        lock (list)
        {
            if (list.Count <= 0)
                return;

            foreach (Player player in list.Values)
            {
                player.tempData.isOwner = false;
            }

            Player p = list.Values.First();
            p.tempData.isOwner = true;
        }
    }

    //房间内广播
    public void Broadcast(ProtocolBase protocol)
    {
        foreach (Player player in list.Values)
        {
            player.Send(protocol);
        }
    }
    //双方玩家都准备，可以开始战斗了
    public void StartGame()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartGame");
        lock (list)
        {
            protocol.AddInt(list.Count);
            foreach (Player p in list.Values)
            {
                p.tempData.hp = 200;
                p.tempData.status = PlayerTempData.Status.Fight;

                protocol.AddString(p.id);
                //每个玩家一个消息队列
                ServNet.instance.command_list.Add(p.id, new Queue<Command>());
            }
            Broadcast(protocol);
        }
    }
    //中途退出战斗
    public void ExitFight(Player player)
    {
        //摧毁退出游戏的玩家
        if (list[player.id] != null)
            list[player.id].tempData.hp = -1;
        //广播消息
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("undefined");
        protocolRet.AddString(player.id);
        Broadcast(protocolRet);
    }
}