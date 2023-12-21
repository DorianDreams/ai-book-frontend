using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{

    public static EventSystem instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public event Action PublishToBook;
    public void PublishToBookEvent()
    {
        if (PublishToBook != null)
        {
            PublishToBook();
        }
    }

    public event Action SendToAI;
    public void SendToAIEvent()
    {
        if (SendToAI != null)
        {
            SendToAI();
        }
    }
}
