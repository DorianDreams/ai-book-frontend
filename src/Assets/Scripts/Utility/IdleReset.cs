using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleReset : MonoBehaviour
{
    private float _idleTime = 0f;


    // Update is called once per frame
    void Update()
    {
        // Check if any input is detected
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f)
        {
            // Reset idle time and set isIdle to false
            ResetIdleState();

            //Debug.Log("Reset Idle");
        }
        else
        {
            // Increment idle time
            _idleTime += Time.deltaTime;

            // Check if idle time exceeds 15 seconds and player is not already idle
            if (_idleTime >= 180f)
            {
                //Debug.Log("Idle");
                EventSystem.instance.FinishPlaythroughEvent();
                // Set isIdle to true to prevent multiple prints
            }
        }
    }

    // Reset the idle state by setting the idle time to 0 and isIdle flag to false
    private void ResetIdleState()
    {
        _idleTime = 0f;
    }

}
