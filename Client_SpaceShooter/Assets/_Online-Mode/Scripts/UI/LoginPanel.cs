using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    public InputField IDInput;
    public GameObject MenuPanel;
    public void OnConfirmBtn()
    {
        if (IDInput.text.Length != 0)//这里需要做更多有效性检验
        {
            GameMgr.instance.local_player_ID = IDInput.GetComponent<InputField>().text;
            Debug.Log("GameManager.instance.player_ID is " + GameMgr.instance.local_player_ID);

            MenuPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Invalid ID ");
        }
    }
}
