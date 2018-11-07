using UnityEngine;
using System.Collections;

[System.Serializable]
public class Done_Boundary
{
    public float xMin, xMax, zMin, zMax;
}
public enum CtrlType
{
    none,
    player,
    computer,
    net,
}
// TODO：定义好要同步的位置数据包
public struct NetTransform
{

}

public class PlayerController : MonoBehaviour
{
    public CtrlType ctrlType = CtrlType.player;
    public float speed;
    public float tilt;
    public Done_Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;

    private float nextFire;

	//初始完毕后才加监听函数
	void Start(){
		NetMgr.srvConn.msgDist.AddListener("NetTransform", NetTransform);
	}
	//同步变换信息，使用影子跟随算法
	public void NetTransform(ProtocolBase proto){
		int start = 0;
        ProtocolBytes protocol =  (ProtocolBytes)proto;
        string protoName = protocol.GetString(start, ref start);
        if (protoName != "NetTransform")
            return;
		//同步位置、速度、转向

	}

    void Update()
    {
        //本地玩家根据Input：键盘输入、鼠标输入来驱动
        if (ctrlType == CtrlType.player)
        {
            if (Input.GetButton("Fire1") && Time.time > nextFire)
                Fire();
        }
    }


    //检测是否可以开火不放在Fire函数中，这样可以减少无效的函数调用，增加程序效率。
    public void Fire()
    {
        nextFire = Time.time + fireRate;
        Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        GetComponent<AudioSource>().Play();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        GetComponent<Rigidbody>().velocity = movement * speed;

        GetComponent<Rigidbody>().position = new Vector3
        (
            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
        );

        GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
    }
}
