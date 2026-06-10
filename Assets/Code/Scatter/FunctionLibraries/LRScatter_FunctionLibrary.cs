using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class LRScatter
{
    public static BVHNode[] BuildBVH(ScatterGroup[] scatterGroups, ref uint2[] pointIdx, int maxPointsInLeaf)
    {
        pointIdx = new uint2[0];
        uint numPoints = 0;
        List<uint2> pointIdxList = new List<uint2>();
        for (int i = 0; i < scatterGroups.Length; i++)
        {
            if (scatterGroups[i].positions == null) continue;
            for (int j = 0; j < scatterGroups[i].positions.Length; j++)
            {
                pointIdxList.Add(new uint2((uint)i, (uint)j));
                numPoints++;
            }
        }
        pointIdx = pointIdxList.ToArray();
        BVHNode[] BVH = new BVHNode[numPoints * 2 - 1];
        uint nodesUsed = 1;

        ref BVHNode root = ref BVH[0];
        root.leftChild = 0;
        root.firstPoint = 0;
        root.pointCount = (uint)numPoints;

        UpdateNodeBounds(ref BVH, 0, scatterGroups, ref pointIdx);
        Subdivide(ref BVH, 0, scatterGroups, ref pointIdx, maxPointsInLeaf, ref nodesUsed);

        BVHNode[] trimmed = new BVHNode[nodesUsed];
        Array.Copy(BVH, trimmed, nodesUsed);
        BVH = trimmed;

        return BVH;
    }

    public static void UpdateNodeBounds(ref BVHNode[] BVH, uint nodeIdx, ScatterGroup[] scatterGroups, ref uint2[] pointIdx)
    {
        ref BVHNode node = ref BVH[nodeIdx];
        node.min = Vector3.positiveInfinity;
        node.max = Vector3.negativeInfinity;
        for (uint first = node.firstPoint, i = 0; i < node.pointCount; i++)
        {
            Vector3 position = scatterGroups[pointIdx[(int)first + i].x].positions[pointIdx[(int)first + i].y];
            node.min.x = Mathf.Min(position.x, node.min.x);
            node.min.y = Mathf.Min(position.y, node.min.y);
            node.min.z = Mathf.Min(position.z, node.min.z);
            node.max.x = Mathf.Max(position.x, node.max.x);
            node.max.y = Mathf.Max(position.y, node.max.y);
            node.max.z = Mathf.Max(position.z, node.max.z);
        }
    }

    public static void Subdivide(ref BVHNode[] BVH, uint nodeIdx, ScatterGroup[] scatterGroups, ref uint2[] pointIdx, int maxPointsInLeaf, ref uint nodesUsed)
    {
        ref BVHNode node = ref BVH[nodeIdx];
        if (node.pointCount <= maxPointsInLeaf) return;
        Vector3 extent = node.max - node.min;
        int axis = 0;
        if (extent.y > extent.x) axis = 1;
        if (extent.z > extent[axis]) axis = 2;
        float splitPos = node.min[axis] + extent[axis] * 0.5f;
        int i = (int)node.firstPoint;
        int j = i + (int)node.pointCount - 1;
        while (i <= j)
        {
            if (scatterGroups[pointIdx[i].x].positions[pointIdx[i].y][axis] <= splitPos)
            {
                i++;
            }
            else
            {
                (pointIdx[i], pointIdx[j]) = (pointIdx[j], pointIdx[i]);

                j--;
            }
        }
        uint leftCount = (uint)i - node.firstPoint;
        if (leftCount == 0 || leftCount == node.pointCount) return;
        int leftChildIdx = (int)nodesUsed++;
        int rightChildIdx = (int)nodesUsed++;

        BVH[leftChildIdx] = new BVHNode();
        BVH[rightChildIdx] = new BVHNode();
        BVH[leftChildIdx].firstPoint = node.firstPoint;
        BVH[leftChildIdx].pointCount = leftCount;
        BVH[rightChildIdx].firstPoint = (uint)i;
        BVH[rightChildIdx].pointCount = node.pointCount - leftCount;

        node.leftChild = (uint)leftChildIdx;
        node.pointCount = 0;

        UpdateNodeBounds(ref BVH, (uint)leftChildIdx, scatterGroups, ref pointIdx);
        UpdateNodeBounds(ref BVH, (uint)rightChildIdx, scatterGroups, ref pointIdx);

        Subdivide(ref BVH, (uint)leftChildIdx, scatterGroups, ref pointIdx, maxPointsInLeaf, ref nodesUsed);
        Subdivide(ref BVH, (uint)rightChildIdx, scatterGroups, ref pointIdx, maxPointsInLeaf, ref nodesUsed);
    }
}
