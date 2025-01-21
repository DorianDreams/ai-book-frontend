using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


//This controller has all the references to the views, which are enabled/disabled through Events
public class Main : MonoBehaviour
{
    public GameObject start;
    public GameObject draw;
    public GameObject result;
    public GameObject sentence;
    public GameObject navigation;
    public GameObject ownership;
    public GameObject book;
    public GameObject bookCovers;
    //Restart Functionality
    public GameObject overlay;
    public GameObject escapeButton;
    public GameObject mainCanvas;

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.instance.StartStory += OnStartStory;
        EventSystem.instance.EnableDrawingScreen += OnEnableDrawingScreen;
        EventSystem.instance.EnableResultScreen += OnEnableResultScreen;
        EventSystem.instance.DisableResultScreen += OnDisableResultScreen;
        EventSystem.instance.EnableOwnershipScreen += OnEnableOwnsershipScreen;
        EventSystem.instance.DisableOwnershipScreen += OnDisableOwnsershipScreen;

        EventSystem.instance.FinishPlaythrough += OnFinishPlaythrough;
        EventSystem.instance.EnableRestartButton += OnEnableRestartButton;
        EventSystem.instance.DisableRestartButton += OnDisableRestartButton;

        bookCovers.SetActive(true);
        start.SetActive(true);
        draw.SetActive(false);
        result.SetActive(false);
        sentence.SetActive(false);
        navigation.SetActive(false);
        ownership.SetActive(false);
        book.SetActive(false);
        overlay.SetActive(false);
        escapeButton.SetActive(false);

        //Add global error handling that restarts the scene
       // Application.logMessageReceived += HandleException;

    }

    void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            Debug.Log("Global Error Handler: " + logString);
            Debug.Log("Restarting Scene"); 
            SceneManager.LoadScene("Playthrough");
        }
    }

    void OnEnableRestartButton()
    {
        escapeButton.SetActive(true);
    }

    void OnDisableRestartButton()
    {
        escapeButton.SetActive(false);
    }

    public void OnEnableResultScreen()
    {
        result.SetActive(true);
    }

    public void OnDisableResultScreen()
    {
        result.SetActive(false);
    }

    public void OnStartStory() 
    {
        StartCoroutine(Request.CreateStoryBook());
        bookCovers.SetActive(false);
        book.SetActive(true);
    }

    public void OnEnableDrawingScreen()
    {
        draw.SetActive(true);
    }

    public void OnEnableOwnsershipScreen()
    {
        ownership.SetActive(true);
    }

    public void OnDisableOwnsershipScreen()
    { ownership.SetActive(false); }

    public void OnEscape()
    {
        overlay.SetActive(true);
        escapeButton.SetActive(false);
        mainCanvas.SetActive(false);

    }

    public void OnNo()
    {
        overlay.SetActive(false);
        escapeButton.SetActive(true);
        mainCanvas.SetActive(true);
    }

    public void OnYes()
    {
        overlay.SetActive(false);
        escapeButton.SetActive(true);
        mainCanvas.SetActive(true);
        SceneManager.LoadScene("Playthrough");
    }

    void OnFinishPlaythrough()
    {
        EventSystem.instance.SaveCurrentCoverEvent();
        SceneManager.LoadScene("Playthrough");


    }

}
