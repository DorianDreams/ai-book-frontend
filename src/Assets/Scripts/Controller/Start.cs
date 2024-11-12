using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Localization.Tables;

public class StartSelectionController : MonoBehaviour
{
    private int initialPrompts = 3;

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
    private GameObject[] ImageResultBackground = new GameObject[6];


    [Header("Localized Texts")]
    [SerializeField]
    private LocalizedString StartingText;

    [SerializeField]
    private LocalizedString InfoBoxText;

    public GameObject InfoText;

    private bool textboxopen = false;

    private int? _currentSelectedIndex = null;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < ImageResultBackground.Length; i++)
        {
            ImageResultBackground[i] = imageResultBackgrounds.transform.GetChild(i).gameObject;
        }
        StartingScreen.SetActive(true);
        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
        InfoText.GetComponent<TextMeshProUGUI>().text = InfoBoxText.GetLocalizedString();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];

        Debug.Log("Start got enabled");
    }

    private void OnDisable()
    {
        Debug.Log("Start got disabled");
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


        if (_currentSelectedIndex == null)
        {
            _currentSelectedIndex = Random.Range(0, 6);
        }

        switch (_currentSelectedIndex)
        {
            case 0:
                Metadata.Instance.selectedCharacter = "Datenkrake";
                break;
            case 1:
                Metadata.Instance.selectedCharacter = "Grandma";
                break;
            case 2:
                Metadata.Instance.selectedCharacter = "Pfiffikus";

                break;
            case 3:
                Metadata.Instance.selectedCharacter = "Edgar";
                
                break;
            case 4:
                Metadata.Instance.selectedCharacter = "Blauzahn";
                break;
            case 5:
                Metadata.Instance.selectedCharacter = "Wanda";
                break;
        }
        
        EventSystem.instance.StartStoryEvent();
    }

    string getInitialPrompts()
    {
      ArrayList initialPromptList;

     string[] initialPromptArray;
    initialPromptList = new ArrayList();
        StringTable table = LocalizationSettings.StringDatabase.GetTable("Translations");
        for (int i = 1; i < initialPrompts + 1; i++)
        {
            initialPromptList.Add(table.GetEntry("InitialPrompt" + i).LocalizedValue);

        }
        initialPromptArray = (string[])initialPromptList.ToArray(typeof(string));
        reshuffle(initialPromptArray);
        return initialPromptArray[0];
    }

    void reshuffle(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
        InfoText.GetComponent<TextMeshProUGUI>().text = InfoBoxText.GetLocalizedString();
    }

    public void OnSelectImage(int i)
    {
        foreach (GameObject image in ImageResultBackground)
        {
            image.SetActive(false);
        }
            
        _currentSelectedIndex = i;
        ImageResultBackground[i].SetActive(true);
    }
}
