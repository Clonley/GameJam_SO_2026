using UnityEngine;

public struct BVHNode
{
    public Vector3 min, max;
    public uint leftChild;
    public uint firstPoint, pointCount;
}

public class Instance
{
    public Vector3 position;
    public Vector4 rotation;
    public Vector3 scale;

    public uint scatterGroupID;
}
[System.Serializable]
public class ScatterGroup
{
    [HideInInspector] public Vector3[] positions;
    [HideInInspector] public Vector4[] rotations;
    [HideInInspector] public Vector3[] scales;

    [SerializeField] public ScatterObject scatterObject;
}

public struct PropertyLibrary
{
    public Mesh[] meshes;
    public Material[] materials;
}

[System.Serializable]
public class ScatterObjectData
{
    public Mesh lod;
    public Material material;
    public float distance = 50f;
}
