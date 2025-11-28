using UnityEngine;
using System;

namespace BuildingPlus.UI
{
    public class ColorPickerDialog
    {
        private Rect windowRect = new Rect(400, 200, 350, 300);
        private bool visible = false;

        private Color currentColor;
        private Color defaultColor; 
        private string hexColorString;

        private Action<bool, Color, bool> callback; 

        private GUIStyle colorPreviewStyle;
        private Texture2D colorPreviewTexture;

        public void Show(Color initial, Action<bool, Color, bool> resultListener, Color defaultColor)
        {
            if (colorPreviewTexture == null)
            {
                InitializeColorSwatch();
            }

            this.defaultColor = defaultColor;
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

        public void Hide(bool accepted, bool reset = false)
        {
            visible = false;

            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            if (callback != null)
            {
                if (accepted)
                    callback(true, currentColor, reset);
                else
                    callback(false, Color.clear, false);
            }
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

            DrawColorPreview();
            GUILayout.Space(10);
            DrawHexField();
            GUILayout.Space(10);
            DrawRgbSliders();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", GUILayout.Height(28)))
                Hide(true);

            if (GUILayout.Button("Cancel", GUILayout.Height(28)))
                Hide(false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Default", GUILayout.Height(28)))
            {
                currentColor = defaultColor;
                currentColor.a = 1f;
                hexColorString = ColorToHex(currentColor);
                Hide(true, true);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawColorPreview()
        {
            if (colorPreviewTexture == null)
            {
                InitializeColorSwatch();
            }

            colorPreviewTexture.SetPixel(0, 0, currentColor);
            colorPreviewTexture.Apply(); 

            Color previousColor = GUI.color;
            GUI.color = Color.white;

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

            if (newHex != hexColorString)
            {
                hexColorString = newHex;

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

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private bool IsValidHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return false;
            foreach (char c in hex)
                if (!Uri.IsHexDigit(c)) return false;
            return true;
        }

        private void DrawRgbSliders()
        {
            float newR, newG, newB;
            bool changed = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("R", GUILayout.Width(20));
            newR = GUILayout.HorizontalSlider(currentColor.r * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newR).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("G", GUILayout.Width(20));
            newG = GUILayout.HorizontalSlider(currentColor.g * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newG).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("B", GUILayout.Width(20));
            newB = GUILayout.HorizontalSlider(currentColor.b * 255f, 0f, 255f);
            GUILayout.Label(Mathf.RoundToInt(newB).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

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
                bool editingHex = GUI.GetNameOfFocusedControl() == "HexInput";
                if (!editingHex)
                    hexColorString = ColorToHex(currentColor);
            }
        }

        private string ColorToHex(Color color)
        {
            byte r = (byte)(color.r * 255f);
            byte g = (byte)(color.g * 255f);
            byte b = (byte)(color.b * 255f);
            return string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        private Color HexToColor(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return currentColor;
            try
            {
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }
            catch
            {
                return currentColor;
            }
        }
    }
}
