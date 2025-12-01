using BuildingPlus.Selection;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    public float dragSpeed = 0.01f;
    public float edgeScrollSpeed = 40f;
    public float edgeSize = 50f;

    public float zoomSensitivity = 20f;
    public float minFov = 2f;
    public float maxFov = 125f;

    public float doubleClickTime = 0.3f; // Max time between clicks for double-click

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private Vector3 originPosition;
    private float lastMMBClickTime = -1f;

    void Start()
    {
        cam = GetComponent<Camera>();
        originPosition = transform.position; // Store original camera position
    }

    void Update()
    {
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
        // Mouse scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.fieldOfView -= scroll * zoomSensitivity * 100f * Time.deltaTime; // multiplied to make scroll feel responsive

        // Keyboard input
        if (Input.GetKey(KeyCode.I))
            cam.fieldOfView -= zoomSensitivity * Time.deltaTime;
        if (Input.GetKey(KeyCode.K))
            cam.fieldOfView += zoomSensitivity * Time.deltaTime;

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
}
