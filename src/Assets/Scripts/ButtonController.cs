using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    public GameObject greenButton;
    public GameObject redButton;
    public GameObject blueButton;
    public GameObject orangeButton;
    public GameObject blackButton;

    [SerializeField]
    float widthSelected = 1.4f;
    public void AssignColor(string color)
    {
        GameObject.Find("LineGenerator(Clone)").GetComponent<LineGenerator>().selectedColor = color;
        scaleButtonNormal();
        Debug.Log(color);
        switch (color)
        {
            case "green":
                scaleButton(greenButton);
                break;
            case "red":
                scaleButton(redButton);
                break;
            case "blue":
                scaleButton(blueButton);
                break;
            case "orange":
                scaleButton(orangeButton);
                break;
            case "black":
                scaleButton(blackButton);
                break;
        }
    }

    void scaleButton(GameObject button)
    {
        button.transform.localScale = new Vector3(widthSelected, widthSelected, widthSelected);
    }

    void scaleButtonNormal()
    {
        greenButton.transform.localScale = new Vector3(1, 1, 1);
        redButton.transform.localScale = new Vector3(1, 1, 1);
        blueButton.transform.localScale = new Vector3(1, 1, 1);
        orangeButton.transform.localScale = new Vector3(1, 1, 1);
        blackButton.transform.localScale = new Vector3(1, 1, 1);
    }
}
