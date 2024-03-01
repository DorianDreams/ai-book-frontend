using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Metadata : MonoBehaviour
{
    
    public static Metadata Instance { get; private set; }

    public int drawingDuration;
    public string storyBookId;
    public string selectedOpeningSentence;
    public string firstGeneratedSentence;
    public string secondGeneratedSentence = "What is happening next ?";
    public string thirdGeneratedSentence = "How should the story end ?";
    public static bool singleScreenVersion = true;

    public int currentTextPage = 0;

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
        selectedOpeningSentence = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
