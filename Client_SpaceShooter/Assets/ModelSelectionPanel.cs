using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModelSelectionPanel : MonoBehaviour {
    public void OnClickLockStepBtn()
    {
        SceneManager.LoadScene("_LockStepMenu");
    }

    public void OnClickStateSyncBtn()
    {
        SceneManager.LoadScene("_StateSyncMenu");
    }
}
