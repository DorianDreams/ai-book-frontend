using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class LightControl : MonoBehaviour
{
    public string portName = "COM4";
    SerialPort arduino;

    public bool lightOn = false;

    // Start is called before the first frame update
    void Start()
    {
        if(lightOn)
        {
            arduino = new SerialPort(portName, 9600);
            arduino.Open();
            EventSystem.instance.CubeCrossfade += Crossfade;
            EventSystem.instance.CubeOff += Off;
            EventSystem.instance.CubeBlink += Blink;
            EventSystem.instance.CubeShowColor += ShowColor;
            EventSystem.instance.CubeWaveUp += WaveUp;
            EventSystem.instance.CubeWaveRight += WaveRight;
            EventSystem.instance.CubeWaveLeft += WaveLeft;
        }
    }

    void Crossfade()
    {
        arduino.Write("1");
    }

    void Off()
    {
        arduino.Write("0");
    }

    void Blink()
    {
        arduino.Write("2");
    }

    void WaveUp()
    {
        arduino.Write("4");
    }

    void WaveRight()
    {
        arduino.Write("5");
    }

    void WaveLeft()
    {
        arduino.Write("6");
    }

    // Color as hex string
    void ShowColor(string color)
    {
        arduino.Write("3" + color);
    }

    // Update is called once per frame
    void Update()
    {
        if (lightOn)
        {
            if (arduino.IsOpen)
            {
                if (Input.GetKeyDown("0"))
                {
                    arduino.Write("0");
                    Debug.Log(0);
                }
                if (Input.GetKeyDown("1"))
                {
                    arduino.Write("1");
                    Debug.Log(1);
                }
                else if (Input.GetKeyDown("2"))
                {
                    arduino.Write("2");
                    Debug.Log(2);
                }
                else if (Input.GetKeyDown("3"))
                {
                    arduino.Write("30x00FFFF");
                    Debug.Log(3);
                }
                else if (Input.GetKeyDown("4"))
                {
                    arduino.Write("4");
                    Debug.Log(4);
                }
                else if (Input.GetKeyDown("5"))
                {
                    arduino.Write("5");
                    Debug.Log(5);
                }
                else if (Input.GetKeyDown("6"))
                {
                    arduino.Write("6");
                    Debug.Log(5);
                }
            }
        }
    }
}
