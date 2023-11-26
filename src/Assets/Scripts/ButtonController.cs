using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    public Transform buttonGroup;
    /*public GameObject greenButton;
    public GameObject redButton;
    public GameObject blueButton;
    public GameObject orangeButton;
    public GameObject blackButton;*/

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
                scaleButton(buttonGroup.transform.GetChild(0).gameObject);
                break;
            case "orange":
                scaleButton(buttonGroup.transform.GetChild(1).gameObject);
                break;
            case "blue":
                scaleButton(buttonGroup.transform.GetChild(2).gameObject);
                break;
            case "black":
                scaleButton(buttonGroup.transform.GetChild(3).gameObject);
                break;
            case "red":
                scaleButton(buttonGroup.transform.GetChild(4).gameObject);
                break;          
            
        }
    }

    void scaleButton(GameObject button)
    {
        button.transform.localScale = new Vector3(widthSelected, widthSelected, widthSelected);
    }

    void scaleButtonNormal()
    {
        foreach (Transform child in transform)
        {
            child.localScale = new Vector3(1, 1, 1);
        }
    }
}
