using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using TMPro;

public class ResultScreenController : MonoBehaviour
{
    //Resulting Image for Selection

    enum State 
    {
        GENERATING, 
        INPUT,
        BACK,
        PUBLISH,
        SELECTED    
    }

    private State currentState;


    public GameObject ResultScreen;

    [Header("Result Images")]
    public GameObject imageResults;
    public GameObject imageResultsSelected;
    public GameObject Spinners;
    public GameObject textResults;

    private GameObject[] SpinnerArr = new GameObject[4];
    private GameObject[] SelectionArr = new GameObject[4];
    private GameObject[] ImageResult = new GameObject[4];
    private GameObject[] TextResult = new GameObject[4];

    [Header("Buttons")]
    public GameObject ChooseImage;
    public GameObject BacktoDrawing;
    public GameObject ReGenerateImages;
    public GameObject ChooseText;
    public GameObject ReGeneratText;


    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString AIUnderstandingSentence;
    [SerializeField]
    private LocalizedString NotSatisfiedText;

    //Todo: interaction with book controller?
    string chapter = "chapter_1_prompt";

    // private vars for game states
    private int _numberOfImages; 
    private Image _currentSelectedImage = null;
    private byte[] _currentSelectedImageBytes = null;
    private string _currentSelectedText = null;
    private int selectedImageIndex;
    private int selectedTextIndex;
    private byte[] currentScreenshot;
    private List<byte[]> imageByteList = new List<byte[]>();
    private List<Dictionary<string, object>> imageReturnVals = new List<Dictionary<string, object>>();

    private void Start()
    {
        ReGeneratText.SetActive(false);
        ChooseText.SetActive(false);
        textResults.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            SpinnerArr[i] = Spinners.transform.GetChild(i).gameObject;
            SelectionArr[i] = imageResultsSelected.transform.GetChild(i).gameObject;
            ImageResult[i] = imageResults.transform.GetChild(i).gameObject;
            TextResult[i] = textResults.transform.GetChild(i).gameObject;

        }

        ResultScreen.SetActive(false);

        foreach (GameObject spinner in SpinnerArr)
            spinner.SetActive(false);

        EventSystem.instance.EnableResultScreen += Enable;
        EventSystem.instance.DisableResultScreen += Disable;
        EventSystem.instance.SendImageToAI += OnSendImageToAI;
        
        DisableSelectionButtons();
                
