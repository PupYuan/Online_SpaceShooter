using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject[] hazards;
	public Vector3 spawnValues;
	public int hazardCount;
	public float spawnWait;
	public float startWait;
	public float waveWait;
    public int playerLeft = 1;//剩余玩家数量
    public Dictionary<string, DestroyByContact> hazardsList = new Dictionary<string, DestroyByContact>();

    public GUIText scoreText;
	public GUIText restartText;
	public GUIText gameOverText;
	
	private bool gameOver;
	private bool restart;
	private int score;
    public int playerLeft = 2;//剩余玩家数量
    //用确定的随机数种子
    private System.Random random;
    private int totalHazrds = 0;//记录出现过的敌军总数
    

    private void Awake()
    {
        instance = this;
        NetMgr.srvConn.msgDist.AddListener("SyncHazardDie", SyncHazardDie);
    }
    private void OnDestroy()
    {
        NetMgr.srvConn.msgDist.DelListener("SyncHazardDie", SyncHazardDie);
    }
    public void SyncHazardDie(ProtocolBase proto)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        if (protoName != "SyncHazardDie")
            return;
        string player_id = protocol.GetString(start, ref start);
        string hazard_id = protocol.GetString(start, ref start);

        RemoveHazard(hazard_id);
    }
    public void RemoveHazard(string _hazard_id)
    {
        //根据id去更新PlayerController的信息
        if (!hazardsList.ContainsKey(_hazard_id))
        {
            Debug.Log("出现多个玩家都击中同一个物体的现象，这里忽略掉，因为并不需要判断是谁击中了 ");
            return;
        }
        DestroyByContact pc = hazardsList[_hazard_id];
        pc.BeDestroyed();
        hazardsList.Remove(_hazard_id);
    }

    void Start ()
	{
        random = new System.Random(1000);
        gameOver = false;
		restart = false;
		restartText.text = "";
		gameOverText.text = "";
		score = 0;
		UpdateScore ();
		StartCoroutine (SpawnWaves ());
	}
	
	void Update ()
	{
		if (restart)
		{
			if (Input.GetKeyDown (KeyCode.R))
			{
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		}
	}
	
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			for (int i = 0; i < hazardCount; i++)
			{
                totalHazrds++;
                int randIndex = random.Next(0, hazards.Length);
                GameObject hazard = hazards[randIndex];
                DestroyByContact hazardhandle = hazard.GetComponent<DestroyByContact>();
                hazardhandle.ID = "hazard" + totalHazrds.ToString();
                hazardsList.Add(hazardhandle.ID, hazardhandle);
                //GameObject hazard = hazards [Random.Range (0, hazards.Length)];

                var r = random.NextDouble();
                float randX = (float)(r * (-spawnValues.x - spawnValues.x) + spawnValues.x);
    
                Vector3 spawnPosition = new Vector3(randX, spawnValues.y, spawnValues.z);
                //Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
			yield return new WaitForSeconds (waveWait);
			
			if (gameOver)
			{
				restartText.text = "Press 'R' for Restart";
				restart = true;
				break;
			}
		}
	}
	
	public void AddScore (int newScoreValue)
	{
		score += newScoreValue;
		UpdateScore ();
	}
	
	void UpdateScore ()
	{
		scoreText.text = "Score: " + score;
	}
	
	public void GameOver ()
	{
		gameOverText.text = "Game Over!";
		gameOver = true;
	}

    public void PlayerDie()
    {
        playerLeft--;
        if (playerLeft <= 0)
            GameOver();
    }
}