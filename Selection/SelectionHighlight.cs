using UnityEngine;

namespace BuildingPlus.Selection
{
    internal class SelectionHighlight : MonoBehaviour
    {
        private GameObject root;
        private LineRenderer outline;
        private GameObject fill;

        private Placeable placeable;

        private Bounds bounds;

        void Awake()
        {
            if (root != null) return;

            CreateVisuals();
            placeable = GetComponent<Placeable>();
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

            //BuildingPlusPlugin.LogInfo("solidcolliders for " + gameObject.name + ": " + colliders.Count);

            foreach (var collider in colliders)
            {
                Bounds bounds = collider.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;

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

                var mat = new Material(Shader.Find("Sprites/Default"));
                mr.material = mat;

                mat.color = new Color(0.2f, 0.5f, 1f, 0.65f);
                mr.sortingLayerName = "UI 1";
                mr.sortingOrder = 13000;
                mat.SetInt("_ZWrite", 0);
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            }
        }

        public void RefreshBounds()
        {
            if (root == null) return;

            foreach (Transform child in root.transform)
            {
                var quad = child.gameObject;

                var colliders = SelectionCheckCollision.GetCollidersOf(placeable, "SolidCollider");
                if (colliders.Count == 0)
                    colliders = SelectionCheckCollision.GetCollidersOf(placeable, "PlacementCollider");

                if (colliders.Count == 0)
                    continue;

                var b = colliders[0].bounds;

                quad.transform.position = b.center + new Vector3(0, 0, -0.01f);

                quad.transform.localScale = new Vector3(b.size.x, b.size.y, 1f);
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
