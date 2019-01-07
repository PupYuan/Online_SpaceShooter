using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeterministicLockstepDemo
{
    public class CanvasMgr : MonoBehaviour
    {
        public static CanvasMgr instance;
        public Text Text_packetSizePerSecond;
        public GridLayoutGroup PlayerInfo;
        public GameObject Text_playerPosPrefab;
        private Dictionary<string, Text> Text_playerPosList = new Dictionary<string, Text>();
        void Awake()
        {
            if (!instance)
            {
                instance = this;

            }
            if (PlayerInfo)
            {
                foreach (var player in GameMgr.instance.player_list)
                {
                    GameObject Text_playerPos_obj = (GameObject)Instantiate(Text_playerPosPrefab);
                    Text_playerPos_obj.name = player.id;//场景中生成的GameObject名字改成id
                    Text_playerPos_obj.transform.SetParent(PlayerInfo.transform);
                    Text_playerPosList.Add(player.id, Text_playerPos_obj.GetComponent<Text>());
                }
            }
        }

        void Start()
        {
            Application.runInBackground = true;

        }
        private void Update()
        {
            foreach (var player in GameLoopMgr.instance.m_playerControllerList)
            {
                Text_playerPosList[player.Key].text = "Player:"+ player.Key+",X =" + player.Value.logicPosition.x + " ,Z=" + player.Value.logicPosition.z + ";";
            }
        }
        //统计发送速率
        public void UpdateUpStream(int packetSizePerSecond)
        {
            Text_packetSizePerSecond.text = "UpStream :" + packetSizePerSecond + " bps";
        }
    }
}

