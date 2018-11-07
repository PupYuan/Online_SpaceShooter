using System;
using System.Collections.Generic;
using System.Linq;

public class RoomMgr
{
    //单例
    public static RoomMgr instance;
    public RoomMgr()
    {
        instance = this;
    }

    //房间列表
    public List<Room> list = new List<Room>();

    //创建房间
    public void CreateRoom(Player player1,Player player2)
    {
        Room room = new Room();
        lock (list)
        {
            list.Add(room);
            room.AddPlayer(player1);
            room.AddPlayer(player2);
            //这里简单地直接开始
            room.StartGame();
        }
    }
    

    //玩家离开
    public void LeaveRoom(Player player)
    {
        PlayerTempData tempDate = player.tempData;
        if (tempDate.status == PlayerTempData.Status.None)
            return;

        Room room = tempDate.room;

        lock (list)
        {
            room.DelPlayer(player.id);
            if (room.list.Count == 0)
                list.Remove(room);
        }
    }
}