using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingPlus.Selection
{
    internal class SelectionManager
    {
        private readonly List<Placeable> selectedPlaceables = new List<Placeable>();

        public List<Placeable> GetSelectedPlaceables() => selectedPlaceables;

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

            // Already selected? Ignore.
            if (selectedPlaceables.Contains(place))
                return;

            // Ensure highlight exists
            var highlight = place.GetComponent<SelectionHighlight>();
            if (highlight == null)
                highlight = place.gameObject.AddComponent<SelectionHighlight>();

            // Show visual
            highlight.Show();

            selectedPlaceables.Add(place);
        }
    }
}
