using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Metadata : MonoBehaviour
{

    // JSON Objects
    public StoryBook storyBook;


    public static Metadata Instance { get; private set; }

    public int drawingDuration;
    public string storyBookId;
    public string currentPrompt;
    public static bool singleScreenVersion = true;

    public int currentTextPage = 0;

    public int testVersion;

    public string currentChapter = "ch1";

    private void Awake()
    {
            int drawingDuration = 0;
            string storyBookId = "";
             
        bool singleScreenVersion = true;

        int currentTextPage = 0;

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

        currentPrompt = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
