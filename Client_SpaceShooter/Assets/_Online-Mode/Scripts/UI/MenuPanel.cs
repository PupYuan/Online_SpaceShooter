using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPanel : MonoBehaviour
{
    public Text PlayerName;
    public GameObject MatchingPanel;
    private void Start()
    {
        PlayerName.text = "您好，指挥官：" + GameMgr.instance.local_player_ID;
    }

    public void OnSingleGameBtn()
    {
        SceneManager.LoadScene("SinglePlayer-Game");
    }

    public void OnMatchingBtn()
    {
        //连接服务器
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            NetMgr.srvConn.proto = new ProtocolBytes();
            if (!NetMgr.srvConn.Connect(GameMgr.instance.host, GameMgr.instance.port))
                Debug.Log("连接服务器失败!");
            //PanelMgr.instance.OpenPanel<TipPanel>("", "");
        }
        //切换到MatchingPanel
        MatchingPanel.SetActive(true);
        this.gameObject.SetActive(false);
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Matching");
        protocol.AddString(GameMgr.instance.local_player_ID);
        Debug.Log("发送 " + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol);
    }
}
