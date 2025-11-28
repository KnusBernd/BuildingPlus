using UnityEngine;

namespace BuildingPlus.Selection
{
    public class SelectorUI
    {
        private GameObject selectionOutlineRoot;
        private LineRenderer outlineRendererVisual;
        private GameObject selectionFill;
        private Material fillMaterial;

        private Transform parent;

        public SelectorUI(Transform parent)
        {
            this.parent = parent;
            CreateSelectionVisuals();
        }

        public void ShowOutline(bool visible)
        {
            if (selectionOutlineRoot != null)
                selectionOutlineRoot.SetActive(visible);
        }

        public void UpdateBox(Vector3 a, Vector3 b)
        {
            Vector3 min = Vector3.Min(a, b);
            Vector3 max = Vector3.Max(a, b);

            Vector3[] corners = new Vector3[5];
            corners[0] = new Vector3(min.x, min.y);
            corners[1] = new Vector3(min.x, max.y);
            corners[2] = new Vector3(max.x, max.y);
            corners[3] = new Vector3(max.x, min.y);
            corners[4] = corners[0];

            outlineRendererVisual.SetPositions(corners);

            selectionFill.transform.position = (min + max) * 0.5f;
            selectionFill.transform.localScale = new Vector3(max.x - min.x, max.y - min.y, 1f);
        }

        private void CreateSelectionVisuals()
        {
            selectionOutlineRoot = new GameObject("SelectionOutlineRoot");
            selectionOutlineRoot.transform.SetParent(parent);
            selectionOutlineRoot.SetActive(false);

            // Outline
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(selectionOutlineRoot.transform);

            outlineRendererVisual = outlineObj.AddComponent<LineRenderer>();
            outlineRendererVisual.material = new Material(Shader.Find("Sprites/Default"));
            outlineRendererVisual.startColor = outlineRendererVisual.endColor = new Color(0.2f, 0.5f, 1f, 0.65f);
            outlineRendererVisual.startWidth = outlineRendererVisual.endWidth = 0.05f;
            outlineRendererVisual.loop = false;
            outlineRendererVisual.positionCount = 5;
            outlineRendererVisual.sortingLayerName = "UI";
            outlineRendererVisual.sortingOrder = 32767;

            // Fill
            selectionFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject.Destroy(selectionFill.GetComponent<Collider>());
            selectionFill.transform.SetParent(selectionOutlineRoot.transform);

            fillMaterial = new Material(Shader.Find("Sprites/Default"));
            fillMaterial.color = new Color(0.2f, 0.5f, 1f, 0.35f);

            var fillRenderer = selectionFill.GetComponent<MeshRenderer>();
            fillRenderer.material = fillMaterial;
            fillRenderer.sortingLayerName = "UI";
            fillRenderer.sortingOrder = 32766;
        }
    }
}
