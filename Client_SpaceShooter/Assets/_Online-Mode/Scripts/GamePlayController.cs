using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GamePlayController负责每局游戏的逻辑控制，使用NetMgr的网络接口
//SpaceShooter的游戏逻辑包括：初始化玩家
public class GamePlayController : MonoBehaviour {
	public GameObject[] PlayerPrefab;
	void Awake(){
		int Count = 0;//生成玩家时给予一个偏移量
		foreach(var player in GameMgr.instance.player_list){
			GameObject player_obj = GameObject.Instantiate(PlayerPrefab[0],new Vector3(1,0,0)*Count ,Quaternion.identity);
			player_obj.name = player.id;//场景中生成的GameObject名字改成id
			if(player.id != GameMgr.instance.local_player_ID){
				player_obj.GetComponent<PlayerController>().ctrlType = CtrlType.net;//网络同步
			}
			Count++;
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	// Update is called once per frame
	void Update () {
		
	}
}
