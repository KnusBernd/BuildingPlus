using UnityEngine;
using System;

namespace BuildingPlus.UI
{
    public class ColorPickerDialog
    {
        private Rect windowRect = new Rect(400, 200, 350, 300); 
        private bool visible = false;

        private Color currentColor;
        private string hexColorString; 

        private Action<bool, Color> callback;

        private GUIStyle colorPreviewStyle;
        private Texture2D colorPreviewTexture;

        public void Show(Color initial, Action<bool, Color> resultListener)
        {
            if (colorPreviewTexture == null)
            {
                InitializeColorSwatch();
            }

            currentColor = initial;
            currentColor.a = 1f; // always fully opaque

            hexColorString = ColorToHex(currentColor); // Initialize hex string
            
            PositionRightOfCenter();

            callback = resultListener;
            visible = true;

            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
        private void InitializeColorSwatch()
        {
            colorPreviewTexture = new Texture2D(1, 1);
            colorPreviewTexture.SetPixel(0, 0, Color.white);
            colorPreviewTexture.Apply();

            colorPreviewStyle = new GUIStyle();
            colorPreviewStyle.normal.background = colorPreviewTexture;
        }


        public void Hide(bool accepted)
        {
            visible = false;

            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            if (accepted)
                callback?.Invoke(true, currentColor);
            else
                callback?.Invoke(false, Color.clear);
        }

        public void Draw()
        {
            if (!visible)
                return;

            windowRect = GUILayout.Window(
                892134,
                windowRect,
                DrawWindow,
                "Select Color",
                GUILayout.Width(350),
                GUILayout.Height(300)
            );
        }

        private void PositionRightOfCenter()
        {
            float offsetX = 150f;
            float offsetY = 0f;

            windowRect.x = (Screen.width - windowRect.width) / 2 + offsetX;
            windowRect.y = (Screen.height - windowRect.height) / 2 + offsetY;
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            // COLOR PREVIEW FIELD
            DrawColorPreview();

            GUILayout.Space(10);

            // HEX FIELD 
            DrawHexField();

            GUILayout.Space(10);

            // RGB SLIDERS
            DrawRgbSliders();

            GUILayout.Space(10);

            // OK/CANCEL BUTTONS 
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", GUILayout.Height(28)))
                Hide(true);

            if (GUILayout.Button("Cancel", GUILayout.Height(28)))
                Hide(false);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawColorPreview()
        {
            if (colorPreviewStyle == null)
            {
                InitializeColorSwatch();
            }

            Color previousColor = GUI.color;

            // Update the preview color
            GUI.color = currentColor;

            GUILayout.Box("", colorPreviewStyle, GUILayout.Height(40), GUILayout.ExpandWidth(true));

            GUI.color = previousColor;
        }

        private void DrawHexField()
        {
            bool editingHex = GUI.GetNameOfFocusedControl() == "HexInput";

            GUILayout.BeginVertical();

            string displayHex = "#" + ColorToHex(currentColor);
            GUILayout.Label("Current Hex: " + displayHex);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hex:", GUILayout.Width(40));

            GUI.SetNextControlName("HexInput");
            string newHex = GUILayout.TextField(hexColorString, 7, GUILayout.ExpandWidth(true));

            // Check if the text changed
            if (newHex != hexColorString)
            {
                hexColorString = newHex;

                // If input is valid, update the color and remove focus
                if ((hexColorString.Length == 6 ||
                     (hexColorString.Length == 7 && hexColorString.StartsWith("#"))) &&
                    IsValidHex(hexColorString))
                {
                    currentColor = HexToColor(hexColorString);
                    currentColor.a = 1f;

                    GUI.FocusControl(null);
                }
            }

            Event e = Event.current;
            if (e.isKey && e.keyCode == KeyCode.Return && editingHex)
            {
                // If hex is valid, update color
                if ((hexColorString.Length == 6 ||
                     (hexColorString.Length == 7 && hexColorString.StartsWith("#"))) &&
                    IsValidHex(hexColorString))
                {
                    currentColor = HexToColor(hexColorString);
                    currentColor.a = 1f;
                }

                GUI.FocusControl(null);

                e.Use();
            }

            editingHex = GUI.GetNameOfFocusedControl() == "HexInput";

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private bool IsValidHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return false;

            foreach (char c in hex)
            {
                if (!Uri.IsHexDigit(c)) return false;
            }
            return true;
        }

        private void DrawRgbSliders()
        {
            float newR, newG, newB;
            bool changed = false;

            // RED Slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("R", GUILayout.Width(20));
            newR = GUILayout.HorizontalSlider(currentColor.r * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newR).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            // GREEN Slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("G", GUILayout.Width(20));
            newG = GUILayout.HorizontalSlider(currentColor.g * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newG).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            // BLUE Slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("B", GUILayout.Width(20));
            newB = GUILayout.HorizontalSlider(currentColor.b * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newB).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            // Check for change and update currentColor
            if (newR / 255f != currentColor.r)
            {
                currentColor.r = newR / 255f;
                changed = true;
            }
            if (newG / 255f != currentColor.g)
            {
                currentColor.g = newG / 255f;
                changed = true;
            }
            if (newB / 255f != currentColor.b)
            {
                currentColor.b = newB / 255f;
                changed = true;
            }

            if (changed)
            {
                currentColor.a = 1f;
                // Update hex string when RGB sliders change

                bool editingHex = GUI.GetNameOfFocusedControl() == "HexInput";
                if (!editingHex)
                {
                    hexColorString = ColorToHex(currentColor); // Initialize hex string
                }
            }
        }

        /// <summary>Converts a Unity Color (RGB, 0-1) to an RRGGBB hex string.</summary>
        private string ColorToHex(Color color)
        {
            // Convert float components (0-1) to bytes (0-255)
            byte r = (byte)(color.r * 255f);
            byte g = (byte)(color.g * 255f);
            byte b = (byte)(color.b * 255f);

            // Format as RRGGBB hex string (e.g., "FF00CC")
            return string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        /// <summary>Converts an RRGGBB or #RRGGBB hex string to a Unity Color.</summary>
        private Color HexToColor(string hex)
        {
            // Remove '#' if it exists
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Ensure hex string is 6 characters for RRGGBB
            if (hex.Length != 6)
            {
                return currentColor; // Return current color if invalid format
            }

            try
            {
                // Parse R, G, B components from the hex string
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                return new Color32(r, g, b, 255);
            }
            catch (Exception)
            {
                return currentColor; 
            }
        }
    }
}