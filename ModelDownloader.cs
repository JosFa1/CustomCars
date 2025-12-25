using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomCars;

public class ModelDownloader : MonoBehaviour
{
    private ManualLogSource logger;
    private string assetBundlePath;
    private Action<string, bool, string> onDownloadComplete; // (modelName, success, message)
    private Action<string, float> onDownloadProgress; // (modelName, progress 0-1)
    
    public void Initialize(ManualLogSource log, string bundlePath, 
        Action<string, bool, string> completeCallback,
        Action<string, float> progressCallback = null)
    {
        logger = log;
        assetBundlePath = bundlePath;
        onDownloadComplete = completeCallback;
        onDownloadProgress = progressCallback;
    }
    
    public void DownloadModel(CarModelInfo model)
    {
        StartCoroutine(DownloadModelCoroutine(model));
    }
    
    private IEnumerator DownloadModelCoroutine(CarModelInfo model)
    {
        logger.LogInfo($"Downloading model: {model.displayName} ({model.name})");
        
        // Check if already exists
        string targetPath = Path.Combine(assetBundlePath, model.name);
        if (File.Exists(targetPath))
        {
            logger.LogWarning($"Model '{model.name}' already exists. Overwriting...");
        }
        
        using (UnityWebRequest request = UnityWebRequest.Get(model.downloadUrl))
        {
            request.SetRequestHeader("User-Agent", "CustomCars-BepInEx-Mod");
            
            var operation = request.SendWebRequest();
            
            // Track progress
            while (!operation.isDone)
            {
                float progress = request.downloadProgress;
                onDownloadProgress?.Invoke(model.name, progress);
                yield return null;
            }
            
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = $"Failed to download {model.displayName}: {request.error}";
                logger.LogError(errorMsg);
                onDownloadComplete?.Invoke(model.name, false, errorMsg);
                yield break;
            }
            
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(assetBundlePath))
                {
                    Directory.CreateDirectory(assetBundlePath);
                }
                
                // Save the file
                byte[] data = request.downloadHandler.data;
                File.WriteAllBytes(targetPath, data);
                
                string successMsg = $"Successfully downloaded {model.displayName} ({data.Length / 1024f / 1024f:F2} MB)";
                logger.LogInfo(successMsg);
                onDownloadComplete?.Invoke(model.name, true, successMsg);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to save model {model.name}: {ex.Message}";
                logger.LogError(errorMsg);
                onDownloadComplete?.Invoke(model.name, false, errorMsg);
            }
        }
    }
    
    public bool IsModelInstalled(string modelName)
    {
        string targetPath = Path.Combine(assetBundlePath, modelName);
        return File.Exists(targetPath);
    }
}
