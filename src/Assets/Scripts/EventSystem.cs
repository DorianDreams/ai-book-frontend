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

    // --------------- Start Story Events ---------------
    public event Action ChangeLocale;
    public void ChangeLocaleEvent()
    {
        if (ChangeLocale != null)
        {
            Debug.Log("Event fired: ChangeLocaleEvent");
            ChangeLocale();
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


    public event Action <float> SetLineRendererWidth;
    public void SetLineRendererWidthEvent(float width)
    {
        if (SetLineRendererWidth != null)
        {
            Debug.Log("Event fired: SetLineRendererWidthEvent");
            SetLineRendererWidth(width);
        }
    }
    

    // --------------- Ownership Events ---------------
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

    // --------------- Drawing Events ---------------
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

    public event Action PauseDrawing;
    public void PauseDrawingEvent()
    {
        if (PauseDrawing != null)
        {
            Debug.Log("Event fired: PauseDrawingEvent");
            PauseDrawing();
        }
    }

    public event Action ContinueDrawing;
    public void ContinueDrawingEvent()
    {
        if (ContinueDrawing != null)
        {
            Debug.Log("Event fired: ContinueDrawingEvent");
            ContinueDrawing();
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

    // --------------- Result Screen Events ---------------
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

    public event Action <Sprite, string, string, int> PublishToBook;
    public void PublishToBookEvent(Sprite sprite, String description, String continuation, int index)
    {
        if (PublishToBook != null)
        {
            Debug.Log("Event fired: PublishToBookEvent");
            PublishToBook(sprite, description, continuation, index);
        }
    }

    public event Action PublishMetadata;
    public void PublishMetadataEvent()
    {
        if (PublishMetadata != null)
        {
            Debug.Log("Event fired: PublishMetadataEvent");
            PublishMetadata();
        }
    }
}
