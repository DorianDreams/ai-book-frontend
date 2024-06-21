using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class Metadata : MonoBehaviour
{

    // JSON Objects
    public StoryBook storyBook;

    //public bool useCaptioning;

    public static Metadata Instance { get; private set; }

    public string storyBookId;
    public string currentPrompt;
    public string startingPrompt;
    public string previousPrompt;

    public string currentImgID;

    public bool testingMode = false;

    public string consistencyPrompt;



    public enum LLM
    {
        TinyLlama,
        BLIPInstruct,
        Mistral
    };

    public LLM currentLLM;



    // Metadata used for State Management: Todo: Move to StateManager
    public static bool singleScreenVersion = true;
    public int currentTextPage = 0;
    public string currentChapter = "ch1";

    private void Awake()
    {
        storyBookId = "";
        currentPrompt = "";
        startingPrompt = "";
        previousPrompt = "";
        currentImgID = "";
        storyBook = new StoryBook();
        currentChapter = "ch1";
        currentTextPage = 0;


        DontDestroyOnLoad(this.gameObject);

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
        EventSystem.instance.PublishMetadata += OnPublishMetadata;
        currentPrompt = "";
        currentChapter = "ch1";
        currentTextPage = 0;

    }

    public void OnPublishMetadata()
    {
        StartCoroutine(PutFinishedStoryBook(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }));
    }

    IEnumerator PutFinishedStoryBook(System.Action callback)
    {
        this.storyBook.finished_playthrough = true;
        //string json = JsonUtility.ToJson(this.storyBook);
        string json = JsonConvert.SerializeObject(this.storyBook);
        Debug.Log("json: " + json);
        using (UnityWebRequest request = UnityWebRequest.Put("http://127.0.0.1:8000/api/storybooks/"+storyBookId, json))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                //Debug.Log(request.downloadHandler.text);
                Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, object>>(request.downloadHandler.text);
            }
            callback();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
