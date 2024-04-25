using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class StartSelectionController : MonoBehaviour
{
    public GameObject Textboxes;

    // Starting Sentence Selection Screen GameObjects
    [Header("Starting Screen Objects")]
    public GameObject StartingScreen;
    public GameObject StartingHeadline;

    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString StartingText;


    // Start is called before the first frame update
    void Start()
    {
        StartingScreen.SetActive(true);
        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }
    
    public void OnButtonEnglish()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        EventSystem.instance.ChangeLocaleEvent();
    }
    public void OnButtonGerman()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        EventSystem.instance.ChangeLocaleEvent();
    }

    void ShowDrawingSceneStart()
    {
        StartingScreen.SetActive(false);
    }
    public void OnStartStoryButton()
    {
        Metadata.Instance.currentTextPage = 1;
        if (Metadata.Instance.currentPrompt == "")
        {
            Debug.Log("Get Random Prompt");
            string random_prompt = Textboxes.GetComponent<Textboxes>().getRandomInitialPrompt();
            Metadata.Instance.currentPrompt = random_prompt;
            Metadata.Instance.startingPrompt = random_prompt;
        }
        StartCoroutine(Request.CreateStoryBook());
        EventSystem.instance.StartStoryEvent();
    }


    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }
}
