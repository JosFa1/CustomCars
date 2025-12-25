using UnityEngine;
using UnityEditor;

public class CarPrefabValidator : MonoBehaviour
{
    [Header("Car Prefab Requirements")]
    [Tooltip("This prefab must have MeshFilter and MeshRenderer on the root")]
    public bool isValid = false;

    private void OnValidate()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        
        isValid = meshFilter != null && 
                  meshRenderer != null && 
                  meshFilter.sharedMesh != null;
        
        if (!isValid)
        {
            string missing = "";
            if (meshFilter == null || meshFilter.sharedMesh == null) missing += "MeshFilter with mesh, ";
            if (meshRenderer == null) missing += "MeshRenderer, ";
            
            Debug.LogWarning($"[{name}] Car prefab is INVALID! Missing: {missing.TrimEnd(',', ' ')}");
        }
        else
        {
            Debug.Log($"[{name}] Car prefab is valid ✓");
        }
    }
}
