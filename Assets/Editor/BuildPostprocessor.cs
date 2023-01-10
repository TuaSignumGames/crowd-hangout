using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS

using UnityEditor.iOS.Xcode;

#endif

using System.IO;

public class BuildPostprocessor
{

    #if UNITY_IOS

    private static PlistDocument plistDocument;

    #endif

    private static string appTrackingDescription = "Your data will be used to provide you a better and personalized ad experience.";

    [PostProcessBuild(0)]
    public static void OnBuildPostProcessing(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            #if UNITY_IOS

            AddPlistItems(buildPath + "/Info.plist");

            #endif
        }
    }

    #if UNITY_IOS

    private static void AddPlistItems(string plistPath)
    {
        plistDocument = new PlistDocument();

        plistDocument.ReadFromFile(plistPath);

        AddPlistATTRequest(appTrackingDescription);
        AddPlistSKAdNetworkIdentifiers(new string[] { "v9wttpbfk9.skadnetwork", "n38lu8286q.skadnetwork" });
        AddPlistAppsFlyerEndpoint("https://appsflyer-skadnetwork.com/");

        File.WriteAllText(plistPath, plistDocument.WriteToString());
    }

    private static void AddPlistATTRequest(string description)
    {
        plistDocument.root.SetString("NSUserTrackingUsageDescription", description);
    }

    private static void AddPlistSKAdNetworkIdentifiers(string[] identifiers)
    {
        PlistElementArray identifiersArray = plistDocument.root.CreateArray("SKAdNetworkItems");

        for (int i = 0; i < identifiers.Length; i++)
        {
            identifiersArray.AddDict().SetString("SKAdNetworkIdentifier", identifiers[i]);
        }
    }

    private static void AddPlistAppsFlyerEndpoint(string url)
    {
        plistDocument.root.SetString("NSAdvertisingAttributionReportEndpoint", url);
    }

    #endif
}