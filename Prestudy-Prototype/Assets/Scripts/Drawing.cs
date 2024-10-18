using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class Drawing 
{

    public Dictionary<string, DrawingPage> drawingPages;


    public Drawing()
    {
        //this.drawingPages = new List<DrawingPage>();
        this.drawingPages = new Dictionary<string, DrawingPage>();
    }
}
[Serializable]
public class DrawingPage
{
    public int strokes;
    public int points;
    public int iterations;
    public float time;
    public int selected_image;

    public DrawingPage(int strokes, int points, int iterations, float time)
    {
        this.strokes = strokes;
        this.points = points;
        this.iterations = iterations;
        this.time = time;
    }
}