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
        public GameObject BookNavigator;
        public GameObject SentenceRegeneration;
        public Button previousPage;
        public Button nextPage;
        public GameObject proposalText;
        public Button regenerateText;
        public Button publishSentence;
        public GameObject finishBook;


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

        void OnEnable()
        {
            CompleteTextRevealed += () => OnCompleteTextRevealed();
            StartStory = (EndlessBook.StateEnum fromState,EndlessBook.StateEnum toState, int pageNumber)
             =>
                    {
                        bookPrompt.GetComponent<TextMeshProUGUI>().text = Metadata.Instance.currentPrompt + "...";
                        bookState = BookState.Chapter1;

                        PrepareForNewText(bookPrompt);
                    };
            book.SetState(EndlessBook.StateEnum.OpenMiddle, onCompleted: StartStory);
            Debug.Log("Enable Book");

            //Initialize Typewriter
            _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
            _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
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
            Debug.Log("Publish Text");
            bookPrompt.SetActive(false);
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
            bookTextPages.Add(bookText.GetComponent<TextMeshProUGUI>().text);
            bookText.GetComponent<TextMeshProUGUI>().text = "";

        }
    public void GoToNextChapter()
        {

            SentenceRegeneration.SetActive(false);
            EventSystem.instance.CubeWaveRightEvent();
            if (book.CurrentPageNumber == 5) 
            {
                BookNavigator.SetActive(true);
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
            SentenceRegeneration.SetActive(true);
            CoroutineWithData cd_completion = new CoroutineWithData(this, Request.GetSentenceCompletion(genImage, prompt, _currentTemperature));
            yield return cd_completion.coroutine;
            string completion = (string)cd_completion.result;
            Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
            ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
            if (currentSelectedLocale == availableLocales.GetLocale("de"))
            {
                CoroutineWithData translation = new CoroutineWithData(this, Request.TranslateSentence(completion));
                yield return translation.coroutine;
                completion = (string)translation.result;
            }
            proposalText.GetComponent<TextMeshProUGUI>().text = completion;
            _isGenerating = false;
            Spinner.SetActive(false);
            regenerateText.interactable = true;
            publishSentence.interactable = true;
            previousPage.interactable = true;
            nextPage.interactable = true;
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

        public IEnumerator CreateNextPrompt(string completion, string imgID)
        {
            CoroutineWithData cd_nextPrompt = new CoroutineWithData(this, Request.GetNextprompt(completion));
            yield return cd_nextPrompt.coroutine;
            string nextPrompt = (string)cd_nextPrompt.result;
            
            StartCoroutine(Request.PostImageDescription(completion, Metadata.Instance.currentImgID)); 
            _isGenerating = false;
            /*
            if (Metadata.Instance.currentPrompt == "Edgar the elephant loves music. He plays")
            {
                nextPrompt = "One day Edgar the elephant plays with a group of joyful musicians in front of";
            }
            else if (Metadata.Instance.currentPrompt == "One day Edgar the elephant plays with a group of joyful musicians in front of")
            {
                nextPrompt = "Edgar the elephant has great success with his music. One day he wins";
            }
            else if (Metadata.Instance.currentPrompt == "Ratty lives in a dustbin. He dreams of being")
            {
                nextPrompt = "Ratty the rat is rather poor. But one day he finds";
            }
            else if (Metadata.Instance.currentPrompt == "Ratty the rat is rather poor. But one day he finds")
            {
                nextPrompt = "Ratty the rat lived happily ever after, in his new home inside a";
            }
            else if (Metadata.Instance.currentPrompt == "I love to stay at grandma's house. She always")
            {
                nextPrompt = "Today Grandma cooks my all time favourite meal. She cooks";
            }
            else if (Metadata.Instance.currentPrompt == "Today Grandma cooks my all time favourite meal. She cooks")
            {
                nextPrompt = "I am very happy about my granny. May she always";
            }
            else if (Metadata.Instance.currentPrompt == "Wanda is a witch. She always")
            {
                nextPrompt = "One day Wanda the witch encounters one of her greatest enemies. A fearsome";
            }
            else if (Metadata.Instance.currentPrompt == "One day Wanda the witch encounters one of her greatest enemies. A fearsome")
            {
                nextPrompt = "Wanda the witch triumphs over her foe. To celebrate her victory she";
            }
            */
            Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
            ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
            
            if (currentSelectedLocale == availableLocales.GetLocale("de"))
            {
                CoroutineWithData translation = new CoroutineWithData(this, Request.TranslateSentence(nextPrompt));
                yield return translation.coroutine;
                nextPrompt = (string)translation.result;
            }
            EventSystem.instance.PublishNextPromptEvent(nextPrompt);
        }

       public IEnumerator CreateTitle(string alltext)
        {
            CoroutineWithData cd_title = new CoroutineWithData(this, Request.CreateTitle(alltext));
            yield return cd_title.coroutine;
            string title = (string)cd_title.result;
            BookTitle.GetComponent<TextMeshProUGUI>().text = title;
            BookTitle.SetActive(true);
            Metadata.Instance.storyBook.title = title;
        }

        public void OnNextPage()
        {
            book.TurnForward(turnTime);
            EventSystem.instance.CubeWaveRightEvent();
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
            EventSystem.instance.ChooseCoverImageEvent(imageBytes[0]);

            string alltext = string.Concat(bookTextPages.ToArray());


            var sb = new StringBuilder(alltext.Length);

            foreach (char i in alltext)
                if (i != '\n' && i != '\r' && i != '\t' && i != '"')
                    sb.Append(i);

            alltext = sb.ToString();
            StartCoroutine(CreateTitle(alltext));
        }

        public void GoToPreviousPage()
        {
                book.TurnBackward(turnTime);
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

        void OnPublishNextPrompt(string prompt)
        {
            if (book.CurrentPageNumber == 3)
            {
                Metadata.Instance.currentChapter = "ch2";
            } else if (book.CurrentPageNumber == 5)
            {
                Metadata.Instance.currentChapter = "ch3";
            }
            bookPrompt.GetComponent<TextMeshProUGUI>().text = prompt + "...";
            //Metadata.Instance.previousPrompt = Metadata.Instance.startingPrompt;
            Metadata.Instance.currentPrompt = prompt;
            bookPrompt.SetActive(true);
            PrepareForNewText(bookPrompt);
            
        }
        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, 
            EndlessBook.StateEnum toState, int currentPageNumber)
        {
            Debug.Log("OnBookTurnToPageCompleted: State set to " + toState + ". Current Page Number = " + currentPageNumber);

            _isGenerating = true;
            StartCoroutine(CreateNextPrompt("", Metadata.Instance.currentImgID));
            _currentTemperature = StartingTemperature;
            EventSystem.instance.DisableBookNavigatorEvent();
            //Metadata.Instance.storyBook.drawing.drawingPages["ch1"].regenerateText = regenerationCount; // Todo
            regenerationCount = 0;
        }

    }

}
