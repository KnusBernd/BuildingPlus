using System.Collections.Generic;
using UnityEngine;

namespace BuildingPlus.Selection
{
    public class Selector : MonoBehaviour
    {
        public static Selector Instance;

        private bool isDraggingBox = false;     // true only after threshold is passed
        private bool pressed = false;           // accept button is being held
        private Vector3 startPos;
        private Vector3 endPos;

        public PiecePlacementCursor Cursor { get; set; }

        private LineRenderer outlineRendererVisual;
        private GameObject selectionFill;
        private Material fillMaterial;
        private SelectionManager selection;

        private GameObject selectionOutlineRoot;

        private bool sprinting;
        public bool Sprinting => sprinting;

        private float dragThreshold = 0.15f; // distance to activate drag mode

        void Start()
        {
            selection = new SelectionManager();
            Instance = this;
            CreateSelectionVisuals();
        }

        void Update()
        {
            if (!pressed)
                return;

            Vector3 cursorPos = Cursor.transform.position;
            cursorPos.x -= 0.5f;

            float dist = Vector3.Distance(startPos, cursorPos);

            // ---------------------------------------------------
            // SWITCH TO DRAG MODE AFTER THRESHOLD IS PASSED
            // ---------------------------------------------------
            if (!isDraggingBox && dist > dragThreshold)
            {
                isDraggingBox = true;
                selectionOutlineRoot.SetActive(true);
            }

            // If dragging, update rectangle
            if (isDraggingBox)
            {
                endPos = cursorPos;
                UpdateSelectionBox(startPos, endPos);
            }
        }

        // ---------------------------------------------------------------------
        // ACCEPT DOWN
        // ---------------------------------------------------------------------
        public bool OnAcceptDown()
        {
            // Can't select while building a piece
            if (Cursor.Piece != null)
                return true;

            pressed = true;
            isDraggingBox = false;

            // Start click location
            startPos = Cursor.transform.position;
            startPos.x -= 0.5f;

            // If not sprinting, clear old selection
            if (!Input.GetKey(KeyCode.LeftControl) && Cursor.hoveredPiece == null)
                selection.DeselectAll();

            return true;
        }

        // ---------------------------------------------------------------------
        // ACCEPT UP
        // ---------------------------------------------------------------------
        public bool OnAcceptUp()
        {
            pressed = false;

            // ---------------------------------------------------
            // DRAG SELECT MODE
            // ---------------------------------------------------
            if (isDraggingBox)
            {
                selectionOutlineRoot.SetActive(false);

                Vector3 min = Vector3.Min(startPos, endPos);
                Vector3 max = Vector3.Max(startPos, endPos);

                Bounds selectionBounds = new Bounds();
                selectionBounds.SetMinMax(min, max);

                var placeables = SelectionCheckCollision.checkCollision(selectionBounds);

                foreach (var placeable in placeables)
                {
                    selection.Select(placeable);
                    //BuildingPlusPlugin.LogInfo(placeable.name + "selected");
                }

                return false;
            }

            // ---------------------------------------------------
            // CLICK SELECT MODE
            // ---------------------------------------------------
            var hovered = Cursor.hoveredPiece;

            if (hovered == null)
                return true;

            // Normal click = single select
            if (selection.GetSelectedPlaceables().Count > 0 && Input.GetKey(KeyCode.LeftControl)) 
            { 
                if (selection.GetSelectedPlaceables().Contains(hovered)) 
                {
                    selection.Deselect(hovered);
                } else 
                { 
                    selection.Select(hovered);
                }
                return false;

            }

            selection.DeselectAll();
            selection.Select(hovered);

            return true ;
        }

        // ---------------------------------------------------------------------
        public bool OnSprintDown()
        {
            sprinting = true;
            return true;
        }

        public bool OnSprintUp()
        {
            sprinting = false;
            return true;
        }

        // ---------------------------------------------------------------------
        // CREATE VISUALS
        // ---------------------------------------------------------------------
        private void CreateSelectionVisuals()
        {
            selectionOutlineRoot = new GameObject("SelectionOutlineRoot");
            selectionOutlineRoot.transform.SetParent(transform);
            selectionOutlineRoot.SetActive(false);

            // Outline
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(selectionOutlineRoot.transform);

            outlineRendererVisual = outlineObj.AddComponent<LineRenderer>();
            outlineRendererVisual.material = new Material(Shader.Find("Sprites/Default"));
            outlineRendererVisual.startColor = outlineRendererVisual.endColor = new Color(0.2f, 0.5f, 1f, 0.65f);
            outlineRendererVisual.startWidth = outlineRendererVisual.endWidth = 0.05f;
            outlineRendererVisual.loop = false;
            outlineRendererVisual.positionCount = 5;
            outlineRendererVisual.sortingLayerName = "UI";
            outlineRendererVisual.sortingOrder = 32767;

            // Fill
            selectionFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(selectionFill.GetComponent<Collider>());
            selectionFill.transform.SetParent(selectionOutlineRoot.transform);

            fillMaterial = new Material(Shader.Find("Sprites/Default"));
            fillMaterial.color = new Color(0.2f, 0.5f, 1f, 0.15f);

            var fillRenderer = selectionFill.GetComponent<MeshRenderer>();
            fillRenderer.material = fillMaterial;
            fillRenderer.sortingLayerName = "UI";
            fillRenderer.sortingOrder = 32766;
        }

        // ---------------------------------------------------------------------
        private void UpdateSelectionBox(Vector3 a, Vector3 b)
        {
            Vector3 min = Vector3.Min(a, b);
            Vector3 max = Vector3.Max(a, b);

            Vector3[] corners = new Vector3[5];
            corners[0] = new Vector3(min.x, min.y);
            corners[1] = new Vector3(min.x, max.y);
            corners[2] = new Vector3(max.x, max.y);
            corners[3] = new Vector3(max.x, min.y);
            corners[4] = corners[0];

            outlineRendererVisual.SetPositions(corners);

            selectionFill.transform.position = (min + max) * 0.5f;
            selectionFill.transform.localScale = new Vector3(max.x - min.x, max.y - min.y, 1f);
        }
    }
}
