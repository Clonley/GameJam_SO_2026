#if UNITY_EDITOR
using TriInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[ExecuteAlways]
public class ScatterInitiatorEditor : MonoBehaviour
{
    public string scatterName = "CustomScatter01";
    public uint randomSamples = 10;
    public float distanceFalloff = 1.0f;
    public float frustrumPadding = 0.05f;
    public int lodOffset = 0;
    public bool enablePreview = false;

    public bool drawInstanceGizmos = false;

    [Button("Refresh")]
    private void RefreshButton()
    {
        Refresh();
    }

    private BVHNode[] BVH;
    private uint2[] pointIdx;
    private ScatterGroup[] scatterGroups;
    private Bounds drawBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(100, 100, 100));

    private float[] randomFloats;
    private float bvhCullingdistance;

    //Camera setup
    private Camera cam;
    private Vector3 camPos;
    private Plane[] planes = new Plane[6];

    uint[][] visibleCounts;
    uint[][][] visibleIDs;

    private Material[][] material;
    private Mesh[][] mesh;

    uint[][][] args;
    ComputeBuffer[][] argsBuffer;
    ComputeBuffer[][] visibleIDBuffer;
    private ComputeBuffer[] positionBuffer;
    private ComputeBuffer[] rotationBuffer;
    private ComputeBuffer[] scaleBuffer;

    private void OnValidate()
    {
        Refresh();
    }

    private void Refresh()
    {
        BVH = ScatterDataHandling.ReadBVH(scatterName, out pointIdx);
        scatterGroups = ScatterDataHandling.ReadPointCloud(scatterName);

        randomFloats = new float[randomSamples];
        for (int i = 0; i < randomSamples; i++)
        {
            randomFloats[i] = UnityEngine.Random.Range(0.0f, 1.0f);
        }

        if (BVH == null || scatterGroups == null)
        {
            Debug.LogError("No BVH or scatter groups found");
            return;
        }
        if (!enablePreview) return;
        visibleCounts = new uint[scatterGroups.Length][];
        visibleIDs = new uint[scatterGroups.Length][][];
        material = new Material[scatterGroups.Length][];
        mesh = new Mesh[scatterGroups.Length][];
        Bounds tempBound = new Bounds(new Vector3 (0, 0, 0), new Vector3(Mathf.Max(Mathf.Abs(BVH[0].max.x), Mathf.Abs(BVH[0].min.x)), Mathf.Max(Mathf.Abs(BVH[0].max.y), Mathf.Abs(BVH[0].min.y)), Mathf.Max(Mathf.Abs(BVH[0].max.z), Mathf.Abs(BVH[0].min.z))));
        drawBounds= tempBound;
        for (int i = 0; i < scatterGroups.Length; i++)
        {

            var group = scatterGroups[i];
            ScatterObject scatterObject = group.scatterObject;
            Debug.Log(scatterGroups[i].scatterObject);
            Debug.Log(scatterGroups[i].scatterObject == null);
            Debug.Log(scatterObject.LODs == null);

            visibleCounts[i] = new uint[scatterObject.LODs.Count];
            visibleIDs[i] = new uint[scatterObject.LODs.Count][];
            material[i] = new Material[scatterObject.LODs.Count];
            mesh[i] = new Mesh[scatterObject.LODs.Count];

            for (int j = 0; j < scatterObject.LODs.Count; j++)
            {
                visibleCounts[i][j] = 0;
                visibleIDs[i][j] = new uint[group.positions.Length];
                material[i][j] = Instantiate(scatterObject.LODs[j].material);
                material[i][j].enableInstancing = true;
                mesh[i][j] = scatterObject.LODs[j].lod;
                bvhCullingdistance = Mathf.Max(bvhCullingdistance, scatterObject.LODs[j].distance);
            }
        }
        InitializeBuffers();
    }

    private void ReleaseBuffers()
    {
        for (int i = 0; i < scatterGroups.Length; i++)
        {
            positionBuffer[i]?.Release();
            positionBuffer[i] = null;
            rotationBuffer[i]?.Release();
            rotationBuffer[i] = null;
            scaleBuffer[i]?.Release();
            scaleBuffer[i] = null;

            for (int j = 0; j < scatterGroups[i].scatterObject.LODs.Count; j++)
            {
                argsBuffer[i][j]?.Release();
                argsBuffer[i][j] = null;
                visibleIDBuffer[i][j]?.Release();
                visibleIDBuffer[i][j] = null;
}
        }
    }

    private void InitializeBuffers()
    {
        args = new uint[scatterGroups.Length][][];
        argsBuffer = new ComputeBuffer[scatterGroups.Length][];
        visibleIDBuffer = new ComputeBuffer[scatterGroups.Length][];
        positionBuffer = new ComputeBuffer[scatterGroups.Length];
        rotationBuffer = new ComputeBuffer[scatterGroups.Length];
        scaleBuffer = new ComputeBuffer[scatterGroups.Length];

        for (int i = 0; i < scatterGroups.Length; i++)
        {
            var group = scatterGroups[i];
            int lodCount = group.scatterObject.LODs.Count;

            args[i] = new uint[lodCount][];
            argsBuffer[i] = new ComputeBuffer[lodCount];
            visibleIDBuffer[i] = new ComputeBuffer[lodCount];

            positionBuffer[i] = new ComputeBuffer(group.positions.Length, 3 * sizeof(float));
            positionBuffer[i].SetData(group.positions);
            rotationBuffer[i] = new ComputeBuffer(group.rotations.Length, 4 * sizeof(float));
            rotationBuffer[i].SetData(group.rotations);
            scaleBuffer[i] = new ComputeBuffer(group.scales.Length, 3 * sizeof(float));
            scaleBuffer[i].SetData(group.scales);

            for (int j = 0; j < lodCount; j++)
            {
                args[i][j] = new uint[5];
                args[i][j][0] = mesh[i][j].GetIndexCount(0);
                args[i][j][1] = (uint) scatterGroups[i].positions.Length;
                args[i][j][2] = mesh[i][j].GetIndexStart(0);
                args[i][j][3] = mesh[i][j].GetBaseVertex(0);
                args[i][j][4] = 0;

                argsBuffer[i][j] = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                argsBuffer[i][j].SetData(args[i][j]);

                visibleIDBuffer[i][j] = new ComputeBuffer(scatterGroups[i].positions.Length, sizeof(uint));
                visibleIDBuffer[i][j].SetData(visibleIDs[i][j]);

                material[i][j].SetBuffer("_visibleIDs", visibleIDBuffer[i][j]);
                material[i][j].SetBuffer("_positions", positionBuffer[i]);
                material[i][j].SetBuffer("_rotations", rotationBuffer[i]);
                material[i][j].SetBuffer("_scales", scaleBuffer[i]);
            }
        }
    }

    private void DrawInstances()
    {
        for (int i = 0; i < scatterGroups.Length; i++)
        {
            for(int j = 0; j < scatterGroups[i].scatterObject.LODs.Count; j++)
            {
                if(visibleCounts[i][j] <= 0) continue;

                uint count = visibleCounts[i][j];

                args[i][j][1] = count;
                argsBuffer[i][j].SetData(args[i][j]);

                visibleIDBuffer[i][j].SetData(visibleIDs[i][j], 0, 0, (int)count);

                Graphics.DrawMeshInstancedIndirect(mesh[i][j], 0, material[i][j], drawBounds, argsBuffer[i][j]);
            }
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        SceneView.RepaintAll();
        if (!enablePreview) return;
        cam = sceneView.camera;
        UpdateVisibleInstances(cam);
        DrawInstances();
    }
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ReleaseBuffers();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ReleaseBuffers();
    }

    void UpdateVisibleInstances(Camera cam)
    {
        GeometryUtility.CalculateFrustumPlanes(cam, planes);
        camPos = cam.transform.position;

        for (int i = 0; i < visibleCounts.Length; i++)
        {
            for (int j = 0; j < visibleCounts[i].Length; j++)
                visibleCounts[i][j] = 0;
        }

        IntersectBVH(planes, 0);
    }

    bool IntersectAABB(Plane[] planes, Vector3 min, Vector3 max)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 normal = planes[i].normal;

            Vector3 p = new Vector3(
                normal.x >= 0 ? max.x : min.x,
                normal.y >= 0 ? max.y : min.y,
                normal.z >= 0 ? max.z : min.z
            );

            if (Vector3.Dot(normal, p) + planes[i].distance + frustrumPadding < 0) return false;
        }
        Vector3 center = (min + max) * 0.5f;
        Vector3 halfExtent = (max - min) * 0.5f;
        Vector3 toCam = camPos - center;
        Vector3 clamped = new Vector3(
            Mathf.Max(-halfExtent.x, Mathf.Min(toCam.x, halfExtent.x)),
            Mathf.Max(-halfExtent.y, Mathf.Min(toCam.y, halfExtent.y)),
            Mathf.Max(-halfExtent.z, Mathf.Min(toCam.z, halfExtent.z)));
        float distSq = (camPos - (center + clamped)).sqrMagnitude;
        if (distSq > bvhCullingdistance * bvhCullingdistance) return false;
        return true;
    }

    bool IntersectPoint(Plane[] planes, Vector3 position, uint pointIdx)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 normal = planes[i].normal;

            if (Vector3.Dot(normal, position) + planes[i].distance + frustrumPadding < 0) return false;
        }
        return true;
    }

    void IntersectBVH(Plane[] planes, uint nodeIdx)
    {
        BVHNode node = BVH[nodeIdx];
        if (!IntersectAABB(planes, node.min, node.max)) return;
        if(node.pointCount>0)
        {
            for (uint i = node.firstPoint; i < node.firstPoint + node.pointCount; i++)
            {
                uint2 idx = pointIdx[i];
                uint groupID = idx.x;
                uint localID = idx.y;

                int lodCount = scatterGroups[groupID].scatterObject.LODs.Count;
                uint lod = (uint)(lodCount);

                Vector3 pos = scatterGroups[groupID].positions[localID];

                if (!IntersectPoint(planes, pos, i)) continue;

                float dist = (pos - camPos).sqrMagnitude;

                for (int j = lodOffset; j < lodCount; j++)
                {
                    float maxDist = scatterGroups[groupID].scatterObject.LODs[j].distance + (randomFloats[(i + j) % randomSamples] - 0.5f) * distanceFalloff;
                    float maxDistSq = maxDist * maxDist;

                    if (dist <= maxDistSq)
                    {
                        lod = (uint)j;
                        break;
                    }
                }
                if (lod == lodCount) continue;

                uint count = visibleCounts[groupID][lod];

                if (count < visibleIDs[groupID][lod].Length)
                {
                    visibleIDs[groupID][lod][count] = localID;
                    visibleCounts[groupID][lod]++;
                }
            }
        }
        else
        {
            IntersectBVH(planes, node.leftChild);
            IntersectBVH(planes, node.leftChild + 1);
        }
    }

    void OnDrawGizmos()
    {
        DrawPoints();
    }

    void DrawPoints()
    {
        if (!drawInstanceGizmos) return;
        Gizmos.color = Color.red;

        for (int i = 0; i < scatterGroups.Length; i++)
        {
            for (int j = 0; j < scatterGroups[i].positions.Length; j++)
            {
                Gizmos.DrawSphere(scatterGroups[i].positions[j], 0.1f);
            }
        }
    }
}
#endif