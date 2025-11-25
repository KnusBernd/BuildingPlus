using UnityEngine;
using System;

namespace BuildingPlus.UI
{
    public class ColorPickerDialog
    {
        private Rect windowRect = new Rect(400, 200, 350, 200);
        private bool visible = false;

        private Color currentColor;
        private string rText, gText, bText;

        private Action<bool, Color> callback;

        public void Show(Color initial, Action<bool, Color> resultListener)
        {
            currentColor = initial;
            currentColor.a = 1f; // always fully opaque
            PositionRightOfCenter();
            rText = Mathf.RoundToInt(initial.r * 255).ToString();
            gText = Mathf.RoundToInt(initial.g * 255).ToString();
            bText = Mathf.RoundToInt(initial.b * 255).ToString();

            callback = resultListener;
            visible = true;

            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
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
                GUILayout.Height(200)
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

            GUILayout.Label("RGB Values (0–255)");

            GUILayout.BeginHorizontal();
            DrawField("R", ref rText, v => currentColor.r = v);
            DrawField("G", ref gText, v => currentColor.g = v);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawField("B", ref bText, v => currentColor.b = v);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", GUILayout.Height(28)))
                Hide(true);

            if (GUILayout.Button("Cancel", GUILayout.Height(28)))
                Hide(false);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawField(string label, ref string text, Action<float> applyComponent)
        {
            GUILayout.BeginVertical();

            GUILayout.Label(label);

            string newVal = GUILayout.TextField(text, GUILayout.Width(70));

            if (newVal != text)
            {
                text = newVal;

                if (int.TryParse(newVal, out int v))
                {
                    v = Mathf.Clamp(v, 0, 255);
                    applyComponent(v / 255f);
                }
            }

            GUILayout.EndVertical();
        }
    }
}
