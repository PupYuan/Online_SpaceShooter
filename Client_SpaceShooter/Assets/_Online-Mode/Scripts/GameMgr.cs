using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GameMgr管理游戏的全局信息，例如与服务器的连接、网络循环和网络消息分发及处理
public class GameMgr : MonoBehaviour
{
    public static GameMgr instance;
    public string local_player_ID;
    public List<PlayerTempData> player_list = new List<PlayerTempData>();
    public string host = "127.0.0.1";
    public int port = 1234;
    void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        Application.runInBackground = true;

    }
    //网络更新循环
    void Update()
    {
        NetMgr.Update();
    }
}
