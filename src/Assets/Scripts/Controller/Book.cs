namespace echo17.EndlessBook.Demo03
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using echo17.EndlessBook;
    using UnityEngine.Localization;
    using TMPro;
    using Image = UnityEngine.UI.Image;
    using System.Text;
    using UnityEngine.UI;
    using static echo17.EndlessBook.Demo03.BookController;
    using System.Security.Policy;
    using UnityEngine.Localization.Settings;
    using System;
    using UnityEngine.TextCore.Text;
    using TextAsset = UnityEngine.TextAsset;
    using System.Xml;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;

    public class BookController : MonoBehaviour
    {
        public enum BookState
        {
            Start,
            Chapter1,
            Chapter2,
            Chapter3,
            End
        }

        public BookState bookState = BookState.Start;

        private bool _isGenerating = false;

        private int regenerationCount = 0;

        public GameObject sceneCamera;				// The scene camera used for ray casting
        public EndlessBook book;					// The book to control

        public GameObject BookTitle;


        [Header("Temperature")]
        public float StartingTemperature = 1.0f;
        public float MaxTemperature = 2.5f;
        public float Temperatureincrease = 0.5f;
        private float _currentTemperature;

        private List<byte[]> imageBytes;

        [Header("Book Navigator")]
        [SerializeField]
        private LocalizedString FinishBookText;

        public GameObject FinishBookTextBox;

        public GameObject BookNavigator;
        public GameObject SentenceRegeneration;
        public Button previousPage;
        public Button nextPage;
        public GameObject proposalText;
        public Button regenerateText;
        public Button publishSentence;
        public GameObject finishBook;

        public GameObject AiTextBox;
        [SerializeField]
        private LocalizedString AiText;

        [SerializeField]
        private LocalizedString RegenerateText;
        public GameObject RegenerateTextBox;
        public float turnTime = 1f;
        public float stateAnimationTime = 1f;
        public GameObject Spinner;
        [SerializeField]
        private LocalizedString FirstPageText;
        [SerializeField]
        private LocalizedString ContinueText;

        [SerializeField]
        private LocalizedString FinishedTheBook;
        public GameObject FinishedTheBookTextBox;

        public Vector3 screenPosition;
        [Header("Book Pages")]
        public RenderTexture pageLeft;
        public RenderTexture pageRight;
        public GameObject arrow;
        public GameObject bookImage;
        public GameObject bookPrompt;
        public GameObject bookPrompt2;
        public GameObject bookText;
        public GameObject pageNumberRight;
        public GameObject pageNumberLeft;

        private List<Material> createdBookPages;
        private List<string> bookTextPages;

        //Typewriter in Book
        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 20;
        [SerializeField] private float interpunctuationDelay = 0.5f;
        [SerializeField][Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;
        public event Action CompleteTextRevealed;
        public event Action<char> CharacterRevealed;
        private TMP_Text _textBox;
        private int _currentVisibleCharacterIndex;
        private WaitForSeconds _simpleDelay;
        private WaitForSeconds _interpunctuationDelay;
        private WaitForSeconds _textboxFullEventDelay;

        public enum PageFocus
        {
            leftPage,
            rightPage,
            finishChapter
        }
        public PageFocus current = PageFocus.leftPage;

        private Dictionary<string, CharacterStory> storyDictEn = new Dictionary<string, CharacterStory>();
        private Dictionary<string, CharacterStory> storyDictGer = new Dictionary<string, CharacterStory>();


        void OnEnable()
        {
            if (Metadata.Instance.testingMode)
            {
                charactersPerSecond = 300;
                interpunctuationDelay = 0f;

            }

            LoadJson();
            CompleteTextRevealed += () => OnCompleteTextRevealed();
            StartStory = (EndlessBook.StateEnum fromState,EndlessBook.StateEnum toState, int pageNumber)
             =>
                    {
                        SelectConsistencyPrompt(Metadata.Instance.selectedCharacter);
                        WritePromptInBook();

                        PrepareForNewText(bookPrompt);
                    };
            book.SetState(EndlessBook.StateEnum.OpenMiddle, onCompleted: StartStory);
            Debug.Log("Enable Book");

            //Initialize Typewriter
            _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
            _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
            
        }

        void WritePromptInBook()
        {
            List<string> selectedPrompt = SelectPrompt(storyDictEn, Metadata.Instance.selectedCharacter, Metadata.Instance.currentChapter);
            Metadata.Instance.currentPrompt = selectedPrompt[0];

            Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
            ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
            if (currentSelectedLocale == availableLocales.GetLocale("de"))
            {
                //selectedPrompt = SelectPrompt(storyDictGer, Metadata.Instance.selectedCharacter, Metadata.Instance.currentChapter);
            	Metadata.Instance.currentPrompt = selectedPrompt[0];
                List<string> selectedPromptGer = SelectPrompt(storyDictGer, Metadata.Instance.selectedCharacter, Metadata.Instance.currentChapter);
                Metadata.Instance.currentPrompt = selectedPromptGer[0];
                bookPrompt.GetComponent<TextMeshProUGUI>().text = selectedPromptGer[0] + "..." + "\n\n" + selectedPromptGer[1];


            }
            else
            {
                bookPrompt.GetComponent<TextMeshProUGUI>().text = selectedPrompt[0] + "..." + "\n\n" + selectedPrompt[1];
            }
        }

        List<string> SelectPrompt(Dictionary<string, CharacterStory> storyDict, string character, string chapter)
        {
                CharacterStory story = null;
            List<string> defaultList = null;
            

            if (storyDict.TryGetValue(character, out story))
            {
                switch (chapter)
                {
                    case "ch1":
                        return story.ch1;                    
                        

                    case "ch2":
                        return story.ch2;
                        

                    case "ch3":
                        return story.ch3;
                    
                    default:
                        
                        return defaultList;
                }

            } else { return defaultList; }
        }

        void SelectConsistencyPrompt(string character)
        {
            CharacterStory story = null;
            if (storyDictEn.TryGetValue(character, out story))
            {
                Metadata.Instance.consistencyPrompt = story.consistency;
            }
            else
            {
                Metadata.Instance.consistencyPrompt = "children book, cartoon";
            }
        }

        void LoadJson()
        {
            // Load the JSON file from the Resources folder
            TextAsset jsonFileEn = Resources.Load<TextAsset>("characters_en");
            TextAsset jsonFileGer = Resources.Load<TextAsset>("characters_ger");

            storyDictEn = JsonConvert.DeserializeObject<Dictionary<string, CharacterStory>>(jsonFileEn.text);
            storyDictGer = JsonConvert.DeserializeObject<Dictionary<string, CharacterStory>>(jsonFileGer.text);

        }

        void OnCompleteTextRevealed()
        {
            Debug.Log("Complete Text revealed");
            if (current == PageFocus.leftPage)
            {
                bookPrompt2.SetActive(true);
                bookPrompt2.GetComponent<TextMeshProUGUI>().text = ContinueText.GetLocalizedString();
                PrepareForNewText(bookPrompt2);
                current = PageFocus.rightPage;
            }
            else if (current == PageFocus.rightPage) 
            {
                arrow.SetActive(true);
                EventSystem.instance.EnableDrawingScreenEvent();
                current = PageFocus.finishChapter;
                EventSystem.instance.EnableRestartButtonEvent();
            } else
            {
                GoToNextChapter();
                current = PageFocus.leftPage;
            }
        }

        private void Start()
        {
            imageBytes = new List<byte[]>();
            createdBookPages = new List<Material>();
            bookTextPages = new List<string>();

            RegenerateTextBox.GetComponent<TextMeshProUGUI>().text = RegenerateText.GetLocalizedString();
            FinishBookTextBox.GetComponent<TextMeshProUGUI>().text = FinishBookText.GetLocalizedString();
            AiTextBox.GetComponent<TextMeshProUGUI>().text = AiText.GetLocalizedString();
            FinishedTheBookTextBox.GetComponent<TextMeshProUGUI>().text = FinishedTheBook.GetLocalizedString();


            previousPage.interactable = false;
            _currentTemperature = StartingTemperature;
            EventSystem.instance.StartStory += OnStartStory;
            EventSystem.instance.ChangeLocale += OnChangeLocale;
            EventSystem.instance.SelectImage += OnSelectImage;
            EventSystem.instance.PublishNextPrompt += OnPublishNextPrompt;
            EventSystem.instance.EnableBookNavigator += OnEnableBookNavigator;
            EventSystem.instance.DisableBookNavigator += OnDisableBookNavigator;
            EventSystem.instance.GoToNextPage += GoToNextChapter;
            EventSystem.instance.GoPreviousPage += GoToPreviousPage;
            finishBook.SetActive(false);
        }

        private void PrepareForNewText(GameObject obj)
        {
            _textBox = obj.GetComponent<TMP_Text>();
            _textBox.ForceMeshUpdate();
            _textBox.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;
            TMP_TextInfo textInfo = _textBox.textInfo;
            StartCoroutine(Typewriter(_textBox));
        }

        private IEnumerator Typewriter(TMP_Text _textBox)
        {
            TMP_TextInfo textInfo = _textBox.textInfo;

            while (_currentVisibleCharacterIndex < textInfo.characterCount + 1)
            {
                var lastCharacterIndex = textInfo.characterCount - 1;

                if (_currentVisibleCharacterIndex >= lastCharacterIndex)
                {
                    _textBox.maxVisibleCharacters++;
                    yield return _textboxFullEventDelay;
                    CompleteTextRevealed?.Invoke();
                    yield break;
                }

                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

                _textBox.maxVisibleCharacters++;

                if (
                    (character == '?' || character == '.' || character == ',' || character == ':' ||
                     character == ';' || character == '!' || character == '-'))
                {
                    yield return _interpunctuationDelay;
                }
                else
                {
                    yield return _simpleDelay;
                }

                CharacterRevealed?.Invoke(character);
                _currentVisibleCharacterIndex++;
            }
        }


        void OnStartStory()
        {
            sceneCamera.SetActive(true);
            book.SetState(EndlessBook.StateEnum.OpenMiddle, onCompleted: StartStory);
        }
        private StateChangedDelegate StartStory;

        public void OnPublishTextToBook()
        {
            EventSystem.instance.DisableRestartButtonEvent();

            Debug.Log("Publish Text");
            bookPrompt.SetActive(false);
            bookTextPages.Add(proposalText.GetComponent<TextMeshProUGUI>().text);

            bookText.GetComponent<TextMeshProUGUI>().text = proposalText.GetComponent<TextMeshProUGUI>().text;
            PrepareForNewText(bookText);

            regenerateText.interactable = false;
            publishSentence.interactable = false;
            proposalText.GetComponent<TextMeshProUGUI>().text = "";

        }

        private void SavePage(RenderTexture render)
        {
            Material material = new Material(Shader.Find("Mobile/Diffuse"));
            Texture2D tex = new Texture2D(render.width, render.height, TextureFormat.ARGB32, false);
            UnityEngine.Graphics.CopyTexture(render, tex);
            material.mainTexture = tex;
            book.InsertPageData(book.CurrentPageNumber, material);
            createdBookPages.Add(material);
        }

        private void ClearPages()
        {
            arrow.SetActive(false);
            bookImage.GetComponent<Image>().sprite = null;
            bookImage.SetActive(false);
            bookPrompt.GetComponent<TextMeshProUGUI>().text = "";
            bookPrompt2.GetComponent<TextMeshProUGUI>().text = "";
            bookText.GetComponent<TextMeshProUGUI>().text = "";

        }
    public void GoToNextChapter()
        {

            SentenceRegeneration.SetActive(false);
            EventSystem.instance.CubeWaveRightEvent();
            if (book.CurrentPageNumber == 5) 
            {
                //BookNavigator.SetActive(true); Todo
                OnFinishBook();
                EventSystem.instance.EnableOwnershipScreenEvent();

            }
            else
            {
                SavePage(pageRight);
                SavePage(pageLeft);
                ClearPages();
                pageNumberRight.GetComponent<TextMeshProUGUI>().text = (book.CurrentPageNumber +3).ToString();
                pageNumberLeft.GetComponent<TextMeshProUGUI>().text = (book.CurrentPageNumber + 2).ToString();
                book.TurnForward(turnTime,
                            onCompleted: OnBookTurnToPageCompleted);
            }
        }

        IEnumerator SentenceCompletions(byte[] genImage, string prompt)
        {
                CoroutineWithData cd_unload = new CoroutineWithData(this, Request.UnLoadSDXL());
                yield return cd_unload.coroutine;

            EventSystem.instance.DisableRestartButtonEvent();

            SentenceRegeneration.SetActive(true);
            //todo: check locale
            string completion = "";
            Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
            ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;

            if (currentSelectedLocale == availableLocales.GetLocale("de"))
            {
                CoroutineWithData cd_completion = new CoroutineWithData(this, Request.GetSentenceCompletion(genImage, prompt, _currentTemperature, "ger"));
                yield return cd_completion.coroutine;
                completion = (string)cd_completion.result;
            }
            else {
                CoroutineWithData cd_completion = new CoroutineWithData(this, Request.GetSentenceCompletion(genImage, prompt, _currentTemperature, "en"));
                yield return cd_completion.coroutine;
                completion = (string)cd_completion.result;
            }


            proposalText.GetComponent<TextMeshProUGUI>().text = completion;
            _isGenerating = false;
            Spinner.SetActive(false);
            regenerateText.interactable = true;
            publishSentence.interactable = true;
            previousPage.interactable = true;
            nextPage.interactable = true;
            EventSystem.instance.EnableRestartButtonEvent();

        }


        public void OnRegenerateSentence()
        {
            if (!_isGenerating)
            {
                Spinner.SetActive(true);
                regenerateText.interactable = false;
                publishSentence.interactable = false;
                _isGenerating = true;
                switch (book.CurrentPageNumber)
                {
                    case 1:
                        StartCoroutine(SentenceCompletions(imageBytes[0], Metadata.Instance.currentPrompt));
                        break;
                    case 3:
                        StartCoroutine(SentenceCompletions(imageBytes[1], Metadata.Instance.currentPrompt));
                        break;
                    case 5:
                        StartCoroutine(SentenceCompletions(imageBytes[2], Metadata.Instance.currentPrompt));
                        break;
                }
                        
                regenerationCount++;

            }
            if(_currentTemperature < MaxTemperature)
            {
                _currentTemperature += Temperatureincrease;
            }
        }

       public IEnumerator CreateTitle(string alltext)
        {
                            CoroutineWithData cd_unload = new CoroutineWithData(this, Request.UnLoadSDXL());
                yield return cd_unload.coroutine;

                        Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
            ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
            string title = "";

            
            if (currentSelectedLocale == availableLocales.GetLocale("de"))
            {
            CoroutineWithData cd_title = new CoroutineWithData(this, Request.CreateTitle(alltext,"ger"));
            yield return cd_title.coroutine;
                title = (string)cd_title.result;
            }
            else {
            CoroutineWithData cd_title = new CoroutineWithData(this, Request.CreateTitle(alltext, "en"));
            yield return cd_title.coroutine;
                title = (string)cd_title.result;
            }


            BookTitle.GetComponent<TextMeshProUGUI>().text = title;
            BookTitle.SetActive(true);
            Metadata.Instance.storyBook.title = title;
        }

        public void OnNextPage()
        {
            if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
            {
                book.SetState(EndlessBook.StateEnum.OpenMiddle);
                book.SetPageNumber(1);
            } else
            {

                book.TurnForward(turnTime);
                EventSystem.instance.CubeWaveRightEvent();

            }
        }

        //Todo: implement title generation
        public void OnFinishBook()
        {
            Metadata.Instance.storyBook.drawing.drawingPages["ch3"].regenerateText = regenerationCount;
            regenerationCount = 0;
            StartCoroutine(Request.PostImageDescription(bookText.GetComponent<TextMeshProUGUI>().text, Metadata.Instance.currentImgID));
            
            book.SetState(EndlessBook.StateEnum.ClosedFront);
            EventSystem.instance.DisableBookNavigatorEvent();
            EventSystem.instance.EnableOwnershipScreenEvent();
            string alltext = string.Concat(bookTextPages.ToArray());
            var sb = new StringBuilder(alltext.Length);

            foreach (char i in alltext)
                if (i != '\n' && i != '\r' && i != '\t' && i != '"')
                    sb.Append(i);

            alltext = sb.ToString();


            //StartCoroutine(CreateCover(Metadata.Instance.startingPrompt));
            StartCoroutine(CreateTitle(alltext));
        }

        public void OnFinishPlaythrough()
        {
            EventSystem.instance.FinishPlaythroughEvent();
        }

        IEnumerator CreateCover(string story)
        {
            CoroutineWithData load = new CoroutineWithData(this, Request.LoadSDXL());
            yield return load.coroutine;

            CoroutineWithData cd_cover = new CoroutineWithData(this, Request.GetImageGeneration(story, 1f,imageBytes[0]));
            yield return cd_cover.coroutine;
            Dictionary<string, string> returnVal = (Dictionary<string, string>)cd_cover.result;
            string imagePath = returnVal["image"].ToString();
            string fullpath = "../../storybookcreator" + imagePath;

            byte[] cover = System.IO.File.ReadAllBytes(fullpath);

            EventSystem.instance.ChooseCoverImageEvent(cover);
            CoroutineWithData unload = new CoroutineWithData(this, Request.UnLoadSDXL());
            yield return unload.coroutine;

        }

        public void GoToPreviousPage()
        {
            if(book.CurrentPageNumber == 1)
            {
                book.SetState(EndlessBook.StateEnum.ClosedFront);

            }
            else
            {
                book.TurnBackward(turnTime);

            }
            EventSystem.instance.CubeWaveLeftEvent();

        }
        void OnEnableBookNavigator()
        {
            BookNavigator.SetActive(true);
        }

        void OnDisableBookNavigator()
        {
            BookNavigator.SetActive(false);
        }

        void OnChangeLocale()
        {
            bookPrompt.GetComponent<TextMeshProUGUI>().text = FirstPageText.GetLocalizedString();
        }

        void OnSelectImage(Sprite sprite, int index, byte[] imagebytes)
        {
            _isGenerating = true;
            Spinner.SetActive(true);
            regenerateText.interactable = false;
            publishSentence.interactable = false;
            StartCoroutine(SentenceCompletions(imagebytes, Metadata.Instance.currentPrompt));

            if (book.CurrentPageNumber == 5)
            {
                finishBook.SetActive(true);
            }

            imageBytes.Add(imagebytes);
            bookPrompt2.SetActive(false);
            bookImage.GetComponent<Image>().sprite = sprite;
            bookImage.SetActive(true);
            arrow.SetActive(false);
        }

        void OnPublishNextPrompt()
        {
            StartCoroutine(Request.LoadSDXL());            
            if (book.CurrentPageNumber == 3)
            {
                Metadata.Instance.currentChapter = "ch2";


            } else if (book.CurrentPageNumber == 5)
            {
                Metadata.Instance.currentChapter = "ch3";
            }
            WritePromptInBook();
            bookPrompt.SetActive(true);
            PrepareForNewText(bookPrompt);
            
        }
        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, 
            EndlessBook.StateEnum toState, int currentPageNumber)
        {
            Debug.Log("OnBookTurnToPageCompleted: State set to " + toState + ". Current Page Number = " + currentPageNumber);

            Metadata.Instance.storyBook.drawing.drawingPages[Metadata.Instance.currentChapter].regenerateText = regenerationCount;
            _isGenerating = true;
            EventSystem.instance.PublishNextPromptEvent();
            _currentTemperature = StartingTemperature;
            EventSystem.instance.DisableBookNavigatorEvent();

            regenerationCount = 0;
        }

    }

}
