using System;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using UnityEngine;

namespace BuildingPlus.Selection
{
    public class SelectionManager
    {
        private readonly List<Placeable> oldSelectedPlaceables = new List<Placeable>(); // used when copying
        private readonly HashSet<Placeable> selectedPlaceables = new HashSet<Placeable>();
        private readonly List<Placeable> pickedUpPlaceables = new List<Placeable>();
        private readonly Dictionary<Placeable, SelectionHighlight> highlightCache = new Dictionary<Placeable, SelectionHighlight>();
        private Placeable head;

        public List<Placeable> GetPickedUpPlaceables() => pickedUpPlaceables;

        public List<Placeable> GetSelectedPlaceables() => selectedPlaceables.ToList();

        public List<Placeable> GetOldSelectedPlaceables() => oldSelectedPlaceables;

        public Placeable Head
        {
            get { return head; }
            set { head = value; }
        }

        /// <summary>
        /// Gets or creates a SelectionHighlight component for the given Placeable, using cache.
        /// </summary>
        private SelectionHighlight GetOrCreateHighlight(Placeable place)
        {
            if (place == null || place.gameObject == null)
                return null;

            // Try to get from cache
            if (highlightCache.TryGetValue(place, out var highlight) && highlight != null)
            {
                return highlight;
            }

            // Not in cache or was destroyed - fetch/create it
            highlight = place.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = place.gameObject.AddComponent<SelectionHighlight>();

            // Update cache
            highlightCache[place] = highlight;
            return highlight;
        }

        /// <summary>
        /// Removes a Placeable from the highlight cache (call when destroying placeables).
        /// </summary>
        public void RemoveFromCache(Placeable place)
        {
            if (place != null)
            {
                highlightCache.Remove(place);
            }
        }

        /// <summary>
        /// Clears the entire highlight cache. Useful for cleanup or level changes.
        /// </summary>
        public void ClearCache()
        {
            highlightCache.Clear();
        }

        public void DeselectAll()
        {
            // Copy to avoid modification during iteration
            var placeablesToDeselect = new List<Placeable>(selectedPlaceables);

            foreach (var placeable in placeablesToDeselect)
            {
                Deselect(placeable);
            }
            selectedPlaceables.Clear();
        }

        public void Deselect(Placeable place)
        {
            if (place == null)
                return;

            var highlight = GetOrCreateHighlight(place);
            if (highlight != null)
            {
                highlight.Hide();
            }

            selectedPlaceables.Remove(place);
        }

        public void Select(Placeable place)
        {
            if (place == null)
                return;

            Placeable topPiece = place.GetTopPiece();

            // Single check for the top piece
            if (selectedPlaceables.Contains(topPiece))
                return;

            selectedPlaceables.Add(topPiece);

            // Get or create highlight using cache
            var highlight = GetOrCreateHighlight(topPiece);
            if (highlight != null)
            {
                highlight.Show();
            }
        }

        internal void PickUp(Placeable head)
        {
            if (head == null || !selectedPlaceables.Contains(head) || !Selector.Instance.CanPickUp)
                return;

            pickedUpPlaceables.Clear();
            pickedUpPlaceables.Add(head);

            foreach (var place in selectedPlaceables)
            {
                if (place != null && place != head && place.gameObject != null)
                {
                    head.AttachPiece(place);
                    pickedUpPlaceables.Add(place);
                }
            }

            this.head = head;
        }

        internal void Drop()
        {
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
            //head.DetachAllChildren(true);
            foreach (var place in pickedUpPlaceables)
            {
                //place.GetComponent<SelectionHighlight>().RefreshBounds();
            }
            DetachPieces(head, pickedUpPlaceables);

            head = null;
            pickedUpPlaceables.Clear();
            //BuildingPlusPlugin.LogInfo("[Drop] Completed safely.");
        }

