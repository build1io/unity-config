#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Processors
{
    [InitializeOnLoad]
    internal static class ConfigProcessor
    {
        private static readonly string configSourceFilePath       = Application.dataPath + "/Resources/" + ConfigSource.FileName + ".txt";
        private static readonly string resourcesFolderPath        = Application.dataPath + "/Resources/";
        private static readonly string configSourceResetFilePath  = Application.dataPath + "/Config/config-source-reset.txt";
        private static readonly string configEmbedDefaultFilePath = Application.dataPath + "/Config/config-embed-default.txt";

        public static event Action             OnConfigSourceChanged;
        public static event Action<ConfigNode> OnConfigSaving;
        public static event Action<ConfigNode> OnConfigSaved;

        static ConfigProcessor()
        {
            CheckRequiredResources();
        }

        /*
         * Resources.
         */

        public static void CheckRequiredResources()
        {
            CheckConfigFolder();
            CheckConfigSourceFile();
        }

        private static void CheckConfigFolder()
        {
            var configsRootPath = GetEditorConfigsRootFolderPath();
            if (Directory.Exists(configsRootPath))
                return;

            Log("Configs folder not found. Creating...");
            Directory.CreateDirectory(configsRootPath);
            SetResetConfigSourceForReleaseBuilds(true);
            Log("Done");
        }

        private static void CheckConfigSourceFile()
        {
            if (File.Exists(configSourceFilePath))
            {
                var configSource = GetConfigSource();
                var configs = GetConfigs();
                if (configs.Contains(configSource))
                    return;
            }

            Log("Config file not found. Creating...");
            Log($"Path: {configSourceFilePath}");
            SetConfigSource(ConfigSource.Default);
            Log("Done.");
        }

        /*
         * Config Source.
         */

        public static string GetConfigSource()
        {
            return ConfigSource.Get();
        }

        public static void SetConfigSource(string source)
        {
            File.WriteAllText(configSourceFilePath, source);

            var configResourcePath = resourcesFolderPath + "config.json";
            if (File.Exists(configResourcePath))
                File.Delete(configResourcePath);

            var configResourcePathMeta = resourcesFolderPath + "config.json.meta";
            if (File.Exists(configResourcePathMeta))
                File.Delete(configResourcePathMeta);

            // Firebase is remote, so we don't need to embed anything into the build.
            if (source != ConfigSource.Firebase)
                File.Copy(GetEditorConfigFilePath(source), configResourcePath);

            AssetDatabase.Refresh(ImportAssetOptions.Default);

            Log($"Config Source set to {source}");

            OnConfigSourceChanged?.Invoke();
        }

        public static bool GetConfigSourceResetEnabled()
        {
            if (!File.Exists(configSourceResetFilePath))
                return true;

            var text = File.ReadAllText(configSourceResetFilePath);
            return !bool.TryParse(text, out var enabled) || enabled;
        }

        public static void SetResetConfigSourceForReleaseBuilds(bool value)
        {
            File.WriteAllText(configSourceResetFilePath, value.ToString());

            Log(value
                    ? "Config Source reset for release builds enabled."
                    : "Config Source reset for release builds disabled.");
        }

        public static bool GetEmbedDefaultEnabled()
        {
            if (!File.Exists(configEmbedDefaultFilePath))
                return true;

            var text = File.ReadAllText(configEmbedDefaultFilePath);
            return !bool.TryParse(text, out var enabled) || enabled;
        }

        public static void SetEmbedDefaultEnabled(bool value)
        {
            File.WriteAllText(configEmbedDefaultFilePath, value.ToString());

            Log(value
                    ? "Default config embedding enabled."
                    : "Default config embedding disabled.");
        }

        /*
         * Configs.
         */

        public static List<string> GetConfigs()
        {
            var configsRootPath = GetEditorConfigsRootFolderPath();
            var directories = Directory.GetDirectories(configsRootPath);
            var res = new List<string> { ConfigSource.Firebase };
            res.AddRange(directories.Select(directory => directory.Replace(configsRootPath, "")));
            return res;
        }

        public static void AddConfig(string configName, string content, Action<string> onComplete)
        {
            var folderPath = GetEditorConfigFolderPath(configName);
            Directory.CreateDirectory(folderPath);

            var filePath = GetEditorConfigFilePath(configName);
            File.WriteAllText(filePath, content);

            AssetDatabase.Refresh(ImportAssetOptions.Default);

            onComplete?.Invoke(configName);
        }

        public static void RemoveConfig(string configName, Action<string> onComplete, Action<Exception> onError)
        {
            if (configName == ConfigSource.Firebase)
            {
                onError?.Invoke(new Exception("Firebase config can't be deleted."));
                return;
            }

            var folderPath = GetEditorConfigFolderPath(configName);
            Directory.Delete(folderPath, true);
            File.Delete(folderPath + ".meta");

            var source = GetConfigSource();
            if (source == configName)
            {
                File.Delete(resourcesFolderPath + "config.json");
                SetConfigSource(ConfigSource.Default);
            }

            AssetDatabase.Refresh(ImportAssetOptions.Default);

            onComplete?.Invoke(configName);
        }

        public static void SaveConfig(string configName, string content)
        {
            var filePath = GetEditorConfigFilePath(configName);
            File.WriteAllText(filePath, content);

            var source = GetConfigSource();
            if (source == configName && source != ConfigSource.Firebase)
                File.WriteAllText(resourcesFolderPath + "config.json", content);

            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }

        public static void OnSaving(ConfigNode config) { OnConfigSaving?.Invoke(config); }
        public static void OnSaved(ConfigNode config)  { OnConfigSaved?.Invoke(config); }

        /*
         * Paths.
         */

        public static string GetEditorConfigsRootFolderPath()
        {
            return Application.dataPath + "/Config/";
        }

        public static string GetEditorConfigFolderPath(string configName)
        {
            return $"{Application.dataPath}/Config/{configName}";
        }

        public static string GetEditorConfigFilePath(string configName)
        {
            return $"{Application.dataPath}/Config/{configName}/config.json";
        }

        /*
         * Logging.
         */

        private static void Log(string message)
        {
            Debug.Log($"Config: {message}");
        }
    }
}

#endif