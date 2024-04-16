using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineGenerator : MonoBehaviour
{
    public static LineGenerator instance;

    public GameObject linePrefab;
    public GameObject Slider;
    public Canvas parentCanvas;
    public string selectedColor = "black";

    private UnityEngine.Color color;
    private Material material;
    public Material black, red, green, blue, orange;


    List<GameObject> Lines = new List<GameObject>();
    LineRenderer lineRenderer;
    Line activeLine;

    public float width = 1f;
    public float SmoothingLength = 1.5f;
    public int SmoothingSections = 2;

    private BezierCurve[] Curves;
    //[SerializeField]
    //float tolerance = .1f;   |  Maybe introduce to make it fancier


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    private void Start()
    {
        Material material = new Material(Shader.Find("Unlit/Color"));
        //Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.black;
        this.material = material;
        EventSystem.instance.PressColorButton += OnPressColorButton;
        EventSystem.instance.DeleteAllLines += OnDeleeteAllLines;
        EventSystem.instance.DeleteLastLine += OnDeleteLastLine;
        EventSystem.instance.ShowLines += ShowLines;
        EventSystem.instance.HideLines += HideLines;

        EventSystem.instance.SetLineRendererWidth += OnChangeWidth;

    }

    private void OnChangeWidth(float width)
    {
        //width = Slider.GetComponent<Slider>().value;
        this.width = width;
    }

    private void OnDeleeteAllLines()
    {
        foreach (GameObject line in Lines)
        {
            Destroy(line);
        }
        Lines.Clear();
    }

    private void OnDeleteLastLine()
    {
        if (Lines.Count > 0)
        {
            Destroy(Lines[Lines.Count - 1]);
            Lines.RemoveAt(Lines.Count - 1);
        }
    }

    private void OnPressColorButton(UnityEngine.Color color)
    {
        selectedColor = color.ToString();
        this.color = color;
        material.color = color;
    }
 

    private void OnDestroy()
    {
        foreach (GameObject line in Lines)
        {
            Destroy(line);
        }
    }


    public void HideLines()
    {
        foreach (GameObject line in Lines)
        {
            line.SetActive(false);
        }
    }

    public void ShowLines()
    {
        foreach (GameObject line in Lines)
        {
            line.SetActive(true);
        }
    }

    public int CountStrokes()
    {
        return Lines.Count;
    }

    public int CountPoints()
    {
        int count = 0;
        foreach (GameObject line in Lines)
        {
            count += line.GetComponent<Line>().lineRenderer.positionCount;
        }
        return count;
    }

    // Drawing functions
    bool drawOnImage()
    {
        GraphicRaycaster gr = parentCanvas.GetComponent<GraphicRaycaster>();
        PointerEventData ped = new PointerEventData(null);
        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);



        if (results.Count == 1 && results[0].gameObject.name == "DrawingBackground")
        {
            return true;
        }
       
        else
        {
            return false;
        }
    }

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


    // Update is called once per frame
    void Update()
    {
        //width = Slider.GetComponent<Slider>().value;
        {
            
            if (!drawOnImage())
            {
                activeLine = null;
            } else 
            {

           
            if (Input.GetMouseButtonDown(0) )
            {
                
                    GameObject newLine = Instantiate(linePrefab, parentCanvas.transform);
                    Lines.Add(newLine);
                    lineRenderer = newLine.GetComponent<LineRenderer>();
                    lineRenderer.material = material;
                    lineRenderer.material.renderQueue = 2000 + Lines.Count;
                    lineRenderer.startWidth = width;
                    activeLine = newLine.GetComponent<Line>();
                            
            }

            if (Input.GetMouseButtonUp(0))
            {
                    /*
                    if (lineRenderer.positionCount < 100)
                    {
                        lineRenderer.Simplify(0.03f);
                    } else if (lineRenderer.positionCount < 200)
                    {
                        lineRenderer.Simplify(0.02f);
                    } else
                    {
                        lineRenderer.Simplify(0.01f);
                    }
                    */
                    //lineRenderer.Simplify(tolerance);
                    /* (lineRenderer.positionCount == 1)
                    {
                        Vector3 mousePos = GetMousePosition();
                        activeLine.UpdateLine(mousePos);
                    }*/

                    /*
                  Curves = new BezierCurve[activeLine.lineRenderer.positionCount -1];


                          for (int i = 0; i < Curves.Length; i++)
                          {
                              Curves[i] = new BezierCurve();
                              Vector3 position = activeLine.lineRenderer.GetPosition(i);
                              Vector3 lastPosition = i == 0 ? activeLine.lineRenderer.GetPosition(0) : activeLine.lineRenderer.GetPosition(i - 1);
                              Vector3 nextPosition = activeLine.lineRenderer.GetPosition(i + 1);

                              Vector3 lastDirection = (position - lastPosition).normalized;
                              Vector3 nextDirection = (nextPosition - position).normalized;

                              Vector3 startTangent = (lastDirection + nextDirection) * SmoothingLength;
                              Vector3 endTangent = (nextDirection + lastDirection) * -1 * SmoothingLength;

                              Handles.color = Color.green;
                              Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), position + startTangent, Quaternion.identity, 0.25f, EventType.Repaint);

                              if (i != 0)
                              {
                                  Handles.color = Color.blue;
                                  Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), nextPosition + endTangent, Quaternion.identity, 0.25f, EventType.Repaint);
                              }

                              Curves[i].Points[0] = position; // Start Position (P0)
                              Curves[i].Points[1] = position + startTangent; // Start Tangent (P1)
                              Curves[i].Points[2] = nextPosition + endTangent; // End Tangent (P2)
                              Curves[i].Points[3] = nextPosition; // End Position (P3)
                          }

                          // Apply look-ahead for first curve and retroactively apply the end tangent
                          {
                              Vector3 nextDirection = (Curves[1].EndPosition - Curves[1].StartPosition).normalized;
                              Vector3 lastDirection = (Curves[0].EndPosition - Curves[0].StartPosition).normalized;

                              Curves[0].Points[2] = Curves[0].Points[3] +
                                  (nextDirection + lastDirection) * -1 * SmoothingLength;

                              Handles.color = Color.blue;
                              Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), Curves[0].Points[2], Quaternion.identity, 0.25f, EventType.Repaint);
                          }


                      SmoothPath();
                      */
                    activeLine = null;
            }

            if (activeLine != null)
            {
                
                    Vector3 mousePos = GetMousePosition();
                    activeLine.UpdateLine(mousePos);
                
               
            
            }   
        } }
    }

    private void SmoothPath()
    {
        activeLine.lineRenderer.positionCount = Curves.Length * SmoothingSections;
        int index = 0;
        Debug.Log(Curves);
        for (int i = 0; i < Curves.Length; i++)
        {
            Vector3[] segments = Curves[i].GetSegments(SmoothingSections);
            for (int j = 0; j < segments.Length; j++)
            {
                activeLine.lineRenderer.SetPosition(index, segments[j]);
                index++;
            }
        }

    }
}