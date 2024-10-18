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
    public string selectedCharacter;
    public string currentImgID;

    public bool testingMode = false;

    public string consistencyPrompt;

    public string currentChapter = "ch1";



    public enum LLM
    {
        TinyLlama,
        BLIPInstruct,
        Mistral
    };

    public LLM currentLLM;


    private void Start()
    {
        storyBookId = "";
        currentPrompt = "";
        startingPrompt = "";
        currentImgID = "";
        storyBook = new StoryBook();
        currentChapter = "ch1";


        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        EventSystem.instance.PublishMetadata += OnPublishMetadata;
        currentPrompt = "";
        currentChapter = "ch1";


    }
    public void OnPublishMetadata()
    {
        StartCoroutine(PutFinishedStoryBook(() =>
        {
            SceneManager.LoadScene("Playthrough");

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


}
