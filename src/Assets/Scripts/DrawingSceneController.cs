using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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

    IEnumerator

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


 
}
