#if UNITY_IOS
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class UniWebViewPostBuildProcessorIOS
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS) {
            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            
            var settings = UniWebViewEditorSettings.GetOrCreateSettings();
            if (settings.authCallbackUrls.Length > 0) {
                var domains = GetHttpsAssociatedDomains(settings.authCallbackUrls);
                if (domains.Length > 0) {
                    Debug.Log("<UniWebView> UniWebView Post Build Script is patching associated domains for auth callbacks...");
                    AddAssociatedDomain(projectPath, domains);
                }
            }
        }
    }

    public static string[] GetHttpsAssociatedDomains(string[] urls) {
        return urls
            .Where(url => Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.Scheme == "https")
            .Select(url => new Uri(url).Host)
            .Distinct()
            .Select(domain => "applinks:" + domain)
            .ToArray();
    }

    public static void AddAssociatedDomain(string projectPath, string[] domains) {
        
        PBXProject project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));

        var entitlementsFileName = "Unity-iPhone.entitlements";
        var targetGUID = project.GetUnityMainTargetGuid();
        var capabilityManager = new ProjectCapabilityManager(projectPath, entitlementsFileName, null, targetGUID);

        capabilityManager.AddAssociatedDomains(domains);

        capabilityManager.WriteToFile();
    }
}
#endif