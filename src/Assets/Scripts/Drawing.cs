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
    public int drawingIterations;
    public int regenerateImages;
    public float time;
    public int selected_image;
    public int regenerateText;

    public DrawingPage(int strokes, int points, int drawingIterations, float time)
    {
        this.strokes = strokes;
        this.points = points;
        this.drawingIterations = drawingIterations;
        this.time = time;
        this.regenerateImages = 0;
    }
}