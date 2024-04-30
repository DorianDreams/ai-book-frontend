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


        private string _Chapter1Prompt = null;
        private string _Chapter2Prompt = null;
        private string _Chapter3Prompt = null;
        private byte[] _CurrentImage = null;
        private string _CurrentImageID = null;

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
        public GameObject textP1;               // Should be a prefab for each page!!!
        public GameObject textP2;
        public LocalizedString drawPictureText;
        public GameObject DownArrow2;
        public GameObject imageP2;
        private byte[] imageP2bytes;
        public GameObject textP3;
        public GameObject imageP4;
        private byte[] imageP4bytes;
        public GameObject textP4;
        public GameObject textP5;
        public GameObject imageP6;
        private byte[] imageP6bytes;
        public GameObject textP6;


        [Header("Book Navigator")]
        public GameObject BookNavigator;

        public Button previousPage;
        public Button nextPage;
        public TextMeshProUGUI nextPageText;
        public Button regenerateText;
        public GameObject finishBook;



        // Delete after testing
        private bool endBook = false;
        private bool bookFinished = false;
        private bool nextPageFive = false;

        private int currentTextGenPage = 1;

        private int nextBookPage = 0;

        public float turnTime = 1f;
        public float stateAnimationTime = 1f;

        public GameObject Spinner;

        void OnStartStory()
        {

            book.SetState(EndlessBook.StateEnum.OpenMiddle, onCompleted: StartStory);
            Metadata.Instance.currentTextPage = 1;

            //turnBookPage = false;
        }

        

        void ButtonController(BookState state, int pagenumber){
            switch(state){
                case BookState.Chapter1:
                break;
                case BookState.Chapter2:
                break;
                case BookState.Chapter3:
                break;
            }
        }

        private StateChangedDelegate StartStory;


        IEnumerator SentenceCompletions(byte[] genImage, string prompt, GameObject text)
        {
            nextPage.interactable = false;
            previousPage.interactable = false;
            //nextPage.interactable = false;
            CoroutineWithData cd_completion = new CoroutineWithData(this, Request.GetSentenceCompletion(genImage, prompt, _currentTemperature));
            yield return cd_completion.coroutine;
            string completion = (string)cd_completion.result;
            text.GetComponent<TextMeshProUGUI>().text = completion;
            _isGenerating = false;
            Spinner.SetActive(false);
            regenerateText.interactable = true;
            previousPage.interactable = true;
            nextPage.interactable = true;
            OnBookPageChanged();
        }

        public void OnRegenerateSentence()
        {
            if (!_isGenerating) {
                Spinner.SetActive(true);
                regenerateText.interactable = false;
            
            switch (book.CurrentPageNumber)
            {
                case 1:
                    if(bookState == BookState.Chapter1)
                        {
                            _isGenerating = true;
                            StartCoroutine(SentenceCompletions(imageP2bytes, Metadata.Instance.currentPrompt, textP1));
                            regenerationCount++;
                    }
                    else
                        {
                            //TODO: Fehlermeldung an User
                        }
                    
                    break;
                case 3:
                        if(bookState == BookState.Chapter2)
                        {
                            _isGenerating = true;
                            StartCoroutine(SentenceCompletions(imageP4bytes, Metadata.Instance.currentPrompt, textP3));
                            regenerationCount++;
                        }
                        else
                        {
                            //TODO: Fehlermeldung an User
                        }
                            break;
                case 5:
                        if(bookState == BookState.Chapter3)
                        {
                            _isGenerating = true;
                            StartCoroutine(SentenceCompletions(imageP6bytes, Metadata.Instance.currentPrompt, textP5));
                            regenerationCount++;
                        }
                        else
                        {
                            //TODO: Fehlermeldung an User
                        }
                        break;
            }
                if(_currentTemperature < MaxTemperature)
                {
                    _currentTemperature += Temperatureincrease;
                }
            
            }
        }
        public IEnumerator CreateNextPrompt(string completion, string imgID)
        {
            CoroutineWithData cd_nextPrompt = new CoroutineWithData(this, Request.GetNextprompt(completion));
            yield return cd_nextPrompt.coroutine;
            string nextPrompt = (string)cd_nextPrompt.result;
            EventSystem.instance.PublishNextPromptEvent(nextPrompt);
            StartCoroutine(Request.PostImageDescription(completion, Metadata.Instance.currentImgID)); //todo:logging
            _isGenerating = false;
        }

       public IEnumerator CreateTitle(string alltext)
        {
            CoroutineWithData cd_title = new CoroutineWithData(this, Request.CreateTitle(alltext));
            yield return cd_title.coroutine;
            string title = (string)cd_title.result;
            BookTitle.GetComponent<TextMeshPro>().text = title;
            BookTitle.SetActive(true);
            Metadata.Instance.storyBook.title = title;
        }



        public void GoToNextPage()
        {
            if (!_isGenerating) { 
            switch (book.CurrentPageNumber)
            {
                case 1:
                    book.TurnForward(turnTime,
                        onCompleted: OnBookTurnToPageCompleted,
                        onPageTurnStart: OnPageTurnStart,
                        onPageTurnEnd: OnPageTurnEnd);
                    if (currentTextGenPage == 3)
                    {
                        
                    }
                    break;
                case 3:
                    if (nextPageFive)
                    {
                        book.TurnForward(turnTime,
                        onCompleted: OnBookTurnToPageCompleted,
                        onPageTurnStart: OnPageTurnStart,
                        onPageTurnEnd: OnPageTurnEnd);
                        if (currentTextGenPage == 5)
                        {
                         
                        }
                    }
                    break;
                case 5:
                    Debug.Log("End it");
                    break;
            }}
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
            if (!_isGenerating)
            {
                book.TurnBackward(turnTime, onCompleted: OnBookTurnToPreviousPageCompleted);
                
            }


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

        void Awake()
        {
            // cache the box collider for faster referencing
            boxCollider = gameObject.GetComponent<BoxCollider>();
            Debug.Log(boxCollider);

        }

        public Vector3 screenPosition;


        private StateChangedDelegate OnBookOpened;
        private StateChangedDelegate OnBookClosed;




        private void Start()
        {
            previousPage.interactable = false;
            _currentTemperature = StartingTemperature;
            EventSystem.instance.RestartScene += Reset;
            BookTitle.SetActive(false);

            Debug.Log("Start Book Controller");
            textP0.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(0);
            textP1.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(1);
            textP2.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(2);
            textP3.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(3);
            textP4.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(4);
            textP5.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(5);
            textP6.GetComponent<TypewriterEffect>().CompleteTextRevealed += () => OnCompleteTextRevealed(6);

            EventSystem.instance.StartStory += OnStartStory;
            EventSystem.instance.ChangeLocale += OnChangeLocale;
            //EventSystem.instance.PublishToBook += OnPublishToBook;
            EventSystem.instance.SelectImage += OnSelectImage;

            EventSystem.instance.PublishNextPrompt += OnPublishNextPrompt;

            EventSystem.instance.EnableBookNavigator += OnEnableBookNavigator;
            EventSystem.instance.DisableBookNavigator += OnDisableBookNavigator;

            EventSystem.instance.GoToNextPage += GoToNextPage;
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
            switch (book.CurrentPageNumber)
            {
                case 1:
                    Spinner.SetActive(true);
                    regenerateText.interactable = false;
                    StartCoroutine(SentenceCompletions(imagebytes, Metadata.Instance.currentPrompt, textP1));
                    textP2.SetActive(false);
                    imageP2bytes = imagebytes;
                    imageP2.GetComponent<Image>().sprite = sprite;
                    imageP2.SetActive(true);
                    break;
                case 3:
                    Spinner.SetActive(true);
                    regenerateText.interactable = false;
                    StartCoroutine(SentenceCompletions(imagebytes, Metadata.Instance.currentPrompt, textP3));
                    textP4.SetActive(false);
                    imageP4bytes = imagebytes;
                    imageP4.GetComponent<Image>().sprite = sprite;
                    imageP4.SetActive(true);
                    break;
                case 5:
                    Spinner.SetActive(true);
                    regenerateText.interactable = false;
                    StartCoroutine(SentenceCompletions(imagebytes, Metadata.Instance.currentPrompt, textP5));
                    textP6.SetActive(false);
                    imageP6bytes = imagebytes;
                    imageP6.GetComponent<Image>().sprite = sprite;
                    imageP6.SetActive(true);
                    finishBook.SetActive(true);
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
                    textP2.GetComponent<TextMeshProUGUI>().text = drawPictureText.GetLocalizedString();
                    break;
                case 3:
                    textP4.GetComponent<TextMeshProUGUI>().text = drawPictureText.GetLocalizedString();
                    break;
                case 5:
                    textP6.GetComponent<TextMeshProUGUI>().text = drawPictureText.GetLocalizedString();
                    break;
            }
        }

        void OnBookPageChanged() {
            if (book.CurrentPageNumber == 1) {
                previousPage.interactable = false;
            }
            if (book.CurrentPageNumber == 3) {
                previousPage.interactable = false;
                previousPage.interactable = true;
            }
            if (book.CurrentPageNumber == 5) {
                previousPage.interactable = false;
                previousPage.interactable = true;
            }

            if (book.CurrentPageNumber == 1 && bookState == BookState.Chapter2) {
                nextPage.interactable = false;
               nextPageText.text = "Next Page";
               nextPage.interactable = true;
                
            }
            if (book.CurrentPageNumber == 3 && bookState == BookState.Chapter2) {
                nextPage.interactable = false;
                nextPageText.text = "Create next Drawing";
                nextPage.interactable = true;
            }
            if(book.CurrentPageNumber == 1 && bookState == BookState.Chapter3){
                nextPage.interactable = false;
                nextPageText.text = "Next Page";
                nextPage.interactable = true;
                
            }
            if(book.CurrentPageNumber == 5 && bookState == BookState.Chapter3){
                nextPage.interactable = true;
                nextPage.interactable = false;

                
            }
            if(book.CurrentPageNumber == 1 && bookState == BookState.Chapter3){
                regenerateText.interactable = false;
                nextPage.interactable = false;
                nextPage.interactable = true;
                
            }

            if(book.CurrentPageNumber == 3 && bookState == BookState.Chapter3){
                nextPageText.text = "Next Page";
                regenerateText.interactable = false;
                nextPage.interactable = false;
                nextPage.interactable = true;
                
            }
            

        }
        protected virtual void OnBookTurnToPreviousPageCompleted(EndlessBook.StateEnum fromState, 
            EndlessBook.StateEnum toState, int currentPageNumber)
            {
                OnBookPageChanged();
            }

        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, 
            EndlessBook.StateEnum toState, int currentPageNumber)
        {
            OnBookPageChanged();
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
                            EventSystem.instance.EnableDrawingScreenEvent();
                            EventSystem.instance.DisableBookNavigatorEvent();
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
                            EventSystem.instance.EnableDrawingScreenEvent();
                            EventSystem.instance.DisableBookNavigatorEvent();
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
