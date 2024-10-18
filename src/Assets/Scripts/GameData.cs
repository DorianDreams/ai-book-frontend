using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{


    public static GameData Instance { get; private set; }

    // Book Text
    public string startingPrompt;
    public string secondPrompt;
    public string thirdPrompt;

    [SerializeField]
    public bool test;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
