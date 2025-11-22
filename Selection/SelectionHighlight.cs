using UnityEngine;

namespace BuildingPlus.Selection
{
    internal class SelectionHighlight : MonoBehaviour
    {
        private GameObject root;
        private LineRenderer outline;
        private GameObject fill;

        // --- Static shared materials ---
        private static Material sharedOutlineMaterial;
        private static Material sharedFillMaterial;

        private Bounds bounds;

        void Awake()
        {
            EnsureSharedMaterials();
            CreateVisuals();
        }

        private void EnsureSharedMaterials()
        {
            if (sharedOutlineMaterial == null)
            {
                sharedOutlineMaterial = new Material(Shader.Find("Sprites/Default"));
                sharedOutlineMaterial.color = new Color(0.2f, 0.5f, 1f, 0.65f);
                sharedOutlineMaterial.renderQueue = 5000;
            }

            if (sharedFillMaterial == null)
            {
                sharedFillMaterial = new Material(Shader.Find("Sprites/Default"));
                sharedFillMaterial.color = new Color(0.2f, 0.5f, 1f, 0.45f);
                sharedFillMaterial.renderQueue = 5000;
                sharedFillMaterial.SetInt("_ZWrite", 0);
                sharedFillMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
        }

        private void CreateVisuals()
        {
            // Root
            root = new GameObject("HighlightRoot");
            root.transform.SetParent(transform, false);
            root.SetActive(false);

            var colliders = SelectionCheckCollision.GetCollidersOf(transform.GetComponent<Placeable>(), "SolidCollider");
            if (colliders.Count == 0)
            colliders = SelectionCheckCollision.GetCollidersOf(transform.GetComponent<Placeable>(), "PlacementCollider");

            BuildingPlusPlugin.LogInfo("solidcolliders for " + gameObject.name + ": " + colliders.Count);

            foreach (var collider in colliders)
            {
                Bounds bounds = collider.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;

                /*/ --- OUTLINE ---
                GameObject outlineObj = new GameObject("Outline_" + collider.name);
                outlineObj.transform.SetParent(root.transform, false);

                LineRenderer lr = outlineObj.AddComponent<LineRenderer>();
                lr.sortingLayerName = "UI";
                lr.sortingOrder = 32767;
                lr.positionCount = 5;
                lr.loop = false;
                lr.material = sharedOutlineMaterial;
                lr.startColor = lr.endColor = new Color(0.2f, 0.5f, 1f, 0.65f); // slightly stronger alpha for outline
                lr.startWidth = lr.endWidth = 0.05f;
                lr.sortingOrder = 32767;

                Vector3[] corners = new Vector3[5];
                corners[0] = new Vector3(min.x, min.y, 0);
                corners[1] = new Vector3(min.x, max.y, 0);
                corners[2] = new Vector3(max.x, max.y, 0);
                corners[3] = new Vector3(max.x, min.y, 0);
                corners[4] = corners[0];
                lr.SetPositions(corners);
                
                // Store the LineRenderer on the collider for later refresh
                // -------------------------
                // Fill
                // -------------------------*/
                GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fillObj.transform.SetParent(root.transform, false);

                fillObj.name = "Fill_" + collider.name;
                fillObj.transform.position = bounds.center;
                fillObj.transform.localRotation = transform.localRotation;
                fillObj.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);

                // Remove collider
                GameObject.Destroy(fillObj.GetComponent<Collider>());
                GameObject.Destroy(fillObj.GetComponent<BoxCollider>());

                MeshRenderer mr = fillObj.GetComponent<MeshRenderer>();

                // Create unique material instance FOR THIS quad
                var mat = new Material(Shader.Find("Sprites/Default"));
                mr.material = mat;

                mat.color = new Color(0.2f, 0.5f, 1f, 0.45f);
                mr.sortingLayerName = "UI 1";
                mat.renderQueue = 5000; // always on top
                mat.SetInt("_ZWrite", 0);
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            }


        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }
    }
}
