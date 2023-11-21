using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{

    public GameObject linePrefab;
    public Canvas parentCanvas;
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
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject newLine = Instantiate(linePrefab);
            Lines.Add(newLine);
            lineRenderer = newLine.GetComponent<LineRenderer>();
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
    }
}
