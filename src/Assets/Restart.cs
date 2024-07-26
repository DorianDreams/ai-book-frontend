using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{

    public GameObject Overlay;
    public GameObject EscapeButton;
    public GameObject MainCanvas;

    public void onEscape()
    {
        Overlay.SetActive(true);
        EscapeButton.SetActive(false);
        MainCanvas.SetActive(false);

    }

    public void onNo()
    {
        Overlay.SetActive(false);
        EscapeButton.SetActive(true);
        MainCanvas.SetActive(true);
    }

    public void onYes()
    {
        Overlay.SetActive(false);
        EscapeButton.SetActive(true);
        MainCanvas.SetActive(true);
        Metadata.Instance.currentPrompt = "";
        Metadata.Instance.currentChapter = "ch1";
        Metadata.Instance.currentTextPage = 0;
        Metadata.Instance.storyBookId = "";
        Metadata.Instance.currentPrompt = "";
        Metadata.Instance.startingPrompt = "";
        Metadata.Instance.previousPrompt = "";
        Metadata.Instance.currentImgID = "";
        Metadata.Instance.storyBook = new StoryBook();
        SceneManager.LoadScene("StartScene");
    }
}
