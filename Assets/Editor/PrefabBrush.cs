using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

public class PrefabBrush : EditorWindow
{
    public static PrefabBrush Instance { get; private set; }

    [HideInInspector, NonSerialized] public List<GameObject> spawnedObjects = new List<GameObject>();

    float brushSize = 1, density = 1;  // 브러쉬 사이즈, 밀도
    [Range(0, 10)] float minScale = 0.5f;
    [Range(0, 10)] float maxScale = 1.5f;

    bool useRndRotation = true;
    [Range(-180, 180)] float minRotation = -180;  // 최소 회전 각도
    [Range(-180, 180)] float maxRotation = 180;  // 최대 회전 각도
    [Range(0, 360)] float minSlope = 0f;
    [Range(0, 360)] float maxSlope = 360f;
    Transform ParentObj = null;  // 브러쉬로 배치할 오브젝트
    GameObject PrefabObj = null;  // 브러쉬로 배치할 오브젝트

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

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        ParentObj = (Transform)EditorGUILayout.ObjectField("Parent Object", ParentObj, typeof(Transform), true);
        PrefabObj = (GameObject)EditorGUILayout.ObjectField("Target Object", PrefabObj, typeof(GameObject), false);
        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 20f);
        density = EditorGUILayout.Slider("Density", density, 0.1f, 10f);

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

        if (Tools.current != Tool.View)
        {
            if (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseDrag)
            {
                if (Event.current.button == 0 && PlaceObjects())
                {
                    Event.current.Use();
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                }
                else if (Event.current.button == 1)
                {
                    if (RemoveObjects())
                        Event.current.Use();
                }
            }
        }
    }

    private bool PlaceObjects()
    {
        bool hasPlacedObjects = false;

        int spawnCount = Mathf.RoundToInt(density * brushSize);
        if (spawnCount < 1) spawnCount = 1;

        for (int i = 0; i < spawnCount; i++)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            ray.origin += new Vector3(
                Random.Range(brushSize * -1, brushSize),
                Random.Range(brushSize * -1, brushSize),
                Random.Range(brushSize * -1, brushSize)
                );
            Vector3 startPoint = ray.origin;
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit))
            {
                if (spawnedObjects.Contains(hit.collider.gameObject)) continue;

                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle < minSlope || angle > maxSlope) continue;

                GameObject obj;
                /*if (PrefabObj.gameObject.scene.name != null)
                    obj = Instantiate(PrefabObj, hit.point, Quaternion.identity);
                else*/
                {
                    obj = PrefabUtility.InstantiatePrefab(PrefabObj) as GameObject;
                    obj.transform.position = hit.point;
                    obj.transform.rotation = Quaternion.identity;
                }

                obj.transform.parent = ParentObj;
                hasPlacedObjects = true;
                Undo.RegisterCreatedObjectUndo(obj, "Created " + obj.name + " with brush");

                // Randomize Rotation
                Vector3 rot = Vector3.zero;

                if (useRndRotation)
                {
                    rot.x = GetRandomRotation();
                    rot.y = GetRandomRotation();
                    rot.z = GetRandomRotation();
                }
                obj.transform.Rotate(rot, Space.Self);
                spawnedObjects.AddRange(GetAllChildren(obj));
            }
        }

        return hasPlacedObjects;
    }

    private bool RemoveObjects()
    {
        bool hasRemovedObjects = false;

        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        List<GameObject> removedObj = new List<GameObject>();
        if (Physics.Raycast(ray, out hit))
        {
            foreach (GameObject obj in spawnedObjects)
                if (obj != null && Vector3.Distance(obj.transform.position, hit.point) < brushSize)
                    removedObj.Add(obj);

            foreach (GameObject obj in removedObj)
            {
                spawnedObjects.Remove(obj);
                DestroyImmediate(obj);
                hasRemovedObjects = true;
            }
            removedObj.Clear();
        }

        return hasRemovedObjects;
    }

    #endregion

    private static GameObject[] GetAllChildren(GameObject obj)
    {
        List<GameObject> children = new List<GameObject>();
        if (obj != null)
            foreach (Transform child in obj.transform)
                children.Add(child.gameObject);
        children.Add(obj);
        return children.ToArray();
    }

    private float GetRandomRotation() { return Random.Range(minRotation, maxRotation); }
}