        public void DetachPieces(Placeable head, List<Placeable> pieces)
        {
            // Safety: no head = nothing to detach.
            if (head == null || head.gameObject == null)
            {
                return;
            }

            pieces.RemoveAll(p =>
                p == null ||
                p.gameObject == null ||
                p == head ||
                p.ID == head.ID);

            if (pieces.Count == 0)
            {
                return;
            }

            // Use HashSet to track processed pieces - faster than Distinct()
            var processed = new HashSet<Placeable>();

            foreach (var p in pieces)
            {
                // Skip if already processed, null, or destroyed
                if (p == null || p.gameObject == null || !processed.Add(p))
                    continue;

                Transform t = p.transform;

                // SAFETY CHECK: Transform can be missing if prefab replaced or object was destroyed
                if (t == null)
                {
                    continue;
                }

                // Record world pose
                Vector3 worldPos = t.position;
                Quaternion worldRot = t.rotation;

                t.SetParent(null, true);

                t.SetPositionAndRotation(worldPos, worldRot);

                if (head.ChildPieces.Contains(p))
                {
                    head.ChildPieces.Remove(p);
                }

                if (p.ParentPiece == head)
                {
                    p.ParentPiece = null;
                    p.transform.parent = null;
                    p.relativeAttachPosition = new Vector3(0f, 0f, 0f);
                }
                p.Tint();
            }
        }

        public List<Placeable> CopySelectedPlaceablesRelativeTo(Placeable newHead, Placeable oldHead)
        {
            if (newHead == null || selectedPlaceables.Count == 0)
                return null;

            var cursor = Selector.Instance.Cursor;
            int cursorPlayer = cursor.AssociatedGamePlayer.networkNumber;

            // Cache transform reference - accessing .transform has overhead
            Transform newHeadTransform = newHead.transform;
            Vector3 anchorWorldPos = newHeadTransform.position;
            Quaternion anchorWorldRot = newHeadTransform.rotation;

            //BuildingPlusPlugin.LogInfo("[Postfix] Anchor world pos = " + anchorWorldPos);
            //BuildingPlusPlugin.LogInfo("[Postfix] Anchor world rot = " + anchorWorldRot.eulerAngles);

            Vector3 reusedOldPos = anchorWorldPos;
            Quaternion reusedOldRot = anchorWorldRot;

            // Cache inverse rotation calculation outside loop
            Quaternion invReusedOldRot = Quaternion.Inverse(reusedOldRot);

            // Set the new HEAD
            List<Placeable> newSel = new List<Placeable>();
            foreach (var p in selectedPlaceables)
            {
                if (p.ID == oldHead.ID)
                    continue;

                // Cache transform for this placeable
                Transform pTransform = p.transform;

                //  Compute original local offset relative to the reused piece
                Vector3 localOffsetPos = invReusedOldRot * (pTransform.position - reusedOldPos);
                Quaternion localOffsetRot = invReusedOldRot * pTransform.rotation;

                //  Apply that offset to the head
                Vector3 newWorldPos = anchorWorldPos + (anchorWorldRot * localOffsetPos);
                Quaternion newWorldRot = anchorWorldRot * localOffsetRot;

                // Instantiate new children
                Placeable placeable = UnityEngine.Object.Instantiate(
                    p.PickableBlock.placeablePrefab,
                    newWorldPos,
                    newWorldRot
                );

                placeable.GenerateIDOnPick(placeable.ID, cursorPlayer);
                placeable.SetColor(p.CustomColor);
                placeable.SetInitialDamageLevel(p.damageLevel, allowDamageReset: true);

                // Cache the new placeable's transform too
                Transform placeableTransform = placeable.transform;
                placeableTransform.SetPositionAndRotation(newWorldPos, newWorldRot);
                placeable.Tint();
                newSel.Add(placeable);

                // Attach
                placeableTransform.SetParent(newHeadTransform, worldPositionStays: true);

                //BuildingPlusPlugin.LogInfo($"[Postfix] Attached new piece {placeable.name} at {newWorldPos} rot {newWorldRot.eulerAngles}");
            }
            return newSel;
        }
    }
}