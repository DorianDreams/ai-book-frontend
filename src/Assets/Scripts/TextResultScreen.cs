using echo17.EndlessBook;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Page = echo17.EndlessBook.Page;

public class TextResultScreen : MonoBehaviour
{    
    public GameObject textResultScreen;

    public GameObject previousPage;
    public GameObject nextPage;
    public GameObject regenerateText;

    public EndlessBook book;
    public float stateAnimationTime = 1f;
    public float turnTime = 1f;



    // Start is called before the first frame update
    void Start()
    {
        EventSystem.instance.EnableTextResultScreen += Enable;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Enable()
    {
        textResultScreen.SetActive(true);
    }

    public void GoToNextPage()
    {
        EventSystem.instance.GoToNextPageEvent();
    }

    public void GoToPreviousPage()
    {
        EventSystem.instance.GoPreviousPageEvent();
    }

    public virtual void RegenerateText()
    {
        Debug.Log("RegenerateText");
    }

    protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
    {
        Debug.Log("OnBookTurnToPageCompleted: State set to " + toState + ". Current Page Number = " + currentPageNumber);
    }

}
