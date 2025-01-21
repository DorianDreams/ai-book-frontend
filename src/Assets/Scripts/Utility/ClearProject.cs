using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearProject : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if(PlayerPrefs.HasKey("UnitySelectMonitor")){
            PlayerPrefs.DeleteKey("UnitySelectMonitor");
        }
        PlayerPrefs.Save();
    }

}
