using System;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ServesTypeAttribute : Attribute
    {
        public Type[] ServesTypes { get; }
        
        public ServesTypeAttribute(params Type[] servesTypes)
        {
            this.ServesTypes = servesTypes;
        }
    }

    internal class SwEditorDependenciesInjector
    {
        private static class Keys
        {
            internal const string DEPENDENCIES_KEY = "dependencies";
            internal const string UNITY = "unity";
            internal const string VERSION = "version";
            internal const string DESCRIPTION = "description";
            internal const string DISPLAY_NAME = "displayName";
            internal const string NAME = "name";
        }

        protected virtual SwJsonDictionary GetDependenciesToInject()
        {
            return new SwJsonDictionary();
        }
        
        public void InjectPackageJsonDependenciesIfNeeded()
        {
            var requestedDependencies = GetDependenciesToInject();

            if (requestedDependencies?.SwIsEmpty() ?? true) return;

            var directorySeparatorChar = SwFileUtils.DirectorySeparatorChar;
            var swPackagesFolderPath = Application.dataPath.Replace($"{directorySeparatorChar}Assets", $"{directorySeparatorChar}Packages{directorySeparatorChar}SupersonicWisdom");

            if (!SwFileUtils.CreateDirectory(swPackagesFolderPath)) return;

            var versionString = SwUtils.ComputeVersionId(SwConstants.SdkVersion) == 0 ? "1.0.0" : SwConstants.SdkVersion;
            var swPackageJsonFilePath = $"{swPackagesFolderPath}{directorySeparatorChar}package.json";

            var requiredJsonDictionary = new SwJsonDictionary
            {
                {Keys.NAME, "com.supersonic.wisdom"},
                {Keys.DISPLAY_NAME, "Supersonic Wisdom"},
                {Keys.DESCRIPTION, "Supersonic Wisdom Unity SDK dependencies"},
                {Keys.VERSION, versionString},
                {Keys.UNITY, "2020.3"},
                {
                    Keys.DEPENDENCIES_KEY, requestedDependencies
                },
            };

            var requiredJsonString = requiredJsonDictionary.SwToJsonString();

            try
            {
                var isPackageJsonFileExists = new FileInfo(swPackageJsonFilePath).Exists;

                if (isPackageJsonFileExists)
                {
                    var swCurrentPackageJsonContent = File.ReadAllText(swPackageJsonFilePath);
                    var currentJsonDictionary = SwJsonDictionary.Parse(swCurrentPackageJsonContent) ?? new SwJsonDictionary();
                    
                    // Quit if the the required and the current are identical
                    if (currentJsonDictionary.CompareTo(requiredJsonDictionary) == 0) return;
                    
                    SwFileUtils.DeleteFilesAtPaths(new[] {swPackageJsonFilePath});
                }
                
                File.WriteAllText(swPackageJsonFilePath, requiredJsonString);

                SwEditorLogger.Log("Resolving project's packages with Package Manager in order to fetch Wisdom 3rd party dependencies...");
                Client.Resolve();
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }
        }
    }
}

