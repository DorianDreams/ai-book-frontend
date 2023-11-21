using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera BookCamera;
    public Camera UICamera;
    public GameObject Display1;
    public GameObject Display2;
    public GameObject LineGenerator;
    public int ShowDisplay;
    private GameObject InstantiatedLineGenerator;
    
    
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
            InstantiatedLineGenerator = Instantiate(LineGenerator);
            InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = Display2.GetComponent<Canvas>();
            UICamera.enabled = true;
            Display2.SetActive(true);
            BookCamera.enabled = false;
            Display1.SetActive(false);
            ShowDisplay = 2;
        }
    }



    
}
