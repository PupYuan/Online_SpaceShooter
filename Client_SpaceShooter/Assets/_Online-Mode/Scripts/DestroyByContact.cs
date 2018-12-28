using UnityEngine;
using System.Collections;

public class DestroyByContact : MonoBehaviour
{
    public string ID;//用于同步和调用
    public GameObject explosion;
    public GameObject playerExplosion;
    public int scoreValue;
    private GameController gameController;

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Boundary" || other.tag == "Enemy" )
		{
            return;
        }
        //判断是否是本地玩家的子弹，否则没有击中判断权利
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet)
        {
            if (bullet.attackerID != GameMgr.instance.local_player_ID)
            {
                Destroy(other.gameObject);
                return;
            }
        }

		if (other.tag == "Player")
		{
            //只有本地玩家才会被击毁，网络控制的玩家不会被击毁，得由服务器来控制
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc.ctrlType != CtrlType.player)
                return;
            //玩家实体的爆炸效果交由玩家实体自己生成
			//Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
            pc.Die();//玩家死亡需要发送协议
            SendHazardDie();
            return;
		}
        SendHazardDie();
        Destroy (other.gameObject);
        
    }
    public void SendHazardDie()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("SyncHazardDie");
        proto.AddString(ID);
        NetMgr.srvConn.Send(proto);

        GameController.instance.RemoveHazard(ID);//本地客户端需要自己移除
    }
    //调用销毁函数和爆炸效果
    public void BeDestroyed()
    {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, transform.rotation);
        }
        gameController.AddScore(scoreValue);
        Destroy(gameObject);
    }
}