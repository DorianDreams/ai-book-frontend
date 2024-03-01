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
    private GameObject InstantiatedLineGenerator;


    void Awake()
    {
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        EventSystem.instance.DisableDrawingScreen += Disable;
        EventSystem.instance.EnableDrawingScreen += Enable;
    }

    void Start()
    {
        EventSystem.instance.StartStory += ShowDrawingSceneStart;

        // Assign listeners to buttons
        UndoButton.onClick.AddListener(OnUndoButtonClicked);
        SendToAIButton.GetComponent<Button>().onClick.AddListener(OnSendToAI);
        DeleteAllButton.GetComponent<Button>().onClick.AddListener(EventSystem.instance.DeleteAllLinesEvent);



        foreach (Button button in ButtonGroup.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => OnColorButtonClicked(button));
        }
    }

    private void Enable()
    {
        DrawingMode.SetActive(true);
    }

    private void Disable()
    {
        DrawingMode.SetActive(false);
    }

    void ShowDrawingSceneStart()
    {
        DrawingMode.SetActive(true);
        InstantiateLineGenerator();
    }

    void ShowDrawingSceneEnd()
    {
        UndoButton.gameObject.SetActive(false);
        ButtonGroup.SetActive(false);
        DrawingBackground.SetActive(false);
        Destroy(InstantiatedLineGenerator);
    }
 
    public void OnSendToAI()
    {
        StartCoroutine(CoroutineScrenshot((bytes) =>
        {
            EventSystem.instance.SendImageToAIEvent(bytes);     
        }));
    }

    //Code inspired by https://www.youtube.com/watch?v=d5nENoQN4Tw and https://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
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

        screenShot.ReadPixels(new Rect(startX, startY + 120, textWidth, textHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string filePath = Application.dataPath + "/" + fileName;

        System.IO.File.WriteAllBytes(filePath, bytes);
        
        // Drawing Mode no longer needed
        EventSystem.instance.HideLinesEvent();
        EventSystem.instance.DisableDrawingScreenEvent();
        EventSystem.instance.EnableResultScreenEvent();

        callback(bytes);
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
    }
}
