using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CustomCars;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    // Debug logging toggle
    private static bool debugLogging = true; // Set to false to disable debug logs
    
    // Stored references to racing context objects
    private GameObject singlePlayerRacingContext;
    private GameObject playerTwoRacingContext;
    private GameObject playerThreeRacingContext;
    private GameObject playerFourRacingContext;
    
    // Asset bundle management
    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    private Dictionary<string, GameObject> carPrefabs = new Dictionary<string, GameObject>();
    private string assetBundlePath;
    
    // Configuration
    private CarConfig carConfig;
    
    // Update checker
    private UpdateChecker updateChecker;
    private UpdateNotificationUI updateNotificationUI;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        if (debugLogging) Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Initialize configuration
        carConfig = new CarConfig(Config);
        if (debugLogging) Logger.LogInfo($"Config loaded. AvailableCarsList: {carConfig.AvailableCarsList.Value}");

        // Set up asset bundle path (in BepInEx/plugins/CustomCars folder)
        assetBundlePath = Path.Combine(Paths.PluginPath, "CustomCars", "AssetBundles");
        if (debugLogging) Logger.LogInfo($"AssetBundle path: {assetBundlePath}");

        // Load all asset bundles
        LoadAssetBundles();

        // Update config with available cars
        carConfig.UpdateAvailableCarsList(carPrefabs.Keys);
        if (debugLogging) Logger.LogInfo($"Car prefabs after bundle load: {string.Join(", ", carPrefabs.Keys)}");

        // Initialize and check for updates
        if (carConfig.CheckForUpdates.Value && !carConfig.SilenceUpdateNotifications.Value)
        {
            GameObject checkerObj = new GameObject("CustomCarsUpdateChecker");
            updateChecker = checkerObj.AddComponent<UpdateChecker>();
            DontDestroyOnLoad(checkerObj);
            
            updateChecker.Initialize(Logger, MyPluginInfo.PLUGIN_VERSION, OnUpdateAvailable);
            updateChecker.CheckForUpdates();
        }

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene load events when plugin is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Unload all asset bundles
        UnloadAssetBundles();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the scene name contains "ALL"
        if (scene.name.Contains("ALL"))
        {
            Logger.LogInfo($"Race scene detected: {scene.name}");
            // Start coroutine to find racing contexts (they may spawn after scene load)
            StartCoroutine(FindRacingContextsCoroutine());
        }
    }
    
    private IEnumerator FindRacingContextsCoroutine()
    {
        if (debugLogging) Logger.LogInfo("Starting racing context search coroutine...");
        
        float initialTimeout = 10f; // Check frequently for first 10 seconds
        float elapsed = 0f;
        float fastCheckInterval = 0.5f; // Check every 0.5 seconds initially
        float slowCheckInterval = 30f; // Check every 30 seconds after timeout
        
        // Phase 1: Fast checking for first 10 seconds
        while (elapsed < initialTimeout)
        {
            FindAndSaveRacingContextObjects();
            
            // Count how many contexts we found
            int foundCount = 0;
            if (singlePlayerRacingContext != null) foundCount++;
            if (playerTwoRacingContext != null) foundCount++;
            if (playerThreeRacingContext != null) foundCount++;
            if (playerFourRacingContext != null) foundCount++;
            
            if (foundCount > 0)
            {
                Logger.LogInfo($"Found {foundCount}/4 racing context(s), applying custom cars...");
                ApplyCustomCars();
                yield break; // Exit coroutine once we find at least one
            }
            
            if (debugLogging) Logger.LogInfo($"No racing contexts found yet (elapsed: {elapsed:F1}s), retrying...");
            yield return new WaitForSeconds(fastCheckInterval);
            elapsed += fastCheckInterval;
        }
        
        Logger.LogInfo($"Initial timeout reached ({initialTimeout}s) - continuing to check every {slowCheckInterval}s indefinitely...");
        
        // Phase 2: Keep checking every 30 seconds forever
        while (true)
        {
            FindAndSaveRacingContextObjects();
            
            // Count how many contexts we found
            int foundCount = 0;
            if (singlePlayerRacingContext != null) foundCount++;
            if (playerTwoRacingContext != null) foundCount++;
            if (playerThreeRacingContext != null) foundCount++;
            if (playerFourRacingContext != null) foundCount++;
            
            if (foundCount > 0)
            {
                Logger.LogInfo($"Found {foundCount}/4 racing context(s) after extended wait, applying custom cars...");
                ApplyCustomCars();
                yield break; // Exit coroutine once we find at least one
            }
            
            if (debugLogging) Logger.LogInfo($"Still searching for racing contexts... (next check in {slowCheckInterval}s)");
            yield return new WaitForSeconds(slowCheckInterval);
        }
    }
    
    private void FindAndSaveRacingContextObjects()
    {
        // Clear previous references
        singlePlayerRacingContext = null;
        playerTwoRacingContext = null;
        playerThreeRacingContext = null;
        playerFourRacingContext = null;
        
        // Find all objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            switch (obj.name)
            {
                case "SinglePlayerRacingContext":
                    singlePlayerRacingContext = obj;
                    if (debugLogging) Logger.LogInfo($"Found Player 1 object: {obj.name}");
                    break;
                case "PlayerTwoRacingContext":
                    playerTwoRacingContext = obj;
                    if (debugLogging) Logger.LogInfo($"Found Player 2 object: {obj.name}");
                    break;
                case "PlayerThreeRacingContext":
                    playerThreeRacingContext = obj;
                    if (debugLogging) Logger.LogInfo($"Found Player 3 object: {obj.name}");
                    break;
                case "PlayerFourRacingContext":
                    playerFourRacingContext = obj;
                    if (debugLogging) Logger.LogInfo($"Found Player 4 object: {obj.name}");
                    break;
            }
        }
        
        // Log summary
        int foundCount = 0;
        if (singlePlayerRacingContext != null) foundCount++;
        if (playerTwoRacingContext != null) foundCount++;
        if (playerThreeRacingContext != null) foundCount++;
        if (playerFourRacingContext != null) foundCount++;
        
        if (debugLogging) Logger.LogInfo($"Found {foundCount} racing context object(s) in the scene");
    }
    
    private void LoadAssetBundles()
    {
        if (!Directory.Exists(assetBundlePath))
        {
            Logger.LogWarning($"AssetBundle directory not found: {assetBundlePath}");
            if (debugLogging) Logger.LogInfo("Creating AssetBundles directory...");
            Directory.CreateDirectory(assetBundlePath);
            return;
        }

        // Load all asset bundle files
        string[] bundleFiles = Directory.GetFiles(assetBundlePath, "*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(".manifest") && !f.EndsWith(".meta")).ToArray();

        if (debugLogging) Logger.LogInfo($"Found {bundleFiles.Length} potential asset bundle(s): {string.Join(", ", bundleFiles.Select(Path.GetFileName))}");

        foreach (string bundlePath in bundleFiles)
        {
            try
            {
                string bundleName = Path.GetFileName(bundlePath);
                if (debugLogging) Logger.LogInfo($"Attempting to load asset bundle: {bundleName}");
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

                if (bundle != null)
                {
                    loadedBundles[bundleName] = bundle;
                    if (debugLogging) Logger.LogInfo($"Loaded asset bundle: {bundleName}");

                    // Load all car prefabs from this bundle
                    GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();
                    if (debugLogging) Logger.LogInfo($"Prefabs found in bundle '{bundleName}': {string.Join(", ", prefabs.Select(p => p.name))}");
                    foreach (GameObject prefab in prefabs)
                    {
                        carPrefabs[prefab.name] = prefab;
                        if (debugLogging) Logger.LogInfo($"  - Loaded car prefab: {prefab.name}");
                    }
                }
                else
                {
                    Logger.LogError($"Failed to load asset bundle: {bundleName}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error loading asset bundle {Path.GetFileName(bundlePath)}: {ex.Message}");
            }
        }

        if (debugLogging) Logger.LogInfo($"Total car prefabs loaded: {carPrefabs.Count}");
    }
    
    private void UnloadAssetBundles()
    {
        foreach (var bundle in loadedBundles.Values)
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
        loadedBundles.Clear();
        carPrefabs.Clear();
        if (debugLogging) Logger.LogInfo("Unloaded all asset bundles");
    }
    
    private void ApplyCustomCars()
    {
        if (carPrefabs.Count == 0)
        {
            Logger.LogWarning("No car prefabs loaded from asset bundles");
            return;
        }
        
        // No longer tracking used cars - multiple players can use the same model
        HashSet<string> usedCars = new HashSet<string>(); // Still passed for compatibility
        HashSet<string> availableCars = new HashSet<string>(carPrefabs.Keys.Select(k => k.ToLower()));
        
        // Apply custom car to each racing context (priority order: P1 -> P2 -> P3 -> P4)
        if (singlePlayerRacingContext != null)
        {
            string carChoice = carConfig.GetCarForPlayer(1, usedCars, availableCars);
            ApplyCustomCarToContext(singlePlayerRacingContext, "Player 1", carChoice);
        }
        
        if (playerTwoRacingContext != null)
        {
            string carChoice = carConfig.GetCarForPlayer(2, usedCars, availableCars);
            ApplyCustomCarToContext(playerTwoRacingContext, "Player 2", carChoice);
        }
        
        if (playerThreeRacingContext != null)
        {
            string carChoice = carConfig.GetCarForPlayer(3, usedCars, availableCars);
            ApplyCustomCarToContext(playerThreeRacingContext, "Player 3", carChoice);
        }
        
        if (playerFourRacingContext != null)
        {
            string carChoice = carConfig.GetCarForPlayer(4, usedCars, availableCars);
            ApplyCustomCarToContext(playerFourRacingContext, "Player 4", carChoice);
        }
    }
    
    private void ApplyCustomCarToContext(GameObject racingContext, string playerName, string carChoice)
    {
        // Find the PlayerCarTemplate object first
        if (debugLogging) Logger.LogInfo($"{playerName}: Attempting to find PlayerCarTemplate in context '{racingContext.name}'");
        Transform playerCarTemplate = racingContext.transform.Find("PlayerCarTemplate");
        if (playerCarTemplate == null)
        {
            Logger.LogWarning($"{playerName}: Could not find PlayerCarTemplate object");
            return;
        }
        
        // Find the Car_chasis object under PlayerCarTemplate
        if (debugLogging) Logger.LogInfo($"{playerName}: Attempting to find Car_chasis under PlayerCarTemplate");
        Transform chasis = playerCarTemplate.Find("Car_chasis");
        if (chasis == null)
        {
            Logger.LogWarning($"{playerName}: Could not find Car_chasis object under PlayerCarTemplate");
            return;
        }

        // Disable wing renderers
        Transform frontWing = chasis.Find("Car_FrontWing");
        Transform backWing = chasis.Find("Car_backWing");
        Transform rearWing = chasis.Find("Car_RearWing");

        if (frontWing != null)
        {
            MeshRenderer frontRenderer = frontWing.GetComponent<MeshRenderer>();
            if (frontRenderer != null) frontRenderer.enabled = false;
            if (debugLogging) Logger.LogInfo($"{playerName}: Disabled front wing renderer");
        }

        if (backWing != null)
        {
            MeshRenderer backRenderer = backWing.GetComponent<MeshRenderer>();
            if (backRenderer != null) backRenderer.enabled = false;
            if (debugLogging) Logger.LogInfo($"{playerName}: Disabled back wing renderer");
        }

        if (rearWing != null)
        {
            MeshRenderer rearRenderer = rearWing.GetComponent<MeshRenderer>();
            if (rearRenderer != null) rearRenderer.enabled = false;
            if (debugLogging) Logger.LogInfo($"{playerName}: Disabled rear wing renderer");
        }

        // Check if player has a valid car override
        if (string.IsNullOrEmpty(carChoice))
        {
            if (debugLogging) Logger.LogInfo($"{playerName}: No car override configured or all choices unavailable - using default car");
            return;
        }

        if (debugLogging) Logger.LogInfo($"{playerName}: Car override from config: '{carChoice}'");

        // Find the car prefab (case-insensitive search)
        GameObject carPrefab = carPrefabs.FirstOrDefault(kvp => 
            kvp.Key.ToLower() == carChoice.ToLower()).Value;

        if (carPrefab == null)
        {
            Logger.LogWarning($"{playerName}: Car prefab '{carChoice}' not found in loaded bundles. Available: {string.Join(", ", carPrefabs.Keys)}");
            return;
        }

        if (debugLogging) Logger.LogInfo($"{playerName}: Using car prefab '{carPrefab.name}'");

        // Get mesh filter and renderer from the prefab
        MeshFilter prefabMeshFilter = carPrefab.GetComponent<MeshFilter>();
        MeshRenderer prefabMeshRenderer = carPrefab.GetComponent<MeshRenderer>();

        if (prefabMeshFilter == null || prefabMeshRenderer == null)
        {
            Logger.LogError($"{playerName}: Car prefab '{carChoice}' missing MeshFilter or MeshRenderer");
            return;
        }

        // Disable the original chasis renderer
        MeshRenderer chasisMeshRenderer = chasis.GetComponent<MeshRenderer>();
        if (chasisMeshRenderer != null)
        {
            chasisMeshRenderer.enabled = false;
            if (debugLogging) Logger.LogInfo($"{playerName}: Disabled original chasis renderer");
        }

        // Create a new child GameObject for the custom car visual
        GameObject customCarVisual = new GameObject("CustomCarVisual");
        customCarVisual.transform.SetParent(chasis, false);
        
        // Apply position, rotation, and scale from prefab to the visual child
        customCarVisual.transform.localPosition = carPrefab.transform.localPosition;
        customCarVisual.transform.localRotation = carPrefab.transform.localRotation;
        customCarVisual.transform.localScale = carPrefab.transform.localScale;
        if (debugLogging) Logger.LogInfo($"{playerName}: Applied position ({carPrefab.transform.localPosition}), rotation ({carPrefab.transform.localRotation.eulerAngles}), and scale ({carPrefab.transform.localScale}) to visual child");

        // Add MeshFilter and MeshRenderer to the custom visual
        MeshFilter customMeshFilter = customCarVisual.AddComponent<MeshFilter>();
        MeshRenderer customMeshRenderer = customCarVisual.AddComponent<MeshRenderer>();
        
        customMeshFilter.sharedMesh = prefabMeshFilter.sharedMesh;
        customMeshRenderer.sharedMaterials = prefabMeshRenderer.sharedMaterials;
        
        if (debugLogging) Logger.LogInfo($"{playerName}: Created CustomCarVisual child with mesh and materials");

        if (debugLogging) Logger.LogInfo($"{playerName}: Custom car applied successfully using prefab '{carPrefab.name}'");
    }
    
    private void OnUpdateAvailable(string latestVersion, string releaseUrl)
    {
        Logger.LogInfo($"Update available: {latestVersion}");
        
        // Create UI notification
        GameObject uiObj = new GameObject("CustomCarsUpdateUI");
        updateNotificationUI = uiObj.AddComponent<UpdateNotificationUI>();
        DontDestroyOnLoad(uiObj);
        
        updateNotificationUI.ShowUpdateNotification(
            MyPluginInfo.PLUGIN_VERSION,
            latestVersion,
            releaseUrl,
            OnUserResponse,
            OnSilenceUpdates
        );
    }
    
    private void OnUserResponse(bool acceptUpdate)
    {
        if (acceptUpdate)
        {
            Logger.LogInfo("User chose to view the release");
        }
        else
        {
            Logger.LogInfo("User chose to skip the update");
        }
    }
    
    private void OnSilenceUpdates()
    {
        Logger.LogInfo("User chose to silence update notifications");
        carConfig.SilenceUpdateNotifications.Value = true;
        Config.Save();
    }
}
