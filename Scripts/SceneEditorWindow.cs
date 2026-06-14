using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SceneEditorWindow : EditorWindow
{
    private Vector2 scrollPos;
    private Direction currentDirection = Direction.Right;
    private List<Vector2> currentArrowPoints = new List<Vector2>();
    private bool isDrawingArrow = false;
    private string saveFileName = "level_01";

    private GameObject selectedHeadPrefab;
    private GameObject selectedBodyPrefab;

    [MenuItem("Window/Arrow Scene Editor")]
    public static void ShowWindow()
    {
        GetWindow<SceneEditorWindow>("Arrow Scene Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        LoadPrefabs();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void LoadPrefabs()
    {
        selectedHeadPrefab = Resources.Load<GameObject>("Prefabs/ArrowHeadRight");
        selectedBodyPrefab = Resources.Load<GameObject>("Prefabs/ArrowBodyRight");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Arrow Scene Editor", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Current Direction:", EditorStyles.label);
        currentDirection = (Direction)EditorGUILayout.EnumPopup(currentDirection);
        GUILayout.Space(10);

        GUILayout.Label("Prefabs:", EditorStyles.label);
        selectedHeadPrefab = (GameObject)EditorGUILayout.ObjectField("Head Prefab", selectedHeadPrefab, typeof(GameObject), false);
        selectedBodyPrefab = (GameObject)EditorGUILayout.ObjectField("Body Prefab", selectedBodyPrefab, typeof(GameObject), false);
        GUILayout.Space(10);

        GUILayout.Label("Drawing Arrow:", EditorStyles.label);
        GUILayout.Label($"Points Count: {currentArrowPoints.Count}");
        
        if (currentArrowPoints.Count > 0)
        {
            if (GUILayout.Button("Cancel Arrow"))
            {
                currentArrowPoints.Clear();
                isDrawingArrow = false;
            }
        }
        GUILayout.Space(10);

        GUILayout.Label("Save Level:", EditorStyles.label);
        saveFileName = EditorGUILayout.TextField("File Name", saveFileName);
        
        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }
        
        if (GUILayout.Button("Load Level"))
        {
            LoadLevel();
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Clear All Arrows"))
        {
            ClearAllArrows();
        }

        EditorGUILayout.EndScrollView();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            e.Use();
            Vector2 worldPos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            worldPos = SnapToGrid(worldPos);
            
            if (!isDrawingArrow)
            {
                isDrawingArrow = true;
                currentArrowPoints.Clear();
                currentArrowPoints.Add(worldPos);
            }
            else
            {
                currentArrowPoints.Add(worldPos);
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            e.Use();
            if (isDrawingArrow && currentArrowPoints.Count >= 2)
            {
                CreateArrowFromPoints();
            }
            isDrawingArrow = false;
            currentArrowPoints.Clear();
        }

        if (isDrawingArrow && currentArrowPoints.Count > 0)
        {
            Vector2 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePos = SnapToGrid(mousePos);

            for (int i = 0; i < currentArrowPoints.Count - 1; i++)
            {
                Handles.DrawLine(currentArrowPoints[i], currentArrowPoints[i + 1], Color.green);
            }

            if (currentArrowPoints.Count > 0)
            {
                Handles.DrawLine(currentArrowPoints[currentArrowPoints.Count - 1], mousePos, Color.yellow);
            }

            foreach (var point in currentArrowPoints)
            {
                Handles.Label(point, "●", new GUIStyle { normal = new GUIStyleState { textColor = Color.green } });
            }
        }

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 150, 80));
        GUILayout.Label("Shift+Click: Add Point");
        GUILayout.Label("Right Click: Finish Arrow");
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float gridSize = 1f;
        return new Vector2(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize
        );
    }

    private void CreateArrowFromPoints()
    {
        if (currentArrowPoints.Count < 2) return;

        GameObject arrowObj = new GameObject($"Arrow_{currentDirection}_{currentArrowPoints.Count}");
        Arrow arrow = arrowObj.AddComponent<Arrow>();
        
        List<Vector2> positions = new List<Vector2>(currentArrowPoints);
        arrow.Initialize(currentDirection, positions, selectedHeadPrefab, selectedBodyPrefab);

        Selection.activeGameObject = arrowObj;
    }

    private void SaveLevel()
    {
        Arrow[] arrows = FindObjectsOfType<Arrow>();
        
        LevelData levelData = new LevelData();
        levelData.arrows = new List<ArrowData>();
        
        foreach (var arrow in arrows)
        {
            ArrowData data = new ArrowData();
            data.direction = arrow.direction;
            data.positions = arrow.bodyPositions;
            levelData.arrows.Add(data);
        }

        string path = Path.Combine(Application.dataPath, "Resources/Levels", $"{saveFileName}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path, json);
        
        AssetDatabase.Refresh();
        Debug.Log($"Level saved to: {path}");
    }

    private void LoadLevel()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Levels", $"{saveFileName}.json");
        
        if (File.Exists(path))
        {
            ClearAllArrows();
            
            string json = File.ReadAllText(path);
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            
            foreach (var arrowData in levelData.arrows)
            {
                GameObject arrowObj = new GameObject($"Arrow_{arrowData.direction}_{arrowData.positions.Count}");
                Arrow arrow = arrowObj.AddComponent<Arrow>();
                arrow.Initialize(arrowData.direction, arrowData.positions, selectedHeadPrefab, selectedBodyPrefab);
            }
            
            Debug.Log($"Level loaded from: {path}");
        }
        else
        {
            Debug.LogError($"Level file not found: {path}");
        }
    }

    private void ClearAllArrows()
    {
        Arrow[] arrows = FindObjectsOfType<Arrow>();
        foreach (var arrow in arrows)
        {
            DestroyImmediate(arrow.gameObject);
        }
    }
}

[Serializable]
public class LevelData
{
    public List<ArrowData> arrows = new List<ArrowData>();
}

[Serializable]
public class ArrowData
{
    public Direction direction;
    public List<Vector2> positions = new List<Vector2>();
}
#endif
