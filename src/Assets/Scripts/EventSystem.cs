using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventSystem : MonoBehaviour
{

    public static EventSystem instance;

    public enum State
    {
        Drawing,
        Book
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public event Action SwitchCamera;
    public void SwitchCameraEvent()
    {
        if (SwitchCamera != null)
        {
            Debug.Log("Event fired: SwitchCameraEvent");
            SwitchCamera();
        }
    }

    public event Action EnableResultScreen;
    public void EnableResultScreenEvent()
    {
        if (EnableResultScreen != null)
        {
            Debug.Log("Event fired: EnableResultScreenEvent");
            EnableResultScreen();
        }
    }

    public event Action DisableResultScreen;
    public void DisableResultScreenEvent()
    {
        if (DisableResultScreen != null)
        {
            Debug.Log("Event fired: DisableResultScreenEvent");
            DisableResultScreen();
        }
    }

    public event Action EnableSignScreen;
    public void EnableSignScreenEvent()
    {
        if (EnableSignScreen != null)
        {
            Debug.Log("Event fired: EnableSignScreenEvent");
            EnableSignScreen();
        }
    }

    public event Action DisableSignScreen;
    public void DisableSignScreenEvent()
    {
        if (DisableSignScreen != null)
        {
            Debug.Log("Event fired: DisableSignScreenEvent");
            DisableSignScreen();
        }
    }

    public event Action RestartScene;
    public void RestartSceneEvent()
    {
        if (RestartScene != null)
        {
            Debug.Log("Event fired: RestartSceneEvent");
            RestartScene();
        }
    }


    public event Action EnableOwnershipScreen;
    public void EnableOwnershipScreenEvent()
    {
        if (EnableOwnershipScreen != null)
        {
            Debug.Log("Event fired: EnableOwnershipScreenEvent");
            EnableOwnershipScreen();
        }
    }

    public event Action DisableOwnershipScreen;
    public void DisableOwnershipScreenEvent()
    {
        if (DisableOwnershipScreen != null)
        {
            Debug.Log("Event fired: DisableOwnershipScreenEvent");
            DisableOwnershipScreen();
        }
    }

    public event Action EnableDrawingScreen;
    public void EnableDrawingScreenEvent()
    {
        if (EnableDrawingScreen != null)
        {
            Debug.Log("Event fired: EnableDrawingScreenEvent");
            EnableDrawingScreen();
        }
    }

    public event Action DisableDrawingScreen;
    public void DisableDrawingScreenEvent()
    {
        if (DisableDrawingScreen != null)
        {
            Debug.Log("Event fired: DisableDrawingScreenEvent");
            DisableDrawingScreen();
        }
    }



    public event Action ChangeLocale;
    public void ChangeLocaleEvent()
    {
        if (ChangeLocale != null)
        {
            Debug.Log("Event fired: ChangeLocaleEvent");
            ChangeLocale();
        }
    }

    public event Action DeleteLastLine;
    public void DeleteLastLineEvent()
    {
        if (DeleteLastLine != null)
        {
            Debug.Log("Event fired: DeleteLastLineEvent");
            DeleteLastLine();
        }
    }

    public event Action DeleteAllLines;
    public void DeleteAllLinesEvent()
    {
        if (DeleteAllLines != null)
        {
            Debug.Log("Event fired: DeleteAllLinesEvent");
            DeleteAllLines();
        }
    }

    public event Action<UnityEngine.Color> PressColorButton;
    public void PressColorButtonEvent(UnityEngine.Color color)
    {
        if (PressColorButton != null)
        {
            Debug.Log("Event fired: PressColorButtonEvent");
            PressColorButton(color);
        }
    }

    public event Action<byte[]> SendImageToAI;
    public void SendImageToAIEvent(byte[] image)
    {
        if (SendImageToAI != null)
        {
            Debug.Log("Event fired: sendImageToAIEvent");
            SendImageToAI(image);
        }
    }

    public event Action <Sprite, String> PublishToBook;
    public void PublishToBookEvent(Sprite sprite, String description)
    {
        if (PublishToBook != null)
        {
            Debug.Log("Event fired: PublishToBookEvent");
            PublishToBook(sprite, description);
        }
    }

   
    public event Action OpenBook;
    public void OpenBookEvent()
    {
        if (OpenBook != null)
        {
            Debug.Log("Event fired: OpenBookEvent");
            OpenBook();
        }
    }

    public event Action StartStory;
    public void StartStoryEvent()
    {
        if (StartStory != null)
        {
            Debug.Log("Event fired: StartStoryEvent");
            StartStory();
        }
    }


    public event Action HideLines;
    public void HideLinesEvent()
    {
        if (HideLines != null)
        {
            Debug.Log("Event fired: HideLinesEvent");
            HideLines();
        }
    }

    public event Action ShowLines;
    public void ShowLinesEvent()
    {
        if (ShowLines != null)
        {
            Debug.Log("Event fired: ShowLinesEvent");
            ShowLines();
        }
    }




    //USED EVENTS? TODO: DELETE UNUSED

    public event Action NextPage;
    public void NextPageEvent()
    {
        if (NextPage != null)
        {
            Debug.Log("Event fired: NextPageEvent");
            NextPage();
        }
    }

    public event Action LastPage;
    public void LastPageEvent()
    {
        if (LastPage != null)
        {
            Debug.Log("Event fired: LastPageEvent");
            LastPage();
        }
    }

    public event Action Signature;
    public void SignatureEvent()
    {
        if (Signature != null)
        {
            Debug.Log("Event fired: SignatureEvent");
            Signature();
        }
    }
    public event Action NewScene;
    public void NewSceneEvent()
    {
        if (NewScene != null)
        {
            NewScene();
        }
    }

    public event Action<string> ChangeLanguage;
    public void ChangeLanguageEvent(string language)
    {
        if (ChangeLanguage != null)
        {
            Debug.Log("Event fired: ChangeLanguageEvent");
            ChangeLanguage(language);
        }
    }

    public event Action StartDrawing;
    public void StartDrawingEvent()
    {
        if (StartDrawing != null)
        {
            Debug.Log("Event fired: StartDrawingEvent");
            StartDrawing();
        }
    }

}
