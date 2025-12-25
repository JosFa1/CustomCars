using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomCars;

public class ModelCatalog : MonoBehaviour
{
    private const string CATALOG_URL = "https://raw.githubusercontent.com/JosFa1/CustomCars/main/catalog.json";
    
    private ManualLogSource logger;
    private Action<List<CarModelInfo>> onCatalogLoaded;
    
    public void Initialize(ManualLogSource log, Action<List<CarModelInfo>> callback)
    {
        logger = log;
        onCatalogLoaded = callback;
    }
    
    public void FetchCatalog()
    {
        StartCoroutine(FetchCatalogCoroutine());
    }
    
    private IEnumerator FetchCatalogCoroutine()
    {
        logger.LogInfo("Fetching model catalog from GitHub...");
        
        using (UnityWebRequest request = UnityWebRequest.Get(CATALOG_URL))
        {
            request.SetRequestHeader("User-Agent", "CustomCars-BepInEx-Mod");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                logger.LogWarning($"Failed to fetch model catalog: {request.error}");
                yield break;
            }
            
            try
            {
                string jsonResponse = request.downloadHandler.text;
                List<CarModelInfo> models = ParseCatalog(jsonResponse);
                
                if (models != null && models.Count > 0)
                {
                    logger.LogInfo($"Successfully loaded {models.Count} models from catalog");
                    onCatalogLoaded?.Invoke(models);
                }
                else
                {
                    logger.LogWarning("Catalog is empty or failed to parse");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing catalog: {ex.Message}");
            }
        }
    }
    
    private List<CarModelInfo> ParseCatalog(string json)
    {
        List<CarModelInfo> models = new List<CarModelInfo>();
        
        try
        {
            // Simple JSON parsing for array of models
            // Expected format: {"models":[{...},{...}]}
            
            int modelsArrayStart = json.IndexOf("\"models\"");
            if (modelsArrayStart == -1) return models;
            
            int arrayStart = json.IndexOf("[", modelsArrayStart);
            int arrayEnd = json.LastIndexOf("]");
            
            if (arrayStart == -1 || arrayEnd == -1) return models;
            
            string modelsJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
            
            // Split by objects (simple approach - assumes no nested objects)
            int startIndex = 0;
            int braceDepth = 0;
            int currentStart = -1;
            
            for (int i = 0; i < modelsJson.Length; i++)
            {
                if (modelsJson[i] == '{')
                {
                    if (braceDepth == 0) currentStart = i;
                    braceDepth++;
                }
                else if (modelsJson[i] == '}')
                {
                    braceDepth--;
                    if (braceDepth == 0 && currentStart != -1)
                    {
                        string modelJson = modelsJson.Substring(currentStart, i - currentStart + 1);
                        CarModelInfo model = ParseModelInfo(modelJson);
                        if (model != null) models.Add(model);
                        currentStart = -1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error parsing catalog JSON: {ex.Message}");
        }
        
        return models;
    }
    
    private CarModelInfo ParseModelInfo(string json)
    {
        try
        {
            CarModelInfo model = new CarModelInfo();
            
            model.name = ExtractJsonString(json, "name");
            model.displayName = ExtractJsonString(json, "displayName");
            model.description = ExtractJsonString(json, "description");
            model.author = ExtractJsonString(json, "author");
            model.version = ExtractJsonString(json, "version");
            model.downloadUrl = ExtractJsonString(json, "downloadUrl");
            model.previewUrl = ExtractJsonString(json, "previewUrl");
            
            string sizeStr = ExtractJsonString(json, "fileSize");
            if (!string.IsNullOrEmpty(sizeStr))
            {
                float.TryParse(sizeStr, out model.fileSize);
            }
            
            return model;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error parsing model info: {ex.Message}");
            return null;
        }
    }
    
    private string ExtractJsonString(string json, string key)
    {
        try
        {
            string searchKey = $"\"{key}\"";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return null;
            
            int colonIndex = json.IndexOf(":", keyIndex);
            if (colonIndex == -1) return null;
            
            int quoteStart = json.IndexOf("\"", colonIndex);
            if (quoteStart == -1) return null;
            
            int quoteEnd = json.IndexOf("\"", quoteStart + 1);
            if (quoteEnd == -1) return null;
            
            return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }
        catch
        {
            return null;
        }
    }
}

public class CarModelInfo
{
    public string name;           // Internal name (e.g., "cybertruck")
    public string displayName;    // Display name (e.g., "Cybertruck")
    public string description;    // Description of the car
    public string author;         // Creator name
    public string version;        // Model version
    public string downloadUrl;    // Direct download URL
    public string previewUrl;     // Preview image URL (optional)
    public float fileSize;        // File size in MB
}
