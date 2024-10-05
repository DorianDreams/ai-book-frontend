using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class summarising alls Event Parameters
public class EventParam
{
    public byte[] image;
    public string stringVal;
    public float floatVal;
    public int intVal;
    public UnityEngine.Color color;


    public EventParam() 
    {
    }
    public EventParam(byte[] image)
    {
        this.image = image;
    }

    public EventParam(string stringVal) 
    {
        this.stringVal = stringVal;
    }

    public EventParam(float floatVal)
    {
        this.floatVal = floatVal;
    }

    public EventParam(int intVal)
    {
        this.intVal = intVal;
    }
    public EventParam(UnityEngine.Color color)
    {
        this.color = color;
    }



}
