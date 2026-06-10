using UnityEngine;
using UnityEditor;

public class ScatterSpawner
{
    [MenuItem("GameObject/Scatter/Spawn Scatter Object", false, 10)]
    static void SpawnScatterObject(MenuCommand menuCommand)
    {
        GameObject scatterGO = new GameObject("Scatter Object");
        ScatterObjectVisualizer visualizer = scatterGO.AddComponent<ScatterObjectVisualizer>();

        GameObject parent = menuCommand.context as GameObject;
        if (parent != null)
        {
            Undo.SetTransformParent(scatterGO.transform, parent.transform, "Parent Scatter Object");
        }

        Undo.RegisterCreatedObjectUndo(scatterGO, "Create Scatter Object");
        Selection.activeObject = scatterGO;
    }
}
