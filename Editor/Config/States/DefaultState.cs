#if UNITY_EDITOR

using System.Linq;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Types;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config.States
{
    internal sealed class DefaultState : ConfigEditorState
    {
        private static Vector2 _scrollPosition      = new Vector2(0, 1);
        private static int     _selectedConfigIndex = -1;

        private static string _configName          = string.Empty;
        private static int    _configCopyFromIndex = 0;
        private static bool   _configNameInvalid   = false;
        private static bool   _configAlreadyExists = false;
        
        public DefaultState(ConfigEditorModel model) : base(model)
        {
        }

        public override void Draw()
        {
            var configs = model.Configs;
            var currentConfigSourceIndex = configs.IndexOf(model.ConfigSource);
            
            EGUI.Scroll(ref _scrollPosition, () =>
            {
                EGUI.Title("Config Source", TitleType.H3, 5);
                EGUI.Space(5);
                
                EGUI.Label("Selected config (any except Firebase) will be included and used in the build.\nFirebase will make app load config from Firebase remote.");
                EGUI.Space(3);
                
                EGUI.DropDown(configs, currentConfigSourceIndex, EGUI.DropDownHeight01, newIndex =>
                {
                    model.SetConfigSource(configs[newIndex]);
                });
                EGUI.Space(18);

                EGUI.Checkbox("Reset to Firebase in Release builds", model.ConfigSourceResetEnabled, resetSelected =>
                {
                    model.SetConfigSourceResetEnabled(resetSelected);
                });
                EGUI.Space(40);

                configs = configs.Concat(new[] { "New..." }).ToList();

                EGUI.Title("Config Editor", TitleType.H3, 5);
                EGUI.Space(5);
                
                EGUI.Label("Select config to view / edit.");
                EGUI.Space(5);
                
                EGUI.MessageBox("Firebase config can't be edited from Unity Editor but can be used for another config creation.", MessageType.Info);

                EGUI.SelectionGrid(configs, ref _selectedConfigIndex, 220, 3, 10);
                
                if (_selectedConfigIndex == configs.Count - 1)
                {
                    OnAddConfig();
                    return;
                }

                if (_selectedConfigIndex <= -1) 
                    return;
                
                var configName = configs[_selectedConfigIndex];
                model.SelectConfig(configName);
                
                _selectedConfigIndex = -1;
            });
        }

        private void OnAddConfig()
        {
            EGUI.Space(30);
            
            EGUI.Title("New Config", TitleType.H3, 5);
            EGUI.Space(5);

            var add = false;
            var controlName = string.Empty;
            var configs = model.Configs.Concat(new[] { "None" }).ToList();
            
            EGUI.Horizontally(() =>
            {
                EGUI.Label("Name:", 80, EGUI.DropDownHeight02, false, TextAnchor.MiddleLeft);
                EGUI.TextField(_configName, EGUI.DropDownHeight02, TextAnchor.MiddleLeft, out controlName, configName =>
                {
                    _configName = configName;
                    _configNameInvalid = false;
                    _configAlreadyExists = false;
                });
            });
            
            EGUI.Horizontally(() =>
            {
                EGUI.Label("Copy from:", 80, EGUI.DropDownHeight02, false, TextAnchor.MiddleLeft);
                EGUI.DropDown(configs, ref _configCopyFromIndex, EGUI.DropDownHeight02);    
            });
            
            string configSourceName = null;
            if (_configCopyFromIndex < configs.Count - 1)
                configSourceName = configs[_configCopyFromIndex];

            EGUI.Focus(controlName);
            
            EGUI.Horizontally(() =>
            {
                if (_configNameInvalid)
                {
                    EGUI.Space(8);
                    EGUI.Label("Invalid name. Can't be empty of whitespace. No special symbols or spaces allowed.", EGUI.ButtonHeight01, LabelType.Error, true, TextAnchor.MiddleCenter);
                    EGUI.Space(8);
                }
                else if (_configAlreadyExists)
                {
                    EGUI.Space(8);
                    EGUI.Label($"Config with name '{_configName}' already exist.", EGUI.ButtonHeight01, LabelType.Error, true, TextAnchor.MiddleCenter);
                    EGUI.Space(8);
                }
                else
                {
                    EGUI.Space();
                }
                
                EGUI.Button("Create", out add, 120, EGUI.ButtonHeight01);
            });
            
            if (!add)
                return;

            _configNameInvalid = !model.CheckConfigNameValid(_configName);
            if (_configNameInvalid)
                return;

            _configAlreadyExists = model.CheckConfigExists(_configName);
            if (_configAlreadyExists)
                return;
            
            model.AddConfig(_configName, configSourceName);

            _configName = string.Empty;
            _selectedConfigIndex = -1;
            _configCopyFromIndex = 0;
        }
    }
}

#endif