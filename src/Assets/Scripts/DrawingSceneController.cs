using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawingScreenController : MonoBehaviour
{
    public Camera BookCamera;
    public Camera UICamera;
    public GameObject Display1;
    public GameObject Display2;
    public GameObject LineGeneratorPrefab;
    public GameObject ButtonGroupPrefab;
    public GameObject SendToAIButton;

    public int ShowDisplay;
    private GameObject InstantiatedLineGenerator;

    [SerializeField]
    float widthSelected = 1.4f;



    // Start is called before the first frame update

    void Start()
    {
        Display1.SetActive(true);
        Display2.SetActive(false);
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


    void InstantiateLineGenerator()
    {
        InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
        InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = Display2.GetComponent<Canvas>();
        AssignColor("black");
    }


    public void AssignColor(string color)
    {
        InstantiatedLineGenerator.GetComponent<LineGenerator>().selectedColor = color;
        scaleButtonNormal();
        Debug.Log(color);
        switch (color)
        {
            case "green":
                scaleButton(ButtonGroupPrefab.transform.GetChild(0).gameObject);
                break;
            case "orange":
                scaleButton(ButtonGroupPrefab.transform.GetChild(1).gameObject);
                break;
            case "blue":
                scaleButton(ButtonGroupPrefab.transform.GetChild(2).gameObject);
                break;
            case "black":
                scaleButton(ButtonGroupPrefab.transform.GetChild(3).gameObject);
                break;
            case "red":
                scaleButton(ButtonGroupPrefab.transform.GetChild(4).gameObject);
                break;
        }
    }

    void scaleButton(GameObject button)
    {
        button.transform.localScale = new Vector3(widthSelected, widthSelected, widthSelected);
    }

    void scaleButtonNormal()
    {
        foreach (Transform child in ButtonGroupPrefab.transform)
        {
            child.localScale = new Vector3(1, 1, 1);
        }
    }


}
