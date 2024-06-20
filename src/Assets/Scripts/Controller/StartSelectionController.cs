using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

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
    public GameObject ButtonGerman;
    public GameObject ButtonEnglish;
    private GameObject[] ImageResultBackground = new GameObject[4];


    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString StartingText;
    [SerializeField]
    private LocalizedString EdgarText;
    [SerializeField]
    private LocalizedString RobotText;
    [SerializeField]
    private LocalizedString StillText;
    [SerializeField]
    private LocalizedString WandaText;

    [SerializeField]
    private LocalizedString InfoBoxText;

    public GameObject EdgarTextBox;
    public GameObject RobotTextBox;
    public GameObject StillTextBox;
    public GameObject WandaTextBox;
    public GameObject InfoText;

    private bool textboxopen = false;

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
        InfoText.GetComponent<TextMeshProUGUI>().text = InfoBoxText.GetLocalizedString();
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
        if (!textboxopen) { 
        textboxopen = true;
        InfoBox.SetActive(true);
        StartingImages.SetActive(false);
        LanguageSelection.SetActive(false);
            StartingHeadline.SetActive(false);
        }
        else
        {
            InfoBox.SetActive(false);
            StartingImages.SetActive(true);
            StartingHeadline.SetActive(true);
            textboxopen = false;
        }
    }
    public void OnCloseInfoBox()
    {
        InfoBox.SetActive(false);
        StartingImages.SetActive(true);
        StartingHeadline.SetActive(true);
        textboxopen = false;
    }
    
    public void OnButtonEnglish()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        
        EventSystem.instance.ChangeLocaleEvent();
        ButtonGerman.SetActive(true);
        ButtonEnglish.SetActive(false);
    }
    public void OnButtonGerman()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1]; 
        EventSystem.instance.ChangeLocaleEvent();
        ButtonGerman.SetActive(false);
        ButtonEnglish.SetActive(true);

    }

    void ShowDrawingSceneStart()
    {
        StartingScreen.SetActive(false);
    }
    public void OnStartStoryButton()
    {
        Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
        ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
        switch (_currentSelectedIndex)
        {
            case 0:
                Metadata.Instance.startingPrompt = "Edgar the elephant loves music. He plays";
                Metadata.Instance.currentPrompt = "Edgar the elephant loves music. He plays";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {
                    
                }
                break;
            case 1:
                Metadata.Instance.startingPrompt = "Robert is a Robot. He dreams of being";
                Metadata.Instance.currentPrompt = "Robert is a Robot. He dreams of being";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {

                }
                break;
            case 2:
                Metadata.Instance.startingPrompt = "Rachel ruysch, still life with flowers, oil painting";
                Metadata.Instance.currentPrompt = "Rachel ruysch, still life with flowers, oil painting";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {

                }
                break;
            case 3:
                Metadata.Instance.startingPrompt = "Wanda is a mighty witch. She always";
                Metadata.Instance.currentPrompt = "Wanda is a mighty witch. She always";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {

                }
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
        //EventSystem.instance.StartStoryEvent();
        EventSystem.instance.CubeBlinkEvent();
        SceneManager.LoadScene("Playthrough");
    }


    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
        EdgarTextBox.GetComponent<TextMeshProUGUI>().text = EdgarText.GetLocalizedString();
        RobotTextBox.GetComponent<TextMeshProUGUI>().text = RobotText.GetLocalizedString();
        StillTextBox.GetComponent<TextMeshProUGUI>().text = StillText.GetLocalizedString();
        WandaTextBox.GetComponent<TextMeshProUGUI>().text = WandaText.GetLocalizedString();

        InfoText.GetComponent<TextMeshProUGUI>().text = InfoBoxText.GetLocalizedString();
    }

    public void OnSelectImage(int i)
    {
        foreach (GameObject image in ImageResultBackground)
            image.SetActive(false);
        _currentSelectedIndex = i;
        ImageResultBackground[i].SetActive(true);
    }
}
