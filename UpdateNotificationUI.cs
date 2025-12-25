using UnityEngine;
using System;

namespace CustomCars;

public class UpdateNotificationUI : MonoBehaviour
{
    private bool showNotification = false;
    private string currentVersion;
    private string latestVersion;
    private string releaseUrl;
    private Action<bool> onUserResponse;
    private Action onSilence;
    
    private Rect windowRect = new Rect((Screen.width - 600) / 2, (Screen.height - 400) / 2, 600, 400);
    private GUIStyle windowStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private bool stylesInitialized = false;
    
    public void ShowUpdateNotification(string currentVer, string latestVer, string url, 
        Action<bool> responseCallback, Action silenceCallback)
    {
        currentVersion = currentVer;
        latestVersion = latestVer;
        releaseUrl = url;
        onUserResponse = responseCallback;
        onSilence = silenceCallback;
        showNotification = true;
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        // Window style
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        windowStyle.padding = new RectOffset(20, 20, 20, 20);
        
        // Title style
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        
        // Text style
        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 16;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = true;
        
        // Button style
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.7f, 1f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.8f, 1f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.15f, 0.3f, 0.6f, 1f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        
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
    
    private void OnGUI()
    {
        if (!showNotification) return;
        
        InitializeStyles();
        
        windowRect = GUI.Window(0, windowRect, DrawWindow, "CustomCars Update", windowStyle);
    }
    
    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Space(20);
        
        // Title
        GUILayout.Label("Update Available", titleStyle);
        
        GUILayout.Space(30);
        
        // Version info
        GUILayout.Label($"Current Version: {currentVersion}", textStyle);
        GUILayout.Label($"Latest Version: {latestVersion}", textStyle);
        
        GUILayout.Space(20);
        
        // Message
        GUILayout.Label("A new version is available on GitHub.", textStyle);
        GUILayout.Label("Would you like to view the release?", textStyle);
        
        GUILayout.Space(30);
        
        // Buttons
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("View Release", buttonStyle, GUILayout.Width(150), GUILayout.Height(40)))
        {
            Application.OpenURL(releaseUrl);
            onUserResponse?.Invoke(true);
            CloseNotification();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Later", buttonStyle, GUILayout.Width(150), GUILayout.Height(40)))
        {
            onUserResponse?.Invoke(false);
            CloseNotification();
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Silence button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Don't Show Again", buttonStyle, GUILayout.Width(200), GUILayout.Height(40)))
        {
            onSilence?.Invoke();
            CloseNotification();
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }
    
    private void CloseNotification()
    {
        showNotification = false;
        Destroy(gameObject);
    }
}
