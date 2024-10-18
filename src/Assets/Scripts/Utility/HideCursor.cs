using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour
{
    [SerializeField]
    bool cursorVisible = false;
    // Use this for initialization
    void Start()
    {
        //Set Cursor to not be visible
        Cursor.visible = cursorVisible;
    }
}