using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.Localization;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Localization.Tables;

public class DrawingScreenController : MonoBehaviour
{

    // Starting Sentence Selection Screen GameObjects
    [Header("Starting Screen")]
    public GameObject StartingScreen;
    public Button StartStoryButton;
    public Button EnglishButton;
    public Button GermanButton;
    public GameObject StartingHeadline;
    [SerializeField] 
    private LocalizedString StartingText;


    // Move to other class later
    private int initialPrompts = 11;
    private ArrayList initialPromptList;
    private string[] initialPromptArray;

    //Drawing Scene GameObjects
    [Header("Drawing Mode")]
    public GameObject DrawingMode;
    public Canvas DrawingCanvas;
    public Button UndoButton;
    public GameObject ButtonGroup;
    [SerializeField]
    float SelectedButtonWidth = 1.4f;
    [SerializeField]
    float StandardButtonWidth = 1f;
    [SerializeField]
    float LineWidth = 1f;
    //public GameObject Slider;
    public GameObject SendToAIButton;
    public GameObject DeleteAllButton;
    public GameObject DrawingBackground;
    public GameObject LineGeneratorPrefab;
    private GameObject InstantiatedLineGenerator;


    //Resulting Image for Selection
    [Header("Result Screen")]
    public GameObject ResultScreen;
    public GameObject ImageResult1;
    //public GameObject ImageResult2;
    //public GameObject ImageResult3;
    //public GameObject ImageResult4;
    public Button PublishToBookButton;
    public Button BacktoDrawing;
    private string descriptionCandidate;


    


