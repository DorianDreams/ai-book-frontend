using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Line : MonoBehaviour
{

    public LineRenderer lineRenderer;
    List<Vector3> points;




    public void UpdateLine(Vector3 pos)
    {
           
        if (points == null)
        {
            points = new List<Vector3>();
            SetPoint(pos);
            SetPoint(pos);
            return;
        }

        // Make sure not to add same point twice
        if (Vector3.Distance(points.Last(), pos) > .1f)
        {
            SetPoint(pos);
        } 
    }

    void SetPoint(Vector3 point)
    {
        points.Add(point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);
    }

}
