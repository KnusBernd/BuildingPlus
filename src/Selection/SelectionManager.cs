using System;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
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
        public Placeable Head
        {
            get { return head; }
            set { head = value; }
        }

        public void DeselectAll()
        {
            var copy = selectedPlaceables.ToList();

            foreach (var placeable in copy)
                Deselect(placeable);
            selectedPlaceables.Clear();
        }


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

            if (selectedPlaceables.Contains(place) || selectedPlaceables.Contains(place.GetTopPiece()))
                return;

            Placeable newEntry = place.GetTopPiece();
            selectedPlaceables.Add(newEntry);

            // Ensure highlight exists and show it
            var highlight = newEntry.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = newEntry.gameObject.AddComponent<SelectionHighlight>();

            highlight.Show();
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
            // The piece the cursor is NOW holding (after pickup)

            Vector3 anchorWorldPos = newHead.transform.position;
            Quaternion anchorWorldRot = newHead.transform.rotation;

            //BuildingPlusPlugin.LogInfo("[Postfix] Anchor world pos = " + anchorWorldPos);
            //BuildingPlusPlugin.LogInfo("[Postfix] Anchor world rot = " + anchorWorldRot.eulerAngles);


            Vector3 reusedOldPos = newHead.transform.position;
            Quaternion reusedOldRot = newHead.transform.rotation;

            // Set the new HEAD
            List<Placeable> newSel = new List<Placeable>();
            foreach (var p in selectedPlaceables)
            {
                if (p.ID == oldHead.ID)
                    continue;

                //  Compute original local offset relative to the reused piece
                Vector3 localOffsetPos =
                    Quaternion.Inverse(reusedOldRot) * (p.transform.position - reusedOldPos);

                Quaternion localOffsetRot =
                    Quaternion.Inverse(reusedOldRot) * p.transform.rotation;

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
                placeable.transform.SetPositionAndRotation(newWorldPos, newWorldRot);
                placeable.Tint();
                newSel.Add(placeable);
                // Attach
                placeable.transform.SetParent(newHead.transform, worldPositionStays: true);

                //BuildingPlusPlugin.LogInfo($"[Postfix] Attached new piece {placeable.name} at {newWorldPos} rot {newWorldRot.eulerAngles}");
            }
            foreach (var p in new List<Placeable>(selectedPlaceables))
            {
                Deselect(p);
            }
            return newSel;
        }
    }
}
