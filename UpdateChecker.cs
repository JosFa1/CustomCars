using BepInEx.Logging;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomCars;

public class UpdateChecker : MonoBehaviour
{
    private const string GITHUB_REPO = "JosFa1/CustomCars";
    private const string GITHUB_API_URL = "https://api.github.com/repos/" + GITHUB_REPO + "/releases/latest";
    
    private ManualLogSource logger;
    private string currentVersion;
    private Action<string, string> onUpdateAvailable; // Callback: (latestVersion, releaseUrl)
    
    public void Initialize(ManualLogSource log, string version, Action<string, string> callback)
    {
        logger = log;
        currentVersion = version;
        onUpdateAvailable = callback;
    }
    
    public void CheckForUpdates()
    {
        StartCoroutine(CheckForUpdatesCoroutine());
    }
    
    private IEnumerator CheckForUpdatesCoroutine()
    {
        logger.LogInfo("Checking for updates...");
        
        using (UnityWebRequest request = UnityWebRequest.Get(GITHUB_API_URL))
        {
            // GitHub API requires User-Agent header
            request.SetRequestHeader("User-Agent", "CustomCars-BepInEx-Mod");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                logger.LogWarning($"Failed to check for updates: {request.error}");
                yield break;
            }
            
            try
            {
                string jsonResponse = request.downloadHandler.text;
                GitHubRelease release = ParseGitHubRelease(jsonResponse);
                
                if (release == null)
                {
                    logger.LogWarning("Failed to parse GitHub release information");
                    yield break;
                }
                
                logger.LogInfo($"Latest version on GitHub: {release.tag_name}");
                logger.LogInfo($"Current version: {currentVersion}");
                
                if (IsNewerVersion(release.tag_name, currentVersion))
                {
                    logger.LogInfo("New update available!");
                    onUpdateAvailable?.Invoke(release.tag_name, release.html_url);
                }
                else
                {
                    logger.LogInfo("You are running the latest version");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing update check: {ex.Message}");
            }
        }
    }
    
    private GitHubRelease ParseGitHubRelease(string json)
    {
        try
        {
            // Simple JSON parsing without external dependencies
            GitHubRelease release = new GitHubRelease();
            
            // Extract tag_name
            int tagIndex = json.IndexOf("\"tag_name\"");
            if (tagIndex != -1)
            {
                int colonIndex = json.IndexOf(":", tagIndex);
                int quoteStart = json.IndexOf("\"", colonIndex + 1);
                int quoteEnd = json.IndexOf("\"", quoteStart + 1);
                release.tag_name = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            }
            
            // Extract html_url
            int urlIndex = json.IndexOf("\"html_url\"");
            if (urlIndex != -1)
            {
                int colonIndex = json.IndexOf(":", urlIndex);
                int quoteStart = json.IndexOf("\"", colonIndex + 1);
                int quoteEnd = json.IndexOf("\"", quoteStart + 1);
                release.html_url = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            }
            
            return release;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error parsing GitHub release JSON: {ex.Message}");
            return null;
        }
    }
    
    private bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        // Remove 'v' prefix if present
        latestVersion = latestVersion.TrimStart('v', 'V');
        currentVersion = currentVersion.TrimStart('v', 'V');
        
        try
        {
            Version latest = new Version(latestVersion);
            Version current = new Version(currentVersion);
            return latest > current;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Failed to compare versions: {ex.Message}");
            return false;
        }
    }
    
    private class GitHubRelease
    {
        public string tag_name;
        public string html_url;
    }
}
