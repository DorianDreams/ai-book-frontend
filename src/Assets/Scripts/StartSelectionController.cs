using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Networking;
using System;

public class StartSelectionController : MonoBehaviour
{
    public GameObject Textboxes;

    // Starting Sentence Selection Screen GameObjects
    [Header("Starting Screen Objects")]
    public GameObject StartingScreen;
    public Button StartStoryButton;
    public Button EnglishButton;
    public Button GermanButton;
    public GameObject StartingHeadline;

    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString StartingText;


    // Start is called before the first frame update
    void Start()
    {
        StartingScreen.SetActive(true);

        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;

        EnglishButton.onClick.AddListener(OnButtonEnglish);
        GermanButton.onClick.AddListener(OnButtonGerman);
        StartStoryButton.onClick.AddListener(OnStartStoryButton);

        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }

    // --------------- Events ---------------
    void OnButtonEnglish()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        EventSystem.instance.ChangeLocaleEvent();
    }
    void OnButtonGerman()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        EventSystem.instance.ChangeLocaleEvent();
    }

    void ShowDrawingSceneStart()
    {
        StartingScreen.SetActive(false);
    }
    void OnStartStoryButton()
    {
        if (Metadata.singleScreenVersion)
        {
            EventSystem.instance.SwitchCameraEvent();
        }
        Metadata.Instance.currentTextPage = 1;
        Debug.Log("Start Story Button Clicked");
        Debug.Log(Metadata.Instance.currentPrompt);

        if (Metadata.Instance.currentPrompt == "")
        {
            Debug.Log("Get Random Prompt");
            Metadata.Instance.currentPrompt = Textboxes.GetComponent<Textboxes>().getRandomInitialPrompt();
        }

        StartCoroutine(CreateStoryBook());
        EventSystem.instance.StartStoryEvent();
    }

    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }

    // --------------- Events End ---------------



    IEnumerator CreateStoryBook()
    {        
        StoryBook storyBook = new StoryBook(Metadata.Instance.currentPrompt, false, false);
        Metadata.Instance.storyBook = storyBook;
        string json = JsonUtility.ToJson(storyBook);
        Debug.Log("json: " + json);
        using (UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:8000/api/storybooks", json, "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, object>>(request.downloadHandler.text);

                Metadata.Instance.storyBookId = returnVal["id"].ToString();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
