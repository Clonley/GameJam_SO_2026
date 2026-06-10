#if UNITY_EDITOR
using TriInspector;
using Unity.Mathematics;
using UnityEngine;

namespace EscapeVelocity
{
    public class InitializeHoudiniScatter : MonoBehaviour
    {
        public string scatterName;

        public ScatterObject[] scatterObjects;
        public int maxPointsInNode = 25;
        public bool visualizeBVH = false;

        [Button("Refresh")]
        private void RefreshButton()
        {
            Refresh();
        }

        private ScatterGroup[] scatterGroups;
        private uint2[] pointIdx;
        BVHNode[] BVH;
        private uint numPoints;
        void OnValidate()
        {
            Refresh();
        }

        private void Refresh()
        {
            scatterGroups = ScatterDataHandling.ReadHoudiniPointCloud(scatterName);

            // Resize scatterObjects safely
            if (scatterObjects == null || scatterObjects.Length != scatterGroups.Length)
            {
                ScatterObject[] newArray = new ScatterObject[scatterGroups.Length];
                if (scatterObjects != null)
                {
                    int copyCount = Mathf.Min(scatterObjects.Length, scatterGroups.Length);
                    for (int i = 0; i < copyCount; i++)
                        newArray[i] = scatterObjects[i];
                }
                scatterObjects = newArray;
            }

            // Assign scatterObjects to groups
            for (int i = 0; i < scatterGroups.Length; i++)
                scatterGroups[i].scatterObject = scatterObjects[i];

            // Initialize pointIdx array
            numPoints = 0;
            for (int i = 0; i < scatterGroups.Length; i++)
                numPoints += (uint)scatterGroups[i].positions.Length;

            pointIdx = new uint2[numPoints];

            numPoints = 0;
            for (int i = 0; i < scatterGroups.Length; i++)
            {
                var group = scatterGroups[i];
                for (int j = 0; j < group.positions.Length; j++)
                {
                    pointIdx[numPoints] = new uint2((uint)i, (uint)j);
                    numPoints++;
                }
            }

            ScatterDataHandling.WritePointCloud(scatterGroups, scatterName);
            BVH = LRScatter.BuildBVH(scatterGroups, ref pointIdx, maxPointsInNode);
            ScatterDataHandling.WriteBVH(BVH, scatterName, pointIdx, numPoints);

            Debug.Log($"ScatterGroups: {scatterGroups.Length}, Total Points: {numPoints}");
        }

        private void OnDrawGizmos()
        {
            for (uint i = 0; i < BVH.Length; i++)
            {
                DrawLeafNodes(i);
            }
        }
        void DrawLeafNodes(uint nodeIdx)
        {
            if (!visualizeBVH) return;
            ref BVHNode node = ref BVH[nodeIdx];

            if (node.pointCount > 0)
            {
                Vector3 center = (node.min + node.max) * 0.5f;
                Vector3 size = node.max - node.min;

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, size);
                return;
            }
            uint left = node.leftChild;
            uint right = node.leftChild + 1;

            DrawLeafNodes(left);
            DrawLeafNodes(right);
        }
    }
}
#endif