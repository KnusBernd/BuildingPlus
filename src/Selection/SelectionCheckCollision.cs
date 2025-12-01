using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace BuildingPlus.Selection
{
    internal class SelectionCheckCollision
    {
        public static List<string> IgnoringPlacements = new List<string>() { "Ceiling", "LeftWall", "RightWall", "DeathPit" };

        internal static HashSet<Placeable> checkCollision(Bounds bounds) 
        {
            HashSet<Placeable> placeables = new HashSet<Placeable>();

            foreach (Placeable p in Placeable.AllPlaceables) 
            {
                if (IgnoringPlacements.Contains(p.name)) continue;

                if (p.name.Contains("SpinningDeath"))
                {
                    Vector2 start = new Vector2(p.transform.position.x, p.transform.position.y);
                    Vector2 end = start + new Vector2(1f, 1f);

                    Vector2 size = end - start;
                    Vector2 center = start + size / 2f;

                    Bounds b = new Bounds(center, size);

                    if (b.Intersects(bounds))
                    {
                        placeables.Add(p);
                    }
                    continue;
                }

                var cols = GetCollidersOf(p, "SolidCollider");
                    cols.AddRange(GetCollidersOf(p, "PlacementCollider"));
                bool colision = false;

                foreach (var col in cols)
                {
                    if (col == null)
                        continue;

                    if (!col.bounds.Intersects(bounds))
                        continue;
                    colision = true;
                    break;
                }

                if (colision)
                placeables.Add(p);
            }

            return placeables;
        }

        internal static List<BoxCollider2D> GetCollidersOf(Placeable placeable, string childName) 
        { 
            List<BoxCollider2D> boxCollider2Ds = new List<BoxCollider2D>();
            if ((placeable.Category == Placeable.PieceCategory.PLATFORM || placeable.Category == Placeable.PieceCategory.MOVINGPLATFORM) && childName == "SolidCollider") 
            {
                BoxCollider2D[] boxCollider2DArray = placeable.GetComponentsInChildren<BoxCollider2D>();
                foreach (var boxCollider2D in boxCollider2DArray) 
                { 
                    if (boxCollider2D.name == childName || boxCollider2D.name == "PlacedSolidCollider") 
                    {
                        boxCollider2Ds.Add(boxCollider2D);
                    }
                }
            }
            else 
            { 
                for (int i = 0; i < placeable.transform.childCount; i++) 
                {
                    Transform childTransform = placeable.transform.GetChild(i);

                    if (childTransform.name == childName)
                    {
                        //BuildingPlusPlugin.LogInfo(placeable.name + " get col " + childTransform.name + " adding ");
                        boxCollider2Ds.Add(childTransform.GetComponent<BoxCollider2D>());
                    }
                }
            }
            return boxCollider2Ds;
        }
    }
}
