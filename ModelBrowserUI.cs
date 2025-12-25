using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCars;

public class ModelBrowserUI : MonoBehaviour
{
    private bool showBrowser = false;
    private List<CarModelInfo> availableModels = new List<CarModelInfo>();
    private ManualLogSource logger;
    private ModelDownloader downloader;
    
    private Rect windowRect = new Rect(100, 100, 800, 600);
    private Vector2 scrollPosition = Vector2.zero;
    
    private GUIStyle windowStyle;
    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private GUIStyle installedButtonStyle;
    private GUIStyle downloadingButtonStyle;
    private bool stylesInitialized = false;
    
    private Dictionary<string, bool> downloadingModels = new Dictionary<string, bool>();
    private Dictionary<string, float> downloadProgress = new Dictionary<string, float>();
    private Dictionary<string, bool> installedModels = new Dictionary<string, bool>();
    private string statusMessage = "";
    private float statusMessageTime = 0f;
    
    public void Initialize(ManualLogSource log, ModelDownloader modelDownloader)
    {
        logger = log;
        downloader = modelDownloader;
    }
    
    public void ShowBrowser(List<CarModelInfo> models)
    {
        availableModels = models;
        showBrowser = true;
        
        // Check which models are already installed
        foreach (var model in availableModels)
        {
            installedModels[model.name] = downloader.IsModelInstalled(model.name);
        }
    }
    
    public void HideBrowser()
    {
        showBrowser = false;
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.98f));
        windowStyle.padding = new RectOffset(15, 15, 25, 15);
        
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleLeft;
        titleStyle.normal.textColor = Color.white;
        
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 1f);
        
        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 13;
        textStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        textStyle.wordWrap = true;
        
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.5f, 0.2f, 1f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.6f, 0.3f, 1f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.15f, 0.4f, 0.15f, 1f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        
        installedButtonStyle = new GUIStyle(GUI.skin.button);
        installedButtonStyle.fontSize = 14;
        installedButtonStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1f));
        installedButtonStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        
        downloadingButtonStyle = new GUIStyle(GUI.skin.button);
        downloadingButtonStyle.fontSize = 14;
        downloadingButtonStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.4f, 0.2f, 1f));
        downloadingButtonStyle.normal.textColor = Color.white;
        
        stylesInitialized = true;
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    private void Update()
    {
        // Clear status message after 3 seconds
        if (!string.IsNullOrEmpty(statusMessage) && Time.time - statusMessageTime > 3f)
        {
            statusMessage = "";
        }
    }
    
    private void OnGUI()
    {
        if (!showBrowser) return;
        
        InitializeStyles();
        
        windowRect = GUI.Window(1, windowRect, DrawWindow, "CustomCars Model Browser", windowStyle);
    }
    
    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // Header
        GUILayout.Label("Download Custom Car Models", titleStyle);
        GUILayout.Space(5);
        
        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUILayout.Label(statusMessage, textStyle);
            GUILayout.Space(5);
        }
        
        // Instructions
        GUILayout.Label($"Available Models: {availableModels.Count}", textStyle);
        GUILayout.Label("Click 'Download' to add a model to your game. Restart required after download.", textStyle);
        GUILayout.Space(10);
        
        // Model list (scrollable)
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(420));
        
        foreach (var model in availableModels)
        {
            DrawModelEntry(model);
            GUILayout.Space(10);
        }
        
        GUILayout.EndScrollView();
        
        GUILayout.Space(10);
        
        // Close button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(120), GUILayout.Height(35)))
        {
            HideBrowser();
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
    
    private void DrawModelEntry(CarModelInfo model)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.BeginHorizontal();
        
        // Model info (left side)
        GUILayout.BeginVertical(GUILayout.Width(550));
        
        GUILayout.Label(model.displayName, headerStyle);
        GUILayout.Label($"Internal Name: {model.name}", textStyle);
        GUILayout.Label(model.description, textStyle);
        GUILayout.Label($"Author: {model.author} | Version: {model.version} | Size: {model.fileSize:F2} MB", textStyle);
        
        GUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        
        // Download button (right side)
        GUILayout.BeginVertical(GUILayout.Width(150));
        GUILayout.Space(15);
        
        bool isInstalled = installedModels.ContainsKey(model.name) && installedModels[model.name];
        bool isDownloading = downloadingModels.ContainsKey(model.name) && downloadingModels[model.name];
        
        if (isInstalled && !isDownloading)
        {
            GUI.enabled = false;
            GUILayout.Button("Installed", installedButtonStyle, GUILayout.Height(40));
            GUI.enabled = true;
        }
        else if (isDownloading)
        {
            float progress = downloadProgress.ContainsKey(model.name) ? downloadProgress[model.name] : 0f;
            string progressText = $"Downloading\n{progress * 100:F0}%";
            GUI.enabled = false;
            GUILayout.Button(progressText, downloadingButtonStyle, GUILayout.Height(40));
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Download", buttonStyle, GUILayout.Height(40)))
            {
                DownloadModel(model);
            }
        }
        
        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }
    
    private void DownloadModel(CarModelInfo model)
    {
        logger.LogInfo($"Starting download for: {model.displayName}");
        downloadingModels[model.name] = true;
        downloadProgress[model.name] = 0f;
        
        downloader.DownloadModel(model);
    }
    
    public void OnDownloadComplete(string modelName, bool success, string message)
    {
        downloadingModels[modelName] = false;
        
        if (success)
        {
            installedModels[modelName] = true;
            statusMessage = $"Successfully downloaded {modelName}! Restart the game to use it.";
        }
        else
        {
            statusMessage = $"Failed to download {modelName}: {message}";
        }
        
        statusMessageTime = Time.time;
        logger.LogInfo(statusMessage);
    }
    
    public void OnDownloadProgress(string modelName, float progress)
    {
        downloadProgress[modelName] = progress;
    }
}
