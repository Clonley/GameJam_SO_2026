#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PointCloudCacher : MonoBehaviour
{
    public string scatterName = "CustomScatter01";
    public LayerMask groundLayer;
    public float maxYDistance = 100.0f;

    [Serializable]
    public class ScatterGroupParams
    {
        [Header("Mesh")]
        public ScatterObject scatterObject;

        [Header("Scatter")]
        public Vector3 bounds = Vector3.zero;
        public uint count = 1000;

        public Vector3 scaleRandomization = Vector3.zero;
        public float uniformScaleRandomization = 0f;

        public Vector3 rotationRandomization = Vector3.zero;
    }
    public List<ScatterGroupParams> scatterGroupsParams = new();

    [Header("BVH")]
    public int maxPointsInNode = 25;

    private ScatterGroup[] scatterGroup;
    private BVHNode[] BVH;
    private uint2[] pointIdx;
    private uint numPoints;

    [ContextMenu("Generate Scatter Data")]
    public void GenerateAndWrite()
    {
        numPoints = 0;
        GenerateScatter();

        ScatterDataHandling.WritePointCloud(scatterGroup, scatterName);

        BVH = LRScatter.BuildBVH(scatterGroup, ref pointIdx, maxPointsInNode);
        ScatterDataHandling.WriteBVH(BVH, scatterName, pointIdx, numPoints);
    }

    public void GenerateScatter()
    {
        int groupCount = scatterGroupsParams.Count;
        scatterGroup = new ScatterGroup[groupCount];

        for (int i = 0; i < groupCount; i++)
        {
            var param = scatterGroupsParams[i];

            scatterGroup[i] = new ScatterGroup();
            uint instanceCount = param.count;

            scatterGroup[i].positions = new Vector3[instanceCount];
            scatterGroup[i].rotations = new Vector4[instanceCount];
            scatterGroup[i].scales = new Vector3[instanceCount];
            scatterGroup[i].scatterObject = param.scatterObject;

            for (int j = 0; j < instanceCount; j++)
            {
                Vector3 pos = RandomVector(param.bounds);
                Vector3 N = Vector3.up;
                RaycastHit hit;

                if (Physics.Raycast(pos + Vector3.up*10f, Vector3.down, out hit, maxYDistance, groundLayer))
                {
                    pos = hit.point;
                    N = hit.normal;
                }
                else if (Physics.Raycast(pos, Vector3.up, out hit, maxYDistance, groundLayer))
                {
                    pos = hit.point;
                    N = hit.normal;
                }
                scatterGroup[i].positions[j] = pos;
                float uniform = 1.0f + UnityEngine.Random.Range(-param.uniformScaleRandomization * 0.5f, param.uniformScaleRandomization * 0.5f);
                Vector3 randScale = RandomVector(param.scaleRandomization);
                scatterGroup[i].scales[j] = new Vector3( uniform * (1 + randScale.x), uniform * (1 + randScale.y), uniform * (1 + randScale.z));

                Quaternion randomRotation = Quaternion.Euler(RandomVector(param.rotationRandomization));
                Quaternion align = Quaternion.FromToRotation(Vector3.up, N);
                Quaternion q = align * randomRotation;
                scatterGroup[i].rotations[j] = new Vector4(q.x, q.y, q.z, q.w);
                numPoints++;
            }
        }
    }

    Vector3 RandomVector(Vector3 bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f),
            UnityEngine.Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f),
            UnityEngine.Random.Range(-bounds.z * 0.5f, bounds.z * 0.5f)
        );
    }
}
#endif