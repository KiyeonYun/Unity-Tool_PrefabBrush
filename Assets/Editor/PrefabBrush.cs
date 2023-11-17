using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabBrush : EditorWindow
{
    public static PrefabBrush Instance { get; private set; }

    float brushSize = 1, density = 1;  // 브러쉬 사이즈, 밀도
    [Range(0, 10)] float minScale = 0.5f;
    [Range(0, 10)] float maxScale = 1.5f;

    bool useRndRotation = true;
    [Range(-180, 180)] float minRotation = -180;  // 최소 회전 각도
    [Range(-180, 180)] float maxRotation = 180;  // 최소 회전 각도
    [Range(0, 360)] float minSlope = 0f;
    [Range(0, 360)] float maxSlope = 360f;
    Object obj = null;  // 대상 오브젝트

    #region Editor Window

    [MenuItem("Tools/Prefab Brush")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PrefabBrush));
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Instance = this;
    }

    private void OnGUI()
    {
        obj = EditorGUILayout.ObjectField("Target Object", obj, typeof(GameObject), false);
        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 20f);
        brushSize = EditorGUILayout.Slider("Density", density, 0.1f, 10f);

        EditorGUILayout.Space(2);
        useRndRotation = EditorGUILayout.BeginToggleGroup("Use Random Rotation", useRndRotation);
        EditorGUILayout.LabelField("Min Value:", minRotation.ToString());
        EditorGUILayout.LabelField("Max Value:", maxRotation.ToString());
        EditorGUILayout.MinMaxSlider(ref minRotation, ref maxRotation, -180f, 180f);
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Use painting via Left click", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Use painting via Right click", EditorStyles.boldLabel);


    }

    #endregion

    #region Scene View

    void OnSceneGUI(SceneView sceneView)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Color color = Color.magenta;
            color.a = 0.25f;
            Handles.color = color;
            Handles.DrawSolidArc(
                hit.point, hit.normal, Vector3.Cross(hit.normal, ray.direction), 360, brushSize
                );
        }
    }

    private void PlacePrefab()
    {
        if (Selection.activeTransform)
            Selection.activeTransform.position =
                new Vector3(GetRandomRotation(), GetRandomRotation(), GetRandomRotation());
        
            //if (useRndRotation) 
    }

    #endregion

    private float GetRandomRotation() { return Random.Range(minRotation, maxRotation); }
}
