using System;
using BuildingPlus;
using BuildingPlus.Selection;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    public float dragSpeed = 0.013f;
    public float edgeScrollSpeed = 25f;
    public float edgeSize = 35f;

    public float zoomSensitivity = 7f;
    public float minFov = 2f;
    public float maxFov = 125f;

    public float doubleClickTime = 0.3f; 

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private Vector3 originPosition;
    private float lastMMBClickTime = -1f;
    private FreePlayControl control;

    void Start()
    {
        cam = GetComponent<Camera>();
        originPosition = transform.position;

        dragSpeed = BuildingPlusConfig.CameraDragSpeed.Value;
        edgeScrollSpeed = BuildingPlusConfig.CameraEdgeScrollSpeed.Value;
        zoomSensitivity = BuildingPlusConfig.CameraZoomSensitivity.Value;
        minFov = BuildingPlusConfig.CameraMinFOV.Value;
        maxFov = BuildingPlusConfig.CameraMaxFOV.Value;
    }

    void Update()
    {
        if (control == null || control.InventoryBook == null)
            return;
        if (control.InventoryBook.inInventory)
            return;
        HandleMouseDrag();
        HandleEdgeScroll();
        HandleZoom();
        HandleDoubleMMBClick();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            float speedMultiplier = 1f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speedMultiplier = 1.5f; // Increase drag speed by 50% when Shift held
            }

            // Move camera along world X/Y axes
            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * dragSpeed * speedMultiplier;
            transform.position += move;
        }
    }

    void HandleEdgeScroll()
    {
        if (!Application.isFocused) return;

        Vector3 move = Vector3.zero;

        // Horizontal
        if (Input.mousePosition.x >= Screen.width - edgeSize)
            move.x += edgeScrollSpeed * Time.deltaTime;
        else if (Input.mousePosition.x <= edgeSize)
            move.x -= edgeScrollSpeed * Time.deltaTime;

        // Vertical
        // top edge
        if (Input.mousePosition.y >= Screen.height - edgeSize)
            move.y += edgeScrollSpeed * Time.deltaTime;

        // bottom edge (adjusted for invisible bar)
        float bottomThreshold = edgeSize + 20f; // add extra margin for invisible bar
        if (Input.mousePosition.y <= bottomThreshold)
            move.y -= edgeScrollSpeed * Time.deltaTime;

        transform.position += move;
    }

    void HandleZoom()
    {
        if (Selector.Instance != null && Selector.Instance.Cursor.Piece != null) return;
        // Mouse scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Keyboard input
        if (Input.GetKey(KeyCode.LeftAlt))
            cam.fieldOfView -= scroll * zoomSensitivity * 100f * Time.deltaTime;

        // Clamp to min/max FOV
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFov, maxFov);
    }

    void HandleDoubleMMBClick()
    {
        if (Input.GetMouseButtonDown(2))
        {
            float timeSinceLastClick = Time.time - lastMMBClickTime;
            if (timeSinceLastClick <= doubleClickTime)
            {
                // Double click detected -> reset camera
                transform.position = originPosition;
            }
            lastMMBClickTime = Time.time;
        }
    }

    internal void SetController(FreePlayControl instance)
    {
       control = instance;
    }
}