        _numberOfImages = 0;
    }

    private void DisableSelectionButtons()
    {
        foreach (GameObject image in ImageResult)
            image.GetComponent<Button>().interactable = false;
        
    }
    private void EnableSelectionButtons() {
        foreach (GameObject image in ImageResult)
            image.GetComponent<Button>().interactable = true;
    }
    private void Enable()
    {
        ResultScreen.SetActive(true);
    }
    private void Disable()
    {
        ResultScreen.SetActive(false);
        foreach (GameObject image in ImageResult)
            image.GetComponent<Image>().sprite = null;

        _currentSelectedImage = null;
        DisableSelectionButtons();

        UnselectImage();
        _numberOfImages = 0;
    }

    public void OnRegenerateImages()
    {
        for (int i = 0; i < 4; i++)
        {
            ImageResult[i].GetComponent<Image>().sprite = null;
            SpinnerArr[i].SetActive(true);
            SelectionArr[i].SetActive(false);
        }
        OnSendImageToAI(currentScreenshot);
    }

    public void OnReGenerateText()
    {
        for (int i = 0; i < 4; i++)
        {
            TextResult[i].GetComponent<TextMeshProUGUI>().text = "";
            SpinnerArr[i].SetActive(true);
            SelectionArr[i].SetActive(false);
        }
        StartCoroutine(SentenceCompletions(_currentSelectedImageBytes, Metadata.Instance.currentPrompt));
    }


    public void OnBackToDrawing()
    {
        EventSystem.instance.ContinueDrawingEvent();
        EventSystem.instance.DisableResultScreenEvent();
    }

    IEnumerator SentenceCompletions(byte[] genImage, string sentence)
    {
        float[] tempVals = { 0.8f, 1.2f, 1.6f, 2f };
        int i = 0;
        foreach (var temperature in tempVals) 
        {
            CoroutineWithData cd_completion = new CoroutineWithData(this, Request.GetSentenceCompletion(genImage, sentence, temperature));
            yield return cd_completion.coroutine;
            string completion = (string)cd_completion.result;           

            TextResult[i].GetComponent<TextMeshProUGUI>().text = completion;

            SpinnerArr[i].SetActive(false);
            i++;

        }
    }


    public void OnChooseImage()
    {
        Debug.Log("Choose Image");
        Debug.Log(_currentSelectedImage);
        if (_currentSelectedImage != null){
        byte[] bytes = imageByteList[selectedImageIndex];
        
        UnselectImage();
        foreach (GameObject spinner in SpinnerArr)
            { spinner.SetActive(true); }

        foreach (GameObject image in ImageResult)
            { image.SetActive(false); }
        _numberOfImages = 0;
        ChooseImage.SetActive(false);
        ReGenerateImages.SetActive(false);
        ChooseText.SetActive(true);
        ReGeneratText.SetActive(true);
        textResults.SetActive(true);
        
        EventSystem.instance.SelectImageEvent(_currentSelectedImage.sprite, selectedImageIndex, bytes);

        foreach (GameObject text in TextResult)
            text.SetActive(true);
        StartCoroutine(SentenceCompletions(bytes, Metadata.Instance.currentPrompt));

        }
    }

    public void OnChooseText()
    {
        //Set texts null
        for (int i = 0; i < 4; i++)
        {
            TextResult[i].GetComponent<TextMeshProUGUI>().text = "";
        }
        ChooseText.SetActive(false);
        ReGeneratText.SetActive(false);

        EventSystem.instance.SelectTextEvent(_currentSelectedText);
        EventSystem.instance.DisableResultScreenEvent();
        EventSystem.instance.EnableBookNavigatorEvent();
        StartCoroutine(CreateNextPrompt(_currentSelectedText));
        
    }

    public IEnumerator CreateNextPrompt(string completion)
    {
        CoroutineWithData cd_nextPrompt = new CoroutineWithData(this, Request.GetNextprompt(completion));
        yield return cd_nextPrompt.coroutine;
        string nextPrompt = (string)cd_nextPrompt.result;
        EventSystem.instance.PublishNextPromptEvent(nextPrompt);
        Dictionary<string, object> returnVal = imageReturnVals[selectedImageIndex];
        string imgID = returnVal["id"].ToString();
        StartCoroutine(Request.PostImageDescription(completion, imgID)); //todo:logging
        imageByteList.Clear();
        imageReturnVals.Clear();

    }


    IEnumerator StableDiffusionInference(byte[] screenshot)
    {
        _numberOfImages = 0;
        CoroutineWithData cd_caption = new CoroutineWithData(this, Request.GetImageCaption(screenshot));
        yield return cd_caption.coroutine;
        string caption = (string)cd_caption.result;

        float[] strengthVals = { 0.5f, 0.8f, 0.9f, 1f };
        foreach (float strength in strengthVals)
        {
            CoroutineWithData cd_image = new CoroutineWithData(this, Request.GetImageGeneration(caption, strength, screenshot));
            yield return cd_image.coroutine;
            Dictionary<string, object> returnVal = (Dictionary<string, object>)cd_image.result;
            showImageSelection(returnVal);
            _numberOfImages++;
        }
    }

    public void showImageSelection(Dictionary<string, object> returnVal)
    {
        string imagePath = returnVal["image"].ToString();
        string operatingSystem = SystemInfo.operatingSystem;
        string fullpath = "";
        if (operatingSystem.Contains("Windows"))
        {
            imagePath = imagePath.Replace("/", "\\");
            fullpath = "E:\\thesis\\backend\\storybookcreator" + imagePath;
        }
        else if (operatingSystem.Contains("Mac"))
        {
            imagePath = imagePath.Replace("\\", "/");
        }
        else if (operatingSystem.Contains("Linux"))
        {
            imagePath = imagePath.Replace("\\", "/");
            fullpath = "/home/aidev/Documents/back-end/storybookcreator" + imagePath;
        }

        // Display Image on screen
        byte[] bytes = System.IO.File.ReadAllBytes(fullpath);
        imageReturnVals.Add(returnVal);
        imageByteList.Add(bytes);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);

        ImageResult[_numberOfImages].GetComponent<Image>().sprite = Sprite.Create(texture,
                                       new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        SpinnerArr[_numberOfImages].SetActive(false);
    }

    void OnSendImageToAI(byte[] bytes) {

        imageResults.SetActive(true);
        ReGenerateImages.SetActive(true);
        ChooseImage.SetActive(true);
        this.currentState = State.GENERATING;
        currentScreenshot = bytes;
        foreach (GameObject spinner in SpinnerArr)
            spinner.SetActive(true);
        
        foreach (GameObject image in ImageResult)
            image.SetActive(true); 
        
        foreach (GameObject text in TextResult)
            text.SetActive(false);
        StartCoroutine(StableDiffusionInference(bytes));
        _numberOfImages = 0;
        EnableSelectionButtons();
    }

    // Calls llama to complete the sentence
    IEnumerator GetFullSentences(byte[] bytes, float temperature, System.Action<string> callback)
    {
        string url = "http://127.0.0.1:8000/api/chat/fullsentences?prompt=" + Metadata.Instance.currentPrompt + "&temperature=" + temperature;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            int count = 0;
            while (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                request = UnityWebRequest.Post(url, form);
                yield return request.SendWebRequest();
                count++;
                if (count > 10)
                {
                    EventSystem.instance.RestartSceneEvent();
                }
            }
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string generated_description = returnVal["generated_description"].ToString();
            callback(generated_description);
        }
        else
        {
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string generated_description = returnVal["generated_description"].ToString();
            callback(generated_description);
        }
    }

    // Calls llama to complete the sentence
    IEnumerator GetNextprompt(string previous_story, System.Action<string> callback)
    {
        string url = "http://127.0.0.1:8000/api/chat/nextprompts?prev_story=" + previous_story;
        WWWForm form = new WWWForm();
        //form.headers["Content-Type"] = "multipart/form-data";
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            EventSystem.instance.RestartSceneEvent();
            int count = 0;

            while (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                request = UnityWebRequest.Post(url, form);
                yield return request.SendWebRequest();
                count++;
                if (count > 10)
                {

                }
            }

            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string generated_description = returnVal["next_prompt"].ToString();
            callback(generated_description);
        }
        else
        {
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string generated_description = returnVal["next_prompt"].ToString();
            callback(generated_description);
        }

    }

    // Calls Tiny-Llama
    IEnumerator GetChapterStories(string completion, System.Action<string> callback)
    {
        string url = "http://127.0.0.1:8000/api/chat/chapterstories?ch_index=" + Metadata.Instance.currentChapter + "&prompt=" + completion ;
        WWWForm form = new WWWForm();
        //form.headers["Content-Type"] = "multipart/form-data";
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                EventSystem.instance.RestartSceneEvent();
                int count = 0;
                
                while (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.error);
                    request = UnityWebRequest.Post(url, form);
                    yield return request.SendWebRequest();
                    count++;
                    if (count > 10)
                    {
                        
                    }
                }
                
                Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, string>>(request.downloadHandler.text);
                string generated_description = returnVal["generated_description"].ToString();
                callback(generated_description);
            }
            else
            {
                Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, string>>(request.downloadHandler.text);
                string generated_description = returnVal["generated_description"].ToString();
                callback(generated_description);
            }
        
    }


    private void UnselectImage()
    {
        for (int i = 0; i < 4; i++)
        {
            SelectionArr[i].SetActive(false);
        }
    }
    public void OnSelectImage(int i)
    {
        Debug.Log("Selecting Image");
        UnselectImage();
        SelectionArr[i].SetActive(true);
        _currentSelectedImage = ImageResult[i].GetComponent<Image>();
        selectedImageIndex = i;
        _currentSelectedImageBytes = imageByteList[i];
    }
    public void OnSelectText(int i)
    {
        Debug.Log("Selecting Text");
        UnselectImage();
        SelectionArr[i].SetActive(true);
        _currentSelectedText = TextResult[i].GetComponent<TextMeshProUGUI>().text;
        selectedTextIndex = i;
    }

}

