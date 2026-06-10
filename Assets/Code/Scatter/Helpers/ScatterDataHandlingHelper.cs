using System;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class ScatterDataHandling
{
#if UNITY_EDITOR

    public static void WritePointCloud(ScatterGroup[] scatterGroups, string scatterName)
    {
        if (scatterGroups == null || scatterGroups.Length == 0) return;
        if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, scatterName))) Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, scatterName));

        string path = Path.Combine(Application.streamingAssetsPath, scatterName + "/PointCloud.scatter");

        using BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536));

        writer.Write(scatterGroups.Length);

        foreach(var scatterGroup in scatterGroups)
        {
            writer.Write(scatterGroup.scatterObject.name);
            Debug.Log(scatterGroup.scatterObject.name);
            writer.Write(scatterGroup.positions.Length);

            for (int j = 0; j < scatterGroup.positions.Length; j++)
            {
                writer.Write(scatterGroup.positions[j].x);
                writer.Write(scatterGroup.positions[j].y);
                writer.Write(scatterGroup.positions[j].z);

                writer.Write(scatterGroup.rotations[j].x);
                writer.Write(scatterGroup.rotations[j].y);
                writer.Write(scatterGroup.rotations[j].z);
                writer.Write(scatterGroup.rotations[j].w);

                writer.Write(scatterGroup.scales[j].x);
                writer.Write(scatterGroup.scales[j].y);
                writer.Write(scatterGroup.scales[j].z);
            }
        }
        EditorApplication.delayCall += () =>
        {
            AssetDatabase.Refresh();
        };
    }

    public static void WriteBVH(BVHNode[] BVH, string scatterName, uint2[] pointIdx, uint idxCount)
    {
        string path = Path.Combine(Application.streamingAssetsPath, scatterName + "/BVH.bvh");
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Debug.LogError("BVHManager || " + scatterName + " does not exist, make sure you initialized a pointcloud.");
        }

        using BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536));

        writer.Write(BVH.Length);

        writer.Write(idxCount);

        for (int i = 0; i < idxCount; i++)
        {
            writer.Write(pointIdx[i].x);
            writer.Write(pointIdx[i].y);
        }

        for (int i = 0; i < BVH.Length; i++)
        {
            BVHNode node = BVH[i];

            writer.Write(node.min.x);
            writer.Write(node.min.y);
            writer.Write(node.min.z);

            writer.Write(node.max.x);
            writer.Write(node.max.y);
            writer.Write(node.max.z);

            writer.Write(node.leftChild);
            writer.Write(node.firstPoint);
            writer.Write(node.pointCount);
        }

        EditorApplication.delayCall += () =>
        {
            AssetDatabase.Refresh();
        };
    }
#endif

    #region Write
    public static BVHNode[] ReadBVH(string scatterName, out uint2[] pointIdx)
    {
        BVHNode[] BVH;
        string path = Path.Combine(Application.streamingAssetsPath, scatterName + "/BVH.bvh");
        if (!File.Exists(path))
        {
            Debug.LogError($"BVH file not found: {path}");
            pointIdx = Array.Empty<uint2>();
            return Array.Empty<BVHNode>();
        }
        using BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan));

        int nodeCount = reader.ReadInt32();
        BVH = new BVHNode[nodeCount];

        uint idxCount = reader.ReadUInt32();
        pointIdx = new uint2[idxCount];

        for (int i = 0; i < idxCount; i++)
        {
            pointIdx[i].x = (uint)reader.ReadInt32();
            pointIdx[i].y = (uint)reader.ReadInt32();
        }

        for (int i = 0; i < nodeCount; i++)
        {

            BVH[i].min.x = reader.ReadSingle();
            BVH[i].min.y = reader.ReadSingle();
            BVH[i].min.z = reader.ReadSingle();

            BVH[i].max.x = reader.ReadSingle();
            BVH[i].max.y = reader.ReadSingle();
            BVH[i].max.z = reader.ReadSingle();

            BVH[i].leftChild = reader.ReadUInt32();
            BVH[i].firstPoint = reader.ReadUInt32();
            BVH[i].pointCount = reader.ReadUInt32();
        }

        return BVH;
    }

    public static ScatterGroup[] ReadPointCloud(string scatterName)
    {
        if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, scatterName))) Debug.LogError($"Scatter data for {scatterName} does not exist");
        string path = Path.Combine(Application.streamingAssetsPath, scatterName + "/PointCloud.scatter");

        using BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan));

        int groupCount = reader.ReadInt32();
        if (groupCount <= 0)
        {
            Debug.LogWarning("Scatter file contains no scatter groups");
            return Array.Empty<ScatterGroup>();
        }

        ScatterGroup[] scatterGroup = new ScatterGroup[groupCount];

        for (int i = 0; i < groupCount; i++)
        {
            string scatterObjectName = reader.ReadString();
            int instanceCount = reader.ReadInt32();

            scatterGroup[i] = new ScatterGroup();
            scatterGroup[i].scatterObject = Resources.Load<ScatterObject>(scatterObjectName);
            scatterGroup[i].positions = new Vector3[instanceCount];
            scatterGroup[i].rotations = new Vector4[instanceCount];
            scatterGroup[i].scales = new Vector3[instanceCount];

            for (int j = 0; j < instanceCount; j++)
            {
                scatterGroup[i].positions[j].x = reader.ReadSingle();
                scatterGroup[i].positions[j].y = reader.ReadSingle();
                scatterGroup[i].positions[j].z = reader.ReadSingle();

                scatterGroup[i].rotations[j].x = reader.ReadSingle();
                scatterGroup[i].rotations[j].y = reader.ReadSingle();
                scatterGroup[i].rotations[j].z = reader.ReadSingle();
                scatterGroup[i].rotations[j].w = reader.ReadSingle();

                scatterGroup[i].scales[j].x = reader.ReadSingle();
                scatterGroup[i].scales[j].y = reader.ReadSingle();
                scatterGroup[i].scales[j].z = reader.ReadSingle();
            }
        }
        return scatterGroup;
    }

    public static ScatterGroup[] ReadHoudiniPointCloud(string scatterName)
    {
        string path = Path.Combine(Application.streamingAssetsPath + "/HoudiniAssets/", scatterName + ".HouPC");

        using BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan));
        Debug.Log(path);
        int groupCount = reader.ReadInt32();
        if (groupCount <= 0)
        {
            Debug.LogWarning("Scatter file contains no scatter groups");
            return Array.Empty<ScatterGroup>();
        }

        ScatterGroup[] scatterGroup = new ScatterGroup[groupCount];

        for (int i = 0; i < groupCount; i++)
        {
            int instanceCount = reader.ReadInt32();

            scatterGroup[i] = new ScatterGroup();
            scatterGroup[i].positions = new Vector3[instanceCount];
            scatterGroup[i].rotations = new Vector4[instanceCount];
            scatterGroup[i].scales = new Vector3[instanceCount];

            for (int j = 0; j < instanceCount; j++)
            {
                scatterGroup[i].positions[j].x = reader.ReadSingle();
                scatterGroup[i].positions[j].y = reader.ReadSingle();
                scatterGroup[i].positions[j].z = reader.ReadSingle();

                scatterGroup[i].rotations[j].x = reader.ReadSingle();
                scatterGroup[i].rotations[j].y = reader.ReadSingle();
                scatterGroup[i].rotations[j].z = reader.ReadSingle();
                scatterGroup[i].rotations[j].w = reader.ReadSingle();
                float scale = reader.ReadSingle();
                scatterGroup[i].scales[j] = Vector3.one * scale;
            }
        }
        return scatterGroup;

    }
    #endregion
}