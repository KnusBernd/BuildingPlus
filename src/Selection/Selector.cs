using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingPlus.Selection
{
    public class Selector : MonoBehaviour
    {
        public static Selector Instance;

        private bool isDraggingBox = false;     
        private bool pressed = false;          
        private Vector3 startPos;
        private Vector3 endPos;
        private bool selectionLocked = false;
        public bool CanPickUp => !selectionLocked;

        public PiecePlacementCursor Cursor { get; set; }
        public SelectionManager Selection => selection;

        private SelectionManager selection;

        private bool sprinting;

        private float dragThreshold = 0.2f; 

        private SelectorUI selectorUI;

        void Start()
        {
            selection = new SelectionManager();
            Instance = this;
            selectorUI = new SelectorUI(transform);
        }

        void Update()
        {
            if (!pressed)
                return;

            Vector3 cursorPos = Cursor.transform.position;
            cursorPos.x -= 0.5f;

            float dist = Vector3.Distance(startPos, cursorPos);

            // drag mode
            if (!isDraggingBox && dist > dragThreshold)
            {
                isDraggingBox = true;
                selectorUI.ShowOutline(true);
                if (!Input.GetKey(BuildingPlusConfig.ControlSelectionKey.Value))
                    selection.DeselectAll();

            }

            if (isDraggingBox)
            {
                endPos = cursorPos;
                selectorUI.UpdateBox(startPos, endPos);
            }
        }

        public bool OnAcceptDown()
        {
            if (!CanPickUp)
                return false;

            // Can't select while holding a piece
            if (Cursor.Piece != null)
                return true;


            pressed = true;
            isDraggingBox = false;

            // Start click location
            startPos = Cursor.transform.position;
            startPos.x -= 0.5f;

            // If not holding down leftcontrol, clear old selection
            if (!Input.GetKey(BuildingPlusConfig.ControlSelectionKey.Value) && Cursor.hoveredPiece == null)
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

       
        private bool HandleDragSelection()
        {
            selectorUI.ShowOutline(false);

            Vector3 min = Vector3.Min(startPos, endPos);
            Vector3 max = Vector3.Max(startPos, endPos);

            Bounds selectionBounds = new Bounds();
            selectionBounds.SetMinMax(min, max);

            var placeables = SelectionCheckCollision.checkCollision(selectionBounds);
            selection.GetOldSelectedPlaceables().Clear();
            foreach (var placeable in placeables)
                selection.Select(placeable);

            return false;
        }

        private bool HandleClickSelection()
        {
            var hovered = Cursor.hoveredPiece;

            // Nothing hovered and nothing selected
            if (hovered == null && selection.GetSelectedPlaceables().Count == 0)
                return true;

            // Control key modifies selection
            if (Input.GetKey(BuildingPlusConfig.ControlSelectionKey.Value))
                return HandleSingleSelect(hovered);

            // Normal click = pick up hovered
            if (hovered is MultipiecePart) 
            {
                selection.DeselectAll();
            }
            selection.PickUp(hovered);
            selection.GetOldSelectedPlaceables().Clear();

            return true;
        }

        private bool HandleSingleSelect(Placeable hovered)
        {
            if (SelectionCheckCollision.IgnoringPlacements.Contains(hovered.name)) return false;

            if (selection.GetSelectedPlaceables().Contains(hovered))
                selection.Deselect(hovered);
            else
                selection.Select(hovered);

            return false;
        }

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
