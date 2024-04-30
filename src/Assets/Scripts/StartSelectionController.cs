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


    [Header("Character Selection")]
    public GameObject CharacterTextBoxes;

    private GameObject[] CharacterTextBoxesArr = new GameObject[4];

    private string _currentSelectedCharacter;
    private int _selectedCharacterIndex;


    // Start is called before the first frame update
    void Start()
    {
        StartingScreen.SetActive(true);
        EventSystem.instance.ChangeLocale += OnChangeLocale;
        EventSystem.instance.StartStory += ShowDrawingSceneStart;
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
        /*for (int i = 0; i < 4; i++)
        {
            CharacterTextBoxesArr[i] = CharacterTextBoxes.transform.GetChild(i).gameObject;
        }*/
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
        _selectedCharacterIndex = 0;
        switch (_selectedCharacterIndex)
        {
            case 0:
                Metadata.Instance.startingPrompt =  "Wanda, the witch is a";
                Metadata.Instance.currentPrompt = "Wanda, the witch is a";
                Metadata.Instance.promptExplanation = "What does Wanda look like?";
                break;
            case 1:
                Metadata.Instance.startingPrompt =  "Ronny, the robot likes to";
                Metadata.Instance.currentPrompt = "Ronny, the robot likes to";
                Metadata.Instance.promptExplanation = "What does Ronny like to do?";
                break;
            case 2:
                Metadata.Instance.startingPrompt =  "Edgar, the elephant loves music. He plays";
                Metadata.Instance.currentPrompt = "Ronny, the robot likes to";
                Metadata.Instance.promptExplanation = "What instrument does Edgar play?";
                break;
            default:
                Metadata.Instance.startingPrompt =  "Ratty, the rat lives in a dustbin. He dreams of being";
                Metadata.Instance.currentPrompt = "Ronny, the robot likes to";
                Metadata.Instance.promptExplanation = "What are Rattys dreams?";

                break;
        }
        Metadata.Instance.currentSelectedCharacter = _currentSelectedCharacter;
        Metadata.Instance.currentTextPage = 1;
        /*if (Metadata.Instance.currentPrompt == "")
        {
            Debug.Log("Get Random Prompt");
            string random_prompt = Textboxes.GetComponent<Textboxes>().getRandomInitialPrompt();
            Metadata.Instance.currentPrompt = random_prompt;
            Metadata.Instance.startingPrompt = random_prompt;
        }*/
        StartCoroutine(Request.CreateStoryBook());
        EventSystem.instance.StartStoryEvent();
    }


    void OnChangeLocale()
    {
        StartingHeadline.GetComponent<TextMeshProUGUI>().text = StartingText.GetLocalizedString();
    }


    public void OnSelectCharacter(int i)
    {
        Debug.Log("Selecting Character");
        _currentSelectedCharacter = CharacterTextBoxesArr[i].GetComponent<TextMeshProUGUI>().text;
        _selectedCharacterIndex = i;
    }

}
