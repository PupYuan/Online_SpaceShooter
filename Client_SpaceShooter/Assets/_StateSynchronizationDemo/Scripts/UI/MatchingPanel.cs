using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchingPanel : MonoBehaviour
{
    public GameObject MenuPanel;
    public string nextScene = "StateSynchronizationDemo";
    private void Awake()
    {
        NetMgr.srvConn.msgDist.AddOnceListener("StartGame", OnStartGame);
        NetMgr.srvConn.msgDist.AddOnceListener("Matching", OnMatchingBack);
    }
    public void OnCancelBtn()
    {
        MenuPanel.SetActive(true);
        this.gameObject.SetActive(false);
    }
    //监听StartGame事件
    void OnStartGame(ProtocolBase proto)
    {
        //在这里获取这次游戏的对局信息，包括玩家数量、状态
        int start = 0;
        ProtocolBytes protocol =  (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        if (protoName != "StartGame")
            return;
        int Count = protocol.GetInt(start,ref start);
        GameMgr.instance.player_list.Clear();//先清空
        for (int i=0;i<Count;i++){
            string id = protocol.GetString(start,ref start);
            PlayerTempData player = new PlayerTempData();
            player.id = id;
            GameMgr.instance.player_list.Add(player);
        }
        //这里是简化过后的过程，如果场景很大，会导致配置差的玩家加载慢于配置好的玩家，应该用两次协议进行确认来尽可能地让两个玩家同时开始游戏
        SceneManager.LoadScene(nextScene);
    }
    public void OnMatchingBack(ProtocolBase proto)
    {
        //这里可以根据状态码进行判断，看ID是否合法，现在服务器是否满人
        Debug.Log("正在匹配");
    }

    private void OnDestroy()
    {
        Debug.Log("销毁了MatchingPanel");
        NetMgr.srvConn.msgDist.DelOnceListener("StartGame", OnStartGame);
        NetMgr.srvConn.msgDist.DelOnceListener("Matching", OnMatchingBack);
    }
}
