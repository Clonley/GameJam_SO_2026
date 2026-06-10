using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScatterObject", menuName = "Scatter/ScatterInstance")]
public class ScatterObject : ScriptableObject
{
    [Header("LOD Settings")]
    public List<ScatterObjectData> LODs = new List<ScatterObjectData>();
}
