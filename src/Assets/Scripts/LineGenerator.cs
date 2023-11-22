using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineGenerator : MonoBehaviour
{

    public GameObject linePrefab;
    public Canvas parentCanvas;
    public string selectedColor = "black";
    public Material black, red, green, blue, orange;
    List<GameObject> Lines = new List<GameObject>();
    LineRenderer lineRenderer;
    Line activeLine;

    [SerializeField]
    float width = 1f;
    [SerializeField]
    float tolerance = .1f;

   
    

    Vector3 GetMousePosition()
    {
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition, parentCanvas.worldCamera,
            out movePos);
        Vector3 positionToReturn = parentCanvas.transform.TransformPoint(movePos);
        positionToReturn.z = parentCanvas.transform.position.z - 0.01f;
        return positionToReturn;
    }
    private void OnDestroy()
    {
        foreach (GameObject line in Lines)
        {
            Destroy(line);
        }
    }

    void AssignSelectedColor(string color, LineRenderer lineRenderer)
    {
        switch (color)
        {
            case "black":
                lineRenderer.material = black;
                break;
            case "red":
                lineRenderer.material = red;
                break;
            case "green":
                lineRenderer.material = green;
                break;
            case "blue":
                lineRenderer.material = blue;
                break;
            case "orange":
                lineRenderer.material = orange;
                break;
            default:
                lineRenderer.material = black;
                break;
        }
    }
    bool drawOnTablet()
    {
        GraphicRaycaster gr = parentCanvas.GetComponent<GraphicRaycaster>();
        PointerEventData ped = new PointerEventData(null);
        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);



        if (results.Count == 0)
        {
            return false;
        }
        else if (results.Count == 1 && results[0].gameObject.name == "Tablet")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        {
            if (!drawOnTablet())
            {
                activeLine = null;
            } else
            {

           
            if (Input.GetMouseButtonDown(0) )
            {
                
                    GameObject newLine = Instantiate(linePrefab);
                    Lines.Add(newLine);
                    lineRenderer = newLine.GetComponent<LineRenderer>();
                    AssignSelectedColor(selectedColor, lineRenderer);
                    lineRenderer.startWidth = width;
                    activeLine = newLine.GetComponent<Line>();
                            
            }

            if (Input.GetMouseButtonUp(0))
            {
                //lineRenderer.Simplify(tolerance);
                /* (lineRenderer.positionCount == 1)
                {
                    Vector3 mousePos = GetMousePosition();
                    activeLine.UpdateLine(mousePos);
                }*/
                activeLine = null;
            }

            if (activeLine != null)
            {
                
                    Vector3 mousePos = GetMousePosition();
                    activeLine.UpdateLine(mousePos);
                
               
            
            }
        } }
    }
}
