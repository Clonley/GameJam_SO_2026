#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class ScatterObjectVisualizer : MonoBehaviour
{
    [Header("LOD Preview")]
    [Tooltip("Preview distances for each LOD in the Inspector")]

    public ScatterObject scatterObject;
    public bool showDebugText = true;
    public List<ScatterObjectData> LODs = new List<ScatterObjectData>();

    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5];

    private ComputeBuffer visibleIDBuffer;
    private ComputeBuffer positionBuffer;
    private Vector3 position;
    private ComputeBuffer rotationBuffer;
    private Vector3 rotation;
    private ComputeBuffer scaleBuffer;
    private Vector3 scale;

    private Mesh mesh;
    private Material material;

    private CombineInstance[] instance;

    private int selectedLOD = -1;

    void OnEnable()
    {
        if (argsBuffer == null)
        {
            argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        if (positionBuffer == null)
        {
            positionBuffer = new ComputeBuffer(1, sizeof(float) * 3);
            rotationBuffer = new ComputeBuffer(1, sizeof(float) * 4);
            scaleBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        }

        if (visibleIDBuffer == null)
        {
            visibleIDBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Default);
            visibleIDBuffer.SetData(new uint[] { 0 });
        }

        args[1] = 1;
        args[4] = 0;

        argsBuffer.SetData(args);
    }

    private void OnValidate()
    {
        if (scatterObject == null) return;
        if(LODs.Count == 0) LODs = scatterObject.LODs;
        scatterObject.LODs = LODs;
        instance = new CombineInstance[LODs.Count];
        for(int i = 0; i < LODs.Count; i++)
        {
            instance[i] = new CombineInstance { mesh = LODs[i].lod };
        }
        mesh = new Mesh();
        mesh.CombineMeshes(instance, false, false, false);
    }

    void OnDisable()
    {
        argsBuffer?.Release();
        argsBuffer = null;
        positionBuffer?.Release();
        positionBuffer = null;
        visibleIDBuffer?.Release();
        visibleIDBuffer = null;
    }

    void Update()
    {
        if (scatterObject == null || scatterObject.LODs == null) return;

        position = Vector3.zero;
        positionBuffer.SetData(new Vector3[] { position });
        Quaternion qrot = transform.rotation;
        rotation = new Vector4 (qrot.x, qrot.y, qrot.z);
        rotationBuffer.SetData(new Vector4[] { rotation });
        scale = transform.lossyScale;
        scaleBuffer.SetData(new Vector3[] { scale });

        Camera sceneCam = SceneView.lastActiveSceneView?.camera;
        if (sceneCam == null) return;
        float distanceToCamera = Vector3.Distance(transform.position, sceneCam.transform.position);
        float maxDistance = scatterObject.LODs[scatterObject.LODs.Count - 1].distance;
        if (distanceToCamera > maxDistance) return;

        selectedLOD = -1;
        for (int i = 0; i < scatterObject.LODs.Count; i++)
        {
            if (distanceToCamera <= scatterObject.LODs[i].distance)
            {
                selectedLOD = i;
                break;
            }
        }
        if (selectedLOD == -1) return;

        selectedLOD = Mathf.Clamp(selectedLOD, 0, scatterObject.LODs.Count - 1);

        if (mesh != scatterObject.LODs[selectedLOD].lod) ChangeLOD(selectedLOD);

        material.SetBuffer("_positions", positionBuffer);
        material.SetBuffer("_rotations", rotationBuffer);
        material.SetBuffer("_scales", scaleBuffer);
        material.SetBuffer("_visibleIDs", visibleIDBuffer);

        Graphics.DrawMeshInstancedIndirect(mesh, selectedLOD, material, new Bounds (transform.position, Vector3.one*5), argsBuffer);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugText || scatterObject == null || LODs.Count == 0 ) return;

        Camera sceneCam = SceneView.lastActiveSceneView?.camera;

        float distanceToCamera = Vector3.Distance(transform.position, sceneCam.transform.position);

        int lodForLabel = -1;
        for (int i = 0; i < LODs.Count; i++)
        {
            if (distanceToCamera <= LODs[i].distance)
            {
                lodForLabel = i;
                break;
            }
        }

        float labelHeight = (mesh != null) ? mesh.bounds.max.y + 0.1f : 1f;
        Vector3 labelPos = transform.position + Vector3.up * labelHeight;

        Handles.Label(labelPos, lodForLabel == -1 ? "Culled" : $"LOD {lodForLabel}", EditorStyles.boldLabel);
    }

    void ChangeLOD(int selectedLOD)
    {
        if (selectedLOD < 0) return;
        material = new Material(scatterObject.LODs[selectedLOD].material);

        args[0] = mesh.GetIndexCount(selectedLOD);
        args[2] = mesh.GetIndexStart(selectedLOD);
        args[3] = mesh.GetBaseVertex(selectedLOD);
        argsBuffer.SetData(args);
    }
}
#endif