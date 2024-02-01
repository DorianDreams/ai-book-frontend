using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{

    //Display Control - for switching between drawing and book
    public Camera BookCamera;
    public Camera UICamera;
    public GameObject Display1;
    public GameObject Display2;
    public int ShowDisplay = 1;
    // Start is called before the first frame update
    void Start()
    {
        //Screen.fullScreen = false;

        Screen.SetResolution(3840, 2160, true);
        EventSystem.instance.SwitchCamera += switchCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            EventSystem.instance.SwitchCameraEvent();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }


    public void switchCamera()
    {
        if (ShowDisplay == 2)
        {
            Screen.SetResolution(3840, 2160, true);
            BookCamera.enabled = true;
            Display1.SetActive(true);
            UICamera.enabled = false;
            Display2.SetActive(false);
            ShowDisplay = 1;
            //Screen.fullScreen = false;

            //Screen.SetResolution(1920, 1080, true);
        }
        else if (ShowDisplay == 1)
        {
            Screen.SetResolution(3840, 2160, true);
            UICamera.enabled = true;
            Display2.SetActive(true);
            BookCamera.enabled = false;
            Display1.SetActive(false);
            ShowDisplay = 2;
           // Screen.fullScreen = false;

            //Screen.SetResolution(1080, 1920, true);
        }
    }
}
