using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Security.Cryptography;

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
    public GameObject ImageResult0;
    public GameObject ImageResult0Selected;
    public GameObject ImageResult1;
    public GameObject ImageResult1Selected;
    public GameObject ImageResult2;
    public GameObject ImageResult2Selected;
    public GameObject ImageResult3;
    public GameObject ImageResult3Selected;

    [Header("Buttons")]
    public GameObject PublishToBook;
    public GameObject BacktoDrawing;
    public GameObject ReGenerateImages;

    //[Header("Textboxes")]
    //public GameObject ImageCaptionProposal;
    //public GameObject AIUnderstandingText;
    //public GameObject NotSatisfiedTextBox;

    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString AIUnderstandingSentence;
    [SerializeField]
    private LocalizedString NotSatisfiedText;

    [Header("Stable Diffusion Tuning")]
    [SerializeField]
    float strength = 0.5f;

    //Todo: interaction with book controller?
    string chapter = "chapter_1_prompt";

    private int numberOfImages; // keeps track of number of images on the result screen
    private Image currentSelectedImage = null;
    private int selectedImageIndex;

    public GameObject Spinner1;
    public GameObject Spinner2;
    public GameObject Spinner3;
    public GameObject Spinner4;


    private byte[] currentScreenshot;

    private List<byte[]> imageByteList = new List<byte[]>();
    private List<Dictionary<string, object>> imageReturnVals = new List<Dictionary<string, object>>();

    private void Start()
    {
        Spinner1.SetActive(false);
        Spinner2.SetActive(false);
        Spinner3.SetActive(false);
        Spinner4.SetActive(false);

        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.EnableResultScreen += Enable;
        EventSystem.instance.DisableResultScreen += Disable;
        EventSystem.instance.SendImageToAI += OnSendImageToAI;
        
        DisableSelectionButtons();
        ImageResult0.GetComponent<Button>().onClick.AddListener(() => SelectImage(0) );
        ImageResult1.GetComponent<Button>().onClick.AddListener(() => SelectImage(1));
        ImageResult2.GetComponent<Button>().onClick.AddListener(() => SelectImage(2));
        ImageResult3.GetComponent<Button>().onClick.AddListener(() => SelectImage(3));
        

        BacktoDrawing.GetComponent<Button>().onClick.AddListener(OnBackToDrawing);
        PublishToBook.GetComponent<Button>().onClick.AddListener(OnPublishToBook);
        ReGenerateImages.GetComponent<Button>().onClick.AddListener(OnRegenerateImages);
        
        numberOfImages = 0;
    }

    private void DisableSelectionButtons()
    {
        ImageResult0.GetComponent<Button>().interactable = false;
        ImageResult1.GetComponent<Button>().interactable = false;
        ImageResult2.GetComponent<Button>().interactable = false;
        ImageResult3.GetComponent<Button>().interactable = false;
    }
    private void EnableSelectionButtons() {         
           ImageResult0.GetComponent<Button>().interactable = true;
           ImageResult1.GetComponent<Button>().interactable = true;
           ImageResult2.GetComponent<Button>().interactable = true;
           ImageResult3.GetComponent<Button>().interactable = true;
       }
    private void Enable()
    {
        ResultScreen.SetActive(true);
    }
    private void Disable()
    {
        ResultScreen.SetActive(false);
        ImageResult0.GetComponent<Image>().sprite = null;
        ImageResult1.GetComponent<Image>().sprite = null;
        ImageResult2.GetComponent<Image>().sprite = null;
        ImageResult3.GetComponent<Image>().sprite = null;
        currentSelectedImage = null;
        DisableSelectionButtons();

        //ImageCaptionProposal.SetActive(false);
        //ImageCaptionProposal.GetComponent<TextMeshProUGUI>().text = "";

        //PublishToBook.SetActive(false);
        //NotSatisfiedTextBox.SetActive(false);
        //AIUnderstandingText.SetActive(false);
        UnselectImage();
        numberOfImages = 0;
    }

    void OnChangeLocale()
    {
        //AIUnderstandingText.GetComponent<TextMeshProUGUI>().text = AIUnderstandingSentence.GetLocalizedString();
        //NotSatisfiedTextBox.GetComponent<TextMeshProUGUI>().text = NotSatisfiedText.GetLocalizedString();
    }

    public void OnRegenerateImages()
    {

        ImageResult0Selected.SetActive(false);
        ImageResult1Selected.SetActive(false);
        ImageResult2Selected.SetActive(false);
        ImageResult3Selected.SetActive(false);
        ImageResult0.GetComponent<Image>().sprite = null;
        ImageResult1.GetComponent<Image>().sprite = null;
        ImageResult2.GetComponent<Image>().sprite = null;
        ImageResult3.GetComponent<Image>().sprite = null;

        OnSendImageToAI(currentScreenshot);
    }

    public void OnBackToDrawing()
    {
        EventSystem.instance.ContinueDrawingEvent();
        EventSystem.instance.DisableResultScreenEvent();
    }

    public void OnPublishToBook()
    {
        if (currentSelectedImage != null){
        byte[] bytes = imageByteList[selectedImageIndex];
        imageByteList.Clear();
        Dictionary<string, object> returnVal = imageReturnVals[selectedImageIndex];

        /* //Remove unused Images
            for(int i = 0; i <4; i++){
                if (i==selectedImageIndex){
                    continue;
                } else {
                    StartCoroutine(DeleteStoryImage(imageReturnVals[selectedImageIndex]["id"].ToString()));
                }
            }
        */

        string imgID = returnVal["id"].ToString();
        string imgPath = returnVal["image"].ToString();

        imageReturnVals.Clear();

        DisableSelectionButtons();
        StartCoroutine(GetFullSentences(bytes, 0.5f, (completed_sentence) =>
        {
            StartCoroutine(GetChapterStories(completed_sentence,(story_generation) =>
            {
                //StartCoroutine(PostImageDescription(bytes, imgID, (completed_sentence, bytes) =>
                //{
                    numberOfImages = 0;
                    
                    EventSystem.instance.PublishToBookEvent(currentSelectedImage.sprite, completed_sentence,
                        story_generation, selectedImageIndex, bytes);
                    EventSystem.instance.DisableResultScreenEvent();

                    EventSystem.instance.EnableBookNavigatorEvent();
     
                //}));
            }));
        }));
        }
    }



    void OnSendImageToAI(byte[] bytes) {
        this.currentState = State.GENERATING;
        currentScreenshot = bytes;

        //AIUnderstandingText.SetActive(true);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        Spinner1.SetActive(true);
        Spinner2.SetActive(true);
        Spinner3.SetActive(true);
        Spinner4.SetActive(true);

        StartCoroutine(GetImageCaption(bytes, (caption, bytes) =>
        {
            Debug.Log("Caption: " + caption);
            

            StartCoroutine(SendImageToAIIteration(caption, 0.5f, bytes, (caption,bytes) =>
        {
            
            StartCoroutine(SendImageToAIIteration(caption,0.8f, bytes, (caption, bytes) => 
            {
                
                StartCoroutine(SendImageToAIIteration(caption,0.9f,bytes, (caption, bytes) =>
                {
                    
                    StartCoroutine(SendImageToAIIteration(caption,1f, bytes, (caption, bytes) =>
                    {
                        EnableSelectionButtons();
                        //NotSatisfiedTextBox.SetActive(true);
                        //NotSatisfiedTextBox.GetComponent<TextMeshProUGUI>().text = NotSatisfiedText.GetLocalizedString();
                        numberOfImages = 0;
                    }));
                }));
            }));
        }));
    }));
    }

    IEnumerator GetImageCaption(byte[] bytes, System.Action<string, byte[]> callback)
    {
        string url = "http://127.0.0.1:8000/api/chat/captions?prompt=" + Metadata.Instance.currentPrompt;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";
        Debug.Log(form.ToString());

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            int count = 0;
            while(request.result != UnityWebRequest.Result.Success)
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
            string caption = returnVal["caption"].ToString();
            callback(caption, bytes);
        }
        else
        {
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string caption = returnVal["caption"].ToString();
            callback(caption, bytes);
        }
    }

    IEnumerator DeleteStoryImage(string image_id)
    {
        string url = "http://127.0.0.1:8000/api/images/delete/"+ image_id;
        UnityWebRequest request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
    }


    IEnumerator PostImageDescription(byte[] bytes, string image_id, System.Action<string, byte[]> callback)
    {
        string url = "http://127.0.0.1:8000/api/descriptions/" + Metadata.Instance.storyBookId  + "/" + image_id +
                                                "?prompt=" + Metadata.Instance.currentPrompt ;

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";
        Debug.Log(form.ToString());

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            int count = 0;
            while(request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                request = UnityWebRequest.Post(url, form);
                yield return request.SendWebRequest();
                count++;
                if (count > 10)
                {
                    EventSystem.instance.RestartSceneEvent();
                    break;
                }
            }
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string description = returnVal["description"].ToString();
            callback(description, bytes);
        }
        else
        {
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string description = returnVal["description"].ToString();
            callback(description, bytes);
        }
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

    // Calls Tiny-Llama
    IEnumerator GetChapterStories(string completion, System.Action<string> callback)
    {
        string url = "http://127.0.0.1:8000/api/chat/chapterstories?prompt=" + Metadata.Instance.currentPrompt + completion + "&ch_index=" +Metadata.Instance.currentChapter ;
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


    IEnumerator SendImageToAIIteration(string caption, float strength, byte[] bytes, System.Action<string,byte[]> callback)
    {
        Debug.Log("Send to AI");
        string json = "{\"strength" + "\":" + strength + ",\"story_chapter" + "\":\"" + chapter + "\"}";        
        string prompt = Metadata.Instance.currentPrompt + caption;
        string url = "http://127.0.0.1:8000/api/images/" + Metadata.Instance.storyBookId
                                              + "?prompt=" + prompt
                                              + "&parameters=" + json;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";
        Debug.Log(form.ToString());

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
            Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, object>>(request.downloadHandler.text);

            showImageSelection(returnVal);
            numberOfImages++;

            callback(caption, bytes);

        }
        else
        {
            Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, object>>(request.downloadHandler.text);

            showImageSelection(returnVal);
            numberOfImages++;

            callback(caption,bytes);
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
        switch (numberOfImages)
        {   
            case 0:
                ImageResult0.GetComponent<Image>().sprite = Sprite.Create(texture,
                                                          new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                Spinner1.SetActive(false);
                break;
            case 1:
                ImageResult1.GetComponent<Image>().sprite = Sprite.Create(texture,
                                       new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                Spinner2.SetActive(false);
                break;
            case 2:
                ImageResult2.GetComponent<Image>().sprite = Sprite.Create(texture,
                                       new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                Spinner3.SetActive(false);
                break;
            case 3:
                ImageResult3.GetComponent<Image>().sprite = Sprite.Create(texture,
                                       new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                Spinner4.SetActive(false);
                break;
        }
    }

    private void UnselectImage()
    {
        ImageResult0Selected.SetActive(false);
        ImageResult1Selected.SetActive(false);
        ImageResult2Selected.SetActive(false);
        ImageResult3Selected.SetActive(false);
    }
    private void SelectImage(int i)
    {
        //AIUnderstandingText.SetActive(false);
        UnselectImage();
        //ImageCaptionProposal.GetComponent<TextMeshProUGUI>().text = null;
        //ImageCaptionProposal.SetActive(false);
        PublishToBook.SetActive(true);

        switch (i)
        {
            case 0:
                ImageResult0Selected.SetActive(true);
                currentSelectedImage = ImageResult0.GetComponent<Image>();
                selectedImageIndex = 0;
                break;
            case 1:
                ImageResult1Selected.SetActive(true);
                currentSelectedImage = ImageResult1.GetComponent<Image>();
                selectedImageIndex = 1;
                break;
            case 2:
                ImageResult2Selected.SetActive(true);
                currentSelectedImage = ImageResult2.GetComponent<Image>();
                selectedImageIndex = 2;
                break;
            case 3:
                ImageResult3Selected.SetActive(true);
                currentSelectedImage = ImageResult3.GetComponent<Image>();
                selectedImageIndex = 3;
                break;
        }
    }

}

