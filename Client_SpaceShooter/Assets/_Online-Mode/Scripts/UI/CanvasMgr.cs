using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMgr : MonoBehaviour {
    public static CanvasMgr instance;
    public Text Text_packetSizePerSecond;
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    void Start()
    {
        Application.runInBackground = true;

    }
    //统计发送速率
    public void UpdateUpStream(int packetSizePerSecond)
    {
        Text_packetSizePerSecond.text = "UpStream :" + packetSizePerSecond + " bps";
    }
}
