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

        bookCovers.SetActive(true);
        draw.SetActive(false);
        result.SetActive(false);
        sentence.SetActive(false);
        navigation.SetActive(false);
        ownership.SetActive(false);
        book.SetActive(false);
        overlay.SetActive(false);
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
        bookCovers.SetActive(false);
        book.SetActive(true);
    }

    public void OnEnableDrawingScreen()
    {
        draw.SetActive(true);
    }

    public void onEscape()
    {
        overlay.SetActive(true);
        escapeButton.SetActive(false);
        mainCanvas.SetActive(false);

    }

    public void onNo()
    {
        overlay.SetActive(false);
        escapeButton.SetActive(true);
        mainCanvas.SetActive(true);
    }

    public void onYes()
    {
        overlay.SetActive(false);
        escapeButton.SetActive(true);
        mainCanvas.SetActive(true);
        SceneManager.LoadScene("Playthrough");
    }

    void Finish()
    {
        EventSystem.instance.PublishMetadataEvent();
        EventSystem.instance.SaveCurrentCoverEvent();
        Metadata.Instance.currentChapter = "ch1";
        Metadata.Instance.currentTextPage = 0;
        SceneManager.LoadScene("Playthrough");
    }

}
