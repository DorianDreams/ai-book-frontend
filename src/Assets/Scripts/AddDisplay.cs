using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
        {
            Display.displays[0].Activate();
            Display.displays[0].SetRenderingResolution(1920, 1080);

            Display.displays[1].Activate();
            Display.displays[1].SetRenderingResolution(1920, 1080);
        }
       
    }

}