    void Start()
    {
        //Move to another class later
        initialPromptList = new ArrayList();
        StringTable table = LocalizationSettings.StringDatabase.GetTable("Translations");
        for (int i = 1; i < initialPrompts+1; i++)
        {
            initialPromptList.Add(table.GetEntry("InitialPrompt" + i).LocalizedValue);
            //Debug.Log(initialPromptList[i-1]);

        }
        initialPromptArray = (string[])initialPromptList.ToArray(typeof(string));

        StartingScreen.SetActive(true);
        EventSystem.instance.SendToAI += OnSendToAI;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        EventSystem.instance.ChangeLocale += OnChangeLocale;

        // Assign listeners to buttons
        PublishToBookButton.onClick.AddListener(OnPublishToBook);
        EnglishButton.onClick.AddListener(OnButtonEnglish);
        GermanButton.onClick.AddListener(OnButtonGerman);
        StartStoryButton.onClick.AddListener(OnStartStoryButton);
        UndoButton.onClick.AddListener(OnUndoButtonClicked);
        SendToAIButton.GetComponent<Button>().onClick.AddListener(OnSendToAI);
        DeleteAllButton.GetComponent<Button>().onClick.AddListener(EventSystem.instance.DeleteAllLinesEvent);
        BacktoDrawing.GetComponent<Button>().onClick.AddListener(OnBackToDrawing);

        StartingHeadline.GetComponent<TextMeshProUGUI>().text =StartingText.GetLocalizedString();


        foreach (Button button in ButtonGroup.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => OnColorButtonClicked(button));
        }

    }

    // Move to other class later
    void reshuffle(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    public string GetRandomPrompt()
    {
        reshuffle(initialPromptArray);
        return initialPromptArray[0];
    }

    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }
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

    void OnStartStoryButton()
    {
        if ( Metadata.singleScreenVersion)
        {
            EventSystem.instance.SwitchCameraEvent();
        }
        Metadata.Instance.currentTextPage = 1;
        Metadata.Instance.selectedOpeningSentence = GetRandomPrompt();
        StartCoroutine(CreateStoryBook());
        EventSystem.instance.StartStoryEvent();
    }

    void ShowDrawingSceneStart()
    {
        StartingScreen.SetActive(false);
        DrawingMode.SetActive(true);
        InstantiateLineGenerator();
    }

    void ShowDrawingSceneEnd()
    {
        UndoButton.gameObject.SetActive(false);
        ButtonGroup.SetActive(false);
        DrawingBackground.SetActive(false);
        //Slider.SetActive(false);
        Destroy(InstantiatedLineGenerator);
    }


 
    IEnumerator CreateStoryBook()
    {
        using (UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:8000/api/storybooks",
            "{ \"title\": \"to be defined\", \"duration\": 0, \"iterations\": 0,\"status\": true }", 
            "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Dictionary<string,object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string,object>>(request.downloadHandler.text);

                Metadata.Instance.storyBookId = returnVal["id"].ToString();
            }
        }
    }


  

    public void OnBackToDrawing()
    {
        EventSystem.instance.ShowLinesEvent(); 
        ResultScreen.SetActive(false);
        DrawingMode.SetActive(true);
        ImageResult1.GetComponent<Image>().sprite = null;
    }

    public void OnPublishToBook() {         
           EventSystem.instance.PublishToBookEvent(ImageResult1.GetComponent<Image>().sprite, 
               descriptionCandidate);
           if(Metadata.singleScreenVersion)
        {
               EventSystem.instance.SwitchCameraEvent();
           }
       }


    public void OnSendToAI()
    {
        Debug.Log("Send to AI");
        StartCoroutine(CoroutineScrenshot((bytes) =>
        {
            StartCoroutine(SendImageToAI(bytes,
                (returnVal) => showImageSelection(
                    returnVal))

                );
        }
        ));

    }

    public void showImageSelection(Dictionary<string,object> returnVal)
    {
        ResultScreen.SetActive(true);
        string imagePath = returnVal["image"].ToString();
        descriptionCandidate = returnVal["description"].ToString();
        
        string operatingSystem = SystemInfo.operatingSystem;
        if (operatingSystem.Contains("Windows"))
        {
            imagePath = imagePath.Replace("/", "\\");
            string fullpath= "E:\\thesis\\backend\\storybookcreator" + imagePath;
            byte[] bytes = System.IO.File.ReadAllBytes(fullpath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            ImageResult1.GetComponent<Image>().sprite = Sprite.Create(texture, 
                new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }
        else if (operatingSystem.Contains("Mac"))
        {
            imagePath = imagePath.Replace("\\", "/");
        } else if (operatingSystem.Contains("Linux"))
        {
            imagePath = imagePath.Replace("\\", "/");
            string fullpath = "/home/aidev/Documents/back-end/storybookcreator" + imagePath;
            byte[] bytes = System.IO.File.ReadAllBytes(fullpath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            ImageResult1.GetComponent<Image>().sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        }
        Debug.Log("showImageSelection");
    }

    public void publishToBook()
    {

    }


    IEnumerator SendImageToAI(byte[] bytes, System.Action<Dictionary<string, object>> callback)
    {
        //TODO: Show loading screen
        DrawingMode.gameObject.SetActive(false);
        EventSystem.instance.HideLinesEvent();
        string url = "http://127.0.0.1:8000/api/images/" + Metadata.Instance.storyBookId;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("image", bytes));
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        yield return request.SendWebRequest();


        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, object>>(request.downloadHandler.text);

            Debug.Log("Form upload complete! " + request.downloadHandler.text);
            callback(returnVal);
        }
    }


    void OnUndoButtonClicked()
    {
        EventSystem.instance.DeleteLastLineEvent();
    }

    public void OnColorButtonClicked(Button button)
    {
        foreach (Transform child in ButtonGroup.transform)
        {
            child.localScale = new Vector3(StandardButtonWidth, StandardButtonWidth, StandardButtonWidth);
        }
        button.transform.localScale = new Vector3(SelectedButtonWidth, SelectedButtonWidth, SelectedButtonWidth);
        Debug.Log(button.GetComponent<Image>().color);
        EventSystem.instance.PressColorButtonEvent(button.GetComponent<Image>().color);
    }


    void InstantiateLineGenerator()
    {
        InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
        InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = DrawingCanvas;
        InstantiatedLineGenerator.GetComponent<LineGenerator>().width = LineWidth;
        //InstantiatedLineGenerator.GetComponent<LineGenerator>().Slider = Slider;
    }


    //Code inspired by https://www.youtube.com/watch?v=d5nENoQN4Tw andhttps://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
    private IEnumerator CoroutineScrenshot(System.Action<byte[]> callback)
    {
        yield return new WaitForEndOfFrame();
        Rect rect = RectTransformUtility.PixelAdjustRect(DrawingBackground.GetComponent<RectTransform>(), DrawingCanvas); //public vars
        int textWidth = System.Convert.ToInt32(rect.width); // width of the object to capture
        int textHeight = System.Convert.ToInt32(rect.height); // height of the object to capture
        var startX = System.Convert.ToInt32(rect.x) + Screen.width / 2; // offset X
        var startY = System.Convert.ToInt32(rect.y) + Screen.height / 2; // offset Y
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        Camera.main.targetTexture = rt;
        Texture2D screenShot = new Texture2D(textWidth, textHeight, TextureFormat.RGB24, false);
        Camera.main.Render();
        RenderTexture.active = rt;

        screenShot.ReadPixels(new Rect(startX, startY-120, textWidth, textHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string filePath = Application.dataPath + "/" + fileName;

        System.IO.File.WriteAllBytes(filePath, bytes);
        callback(bytes);
    }
}
