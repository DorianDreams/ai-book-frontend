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
    private LocalizedString GrandmaText;

    [SerializeField]
    private LocalizedString InfoBoxText;

    public GameObject GrandmaTextBox;
    public GameObject InfoText;

    private bool textboxopen = false;

    private int _currentSelectedIndex;

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
        switch (_currentSelectedIndex)
        {
            case 0:
                Metadata.Instance.selectedCharacter = "Datenkrake";
                Metadata.Instance.currentPrompt = "Edgar the elephant loves music. He plays";

                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {
                    Metadata.Instance.startingPrompt = "Edgar, der Elefant, liebt Musik. Er spielt";
                    Metadata.Instance.currentPrompt = "Edgar, der Elefant, liebt Musik. Er spielt";
                }
                Metadata.Instance.consistencyPrompt = "Edgar, the elephant, joyful";
                break;
            case 1:
                Metadata.Instance.selectedCharacter = "Grandma";
                Metadata.Instance.startingPrompt = "Robert is a Robot. He dreams of being";
                Metadata.Instance.currentPrompt = "Robert is a Robot. He dreams of being";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {
                    Metadata.Instance.startingPrompt = "Robert ist ein Roboter. Er träumt davon";
                    Metadata.Instance.currentPrompt = "Robert ist ein Roboter. Er träumt davon";
                }
                Metadata.Instance.consistencyPrompt = "A Robot named Robert";
                break;
            case 2:
                Metadata.Instance.selectedCharacter = "Pfiffikus";

                Metadata.Instance.startingPrompt = "A still life, oil painting, consisting of many";
                Metadata.Instance.currentPrompt = "A still life, oil painting, consisting of many";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {
                    Metadata.Instance.startingPrompt = "Ein Stillleben, Ölgemälde, bestehend aus vielen";
                    Metadata.Instance.currentPrompt = "Ein Stillleben, Ölgemälde, bestehend aus vielen";
                }
                Metadata.Instance.consistencyPrompt = "Still life, oil painting";
                break;
            case 3:
                Metadata.Instance.selectedCharacter = "Edgar";
                Metadata.Instance.startingPrompt = "Wanda is a mighty old witch. She always wears";
                Metadata.Instance.currentPrompt = "Wanda is a mighty old witch. She always wears";
                if (currentSelectedLocale == availableLocales.GetLocale("de"))
                {
                    Metadata.Instance.startingPrompt = "Wanda ist eine mächtige alte Hexe. Sie trägt immer";
                    Metadata.Instance.currentPrompt = "Wanda ist eine mächtige alte Hexe. Sie trägt immer";
                }
                Metadata.Instance.consistencyPrompt = "Wanda, old witch";
                break;
            case 4:
                Metadata.Instance.selectedCharacter = "Blauzahn";
                break;
            case 5:
                Metadata.Instance.selectedCharacter = "Wanda";
                break;
        }
        if (Metadata.Instance.selectedCharacter == "")
        {
            Debug.Log("Get Random Prompt");
            string random_prompt = getInitialPrompts();
            Metadata.Instance.currentPrompt = random_prompt;
            Metadata.Instance.startingPrompt = random_prompt;
        }
        StartCoroutine(Request.CreateStoryBook());
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
        GrandmaTextBox.GetComponent<TextMeshProUGUI>().text = GrandmaText.GetLocalizedString();
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
