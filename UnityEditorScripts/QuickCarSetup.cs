using UnityEngine;
using UnityEditor;

public class QuickCarSetup : MonoBehaviour
{
    [MenuItem("GameObject/Setup as Car Prefab", false, 0)]
    private static void SetupCarPrefab()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject first!", "OK");
            return;
        }
        
        // Ensure MeshFilter and MeshRenderer on root
        MeshFilter mf = selected.GetComponent<MeshFilter>();
        MeshRenderer mr = selected.GetComponent<MeshRenderer>();
        
        if (mf == null)
        {
            mf = selected.AddComponent<MeshFilter>();
            Debug.Log($"Added MeshFilter to {selected.name}");
        }
        
        if (mr == null)
        {
            mr = selected.AddComponent<MeshRenderer>();
            Debug.Log($"Added MeshRenderer to {selected.name}");
        }
        
        // Add validator
        if (selected.GetComponent<CarPrefabValidator>() == null)
        {
            selected.AddComponent<CarPrefabValidator>();
            Debug.Log($"Added CarPrefabValidator to {selected.name}");
        }
        
        // Suggest asset bundle name
        string suggestedName = selected.name.ToLower().Replace(" ", "");
        Debug.Log($"Suggested AssetBundle name: '{suggestedName}'");
        Debug.Log($"Set this in Inspector → AssetBundle dropdown at bottom");
        
        EditorUtility.DisplayDialog("Car Prefab Setup", 
            $"Setup complete for '{selected.name}'!\n\n" +
            $"Next steps:\n" +
            $"1. Assign mesh to MeshFilter\n" +
            $"2. Assign materials to MeshRenderer\n" +
            $"3. Set AssetBundle name to: '{suggestedName}'\n" +
            $"4. Save as prefab in Project window", "OK");
    }
    
    [MenuItem("GameObject/Setup as Car Prefab", true)]
    private static bool ValidateSetupCarPrefab()
    {
        return Selection.activeGameObject != null;
    }
}
