using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class DrawMeshFull : MonoBehaviour {

    public static DrawMeshFull Instance { get; private set; }



    [SerializeField] private Material drawMeshMaterial;

    private GameObject lastGameObject;
    private int lastSortingOrder;
    private Mesh mesh;
    private Vector3 lastMouseWorldPosition;
    private float lineThickness = 1f;
    private Color lineColor = Color.green;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (!UtilsClass.IsPointerOverUI()) {
            // Only run logic if not over UI
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            if (Input.GetMouseButtonDown(0)) {
                // Mouse Down
                CreateMeshObject();
                mesh = MeshUtils.CreateMesh(mouseWorldPosition, mouseWorldPosition, mouseWorldPosition, mouseWorldPosition);
                mesh.MarkDynamic();
                lastGameObject.GetComponent<MeshFilter>().mesh = mesh;
                Material material = new Material(drawMeshMaterial);
                material.color = lineColor;
                lastGameObject.GetComponent<MeshRenderer>().material = material;
            }

            if (Input.GetMouseButton(0)) {
                // Mouse Held Down
                float minDistance = .1f;
                if (Vector2.Distance(lastMouseWorldPosition, mouseWorldPosition) > minDistance) {
                    // Far enough from last point
                    Vector2 forwardVector = (mouseWorldPosition - lastMouseWorldPosition).normalized;

                    lastMouseWorldPosition = mouseWorldPosition;

                    MeshUtils.AddLinePoint(mesh, mouseWorldPosition, lineThickness);
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                // Mouse Up
                MeshUtils.AddLinePoint(mesh, mouseWorldPosition, 0f);
            }
        }
    }

    private void CreateMeshObject() {
        lastGameObject = new GameObject("DrawMeshSingle", typeof(MeshFilter), typeof(MeshRenderer));
        lastSortingOrder++;
        lastGameObject.GetComponent<MeshRenderer>().sortingOrder = lastSortingOrder;
    }

    public void SetThickness(float lineThickness) {
        this.lineThickness = lineThickness;
    }

    public void SetColor(Color lineColor) {
        this.lineColor = lineColor;
    }

}