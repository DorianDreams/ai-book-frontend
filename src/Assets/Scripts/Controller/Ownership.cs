using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OwnershipSelectionController : MonoBehaviour
{

    public GameObject OwnershipSelectionScreen;
    public GameObject SelectAIButton;
    public GameObject SelectMeButton;
    public GameObject SelectMEAIButton;
    public GameObject PublishButton;
    public GameObject Headline;
    public GameObject NoButton;
    public GameObject YesButton;

    private ArrayList instantiatedTextBoxes = new ArrayList();
    private ArrayList instantiatedSigningButtons = new ArrayList();

    public GameObject LineGeneratorPrefab;
    public GameObject DrawingBackground;

    [SerializeField]
    float LineWidth = 0.2f;

    public Canvas DrawingCanvas;


    [SerializeField]
    private LocalizedString SignText;
    [SerializeField]
    private LocalizedString SignText2;
    [SerializeField]
    private LocalizedString SignText3;
    [SerializeField]
    private LocalizedString AIText;
    [SerializeField]
    private LocalizedString ME;
    [SerializeField]
    private LocalizedString Yes;
    [SerializeField]
    private LocalizedString No;
    [SerializeField]
    private LocalizedString MEAI;


    public GameObject SignTextBox;
    public GameObject AITextBox;
    public GameObject METext;
    public GameObject MEAIText;
    public GameObject YesText;
    public GameObject NoText;

    private enum CurrentState
    {
        ChoosingOwner,
        SigningDecision,
        Signing
    }

    private CurrentState currentState = CurrentState.ChoosingOwner;
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.instance.CleanLineGeneratorEvent();
        instantiatedTextBoxes.Add(SelectAIButton);
        SelectAIButton.GetComponentInChildren<Button>().onClick.AddListener(() => onButtonPressed(SelectAIButton));
        instantiatedTextBoxes.Add(SelectMeButton);
        SelectMeButton.GetComponentInChildren<Button>().onClick.AddListener(() => onButtonPressed(SelectMeButton));
        instantiatedTextBoxes.Add(SelectMEAIButton);
        SelectMEAIButton.GetComponentInChildren<Button>().onClick.AddListener(() => onButtonPressed(SelectMEAIButton));

        instantiatedSigningButtons.Add(YesButton);
        YesButton.GetComponentInChildren<Button>().onClick.AddListener(() => onButtonPressedSigning(YesButton));
        instantiatedSigningButtons.Add(NoButton);
        NoButton.GetComponentInChildren<Button>().onClick.AddListener(() => onButtonPressedSigning(NoButton));

        PublishButton.GetComponent<Button>().onClick.AddListener(onPublish);
        SignTextBox.GetComponent<TextMeshProUGUI>().text = SignText.GetLocalizedString();
        AITextBox.GetComponent<TextMeshProUGUI>().text = AIText.GetLocalizedString();
        METext.GetComponent<TextMeshProUGUI>().text = ME.GetLocalizedString();
        MEAIText.GetComponent<TextMeshProUGUI>().text = MEAI.GetLocalizedString();
        YesText.GetComponent<TextMeshProUGUI>().text = Yes.GetLocalizedString();
        NoText.GetComponent<TextMeshProUGUI>().text = No.GetLocalizedString();
    }

    void onButtonPressed(GameObject textBox)
    {
        foreach (GameObject tb in instantiatedTextBoxes)
        {
            tb.transform.GetChild(1).gameObject.SetActive(false);
        }
        textBox.transform.GetChild(1).gameObject.SetActive(true);
        //string startingSentence = textBox.GetComponentInChildren<TextMeshProUGUI>().text;
        Metadata.Instance.storyBook.decision_of_authorship = textBox.name;
        EventSystem.instance.ChooseCoverAuthorEvent(textBox.name);
    }

    void onButtonPressedSigning(GameObject textBox)
    {
        foreach (GameObject tb in instantiatedSigningButtons)
        {
            tb.transform.GetChild(1).gameObject.SetActive(false);
        }
        textBox.transform.GetChild(1).gameObject.SetActive(true);
        //string startingSentence = textBox.GetComponentInChildren<TextMeshProUGUI>().text;
        if (textBox.name == "Yes")
        {
            Metadata.Instance.storyBook.signed_the_book = true;
        } else
        {
            Metadata.Instance.storyBook.signed_the_book = false;
            
        }
    }

    void onPublish()
    {
        switch (currentState)
        {
            case CurrentState.ChoosingOwner:
                string authorship = Metadata.Instance.storyBook.decision_of_authorship;
                if (authorship == "AI")
                {
                    foreach (GameObject tb in instantiatedTextBoxes)
                    {
                        tb.SetActive(false);
                    }
                    currentState = CurrentState.SigningDecision;
                    Headline.GetComponent<TextMeshProUGUI>().text = SignText2.GetLocalizedString();
                    foreach (GameObject tb in instantiatedSigningButtons)
                    {
                        tb.SetActive(true);
                    }
                }
                else
                {
                    EventSystem.instance.FinishPlaythroughEvent();
                }
                break;
            case CurrentState.SigningDecision:
                bool signed = Metadata.Instance.storyBook.signed_the_book;
                if (signed)
                {
                    foreach (GameObject tb in instantiatedSigningButtons)
                    {
                        tb.SetActive(false);
                    }
                    currentState = CurrentState.Signing;
                    Headline.GetComponent<TextMeshProUGUI>().text = SignText3.GetLocalizedString();
                    DrawingBackground.SetActive(true);
                    GameObject InstantiatedLineGenerator = Instantiate(LineGeneratorPrefab);
                    InstantiatedLineGenerator.GetComponent<LineGenerator>().parentCanvas = DrawingCanvas;
                    InstantiatedLineGenerator.GetComponent<LineGenerator>().width = LineWidth;
                    EventSystem.instance.PressColorButtonEvent(Color.black);

                }
                else
                {
                    EventSystem.instance.FinishPlaythroughEvent();
                }
                break;
            case CurrentState.Signing:
                EventSystem.instance.FinishPlaythroughEvent();
                break;
        }
    }


}
