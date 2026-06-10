#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScatterObjectVisualizer))]
public class ScatterObjectVisualizerEditor : Editor
{
    private Color[] lodColors;

    private void OnEnable()
    {
        // Ensure bar updates whenever the scene view camera moves
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        var visualizer = (ScatterObjectVisualizer)target;

        // Draw default inspector
        DrawDefaultInspector();

        // Only draw the LOD bar if we have data
        if (visualizer.scatterObject != null && visualizer.LODs != null && visualizer.LODs.Count > 0)
        {
            InitColors(visualizer);

            DrawLODBar(visualizer);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Force inspector repaint when SceneView moves, so the highlight updates
        Repaint();
    }

    private void InitColors(ScatterObjectVisualizer visualizer)
    {
        int lodCount = visualizer.LODs.Count;
        if (lodColors == null || lodColors.Length != lodCount)
        {
            lodColors = new Color[lodCount];
            for (int i = 0; i < lodCount; i++)
                lodColors[i] = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
        }
    }

    private void DrawLODBar(ScatterObjectVisualizer visualizer)
    {
        var LODs = visualizer.LODs;
        float totalDistance = LODs[LODs.Count - 1].distance;

        Rect rect = GUILayoutUtility.GetRect(200, 24);
        float lastX = rect.x;

        // Determine which LOD is active for the SceneView camera
        Camera sceneCam = SceneView.lastActiveSceneView?.camera;
        float distanceToCamera = (sceneCam != null) ? Vector3.Distance(visualizer.transform.position, sceneCam.transform.position) : -1f;
        int activeLOD = -1;
        if (distanceToCamera >= 0f)
        {
            for (int i = 0; i < LODs.Count; i++)
            {
                if (distanceToCamera <= LODs[i].distance)
                {
                    activeLOD = i;
                    break;
                }
            }
        }

        // Draw all LOD segments
        for (int i = 0; i < LODs.Count; i++)
        {
            float width = ((i == 0 ? LODs[i].distance : LODs[i].distance - LODs[i - 1].distance) / totalDistance) * rect.width;
            Rect lodRect = new Rect(lastX, rect.y, width, rect.height);

            Color color = lodColors[i];

            // Highlight the active LOD
            if (i == activeLOD)
                color = Color.Lerp(color, Color.white, 0.4f);

            EditorGUI.DrawRect(lodRect, color);
            lastX += width;
        }

        // Draw culled section
        float culledWidth = (1f - (LODs[LODs.Count - 1].distance / totalDistance)) * rect.width;
        if (culledWidth > 0f)
        {
            Rect culledRect = new Rect(lastX, rect.y, culledWidth, rect.height);
            EditorGUI.DrawRect(culledRect, Color.gray);
        }
    }
}
#endif