using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using TMPro;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Xml.Linq;

public class ResultScreenController : MonoBehaviour
{
    //Resulting Image for Selection

    private bool _isgenerating = false;


    public GameObject ResultScreen;

    [Header("Result Images")]
    public GameObject imageResults;
    public GameObject Spinners;
    public GameObject imageResultBackgrounds;

    private GameObject[] SpinnerArr = new GameObject[4];
    private GameObject[] ImageResult = new GameObject[4];
    private GameObject[] ImageResultBackground = new GameObject[4];

    [Header("Buttons")]
    public GameObject ChooseImage;
    public GameObject BacktoDrawing;
    public GameObject ReGenerateImages;

    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString RegenText;

    public GameObject RegenTextBox;
    //Todo: interaction with book controller?
    string chapter = "chapter_1_prompt";

    // private vars for game states
    private int _numberOfImages; 
    private Image _currentSelectedImage = null;
    private byte[] _currentSelectedImageBytes = null;
    private int selectedImageIndex;
    private byte[] currentScreenshot;
    private List<byte[]> imageByteList = new List<byte[]>();
    private List<Dictionary<string, string>> imageReturnVals = new List<Dictionary<string, string>>();


    private void OnEnable()
    {
    }
    private void Awake()
    {
        ChooseImage.GetComponent<Button>().interactable = false;
        for (int i = 0; i < 4; i++)
        {
            SpinnerArr[i] = Spinners.transform.GetChild(i).gameObject;
            ImageResult[i] = imageResults.transform.GetChild(i).gameObject;
            ImageResultBackground[i] = imageResultBackgrounds.transform.GetChild(i).gameObject;
        }


        foreach (GameObject spinner in SpinnerArr)
            spinner.SetActive(false);

        EventSystem.instance.EnableResultScreen += Enable;
        EventSystem.instance.DisableResultScreen += Disable;
        EventSystem.instance.SendImageToAI += OnSendImageToAI;

        DisableSelectionButtons();
        RegenTextBox.GetComponent<TextMeshProUGUI>().text = RegenText.GetLocalizedString();
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
        _numberOfImages = 0;
    }

    public void OnRegenerateImages()
    {
        DisableSelectionButtons();
        _isgenerating = true;
        for (int i = 0; i < 4; i++)
        {
            ImageResult[i].GetComponent<Image>().sprite = null;
            SpinnerArr[i].SetActive(true);
        }
        OnSendImageToAI(currentScreenshot);
    }

    public void OnBackToDrawing()
    {
        EventSystem.instance.ContinueDrawingEvent();
        EventSystem.instance.DisableResultScreenEvent();
    }

    public void OnChooseImage()
    {
        Debug.Log("Choose Image");
        Debug.Log(_currentSelectedImage);
        if (_currentSelectedImage != null){
        byte[] bytes = imageByteList[selectedImageIndex];
        
        foreach (GameObject spinner in SpinnerArr)
            { spinner.SetActive(true); }

        foreach (GameObject image in ImageResult)
            { image.SetActive(false); }
        _numberOfImages = 0;
        ChooseImage.SetActive(false);
        ReGenerateImages.SetActive(false);

        Metadata.Instance.currentImgID = imageReturnVals[selectedImageIndex]["id"].ToString();
            imageByteList.Clear();
            imageReturnVals.Clear();


            EventSystem.instance.SelectImageEvent(_currentSelectedImage.sprite, selectedImageIndex, bytes);
            EventSystem.instance.DisableResultScreenEvent();
            //EventSystem.instance.EnableBookNavigatorEvent();
        }
    }


    IEnumerator StableDiffusionInference(byte[] screenshot)
    {
        EventSystem.instance.DisableRestartButtonEvent();
        ChooseImage.GetComponent<Button>().interactable = false;
        BacktoDrawing.GetComponent<Button>().interactable = false;
        ReGenerateImages.GetComponent<Button>().interactable = false;

        _numberOfImages = 0;
        CoroutineWithData cd_caption = new CoroutineWithData(this, Request.GetImageCaption(screenshot));
        yield return cd_caption.coroutine;
        string caption = (string)cd_caption.result;

        float[] strengthVals = { 0.5f, 0.8f, 0.9f, 1f };
        foreach (float strength in strengthVals)
        {
            CoroutineWithData cd_image = new CoroutineWithData(this, Request.GetImageGeneration(caption, strength, screenshot));
            yield return cd_image.coroutine;
            Dictionary<string, string> returnVal = (Dictionary<string, string>)cd_image.result;
            showImageSelection(returnVal);
            _numberOfImages++;
        }
        BacktoDrawing.GetComponent<Button>().interactable = true;
        ReGenerateImages.GetComponent<Button>().interactable = true;
        EnableSelectionButtons();
        _isgenerating = false;
        EventSystem.instance.CubeOffEvent();
        EventSystem.instance.EnableRestartButtonEvent();

    }


    public void showImageSelection(Dictionary<string, string> returnVal)
    {   
        string imagePath = returnVal["image"].ToString();
        string operatingSystem = SystemInfo.operatingSystem;
        string fullpath = "";
        if (operatingSystem.Contains("Windows"))
        {
            imagePath = imagePath.Replace("/", "\\");
            fullpath = "..\\..\\storybookcreator" + imagePath;

        }
        else if (operatingSystem.Contains("Mac"))
        {
            imagePath = imagePath.Replace("\\", "/");
        }
        else if (operatingSystem.Contains("Linux"))
        {
            imagePath = imagePath.Replace("\\", "/");

            fullpath = "../../storybookcreator" + imagePath;
        }

        // Display image on screen
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
        _isgenerating = true;
        foreach (GameObject image in ImageResultBackground)
            image.SetActive(false);
        EventSystem.instance.CubeCrossfadeEvent();

        imageResults.SetActive(true);
        ReGenerateImages.SetActive(true);
        ChooseImage.SetActive(true);
        currentScreenshot = bytes;
        foreach (GameObject spinner in SpinnerArr)
            spinner.SetActive(true);
        
        foreach (GameObject image in ImageResult)
            image.SetActive(true); 
        
        StartCoroutine(StableDiffusionInference(bytes));
        _numberOfImages = 0;
        //EnableSelectionButtons();

        //_isgenerating = false;
    }

    public void OnSelectImage(int i)
    {
        if (!_isgenerating) {
        foreach(GameObject image in ImageResultBackground)
            image.SetActive(false);
        Debug.Log("Selecting Image");
        _currentSelectedImage = ImageResult[i].GetComponent<Image>();
        selectedImageIndex = i;
        _currentSelectedImageBytes = imageByteList[i];
        currentScreenshot = _currentSelectedImageBytes;
        ChooseImage.GetComponent<Button>().interactable = true;
        ImageResultBackground[i].SetActive(true); 
        }
    }
}

