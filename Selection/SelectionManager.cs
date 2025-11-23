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
            if (selectedPlaceables.Contains(place))
                return;

            // Add this placeable FIRST to avoid recursive re-entry
            selectedPlaceables.Add(place);

            // Ensure highlight exists and show it
            var highlight = place.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = place.gameObject.AddComponent<SelectionHighlight>();

            highlight.Show();

            // Safely iterate children without modifying list during recursion
            var children = place.ChildPieces.ToList();

            foreach (var child in children)
            {
                if (child != null)
                    Select(child);
            }
        }

        internal void PickUp(Placeable head)
        {
            if (head == null || !selectedPlaceables.Contains(head)) return;
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
                BuildingPlusPlugin.LogWarning("[Drop] Head was null or destroyed. Cleaning up.");
                head = null;
                pickedUpPlaceables.Clear();
                return;
            }

            // -------------------------
            // CLEAN PICKUP LIST (SAFE)
            // -------------------------
            pickedUpPlaceables.RemoveAll(p =>
                p == null ||
                p.gameObject == null ||
                p.transform == null
            );

            // If only head remains, no need to detach
            if (pickedUpPlaceables.Count <= 1)
            {
                BuildingPlusPlugin.LogInfo("[Drop] No attached pieces to detach.");
                head = null;
                pickedUpPlaceables.Clear();
                return;
            }

            // -------------------------
            // DETACH SAFELY
            // -------------------------
            try
            {
                DetachPieces(head, pickedUpPlaceables);
            }
            catch (Exception ex)
            {
                // Should *never* happen with our safe DetachPieces, but if it does:
                BuildingPlusPlugin.LogError($"[Drop] DetachPieces threw an exception: {ex}");
            }

            // -------------------------
            // FINAL CLEANUP
            // -------------------------
            head.ChildPieces.Clear();
            head = null;
            pickedUpPlaceables.Clear();
            BuildingPlusPlugin.LogInfo("[Drop] Completed safely.");

        }

        public void DetachPieces(Placeable head, List<Placeable> pieces)
        {
            // Safety: no head = nothing to detach.
            if (head == null || head.gameObject == null)
            {
                BuildingPlusPlugin.LogWarning("[DetachPieces] Head is null — aborting.");
                return;
            }

            BuildingPlusPlugin.LogInfo($"[DetachPieces] Start — head = {head.name}");

            // --- SANITIZE LIST FIRST ---
            BuildingPlusPlugin.LogInfo($"[DetachPieces] Initial list count: {pieces.Count}");

            pieces.RemoveAll(p =>
                p == null ||
                p.gameObject == null ||
                p == head ||
                p.ID == head.ID);

            BuildingPlusPlugin.LogInfo($"[DetachPieces] After sanitizing: {pieces.Count} pieces.");

            if (pieces.Count == 0)
            {
                BuildingPlusPlugin.LogInfo("[DetachPieces] No valid pieces to detach.");
                return;
            }

            foreach (var p in pieces.Distinct())
            {
                if (p == null || p.gameObject == null)
                    continue;

                BuildingPlusPlugin.LogInfo($"[DetachPieces] Processing piece: {p.name} (ID:{p.ID})");

                Transform t = p.transform;

                // SAFETY CHECK: Transform can be missing if prefab replaced or object was destroyed
                if (t == null)
                {
                    BuildingPlusPlugin.LogWarning($"[DetachPieces] Missing transform on: {p.name}");
                    continue;
                }

                // LOG CURRENT PARENT
                string parentName = t.parent ? t.parent.name : "null";
                BuildingPlusPlugin.LogInfo($"[DetachPieces] Parent before detach: {parentName}");

                // Record world pose
                Vector3 worldPos = t.position;
                Quaternion worldRot = t.rotation;

                // ----------------------
                // DETACH (Safe version)
                // ----------------------
                try
                {
                    BuildingPlusPlugin.LogInfo($"[DetachPieces] Detaching {p.name}...");

                    t.SetParent(null, true);

                    // Log result of SetParent
                    parentName = t.parent ? t.parent.name : "null";
                    BuildingPlusPlugin.LogInfo($"[DetachPieces] Parent AFTER detach: {parentName}");

                    t.SetPositionAndRotation(worldPos, worldRot);
                }
                catch (Exception ex)
                {
                    BuildingPlusPlugin.LogError($"[DetachPieces] Transform detach FAILED on {p.name}: {ex}");
                    continue;
                }

                // ----------------------
                // CLEAN PARENT LINK
                // ----------------------
                try
                {
                    p.ParentPiece = null;
                    BuildingPlusPlugin.LogInfo($"[DetachPieces] Cleared ParentPiece on {p.name}");
                }
                catch (Exception ex)
                {
                    BuildingPlusPlugin.LogError($"[DetachPieces] Failed clearing ParentPiece on {p.name}: {ex}");
                }

                // ----------------------
                // CLEAN HEAD CHILD LIST
                // ----------------------
                try
                {
                    if (head.ChildPieces.Contains(p))
                    {
                        BuildingPlusPlugin.LogInfo($"[DetachPieces] Removing {p.name} from head.ChildPieces");
                        head.ChildPieces.Remove(p);
                    }
                }
                catch (Exception ex)
                {
                    BuildingPlusPlugin.LogError($"[DetachPieces] Failed removing {p.name} from head: {ex}");
                }

                // ----------------------
                // SAFE TINT
                // ----------------------
                try
                {
                    p.Tint();
                    BuildingPlusPlugin.LogInfo($"[DetachPieces] Tint OK on {p.name}");
                }
                catch (Exception ex)
                {
                    BuildingPlusPlugin.LogError($"[DetachPieces] Tint failed on {p.name}: {ex}");
                }

                BuildingPlusPlugin.LogInfo($"[DetachPieces] Finished processing {p.name}");
            }

            BuildingPlusPlugin.LogInfo("[DetachPieces] Safe detach finished.");
        }

    }
}
