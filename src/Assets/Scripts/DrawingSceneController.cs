using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class DrawingScreenController : MonoBehaviour
{

    public Button UndoButton;
    public GameObject ButtonGroup;

    public Camera BookCamera;
    public Camera UICamera;
    public GameObject Display1;
    public GameObject Display2;
    public GameObject LineGeneratorPrefab;
    public GameObject ButtonGroupPrefab;
    public GameObject SendToAIButton;
    public GameObject Slider;
    public Canvas DrawingCanvas;
    public RectTransform rectT;

    public int ShowDisplay;
    private GameObject InstantiatedLineGenerator;

    private string url = "http://localhost:5000/api/Drawings";

    [SerializeField]
    float widthSelected = 1.4f;



    // Start is called before the first frame update

    void Start()
    {
        Display1.SetActive(true);
        Display2.SetActive(false);

        EventSystem.instance.SendToAI += OnSendToAI;

        UndoButton.onClick.AddListener(OnUndoButtonClicked);
        SendToAIButton.GetComponent<Button>().onClick.AddListener(OnSendToAI);

        foreach (Button button in ButtonGroup.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => OnColorButtonClicked(button));
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }


    public void SwitchCamera()
    {
        if(ShowDisplay == 2)
        {
            Destroy(InstantiatedLineGenerator);
            BookCamera.enabled = true;
            Display1.SetActive(true);
            UICamera.enabled = false;
            Display2.SetActive(false);
            ShowDisplay = 1;
        }
        else if(ShowDisplay == 1)
        {
            InstantiateLineGenerator();
            UICamera.enabled = true;
            Display2.SetActive(true);
            BookCamera.enabled = false;
            Display1.SetActive(false);
            ShowDisplay = 2;
        }
    }

    public void OnSendToAI()
    {
        
        Debug.Log("Send to AI");
        StartCoroutine(CoroutineScrenshot((bytes)=>
        {
          
            StartCoroutine(SendImageToAI(bytes));
        }));
        
        
        //StartCoroutine(CreateStoryBook());
        

    }

    IEnumerator SendImageToAI(byte[] bytes)
    {
        string url = "http://127.0.0.1:8000/api/images/f57bfb83-317c-4c54-8881-d37be8fae019";
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("image", bytes));
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();


        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete! " + www.downloadHandler.text);

        }

        /*
        UnityWebRequest www = UnityWebRequest.Post(http://127.0.0.1:8000/api/images/f57bfb83-317c-4c54-8881-d37be8fae019, formData);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Image Upload Complete!");
        }
        */
    }

    IEnumerator CreateStoryBook()
    {

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/api/<", "{ \"title\": \"unity goes brr\", \"duration\": 2, \"iterations\": 0,\"status\": true }", "application/json"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
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
            child.localScale = new Vector3(1, 1, 1);
        }
        button.transform.localScale = new Vector3(widthSelected, widthSelected, widthSelected);
        EventSystem.instance.PressColorButtonEvent(button.GetComponent<Image>().color);

    }


    void InstantiateLineGenerator()
    {
        InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
        InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = Display2.GetComponent<Canvas>();
        InstantiatedLineGenerator.GetComponent<LineGenerator>().Slider = Slider;
    }


    //Code inspired by https://www.youtube.com/watch?v=d5nENoQN4Tw andhttps://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
    private IEnumerator CoroutineScrenshot(System.Action<byte[]>callback)
    {
        yield return new WaitForEndOfFrame();

        Rect rect = RectTransformUtility.PixelAdjustRect(rectT, DrawingCanvas); //public vars
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

        /*
        Vector2 temp = rectT.transform.position;
        int width = System.Convert.ToInt32(rectT.rect.width);
        int height = System.Convert.ToInt32(rectT.rect.height);
        var startX = temp.y - height / 2;
        var startY = temp.x - width / 2;  
        Debug.Log(width);
        Debug.Log(height);
        Debug.Log(temp.y);
        Debug.Log(temp.x);


        //Picture.transform.position.y, Picture.transform.position.x,
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        tex.Apply();
        
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);


        */
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string filePath = Application.dataPath + "/" + fileName;
        Debug.Log(filePath);
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Screenshot taken");
        callback(bytes);
    }
}
