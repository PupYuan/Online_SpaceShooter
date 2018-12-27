using UnityEngine;
using System.Collections;

public class DestroyByContact : MonoBehaviour
{
	public GameObject explosion;
	public GameObject playerExplosion;
	public int scoreValue;
	private GameController gameController;

	void Start ()
	{
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <GameController>();
		}
		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Enemy")
		{
			return;
		}

		if (explosion != null)
		{
			Instantiate(explosion, transform.position, transform.rotation);
		}

		if (other.tag == "Player")
		{
            //只有本地玩家才会被击毁，网络控制的玩家不会被击毁，得由服务器来控制
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc.ctrlType != CtrlType.player)
                return;
			Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
            pc.Die();//包含了Destroy玩家实体的操作
            Destroy(gameObject);
            return;
			//gameController.GameOver();两个玩家都死亡才判定结束
		}
		
		gameController.AddScore(scoreValue);
		Destroy (other.gameObject);
		Destroy (gameObject);
	}
}