#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Build1.UnityConfig.Editor.Processors;
using Build1.UnityConfig.Repositories.Firebase;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config
{
    internal sealed class ConfigEditorModel
    {
        public string ConfigSource             { get; private set; }
        public bool   ConfigSourceResetEnabled { get; private set; }

        public List<string> Configs        { get; private set; }
        public bool         ConfigSelected => SelectedConfig != null;

        public string     SelectedConfigName         { get; private set; }
        public ConfigNode SelectedConfig             { get; private set; }
        public bool       SelectedConfigCanBeSaved   => SelectedConfigName != Build1.UnityConfig.ConfigSource.Firebase;
        public bool       SelectedConfigCanBeDeleted => SelectedConfigName != Build1.UnityConfig.ConfigSource.Firebase;

        public List<string> SelectedConfigSections      { get; private set; }
        public ConfigNode   SelectedConfigSection       { get; private set; }
        public ConfigNode   SelectedConfigSectionBackup { get; private set; }
        public string       SelectedConfigSectionName   { get; private set; }
        public int          SelectedConfigSectionIndex  { get; private set; }

        public bool InProgress { get; private set; }

        public ConfigEditorModel()
        {
            ConfigAssetsPostProcessor.onAssetsPostProcessed += OnAssetsPostProcessed;
            Reset();
        }

        private void OnAssetsPostProcessed()
        {
            if (!InProgress)
                Reset();
        }

        public void Reset()
        {
            ConfigSource = ConfigProcessor.GetConfigSource();
            ConfigSourceResetEnabled = ConfigProcessor.GetConfigSourceResetEnabled();

            Configs = ConfigProcessor.GetConfigs();

            SelectedConfigName = null;
            SelectedConfig = null;

            SelectedConfigSections = null;
            SelectedConfigSection = null;
            SelectedConfigSectionBackup = null;
            SelectedConfigSectionName = null;
            SelectedConfigSectionIndex = -1;
        }

        /*
         * Config Source.
         */

        public void SetConfigSource(string source)
        {
            ConfigProcessor.SetConfigSource(source);
            ConfigSource = source;
        }

        public void SetConfigSourceResetEnabled(bool value)
        {
            ConfigProcessor.SetResetConfigSourceForReleaseBuilds(value);
            ConfigSourceResetEnabled = value;
        }

        /*
         * Config.
         */

        public void SelectConfig(string configName, Action onComplete)
        {
            InProgress = true;

            LoadConfig(configName, config =>
            {
                SelectedConfig = config;
                SelectedConfigName = configName;

                SelectedConfigSections = GetConfigSections(config);
                SelectSection(SelectedConfigSections.IndexOf(LoadLastSelectedSection()));

                InProgress = false;

                onComplete?.Invoke();
            }, exception =>
            {
                Debug.LogException(exception);
                InProgress = false;
            });
        }

        public bool CheckConfigNameValid(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && new Regex(@"^\w+$").IsMatch(name);
        }

        public bool CheckConfigExists(string name)
        {
            var configs = ConfigProcessor.GetConfigs().Select(n => n.ToLower()).ToList();
            return configs.Contains(name) || configs.Contains(name.ToLower());
        }

        public void AddConfig(string name, string sourceConfigName)
        {
            InProgress = true;

            // None.
            if (string.IsNullOrEmpty(sourceConfigName))
            {
                ConfigProcessor.AddConfig(name, "{}", OnConfigAdded);
                return;
            }

            LoadConfig(sourceConfigName, config =>
            {
                var json = config.ToJson(true);
                ConfigProcessor.AddConfig(name, json, OnConfigAdded);
            }, OnConfigFailed);
        }

        public void RemoveConfig(string name)
        {
            InProgress = true;
            ConfigProcessor.RemoveConfig(name, OnConfigRemoved, OnConfigFailed);
        }

        private void OnConfigAdded(string name)
        {
            Reset();
            InProgress = false;
        }

        private void OnConfigRemoved(string name)
        {
            Reset();
            InProgress = false;
        }

        private void OnConfigFailed(Exception exception)
        {
            Debug.LogException(exception);

            Reset();
            InProgress = false;
        }

        private static List<string> GetConfigSections(ConfigNode config)
        {
            var type = config.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Select(p => p.Name).ToList();
        }

        /*
         * Config Loading.
         */

        internal static void LoadConfig(string configName, Action<ConfigNode> onComplete, Action<Exception> onError)
        {
            // Firebase.
            if (configName == Build1.UnityConfig.ConfigSource.Firebase)
                LoadFirebaseConfig(onComplete, onError);
            else
                LoadLocalConfig(configName, onComplete, onError);
        }

        private static void LoadFirebaseConfig(Action<ConfigNode> onComplete, Action<Exception> onError)
        {
            var loader = new FirebaseRemoteConfigLoader();
            loader.onComplete += json =>
            {
                var config = (ConfigNode)JsonConvert.DeserializeObject(json, UnityConfig.Instance.ConfigType);
                onComplete?.Invoke(config);
            };
            loader.onError += exception => { onError?.Invoke(exception); };
            loader.Load();
        }

        private static void LoadLocalConfig(string configName, Action<ConfigNode> onComplete, Action<Exception> onError)
        {
            try
            {
                var path = ConfigProcessor.GetEditorConfigFilePath(configName);
                var json = File.ReadAllText(path);
                var config = (ConfigNode)JsonConvert.DeserializeObject(json, UnityConfig.Instance.ConfigType);
                onComplete?.Invoke(config);
            }
            catch (Exception exception)
            {
                onError?.Invoke(exception);
            }
        }

        /*
         * Section.
         */

        public void SelectSection(int index)
        {
            index = Math.Max(index, 0);

            SelectedConfigSectionBackup = GetSection(SelectedConfig, index, out var sectionName);
            SelectedConfigSection = CloneSection(SelectedConfigSectionBackup);

            SelectedConfigSectionName = sectionName;
            SelectedConfigSectionIndex = index;

            SaveLastSelectedSection(sectionName);
        }

        public void SaveSection()
        {
            InProgress = true;

            var config = SelectedConfig;
            var property = config.GetType().GetProperty(SelectedConfigSectionName);
            if (property == null)
            {
                Debug.LogError("Property not found: " + SelectedConfigSectionName);
                InProgress = false;
                return;
            }

            property.SetValue(config, SelectedConfigSection);

            SelectedConfigSectionBackup = SelectedConfigSection;
            SelectedConfigSection = CloneSection(SelectedConfigSectionBackup);

            ConfigProcessor.SaveConfig(SelectedConfigName, config.ToJson(true));

            InProgress = false;
        }

        public void RevertSection()
        {
            SelectedConfigSection = CloneSection(SelectedConfigSectionBackup);
        }

        public bool CheckSelectedSectionModified()
        {
            return SelectedConfigSection != null &&
                   SelectedConfigSection.ToJson(false) != SelectedConfigSectionBackup.ToJson(false);
        }

        private static ConfigNode GetSection(ConfigNode config, int index, out string name)
        {
            var type = config.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            name = properties[index].Name;

            var property = properties[index];
            var sectionRaw = property.GetValue(config) ?? Activator.CreateInstance(property.PropertyType);
            return (ConfigNode)sectionRaw;
        }

        private ConfigNode CloneSection(ConfigNode node)
        {
            return node == null ? null : ConfigNode.FromJson(node.ToJson(false), node.GetType());
        }

        private static string LoadLastSelectedSection()
        {
            return PlayerPrefs.GetString($"{Application.identifier}_LastSelectedConfigSection");
        }

        private static void SaveLastSelectedSection(string section)
        {
            PlayerPrefs.SetString($"{Application.identifier}_LastSelectedConfigSection", section);
        }
    }
}

#endif