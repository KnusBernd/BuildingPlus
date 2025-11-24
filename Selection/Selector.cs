using System;
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
        private bool selectionLocked = false;
        public bool CanPickUp => !selectionLocked;

        public PiecePlacementCursor Cursor { get; set; }
        public SelectionManager Selection => selection;

        private SelectionManager selection;

        private bool sprinting;

        private float dragThreshold = 0.2f; // distance to activate drag mode

        private SelectorUI selectorUI;

        void Start()
        {
            selection = new SelectionManager();
            Instance = this;
            selectorUI = new SelectorUI(transform); // initialize UI
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
                selectorUI.ShowOutline(true);
                if (!Input.GetKey(KeyCode.LeftControl))
                    selection.DeselectAll();

            }

            // If dragging, update rectangle
            if (isDraggingBox)
            {
                endPos = cursorPos;
                selectorUI.UpdateBox(startPos, endPos);
            }
        }

        // ---------------------------------------------------------------------
        // ACCEPT DOWN
        // ---------------------------------------------------------------------
        public bool OnAcceptDown()
        {
            if (!CanPickUp)
                return false;

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

        public bool OnAcceptUp()
        {
            pressed = false;

            Placeable hovered = Cursor.hoveredPiece;
            if (hovered != null && !CanPickUp) { return false;  }

            // Drag select mode
            if (isDraggingBox)
                return HandleDragSelection();

            // Click select mode
            return HandleClickSelection();
        }

        // ---------------------------------------------------------------------
        // Handles drag selection logic
        // ---------------------------------------------------------------------
        private bool HandleDragSelection()
        {
            selectorUI.ShowOutline(false);

            Vector3 min = Vector3.Min(startPos, endPos);
            Vector3 max = Vector3.Max(startPos, endPos);

            Bounds selectionBounds = new Bounds();
            selectionBounds.SetMinMax(min, max);

            var placeables = SelectionCheckCollision.checkCollision(selectionBounds);

            foreach (var placeable in placeables)
                selection.Select(placeable);

            return false;
        }

        // ---------------------------------------------------------------------
        // Handles click selection logic
        // ---------------------------------------------------------------------
        private bool HandleClickSelection()
        {
            var hovered = Cursor.hoveredPiece;

            // Nothing hovered and nothing selected
            if (hovered == null && selection.GetSelectedPlaceables().Count == 0)
                return true;

            // Control key modifies selection
            if (Input.GetKey(KeyCode.LeftControl))
                return HandleSingleSelect(hovered);


            BuildingPlusPlugin.LogInfo( "keep piece: " + Cursor.KeepPiece);
            // Normal click = pick up hovered
            //
            if (hovered is MultipiecePart) 
            {
                selection.DeselectAll();
            }
            selection.PickUp(hovered);

            return true;
        }

        // ---------------------------------------------------------------------
        // Handles single-select with Control key
        // ---------------------------------------------------------------------
        private bool HandleSingleSelect(Placeable hovered)
        {
            if (selection.GetSelectedPlaceables().Contains(hovered))
                selection.Deselect(hovered);
            else
                selection.Select(hovered);

            return false;
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

        internal void Lock()
        {
            selectionLocked = true;
        }

        internal void Unlock() 
        {
            selectionLocked = false;
        }
    }
}
