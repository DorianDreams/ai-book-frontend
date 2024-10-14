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
using System;

public class DrawingScreenController : MonoBehaviour
{
    //Drawing Scene GameObjects
    [Header("Drawing Mode Objects")]
    public GameObject DrawingMode;
    public Canvas DrawingCanvas;
    public GameObject ButtonGroup;

    public GameObject SmallBrush;
    public GameObject MediumBrush;
    public GameObject LargeBrush;

    public GameObject DrawingBackground;
    public GameObject LineGeneratorPrefab;
    private GameObject InstantiatedLineGenerator = null;

    [SerializeField]
    float SelectedButtonWidth = 1.5f;
    [SerializeField]
    float StandardButtonWidth = 1f;
    [SerializeField]
    float LineWidth = 0.2f;

    
    private DrawingPage drawingPage;
    int currentIteration = 0;
    float timer = 0.0f;
    bool measureTime = false;


    void Awake()
    {
        EventSystem.instance.SelectImage += OnPublishToBook;
        EventSystem.instance.DisableDrawingScreen += Disable;
        EventSystem.instance.PauseDrawing += PauseDrawing;
        EventSystem.instance.ContinueDrawing += ContinueDrawing;
        EventSystem.instance.CleanLineGenerator += DestroyLineGenerator;

        foreach (Button button in ButtonGroup.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => OnColorButtonClicked(button));
        }

        SmallBrush.GetComponent<Button>().onClick.AddListener(() => OnSizeButtonClicked(SmallBrush, 0.1f));
        MediumBrush.GetComponent<Button>().onClick.AddListener(() => OnSizeButtonClicked(MediumBrush, 0.2f));
        LargeBrush.GetComponent<Button>().onClick.AddListener(() => OnSizeButtonClicked(LargeBrush, 0.5f));
    }


    // Events
    private void OnEnable()
    {
        if (InstantiatedLineGenerator == null)
        {
            InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
            InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = DrawingCanvas;
            InstantiatedLineGenerator.GetComponent<LineGenerator>().width = LineWidth;
        }
        measureTime = true;
    }

    private void Disable()
    {
        measureTime = false;
        EventSystem.instance.DeleteAllLinesEvent();
        DrawingMode.SetActive(false);
    }

    private void PauseDrawing()
    {
        measureTime = false;
        DrawingMode.SetActive(false);
        EventSystem.instance.HideLinesEvent();
    }

    private void DestroyLineGenerator()
    {
        Destroy(InstantiatedLineGenerator);
    }

    private void ContinueDrawing()
    {
        measureTime = true;
        DrawingMode.SetActive(true);
        EventSystem.instance.ShowLinesEvent();
    }


    void Update()
    {
        if (measureTime)
        {
            timer += Time.deltaTime;
        }
    }

    // Button functions

    public void OnSizeButtonClicked(GameObject button, float size)
    {
        SmallBrush.transform.GetChild(0).gameObject.SetActive(false);
        MediumBrush.transform.GetChild(0).gameObject.SetActive(false);
        LargeBrush.transform.GetChild(0).gameObject.SetActive(false);
        button.transform.GetChild(0).gameObject.SetActive(true);
        EventSystem.instance.SetLineRendererWidthEvent(size);
    }
    public void OnSendToAI()
    {
        StartCoroutine(CoroutineScrenshot((bytes) =>
        {
            
            EventSystem.instance.SendImageToAIEvent(bytes);     
        }));
    }

    public void OnUndoButtonClicked()
    {
        EventSystem.instance.DeleteLastLineEvent();
    }

    public void OnDeleteButtonClicked()
    {
        EventSystem.instance.DeleteAllLinesEvent();
    }

    public void OnColorButtonClicked(Button button)
    {
        foreach (Transform child in ButtonGroup.transform)
        {
            child.GetChild(0).gameObject.SetActive(false);
        }
        button.transform.GetChild(0).gameObject.SetActive(true);
        Color color = button.GetComponent<Image>().color;
        //button.transform.localScale = new Vector3(SelectedButtonWidth, SelectedButtonWidth, SelectedButtonWidth);
        SmallBrush.GetComponent<Image>().color = color;
        MediumBrush.GetComponent<Image>().color = color;
        LargeBrush.GetComponent<Image>().color = color;

        Debug.Log(button.GetComponent<Image>());
        Debug.Log(button.GetComponent<Image>().color);
        Debug.Log(ColorUtility.ToHtmlStringRGB(color));
        String hexcode = "0x" + ColorUtility.ToHtmlStringRGB(color);
        if (hexcode == "0xFFFFFF")
        {
            SmallBrush.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            MediumBrush.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            LargeBrush.transform.GetChild(0).GetComponent<Image>().color = Color.black;
        } else
        {
            SmallBrush.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            MediumBrush.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            LargeBrush.transform.GetChild(0).GetComponent<Image>().color = Color.white;
        }
        EventSystem.instance.CubeShowColorEvent(hexcode);
        EventSystem.instance.PressColorButtonEvent(color);
    }


    void OnPublishToBook(Sprite sprite, int index, byte[] bytes)
    {
        drawingPage.selected_image = index;
        drawingPage.time = timer;
        drawingPage.drawingIterations = currentIteration;
        Metadata.Instance.storyBook.drawing.drawingPages.Add(Metadata.Instance.currentChapter, drawingPage);
        timer = 0.0f;
        Disable();
        currentIteration = 0;
    }

    //Code inspired by https://www.youtube.com/watch?v=d5nENoQN4Tw
    //and https://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
    private IEnumerator CoroutineScrenshot(System.Action<byte[]> callback)
    {
        Debug.Log("Screenshot");
        yield return new WaitForEndOfFrame();

        currentIteration++;
        int points = InstantiatedLineGenerator.GetComponent<LineGenerator>().CountPoints();
        int strokes = InstantiatedLineGenerator.GetComponent<LineGenerator>().CountStrokes();
        drawingPage = new DrawingPage(strokes, 
                                     points,
                                     currentIteration,
                                     timer                                     
                                     );

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

        string operatingSystem = SystemInfo.operatingSystem;
        if (operatingSystem.Contains("Windows"))
        {
            screenShot.ReadPixels(new Rect(startX, startY+170 , textWidth, textHeight), 0, 0);

        }
        else
        {
            screenShot.ReadPixels(new Rect(startX, startY-170, textWidth, textHeight), 0, 0);
        }
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string filePath = Application.dataPath + "/" + fileName;

        //System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Screenshot");

        EventSystem.instance.PauseDrawingEvent();
        EventSystem.instance.EnableResultScreenEvent();

        callback(bytes);
    }
}