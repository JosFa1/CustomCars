using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCars;

public class CarConfig
{
    private ConfigFile configFile;
    
    // Player car override settings
    public ConfigEntry<string> Player1Car1;
    public ConfigEntry<string> Player1Car2;
    public ConfigEntry<string> Player1Car3;
    public ConfigEntry<string> Player1Car4;
    
    public ConfigEntry<string> Player2Car1;
    public ConfigEntry<string> Player2Car2;
    public ConfigEntry<string> Player2Car3;
    public ConfigEntry<string> Player2Car4;
    
    public ConfigEntry<string> Player3Car1;
    public ConfigEntry<string> Player3Car2;
    public ConfigEntry<string> Player3Car3;
    public ConfigEntry<string> Player3Car4;
    
    public ConfigEntry<string> Player4Car1;
    public ConfigEntry<string> Player4Car2;
    public ConfigEntry<string> Player4Car3;
    public ConfigEntry<string> Player4Car4;
    
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
        Player1Car1 = config.Bind(
            "Player 1",
            "FirstChoice",
            "null",
            "Player 1's first choice car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        Player1Car2 = config.Bind(
            "Player 1",
            "SecondChoice",
            "null",
            "Player 1's second choice car override (used if first choice is taken or unavailable)"
        );
        
        Player1Car3 = config.Bind(
            "Player 1",
            "ThirdChoice",
            "null",
            "Player 1's third choice car override (used if first and second choices are taken or unavailable)"
        );
        
        Player1Car4 = config.Bind(
            "Player 1",
            "FourthChoice",
            "null",
            "Player 1's fourth choice car override (used if first, second, and third choices are taken or unavailable)"
        );
        
        // Player 2 Configuration
        Player2Car1 = config.Bind(
            "Player 2",
            "FirstChoice",
            "null",
            "Player 2's first choice car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        Player2Car2 = config.Bind(
            "Player 2",
            "SecondChoice",
            "null",
            "Player 2's second choice car override"
        );
        
        Player2Car3 = config.Bind(
            "Player 2",
            "ThirdChoice",
            "null",
            "Player 2's third choice car override"
        );
        
        Player2Car4 = config.Bind(
            "Player 2",
            "FourthChoice",
            "null",
            "Player 2's fourth choice car override"
        );
        
        // Player 3 Configuration
        Player3Car1 = config.Bind(
            "Player 3",
            "FirstChoice",
            "null",
            "Player 3's first choice car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        Player3Car2 = config.Bind(
            "Player 3",
            "SecondChoice",
            "null",
            "Player 3's second choice car override"
        );
        
        Player3Car3 = config.Bind(
            "Player 3",
            "ThirdChoice",
            "null",
            "Player 3's third choice car override"
        );
        
        Player3Car4 = config.Bind(
            "Player 3",
            "FourthChoice",
            "null",
            "Player 3's fourth choice car override"
        );
        
        // Player 4 Configuration
        Player4Car1 = config.Bind(
            "Player 4",
            "FirstChoice",
            "null",
            "Player 4's first choice car override (use exact lowercase name from CarList above, or 'null' for default)"
        );
        
        Player4Car2 = config.Bind(
            "Player 4",
            "SecondChoice",
            "null",
            "Player 4's second choice car override"
        );
        
        Player4Car3 = config.Bind(
            "Player 4",
            "ThirdChoice",
            "null",
            "Player 4's third choice car override"
        );
        
        Player4Car4 = config.Bind(
            "Player 4",
            "FourthChoice",
            "null",
            "Player 4's fourth choice car override"
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
        ConfigEntry<string>[] choices = playerNumber switch
        {
            1 => new[] { Player1Car1, Player1Car2, Player1Car3, Player1Car4 },
            2 => new[] { Player2Car1, Player2Car2, Player2Car3, Player2Car4 },
            3 => new[] { Player3Car1, Player3Car2, Player3Car3, Player3Car4 },
            4 => new[] { Player4Car1, Player4Car2, Player4Car3, Player4Car4 },
            _ => null
        };
        
        if (choices == null) return null;
        
        // Try each choice in order
        foreach (var choice in choices)
        {
            string carName = choice.Value?.ToLower().Trim();
            
            // Skip null, empty, or "null" string
            if (string.IsNullOrEmpty(carName) || carName == "null")
                continue;
            
            // Check if car exists in available cars
            if (!availableCars.Contains(carName))
                continue;
            
            // Found a valid car! (No longer checking if already used - multiple players can use same car)
            return carName;
        }
        
        // No valid override found
        return null;
    }
}
