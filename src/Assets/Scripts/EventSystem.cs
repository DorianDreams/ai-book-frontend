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

    // --------------- Enable / Disable Events ---------------
    public event Action EnableBookNavigator;
    public void EnableBookNavigatorEvent()
    {
        if (EnableBookNavigator != null)
        {
            Debug.Log("Event fired: TextResultScreenEvent");
            EnableBookNavigator();
        }
    }
    public event Action DisableBookNavigator;
    public void DisableBookNavigatorEvent()
    {
        if (DisableBookNavigator != null)
        {
            Debug.Log("Event fired: DisableBookNavigatorEvent");
            DisableBookNavigator();
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

    public event Action DisableStartScreen;
    public void DisableStartScreenEvent()
    {
        if(DisableStartScreen != null)
        {
            Debug.Log("Event fired: DisableStartScreenEvent");
            DisableStartScreen();
        }
    }



    // --------------- Gameplay Events ---------------
    public event Action<byte[]> ChooseCoverImage;
    public void ChooseCoverImageEvent(byte[] image)    {
        if (ChooseCoverImage != null)
        {
            Debug.Log("Event fired: ChooseCoverImageEvent");
            ChooseCoverImage(image);
        }
    }

    public event Action<string> ChooseCoverAuthor;
    public void ChooseCoverAuthorEvent(string author)
    {
        if (ChooseCoverAuthor != null)
        {
            Debug.Log("Event fired: ChooseCoverAuthorEvent");
            ChooseCoverAuthor(author);
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

    public event Action GoToNextPage;
    public void GoToNextPageEvent()
    {
        if (GoToNextPage != null)
        {
            Debug.Log("Event fired: GoToNextPageEvent");
            GoToNextPage();
        }
    }
    public event Action GoPreviousPage;
    public void GoPreviousPageEvent()
    {
        if (GoPreviousPage != null)
        {
            Debug.Log("Event fired: GoPreviousPageEvent");
            GoPreviousPage();
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

    public event Action<Sprite, int, byte[]> SelectImage;
    public void SelectImageEvent(Sprite sprite, int index, byte[] imagebytes)
    {
        if (SelectImage != null)
        {
            Debug.Log("Event fired: SelectImageEvent");
            SelectImage(sprite, index, imagebytes);
        }
    }

    public event Action<string> SelectText;
    public void SelectTextEvent(string completion)
    {
        if (SelectText != null)
        {
            Debug.Log("Event fired: SelectTextEvent");
            SelectText(completion);
        }
    }

    public event Action<string> PublishNextPrompt;
    public void PublishNextPromptEvent(string newprompt)
    {
        if (PublishNextPrompt != null)
        {
            Debug.Log("Event fired: PublishNextPromptEvent");
            PublishNextPrompt(newprompt);
        }
    }

    public event System.Action<Sprite, string, string, string, int, byte[]> PublishToBook;
    public void PublishToBookEvent(Sprite sprite, string completion, string description, string newprompt, int index, byte[] imagebytes)
    {
        if (PublishToBook != null)
        {
            Debug.Log("Event fired: PublishToBookEvent");
            PublishToBook(sprite, completion, description, newprompt, index, imagebytes);
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


    // --------------- Drawing Events ---------------
    public event Action<UnityEngine.Color> PressColorButton;
    public void PressColorButtonEvent(UnityEngine.Color color)
    {
        if (PressColorButton != null)
        {
            Debug.Log("Event fired: PressColorButtonEvent");
            PressColorButton(color);
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

    public event Action<float> SetLineRendererWidth;
    public void SetLineRendererWidthEvent(float width)
    {
        if (SetLineRendererWidth != null)
        {
            Debug.Log("Event fired: SetLineRendererWidthEvent");
            SetLineRendererWidth(width);
        }
    }


    // --------------- Utility Events ---------------
    public event Action ChangeLocale;
    public void ChangeLocaleEvent()
    {
        if (ChangeLocale != null)
        {
            Debug.Log("Event fired: ChangeLocaleEvent");
            ChangeLocale();
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

    public event Action SaveCurrentCover;
    public void SaveCurrentCoverEvent()
    {
        if (SaveCurrentCover != null)
        {
            Debug.Log("Event fired: SaveCurrentCoverEvent");
            SaveCurrentCover();
        }
    }


    // --------------- Cube Interaction Events ---------------
    public event Action CubeOff;
    public void CubeOffEvent()
    {
        if (CubeOff != null)
        {
            Debug.Log("Event fired: CubeOffEvent");
            CubeOff();
        }
    }

    public event Action CubeCrossfade;
    public void CubeCrossfadeEvent()
    {
        if (CubeCrossfade != null)
        {
            Debug.Log("Event fired: CubeCrossfadeEvent");
            CubeCrossfade();
        }
    }

    public event Action CubeBlink;
    public void CubeBlinkEvent()
    {
        if (CubeBlink != null)
        {
            Debug.Log("Event fired: CubeBlinkEvent");
            CubeBlink();
        }
    }

    // Shows color of the cube
    public event Action<string> CubeShowColor;
    public void CubeShowColorEvent(string color)
    {
        if (CubeShowColor != null)
        {
            Debug.Log("Event fired: CubeShowColorEvent");
            CubeShowColor(color);
        }
    }

    public event Action CubeWaveUp;
    public void CubeWaveUpEvent()
    {
        if (CubeWaveUp != null)
        {
            Debug.Log("Event fired: CubeWaveUpEvent");
            CubeWaveUp();
        }
    }

    public event Action CubeWaveRight;
    public void CubeWaveRightEvent()
    {
        if (CubeWaveRight != null)
        {
            Debug.Log("Event fired: CubeWaveRightEvent");
            CubeWaveRight();
        }
    }

    public event Action CubeWaveLeft;
    public void CubeWaveLeftEvent()
    {
        if (CubeWaveLeft != null)
        {
            Debug.Log("Event fired: CubeWaveLeftEvent");
            CubeWaveLeft();
        }
    }
}
