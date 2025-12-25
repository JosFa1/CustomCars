using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCars;

public class CarConfig
{
    private ConfigFile configFile;
    
    // Player car override settings
    public ConfigEntry<string> Player1Car;
    public ConfigEntry<string> Player2Car;
    public ConfigEntry<string> Player3Car;
    public ConfigEntry<string> Player4Car;
    
    public ConfigEntry<string> AvailableCarsList;
    
    // Update checker settings
    public ConfigEntry<bool> CheckForUpdates;
    public ConfigEntry<bool> SilenceUpdateNotifications;
    
    // Model browser settings
    public ConfigEntry<KeyCode> OpenModelBrowserKey;
    public ConfigEntry<KeyCode> OpenCarConfigKey;
    public ConfigEntry<bool> AutoCheckForNewModels;
    
    public CarConfig(ConfigFile config)
    {
        configFile = config;
        
        // Available cars list (informational only, updated on plugin load)
        AvailableCarsList = config.Bind(
            "Available Cars",
            "CarList",
            "none",
            "List of available car prefabs found in asset bundles (comma-separated, all lowercase).\n" +
            "This list is automatically updated when the plugin loads and scans asset bundles.\n" +
            "NOTE: This list will be out of date until the next game launch after adding new cars.\n" +
            "Use these exact names (lowercase) in the player override settings below."
        );
        
        // Update checker configuration
        CheckForUpdates = config.Bind(
            "Update Checker",
            "CheckForUpdates",
            true,
            "Check for mod updates on launch (checks GitHub releases)"
        );
        
        SilenceUpdateNotifications = config.Bind(
            "Update Checker",
            "SilenceUpdateNotifications",
            false,
            "Set to true to stop showing update notifications"
        );
        
        // Model browser configuration
        OpenModelBrowserKey = config.Bind(
            "Model Browser",
            "OpenBrowserKey",
            KeyCode.F6,
            "Press this key to open the model browser and download new car models"
        );
        
        OpenCarConfigKey = config.Bind(
            "Model Browser",
            "OpenCarConfigKey",
            KeyCode.F8,
            "Press this key to open the car configuration UI and set your car preferences"
        );
        
        AutoCheckForNewModels = config.Bind(
            "Model Browser",
            "AutoCheckForNewModels",
            true,
            "Automatically check for new models in the catalog on launch"
        );
        
        // Player 1 Configuration
        Player1Car = config.Bind(
            "Player 1",
            "Car",
            "null",
            "Player 1's car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        // Player 2 Configuration
        Player2Car = config.Bind(
            "Player 2",
            "Car",
            "null",
            "Player 2's car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        // Player 3 Configuration
        Player3Car = config.Bind(
            "Player 3",
            "Car",
            "null",
            "Player 3's car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        // Player 4 Configuration
        Player4Car = config.Bind(
            "Player 4",
            "Car",
            "null",
            "Player 4's car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
    }
    
    public void UpdateAvailableCarsList(IEnumerable<string> carNames)
    {
        if (carNames == null || !carNames.Any())
        {
            AvailableCarsList.Value = "none";
            return;
        }
        
        // Convert all to lowercase and join with commas
        var lowerCaseNames = carNames.Select(name => name.ToLower()).OrderBy(name => name);
        AvailableCarsList.Value = string.Join(", ", lowerCaseNames);
        configFile.Save();
    }
    
    public string GetCarForPlayer(int playerNumber, HashSet<string> usedCars, HashSet<string> availableCars)
    {
        ConfigEntry<string> carEntry = playerNumber switch
        {
            1 => Player1Car,
            2 => Player2Car,
            3 => Player3Car,
            4 => Player4Car,
            _ => null
        };
        
        if (carEntry == null) return null;
        
        string carName = carEntry.Value?.ToLower().Trim();
        
        // Skip null, empty, or "null" string
        if (string.IsNullOrEmpty(carName) || carName == "null")
            return null;
        
        // Check if car exists in available cars
        if (!availableCars.Contains(carName))
            return null;
        
        return carName;
    }
}
