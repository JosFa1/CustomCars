using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCars;

public class CarConfigUI : MonoBehaviour
{
    private bool showConfig = false;
    private List<string> availableCars = new List<string>();
    private ManualLogSource logger;
    private CarConfig carConfig;
    private Action onConfigSaved;
    
    private Rect windowRect = new Rect(150, 150, 900, 700);
    private Vector2 scrollPosition = Vector2.zero;
    
    private GUIStyle windowStyle;
    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private GUIStyle selectedButtonStyle;
    private GUIStyle boxStyle;
    private bool stylesInitialized = false;
    
    // Temporary config state (until saved)
    private Dictionary<string, string[]> tempConfig = new Dictionary<string, string[]>();
    private int currentPlayer = 1;
    private string statusMessage = "";
    private float statusMessageTime = 0f;
    
    public void Initialize(ManualLogSource log, CarConfig config, Action saveCallback)
    {
        logger = log;
        carConfig = config;
        onConfigSaved = saveCallback;
    }
    
    public void ShowConfig(List<string> cars)
    {
        availableCars = cars.OrderBy(c => c).ToList();
        showConfig = true;
        
        // Load current configuration into temp state
        LoadCurrentConfig();
    }
    
    public void HideConfig()
    {
        showConfig = false;
    }
    
