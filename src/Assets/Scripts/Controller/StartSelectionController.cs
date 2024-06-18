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
    public GameObject LanguageSelection;
    public GameObject StartingImages;
    public GameObject InfoBox;
    public GameObject imageResultBackgrounds;
    private GameObject[] ImageResultBackground = new GameObject[4];

    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString StartingText;


    private int _currentSelectedIndex;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            ImageResultBackground[i] = imageResultBackgrounds.transform.GetChild(i).gameObject;
        }
        StartingScreen.SetActive(true);
        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }

    public void OnLanguageButtonClick()
    {
        LanguageSelection.SetActive(false);
        StartingImages.SetActive(true);
    }

    public void OnShowLanguageSelection()
    {
        StartingImages.SetActive(false);
        InfoBox.SetActive(false);
        LanguageSelection.SetActive(true);
    }
    
    public void OnShowInfoBox()
    {
        InfoBox.SetActive(true);
        StartingImages.SetActive(false);
        LanguageSelection.SetActive(false);
    }
    public void OnCloseInfoBox()
    {
        InfoBox.SetActive(false);
        StartingImages.SetActive(true);
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
        switch(_currentSelectedIndex)
        {
            case 0:
                Metadata.Instance.startingPrompt = "Edgar the elephant loves music. He plays";
                Metadata.Instance.currentPrompt = "Edgar the elephant loves music. He plays";
                break;
            case 1:
                Metadata.Instance.startingPrompt = "Ratty lives in a dustbin. He dreams of being";
                Metadata.Instance.currentPrompt = "Ratty lives in a dustbin. He dreams of being";
                break;
            case 2:
                Metadata.Instance.startingPrompt = "Rachel ruysch, still life with flowers, oil painting";
                Metadata.Instance.currentPrompt = "Rachel ruysch, still life with flowers, oil painting";
                break;
            case 3:
                Metadata.Instance.startingPrompt = "Wanda is a witch. She always";
                Metadata.Instance.currentPrompt = "Wanda is a witch. She always";
                break;
        }
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
        EventSystem.instance.CubeBlinkEvent();
    }


    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }

    public void OnSelectImage(int i)
    {
        foreach (GameObject image in ImageResultBackground)
            image.SetActive(false);
        _currentSelectedIndex = i;
        ImageResultBackground[i].SetActive(true);
    }
}
