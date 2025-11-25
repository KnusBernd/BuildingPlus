using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingPlus.Selection
{
    public class SelectionManager
    {
        private readonly List<Placeable> selectedPlaceables = new List<Placeable>();
        private readonly List<Placeable> pickedUpPlaceables = new List<Placeable>();
        private Placeable head;
        public List<Placeable> GetPickedUpPlaceables() => pickedUpPlaceables;

        public List<Placeable> GetSelectedPlaceables() => selectedPlaceables;
        public Placeable Head { get { return head; } }

        // --------------------------------------------------
        //  Deselect ALL – SAFE VERSION (no modified-while-enum)
        // --------------------------------------------------
        public void DeselectAll()
        {
            // Copy before iterating to avoid enumerator exceptions
            var copy = selectedPlaceables.ToList();

            foreach (var placeable in copy)
                Deselect(placeable);
        }

        // --------------------------------------------------
        //  Deselect ONE
        // --------------------------------------------------
        public void Deselect(Placeable place)
        {
            if (place == null)
                return;

            var highlight = place.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = place.gameObject.AddComponent<SelectionHighlight>();

            highlight.Hide();

            selectedPlaceables.Remove(place);
        }

        public void Select(Placeable place)
        {
            if (place == null)
                return;

            // Already selected? Stop here.
            if (selectedPlaceables.Contains(place) || selectedPlaceables.Contains(place.GetTopPiece()))
                return;

            Placeable newEntry = place.GetTopPiece();
            selectedPlaceables.Add(newEntry);

            // Ensure highlight exists and show it
            var highlight = newEntry.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = newEntry.gameObject.AddComponent<SelectionHighlight>();

            highlight.Show();

            // Safely iterate children without modifying list during recursion
           /* var children = newEntry.ChildPieces.ToList();

            foreach (var child in children)
            {
                if (child != null)
                    Select(child);
            }*/
        }

        internal void PickUp(Placeable head)
        {
            if (head == null || !selectedPlaceables.Contains(head) || !Selector.Instance.CanPickUp) return;
            pickedUpPlaceables.Clear();
            pickedUpPlaceables.Add(head);

            foreach (var place in selectedPlaceables)
            {
                if (place == null) continue;
                if (place == head) continue;
                if (place.gameObject == null) continue;
                if (place == head) continue; // skip the head itself
                head.AttachPiece(place);
                pickedUpPlaceables.Add((Placeable)place);
            }
            this.head = head;
        }

        internal void Drop()
        {
            // -------------------------
            // BASIC VALIDATION
            // -------------------------
            if (head == null || head.gameObject == null)
            {
                //BuildingPlusPlugin.LogWarning("[Drop] Head was null or destroyed. Cleaning up.");
                head = null;
                pickedUpPlaceables.Clear();
                return;
            }

            // If only head remains, no need to detach
            if (pickedUpPlaceables.Count <= 1)
            {
                //BuildingPlusPlugin.LogInfo("[Drop] No attached pieces to detach.");
                head = null;
                pickedUpPlaceables.Clear();
                return;
            }

            head.DetachAllChildren(true);
            
            head = null;
            pickedUpPlaceables.Clear();
            //BuildingPlusPlugin.LogInfo("[Drop] Completed safely.");

            return;

            //DetachPieces(head, pickedUpPlaceables);
        }

       /* public void DetachPieces(Placeable head, List<Placeable> pieces)
        {
            // Safety: no head = nothing to detach.
            if (head == null || head.gameObject == null)
            {
                return;
            }

            // --- SANITIZE ---

            pieces.RemoveAll(p =>
                p == null ||
                p.gameObject == null ||
                p == head ||
                p.ID == head.ID);


            if (pieces.Count == 0)
            {
                return;
            }

            foreach (var p in pieces.Distinct())
            {
                if (p == null || p.gameObject == null)
                    continue;


                Transform t = p.transform;

                // SAFETY CHECK: Transform can be missing if prefab replaced or object was destroyed
                if (t == null)
                {
                    continue;
                }

                // LOG CURRENT PARENT
                string parentName = t.parent ? t.parent.name : "null";

                // Record world pose
                Vector3 worldPos = t.position;
                Quaternion worldRot = t.rotation;

                t.SetParent(null, true);

                // Log result of SetParent
                parentName = t.parent ? t.parent.name : "null";

                t.SetPositionAndRotation(worldPos, worldRot);
              
                if (head.ChildPieces.Contains(p))
                {
                    head.ChildPieces.Remove(p);
                }

                if (p.ParentPiece == p)
                {
                    p.ParentPiece = null;
                    p.transform.parent = null;
                    p.relativeAttachPosition = new Vector3(0f, 0f, 0f);
                }
                p.Tint();
            }

        } */

    }
}
