namespace echo17.EndlessBook.Demo03
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using echo17.EndlessBook;
    using UnityEngine.Localization;
    using TMPro;
    using Image = UnityEngine.UI.Image;
    using UnityEngine.SceneManagement;
    using UnityEngine.Networking;
    using Newtonsoft.Json;
    using System.Text;
    using UnityEngine.UI;
    using static echo17.EndlessBook.Demo03.BookController;
    using System.Security.Policy;
    using UnityEngine.Localization.Settings;


    /// <summary>
    /// This demo shows one way you could implement manual page dragging in your book
    /// </summary>
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

        // Book and Page Control
        public Camera sceneCamera;					// The scene camera used for ray casting
        public EndlessBook book;					// The book to control
        public float turnStopSpeed;					// The speed to play the page turn animation when the mouse is let go
        public bool reversePageIfNotMidway = true;  // reverse direction if not past midway point of book
        protected BoxCollider boxCollider;			// The box collider to check for mouse motions
        protected bool isMouseDown;					// Whether the mouse is currently down
        protected bool turnBookPage = true;			// Keep track whether or not the book can be turned

        // Audio Sources
        protected bool audioOn = false;				// =false so that we don't get an open sound at the beginning
        public AudioSource bookOpenSound;			// The sound to make when the book opens
        public AudioSource bookCloseSound;			// The sound to make when the book closes
        public AudioSource pageTurnSound;			// The sounds for each of the page components' turn
        public AudioSource pagesFlippingSound;		// The sound to make when multiple pages are turning


        public GameObject BookTitle;


        [Header("Temperature")]
        public float StartingTemperature = 1.0f;
        public float MaxTemperature = 2.5f;
        public float Temperatureincrease = 0.5f;
        private float _currentTemperature;


        [Header("Page Objects")]
        public GameObject RenderPages;				
        public GameObject textP0;
        public GameObject DownArrow;            // Should be a prefab for each page
        public GameObject textP1;
        public GameObject textP1Final;
        public GameObject textP2;
        public LocalizedString drawPictureText;
        public GameObject DownArrow2;
        public GameObject imageP2;
        private byte[] imageP2bytes;
        public GameObject textP3;
        public GameObject textP3Final;

        public GameObject imageP4;
        public GameObject DownArrow4;

        private byte[] imageP4bytes;
        public GameObject textP4;
        public GameObject textP5;
        public GameObject TextP5Final;
        public GameObject imageP6;
        public GameObject DownArrow6;

        private byte[] imageP6bytes;
        public GameObject textP6;


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


        private bool bookFinished = false;
        private bool nextPageFive = false;

        private int currentTextGenPage = 1;

        private int nextBookPage = 0;

        public float turnTime = 1f;
        public float stateAnimationTime = 1f;

        public GameObject Spinner;



        private void Start()
        {
            
            RegenerateTextBox.GetComponent<TextMeshProUGUI>().text = RegenerateText.GetLocalizedString();
            previousPage.interactable = false;
            _currentTemperature = StartingTemperature;
            EventSystem.instance.RestartScene += Reset;

            Debug.Log("Start Book Controller");
            textP0.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(0);
            textP1.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(1);
            textP2.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(2);
            textP3.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(3);
            textP4.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(4);
            textP5.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(5);
            textP6.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(6);

            textP1Final.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(10);
            textP3Final.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(30);
            TextP5Final.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(50);


            EventSystem.instance.StartStory += OnStartStory;
            EventSystem.instance.ChangeLocale += OnChangeLocale;
            //EventSystem.instance.PublishToBook += OnPublishToBook;
            EventSystem.instance.SelectImage += OnSelectImage;

            EventSystem.instance.PublishNextPrompt += OnPublishNextPrompt;

            EventSystem.instance.EnableBookNavigator += OnEnableBookNavigator;
            EventSystem.instance.DisableBookNavigator += OnDisableBookNavigator;

            EventSystem.instance.GoToNextPage += GoToNextChapter;
            EventSystem.instance.GoPreviousPage += GoToPreviousPage;
            StartStory = (EndlessBook.StateEnum fromState,
                                                EndlessBook.StateEnum toState,
                                                int pageNumber) =>
            {


                textP1.GetComponent<TextMeshProUGUI>().text = Metadata.Instance.currentPrompt + "...";
                bookState = BookState.Chapter1;
            };



            OnBookOpened = (EndlessBook.StateEnum fromState,
                             EndlessBook.StateEnum toState,
                                            int pageNumber) =>
            {
                //turnBookPage = false;

                textP0.GetComponent<TextMeshProUGUI>().text = FirstPageText.GetLocalizedString();
            };

            OnBookClosed = (EndlessBook.StateEnum fromState,
                             EndlessBook.StateEnum toState,
                                            int pageNumber) =>
            {
                StartCoroutine(RestartInThree());
            };

            finishBook.SetActive(false);
            OnStartStory();
        }



        void OnStartStory()
        {
            book.SetState(EndlessBook.StateEnum.OpenMiddle, onCompleted: StartStory);
            Metadata.Instance.currentTextPage = 1;
        }

       
        private StateChangedDelegate StartStory;


        IEnumerator SentenceCompletions(byte[] genImage, string prompt)
        {
            SentenceRegeneration.SetActive(true);
            //nextPage.interactable = false;
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

        public void OnPublishTextToBook()
        {
            switch (book.CurrentPageNumber)
            {
                case 1:
                    textP1.SetActive(false);
                    textP1Final.GetComponent<TextMeshProUGUI>().text = proposalText.GetComponent<TextMeshProUGUI>().text;
                    regenerateText.interactable = false;
                    publishSentence.interactable = false;


                    break;
                case 3:
                    textP3.SetActive(false);
                    textP3Final.GetComponent<TextMeshProUGUI>().text = proposalText.GetComponent<TextMeshProUGUI>().text;
                    regenerateText.interactable = false;
                    publishSentence.interactable = false;
                    break;
                case 5:
                    textP5.SetActive(false);
                    TextP5Final.GetComponent<TextMeshProUGUI>().text = proposalText.GetComponent<TextMeshProUGUI>().text;
                    regenerateText.interactable = false;
                    publishSentence.interactable = false;
                    break;
            }
            
            proposalText.GetComponent<TextMeshProUGUI>().text = "";
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
                        StartCoroutine(SentenceCompletions(imageP2bytes, Metadata.Instance.currentPrompt));
                        break;
                    case 3:
                        StartCoroutine(SentenceCompletions(imageP4bytes, Metadata.Instance.currentPrompt));
                        break;
                    case 5:
                        StartCoroutine(SentenceCompletions(imageP6bytes, Metadata.Instance.currentPrompt));
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
            /*
            CoroutineWithData cd_nextPrompt = new CoroutineWithData(this, Request.GetNextprompt(completion));
            yield return cd_nextPrompt.coroutine;
            string nextPrompt = (string)cd_nextPrompt.result;
            
            StartCoroutine(Request.PostImageDescription(completion, Metadata.Instance.currentImgID)); 
            _isGenerating = false;
            */
            string nextPrompt = "";
            int rand = Random.Range(0, 3);
            if (Metadata.Instance.consistencyPrompt == "Edgar, the elephant, joyful")
            {
                switch(Metadata.Instance.currentChapter)
                {
                    case "ch1":
                        switch(rand)
                        {
                            case 0:
                                nextPrompt = "One day Edgar the elephant plays with a group of joyful musicians in front of";
                                break;
                            case 1:
                                nextPrompt = "Edgar the elephant decides to explore a new genre of music. He starts learning";
                                break;
                            case 2:
                                nextPrompt = "On a sunny afternoon, Edgar the elephant and his musician friends are invited to perform at";
                                break;
                        }
                        break;
                    case "ch2":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "Edgar the elephant has great success with his music. One day he wins";
                                break;
                            case 1:
                                nextPrompt = "After mastering several instruments, Edgar the elephant collaborates with a famous artist to create";
                                break;
                            case 2:
                                nextPrompt = "Edgar the elephants music becomes so popular that he receives an invitation to play at";
                                break;
                        }
                        break;
                    default:
                        nextPrompt = "Edgar the elephant loves music. He plays";
                        break;
                }
            }
            else if (Metadata.Instance.consistencyPrompt == "A Robot named Robert")
            {
                switch (Metadata.Instance.currentChapter)
                {
                    case "ch1":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "Robert the Robot loves exploring new places. One day he decides to visit";
                                break;
                            case 1:
                                nextPrompt = "On a rainy day, Robert the Robot and his lively friend discover a mysterious";
                                break;
                            case 2:
                                nextPrompt = "One evening, Robert the Robot stumbles upon an old, forgotten";
                                break;
                        }
                        break;
                    case "ch2":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "Robert the Robot works hard to achieve his dream. Eventually, he becomes a";
                                break;
                            case 1:
                                nextPrompt = "Robert the Robot's dream takes him to a futuristic";
                                break;
                            case 2:
                                nextPrompt = "Inspired by his dreams, Robert the Robot invents a new kind of";
                                break;
                        }
                        break;
                    default:
                        nextPrompt = "Edgar the elephant loves music. He plays";
                        break;
                }
            }
            else if (Metadata.Instance.consistencyPrompt == "Still life, oil painting")
            {
                switch (Metadata.Instance.currentChapter)
                {
                    case "ch1":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "A still life, oil painting, depicting a table adorned with various";
                                break;
                            case 1:
                                nextPrompt = "A still life, oil painting, capturing the delicate details of a bowl filled with";
                                break;
                            case 2:
                                nextPrompt = "A still life, oil painting, illustrating a cozy kitchen filled with";
                                break;
                        }
                        break;
                    case "ch2":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "A still life, oil painting, highlighting a collection of antique";
                                break;
                            case 1:
                                nextPrompt = "A still life, portraying a serene garden scene with an array of";
                                break;
                            case 2:
                                nextPrompt = "A still life, oil painting, emphasizing the vibrant colors of a plate of fresh";
                                break;
                        }
                        break;
                    default:
                        nextPrompt = "Edgar the elephant loves music. He plays";
                        break;
                }
            }
            else if (Metadata.Instance.consistencyPrompt == "Wanda, old witch")
            {
                switch (Metadata.Instance.currentChapter)
                {
                    case "ch1":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = "One day Wanda the witch encounters one of her greatest enemies. A fearsome";
                                break;
                            case 1:
                                nextPrompt = "Wanda the witch lives in an ancient forest. Her favorite place to relax is";
                                break;
                            case 2:
                                nextPrompt = "On a stormy night, Wanda the witch brews a powerful potion consisting of";
                                break;
                        }
                        break;
                    case "ch2":
                        switch (rand)
                        {
                            case 0:
                                nextPrompt = " Wanda the witch's most treasured possession is a magical";
                                break;
                            case 1:
                                nextPrompt = "One morning, Wanda the witch receives an urgent message from a distant";
                                break;
                            case 2:
                                nextPrompt = "During a magical festival, Wanda the witch shows off her skills by";
                                break;
                        }
                        break;
                    default:
                        nextPrompt = "Edgar the elephant loves music. He plays";
                        break;
                }
            }


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



        public void GoToNextChapter()
        {
            SentenceRegeneration.SetActive(false);
            EventSystem.instance.CubeWaveRightEvent();
            
            switch (book.CurrentPageNumber)
            {
                case 1:
                    book.TurnForward(turnTime,
                        onCompleted: OnBookTurnToPageCompleted,
                        onPageTurnStart: OnPageTurnStart,
                        onPageTurnEnd: OnPageTurnEnd);
                    break;
                case 3:

                        book.TurnForward(turnTime,
                        onCompleted: OnBookTurnToPageCompleted,
                        onPageTurnStart: OnPageTurnStart,
                        onPageTurnEnd: OnPageTurnEnd);
                    break;
                case 5:
                    Debug.Log("End it");
                    BookNavigator.SetActive(true);

                    break;
            }
            
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


            StartCoroutine(Request.PostImageDescription(textP5.GetComponent<TextMeshProUGUI>().text, Metadata.Instance.currentImgID));


            book.SetState(EndlessBook.StateEnum.ClosedFront);
            EventSystem.instance.DisableBookNavigatorEvent();
            EventSystem.instance.EnableOwnershipScreenEvent();

            EventSystem.instance.ChooseCoverImageEvent(imageP2bytes);

            string alltext = textP1.GetComponent<TextMeshProUGUI>().text +
                         textP3.GetComponent<TextMeshProUGUI>().text +
                         textP5.GetComponent<TextMeshProUGUI>().text;

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


        protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            Debug.Log("OnPageTurnStart: front [" + pageNumberFront + "] back [" + pageNumberBack + "] fv [" + pageNumberFirstVisible + "] lv [" + pageNumberLastVisible + "] dir [" + turnDirection + "]");
        }
        
        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            Debug.Log("OnPageTurnEnd: front [" + pageNumberFront + "] back [" + pageNumberBack + "] fv [" + pageNumberFirstVisible + "] lv [" + pageNumberLastVisible + "] dir [" + turnDirection + "]");
        }


        [SerializeField]
        private LocalizedString FirstPageText;
        [SerializeField]
        private LocalizedString ContinueText;

        void Awake()
        {
            // cache the box collider for faster referencing
            boxCollider = gameObject.GetComponent<BoxCollider>();
            Debug.Log(boxCollider);

        }

        public Vector3 screenPosition;


        private StateChangedDelegate OnBookOpened;
        private StateChangedDelegate OnBookClosed;



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
            textP0.GetComponent<TextMeshProUGUI>().text = FirstPageText.GetLocalizedString();
        }

        public void Reset()
        {
            StartCoroutine(RestartInThree());
        }

        IEnumerator RestartInThree()
        {
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        void OnSelectImage(Sprite sprite, int index, byte[] imagebytes)
        {
            _isGenerating = true;
            Spinner.SetActive(true);
            regenerateText.interactable = false;
            publishSentence.interactable = false;
            StartCoroutine(SentenceCompletions(imagebytes, Metadata.Instance.currentPrompt));

            // Todo: is there a way to get rid of this switch statement? Example: Prefabs for new renderpage content
            // Todo: switch this to work with book state system
            switch (book.CurrentPageNumber)
            {
                case 1:           
                    textP2.SetActive(false);
                    imageP2bytes = imagebytes;
                    imageP2.GetComponent<Image>().sprite = sprite;
                    imageP2.SetActive(true);
                    DownArrow2.SetActive(false);
                    break;
                case 3:
                    textP4.SetActive(false);
                    imageP4bytes = imagebytes;
                    imageP4.GetComponent<Image>().sprite = sprite;
                    imageP4.SetActive(true);
                    DownArrow4.SetActive(false);
                    break;
                case 5:
                    textP6.SetActive(false);
                    imageP6bytes = imagebytes;
                    imageP6.GetComponent<Image>().sprite = sprite;
                    imageP6.SetActive(true);
                    finishBook.SetActive(true);
                    DownArrow6.SetActive(false);
                    break;
            }
        }


        void OnPublishNextPrompt(string prompt)
        {
            switch (book.CurrentPageNumber)
            {
                case 3:
                    Metadata.Instance.currentChapter = "ch2";
                    textP3.GetComponent<TextMeshProUGUI>().text = prompt + "...";
                    Metadata.Instance.previousPrompt = Metadata.Instance.startingPrompt;
                    Metadata.Instance.currentPrompt = prompt;
                    turnBookPage = true;
                    nextPageFive = true;
                    break;
                case 5:
                    Metadata.Instance.currentChapter = "ch3";
                    textP5.GetComponent<TextMeshProUGUI>().text = prompt + "...";
                    Metadata.Instance.previousPrompt = Metadata.Instance.currentPrompt;
                    Metadata.Instance.currentPrompt = prompt;
                    
                    break;

            }
        }


        void OnCompleteTextRevealed(int c)
        {
            Debug.Log("OnCompleteTextRevealed");
            switch (c)
            {
                case 0:
                    DownArrow.SetActive(true);
                    break;
                case 1:
                    textP2.GetComponent<TextMeshProUGUI>().text = ContinueText.GetLocalizedString();
                    
                    break;
                case 2:
                    DownArrow2.SetActive(true);
                    EventSystem.instance.EnableDrawingScreenEvent();
                                        break;
                case 3:
                    textP4.GetComponent<TextMeshProUGUI>().text = ContinueText.GetLocalizedString();


                    break;
                case 4:
                    DownArrow4.SetActive(true);
                    EventSystem.instance.EnableDrawingScreenEvent();
                    EventSystem.instance.DisableBookNavigatorEvent();
                    break;
                case 5:
                    textP6.GetComponent<TextMeshProUGUI>().text = ContinueText.GetLocalizedString();
                    break;
                case 6:
                    DownArrow6.SetActive(true);
                    EventSystem.instance.EnableDrawingScreenEvent();
                    EventSystem.instance.DisableBookNavigatorEvent();
                    break;
                case 10:
                    GoToNextChapter();
                    break;
                case 30:
                    GoToNextChapter();
                    break;
                case 50:
                    GoToNextChapter();
                    break;
            }
        }

        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, 
            EndlessBook.StateEnum toState, int currentPageNumber)
        {
            Debug.Log("OnBookTurnToPageCompleted: State set to " + toState + ". Current Page Number = " + currentPageNumber);
            if (!bookFinished)
            {
                switch (book.CurrentPageNumber)
                {
                    case 3:
                        if (nextBookPage != 5)
                        {
                            bookState = BookState.Chapter2;
                            _isGenerating = true;
                            StartCoroutine(CreateNextPrompt(textP1.GetComponent<TextMeshProUGUI>().text, Metadata.Instance.currentImgID));
                            _currentTemperature = StartingTemperature;
                            
                            nextBookPage = 5;
                            Metadata.Instance.storyBook.drawing.drawingPages["ch1"].regenerateText = regenerationCount;
                            regenerationCount = 0;
                        }
                        break;

                    case 5:
                        if (nextBookPage == 5)
                        {
                            bookState = BookState.Chapter3;
                            _isGenerating = true;
                            StartCoroutine(CreateNextPrompt(textP3.GetComponent<TextMeshProUGUI>().text, Metadata.Instance.currentImgID));
                            _currentTemperature = StartingTemperature;
                            bookFinished = true;
                            Metadata.Instance.storyBook.drawing.drawingPages["ch2"].regenerateText = regenerationCount;
                            regenerationCount = 0;
                        }
                        break;
                }
            }
            DebugCurrentState();
        }
        void DebugCurrentState()
        {
            Debug.Log("CurrentState: " + book.CurrentState);
            Debug.Log("CurrentPageNumber: " + book.CurrentPageNumber);
        }
    }

}