    private void LoadCurrentConfig()
    {
        tempConfig.Clear();
        
        for (int player = 1; player <= 4; player++)
        {
            string[] choices = new string[4];
            
            var configEntries = player switch
            {
                1 => new[] { carConfig.Player1Car1, carConfig.Player1Car2, carConfig.Player1Car3, carConfig.Player1Car4 },
                2 => new[] { carConfig.Player2Car1, carConfig.Player2Car2, carConfig.Player2Car3, carConfig.Player2Car4 },
                3 => new[] { carConfig.Player3Car1, carConfig.Player3Car2, carConfig.Player3Car3, carConfig.Player3Car4 },
                4 => new[] { carConfig.Player4Car1, carConfig.Player4Car2, carConfig.Player4Car3, carConfig.Player4Car4 },
                _ => null
            };
            
            if (configEntries != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    choices[i] = configEntries[i].Value;
                    if (choices[i] == "null" || string.IsNullOrEmpty(choices[i]))
                    {
                        choices[i] = "none";
                    }
                }
            }
            
            tempConfig[$"Player{player}"] = choices;
        }
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.12f, 0.98f));
        windowStyle.padding = new RectOffset(15, 15, 25, 15);
        
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleLeft;
        titleStyle.normal.textColor = Color.white;
        
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.9f, 0.9f, 1f, 1f);
        
        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 13;
        textStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.35f, 1f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.45f, 1f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.3f, 1f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.padding = new RectOffset(8, 8, 6, 6);
        
        selectedButtonStyle = new GUIStyle(buttonStyle);
        selectedButtonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.5f, 0.8f, 1f));
        selectedButtonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.6f, 0.9f, 1f));
        
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.22f, 0.8f));
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        
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
        if (!string.IsNullOrEmpty(statusMessage) && Time.time - statusMessageTime > 3f)
        {
            statusMessage = "";
        }
    }
    
    private void OnGUI()
    {
        if (!showConfig) return;
        
        InitializeStyles();
        
        windowRect = GUI.Window(2, windowRect, DrawWindow, "CustomCars Configuration", windowStyle);
    }
    
    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // Header
        GUILayout.Label("Configure Car Preferences", titleStyle);
        GUILayout.Space(5);
        
        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUILayout.Label(statusMessage, textStyle);
            GUILayout.Space(5);
        }
        
        // Instructions
        GUILayout.Label($"Available Cars: {availableCars.Count} | Select up to 4 cars per player in order of preference", textStyle);
        GUILayout.Space(10);
        
        // Player tabs
        GUILayout.BeginHorizontal();
        for (int i = 1; i <= 4; i++)
        {
            var style = (currentPlayer == i) ? selectedButtonStyle : buttonStyle;
            if (GUILayout.Button($"Player {i}", style, GUILayout.Width(100), GUILayout.Height(35)))
            {
                currentPlayer = i;
            }
            GUILayout.Space(5);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        
        // Current player configuration
        DrawPlayerConfig();
        
        GUILayout.Space(15);
        
        // Action buttons
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Save Configuration", buttonStyle, GUILayout.Width(150), GUILayout.Height(40)))
        {
            SaveConfiguration();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Reset Player", buttonStyle, GUILayout.Width(120), GUILayout.Height(40)))
        {
            ResetPlayer(currentPlayer);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(100), GUILayout.Height(40)))
        {
            HideConfig();
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
    
    private void DrawPlayerConfig()
    {
        string playerKey = $"Player{currentPlayer}";
        if (!tempConfig.ContainsKey(playerKey))
            return;
        
        string[] choices = tempConfig[playerKey];
        
        GUILayout.BeginVertical(boxStyle);
        
        GUILayout.Label($"Player {currentPlayer} Car Preferences", headerStyle);
        GUILayout.Space(10);
        
        // Show current choices
        for (int i = 0; i < 4; i++)
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Label($"Choice {i + 1}:", textStyle, GUILayout.Width(80));
            
            string currentChoice = choices[i];
            string displayText = (currentChoice == "none" || string.IsNullOrEmpty(currentChoice)) ? "Not Set" : currentChoice;
            
            GUILayout.Label(displayText, headerStyle, GUILayout.Width(150));
            
            if (GUILayout.Button("Clear", buttonStyle, GUILayout.Width(70), GUILayout.Height(25)))
            {
                choices[i] = "none";
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        
        GUILayout.Space(15);
        
        // Available cars selection
        GUILayout.Label("Click a car to set it as the next preference:", textStyle);
        GUILayout.Space(5);
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(280));
        
        int columns = 3;
        int count = 0;
        
        GUILayout.BeginHorizontal();
        
        foreach (var car in availableCars)
        {
            bool isAlreadySelected = choices.Contains(car);
            var style = isAlreadySelected ? selectedButtonStyle : buttonStyle;
            
            if (GUILayout.Button(car, style, GUILayout.Width(260), GUILayout.Height(35)))
            {
                SetNextChoice(choices, car);
            }
            
            count++;
            if (count % columns == 0)
            {
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
            }
        }
        
        // Add "Default Car" option
        bool isDefaultSelected = choices.Contains("none");
        var defaultStyle = isDefaultSelected ? selectedButtonStyle : buttonStyle;
        
        if (GUILayout.Button("Default Car (none)", defaultStyle, GUILayout.Width(260), GUILayout.Height(35)))
        {
            SetNextChoice(choices, "none");
        }
        
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
    }
    
    private void SetNextChoice(string[] choices, string car)
    {
        // Find the first empty slot or the first occurrence of this car
        int targetIndex = -1;
        
        // First, check if this car is already selected - if so, remove it
        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == car)
            {
                choices[i] = "none";
                logger.LogInfo($"Removed {car} from choice {i + 1}");
                return;
            }
        }
        
        // Find first empty slot
        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == "none" || string.IsNullOrEmpty(choices[i]))
            {
                targetIndex = i;
                break;
            }
        }
        
        if (targetIndex != -1)
        {
            choices[targetIndex] = car;
            logger.LogInfo($"Set choice {targetIndex + 1} to {car}");
        }
        else
        {
            // All slots full, replace the last one
            choices[3] = car;
            logger.LogInfo($"All slots full, replaced last choice with {car}");
        }
    }
    
    private void ResetPlayer(int player)
    {
        string playerKey = $"Player{player}";
        if (tempConfig.ContainsKey(playerKey))
        {
            tempConfig[playerKey] = new string[] { "none", "none", "none", "none" };
            statusMessage = $"Player {player} preferences reset";
            statusMessageTime = Time.time;
            logger.LogInfo($"Reset Player {player} configuration");
        }
    }
    
    private void SaveConfiguration()
    {
        logger.LogInfo("Saving car configuration...");
        
        // Save to actual config
        for (int player = 1; player <= 4; player++)
        {
            string playerKey = $"Player{player}";
            if (!tempConfig.ContainsKey(playerKey))
                continue;
            
            string[] choices = tempConfig[playerKey];
            
            var configEntries = player switch
            {
                1 => new[] { carConfig.Player1Car1, carConfig.Player1Car2, carConfig.Player1Car3, carConfig.Player1Car4 },
                2 => new[] { carConfig.Player2Car1, carConfig.Player2Car2, carConfig.Player2Car3, carConfig.Player2Car4 },
                3 => new[] { carConfig.Player3Car1, carConfig.Player3Car2, carConfig.Player3Car3, carConfig.Player3Car4 },
                4 => new[] { carConfig.Player4Car1, carConfig.Player4Car2, carConfig.Player4Car3, carConfig.Player4Car4 },
                _ => null
            };
            
            if (configEntries != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    string value = choices[i];
                    if (value == "none" || string.IsNullOrEmpty(value))
                    {
                        value = "null";
                    }
                    configEntries[i].Value = value;
                }
            }
        }
        
        statusMessage = "Configuration saved! Changes will apply in your next race.";
        statusMessageTime = Time.time;
        logger.LogInfo("Car configuration saved successfully");
        
        // Trigger callback
        onConfigSaved?.Invoke();
    }
}
