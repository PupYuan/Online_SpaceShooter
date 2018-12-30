using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    public InputField IDInput;
    public GameObject MenuPanel;
    private void Awake()
    {
        if (NetMgr.srvConn.status == Connection.Status.Connected)//已经连接上了
        {
            MenuPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
    public void OnConfirmBtn()
    {
        //这里需要做更多有效性检验
        if (IDInput.text.Length == 0)
        {
            Debug.Log("Invalid ID ");
            return;
        }
        //连接服务器
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            NetMgr.srvConn.proto = new ProtocolBytes();
            if (!NetMgr.srvConn.Connect(GameMgr.instance.host, GameMgr.instance.port))
                Debug.Log("连接服务器失败!");
        }

        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(IDInput.text);
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }
    
    public void OnLoginBack(ProtocolBase proto)
    {
        //在这里获取这次游戏的对局信息，包括玩家数量、状态
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        int Ret = protocol.GetInt(start, ref start);
        if (Ret == 0)
        {
            Debug.Log("登录成功");
            GameMgr.instance.local_player_ID = IDInput.GetComponent<InputField>().text;
            Debug.Log("GameManager.instance.player_ID is " + GameMgr.instance.local_player_ID);

            MenuPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }
        else Debug.Log("登录失败");
    }
}
