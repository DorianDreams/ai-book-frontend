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
    public Button UndoButton;
    public GameObject ButtonGroup;
    [SerializeField]
    float SelectedButtonWidth = 1.4f;
    [SerializeField]
    float StandardButtonWidth = 1f;
    [SerializeField]
    float LineWidth = 1f;

    public GameObject SendToAIButton;
    public GameObject DeleteAllButton;
    public GameObject DrawingBackground;
    public GameObject LineGeneratorPrefab;
    private GameObject InstantiatedLineGenerator=null;

    private DrawingPage drawingPage;
    int currentIteration = 0;
    float timer = 0.0f;
    bool measureTime = false;



    void Awake()
    {
        EventSystem.instance.StartStory += Enable;
        EventSystem.instance.PublishToBook += OnPublishToBook;
        EventSystem.instance.DisableDrawingScreen += Disable;
        EventSystem.instance.EnableDrawingScreen += Enable;
        EventSystem.instance.PauseDrawing+= PauseDrawing;
        EventSystem.instance.ContinueDrawing += ContinueDrawing;
    }

    void Start()
    {

        // Assign listeners to buttons
        UndoButton.onClick.AddListener(OnUndoButtonClicked);
        SendToAIButton.GetComponent<Button>().onClick.AddListener(OnSendToAI);
        DeleteAllButton.GetComponent<Button>().onClick.AddListener(EventSystem.instance.DeleteAllLinesEvent);

        foreach (Button button in ButtonGroup.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => OnColorButtonClicked(button));
        }
    }

    void Update()
    {
        if (measureTime)
        {
            timer += Time.deltaTime;
        }
    }

    private void Enable()
    {
        if (InstantiatedLineGenerator == null)
        {
            InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
            InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = DrawingCanvas;
            InstantiatedLineGenerator.GetComponent<LineGenerator>().width = LineWidth;
        }
        
        measureTime = true;
        DrawingMode.SetActive(true);
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


    private void ContinueDrawing()
    {
        measureTime = true;
        DrawingMode.SetActive(true);
        EventSystem.instance.ShowLinesEvent();
    }
 
    public void OnSendToAI()
    {
        StartCoroutine(CoroutineScrenshot((bytes) =>
        {
            EventSystem.instance.SendImageToAIEvent(bytes);     
        }));
    }

    //Code inspired by https://www.youtube.com/watch?v=d5nENoQN4Tw
    //and https://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
    private IEnumerator CoroutineScrenshot(System.Action<byte[]> callback)
    {
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
            screenShot.ReadPixels(new Rect(startX, startY - 120, textWidth, textHeight), 0, 0);

        }
        else
        {
            screenShot.ReadPixels(new Rect(startX, startY + 120, textWidth, textHeight), 0, 0);
        }
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string filePath = Application.dataPath + "/" + fileName;

        //System.IO.File.WriteAllBytes(filePath, bytes);

        EventSystem.instance.PauseDrawingEvent();
        EventSystem.instance.EnableResultScreenEvent();

        callback(bytes);
    }

    void OnPublishToBook(Sprite sprite, string description, string continuation, int index)
    {
        drawingPage.selected_image = index;
        drawingPage.time = timer;
        drawingPage.iterations = currentIteration;
        Metadata.Instance.storyBook.drawing.drawingPages.Add(Metadata.Instance.currentChapter, drawingPage);
        timer = 0.0f;
        Disable();
        currentIteration = 0;
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
}
