using System;
using System.Collections.Generic;

//�����һ���е�״̬
public class PlayerTempData
{
    public PlayerTempData()
    {
        status = Status.None;
    }
    //״̬
    public enum Status
    {
        None,
        Room,
        Fight,
    }
    public Status status;
    //room״̬
    public Room room;
    public int team = 1;
    public bool isOwner = false;
    //ս�����
    public long lastUpdateTime;
    public float posX;
    public float posY;
    public float posZ;
    public long lastShootTime;
    public float hp = 200;
}