using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class CarAssetBundleBuilder : EditorWindow
{
    private string outputPath = "AssetBundles";
    private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    private bool cleanBuild = true;
    
    [MenuItem("Tools/Car Asset Bundle Builder")]
    public static void ShowWindow()
    {
        GetWindow<CarAssetBundleBuilder>("Car Asset Bundles");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Car Asset Bundle Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        outputPath = EditorGUILayout.TextField("Output Folder", outputPath);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        cleanBuild = EditorGUILayout.Toggle("Clean Build (Delete Old)", cleanBuild);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Validate All Car Prefabs", GUILayout.Height(30)))
        {
            ValidateCarPrefabs();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Build Asset Bundles", GUILayout.Height(40)))
        {
            BuildAssetBundles();
        }
        
        EditorGUILayout.Space();
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "CAR PREFAB STRUCTURE:\n" +
            "Root (your car name, e.g., 'cybertruck')\n" +
            "├─ MeshFilter (your car mesh)\n" +
            "└─ MeshRenderer (materials/textures)\n\n" +
            "SETUP STEPS:\n" +
            "1. Create car prefab with structure above\n" +
            "2. Set AssetBundle name (bottom of Inspector)\n" +
            "3. Click 'Build Asset Bundles'\n" +
            "4. Copy to: BepInEx/plugins/CustomCars/AssetBundles/",
            MessageType.Info);
    }
    
    private void ValidateCarPrefabs()
    {
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int validCount = 0;
        int invalidCount = 0;
        List<string> invalidPrefabs = new List<string>();
        
        foreach (string guid in allPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
                
                bool isValid = meshFilter != null && 
                              meshRenderer != null && 
                              meshFilter.sharedMesh != null &&
                              meshRenderer.sharedMaterials != null &&
                              meshRenderer.sharedMaterials.Length > 0;
                
                if (isValid)
                {
                    validCount++;
                    Debug.Log($"✓ Valid car prefab: {prefab.name}");
                }
                else
                {
                    invalidCount++;
                    invalidPrefabs.Add(prefab.name);
                    
                    string missing = "";
                    if (meshFilter == null || meshFilter.sharedMesh == null) missing += "MeshFilter/Mesh, ";
                    if (meshRenderer == null) missing += "MeshRenderer, ";
                    Debug.LogWarning($"✗ Invalid: {prefab.name} - Missing: {missing.TrimEnd(',', ' ')}");
                }
            }
        }
        
        Debug.Log($"=== Validation Complete ===\nValid: {validCount}\nInvalid: {invalidCount}");
        
        if (invalidCount > 0)
        {
            Debug.LogWarning($"Invalid prefabs: {string.Join(", ", invalidPrefabs)}");
        }
        
        EditorUtility.DisplayDialog("Validation Complete", 
            $"Valid Prefabs: {validCount}\nInvalid Prefabs: {invalidCount}", "OK");
    }
    
    private void BuildAssetBundles()
    {
        string fullOutputPath = Path.Combine(Application.dataPath, "..", outputPath);
        
        if (!Directory.Exists(fullOutputPath))
        {
            Directory.CreateDirectory(fullOutputPath);
        }
        
        if (cleanBuild)
        {
            Debug.Log("Cleaning old asset bundles...");
            foreach (string file in Directory.GetFiles(fullOutputPath))
            {
                File.Delete(file);
            }
        }
        
        Debug.Log($"Building asset bundles to: {fullOutputPath}");
        
        BuildPipeline.BuildAssetBundles(
            fullOutputPath,
            BuildAssetBundleOptions.None,
            buildTarget
        );
        
        Debug.Log("Asset bundles built successfully!");
        
        // List all built bundles
        string[] bundles = Directory.GetFiles(fullOutputPath)
            .Where(f => !f.EndsWith(".manifest") && !f.EndsWith(".meta"))
            .Select(Path.GetFileName)
            .ToArray();
        
        Debug.Log($"Built {bundles.Length} asset bundle(s):");
        foreach (string bundle in bundles)
        {
            Debug.Log($"  - {bundle}");
        }
        
        EditorUtility.DisplayDialog("Build Complete", 
            $"Built {bundles.Length} asset bundle(s) to:\n{fullOutputPath}\n\n" +
            "Copy these files to:\nBepInEx/plugins/CustomCars/AssetBundles/", "OK");
        
        EditorUtility.RevealInFinder(fullOutputPath);
    }
}
