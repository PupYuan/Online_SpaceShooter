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
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Matching");
        protocol.AddString(GameMgr.instance.local_player_ID);
        Debug.Log("发送 " + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol);

        //切换到MatchingPanel
        MatchingPanel.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
