using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindBrokenReferences : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> brokenReferences = new List<string>();

    [MenuItem("Tools/Find Broken References")]
    public static void ShowWindow()
    {
        GetWindow<FindBrokenReferences>("Find Broken References");
    }

    private void OnGUI()
    {
        GUILayout.Label("Broken References Finder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Scan All Prefabs and Scene", GUILayout.Height(30)))
        {
            ScanForBrokenReferences();
        }

        GUILayout.Space(10);

        if (brokenReferences.Count > 0)
        {
            GUILayout.Label($"Found {brokenReferences.Count} broken references:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (string reference in brokenReferences)
            {
                EditorGUILayout.HelpBox(reference, MessageType.Warning);
            }
            
            EditorGUILayout.EndScrollView();
        }
        else if (brokenReferences.Count == 0 && Event.current.type == EventType.Layout)
        {
            GUILayout.Label("No broken references found. Click 'Scan' to check.", EditorStyles.helpBox);
        }
    }

    private void ScanForBrokenReferences()
    {
        brokenReferences.Clear();
        
        // Scan all prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                CheckGameObjectForBrokenReferences(prefab, path, true);
            }
        }

        // Scan current scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded)
        {
            GameObject[] sceneObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in sceneObjects)
            {
                CheckGameObjectForBrokenReferences(obj, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, false);
            }
        }

        Debug.Log($"Scan complete. Found {brokenReferences.Count} broken references.");
    }

    private void CheckGameObjectForBrokenReferences(GameObject obj, string location, bool isPrefab)
    {
        if (obj == null) return;

        // Check all components
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null)
            {
                string context = isPrefab ? "Prefab" : "Scene";
                brokenReferences.Add($"[{context}] {location}: GameObject '{obj.name}' has a missing component");
                continue;
            }

            // Use SerializedObject to check for broken references
            SerializedObject serializedObject = new SerializedObject(comp);
            SerializedProperty property = serializedObject.GetIterator();
            
            property.NextVisible(true);
            do
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                    {
                        string context = isPrefab ? "Prefab" : "Scene";
                        brokenReferences.Add($"[{context}] {location}: '{obj.name}' -> Component '{comp.GetType().Name}' -> Property '{property.name}' has a broken reference");
                    }
                }
            } while (property.NextVisible(false));
        }

        // Recursively check children
        foreach (Transform child in obj.transform)
        {
            CheckGameObjectForBrokenReferences(child.gameObject, location, isPrefab);
        }
    }
}
