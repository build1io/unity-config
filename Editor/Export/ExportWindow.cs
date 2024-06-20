#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using Build1.UnityConfig.Editor.Config;
using Build1.UnityConfig.Editor.Json;
using Build1.UnityConfig.Editor.Processors;
using Build1.UnityConfig.Utils;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Window;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Export
{
    internal sealed class ExportWindow : EGUIWindow
    {
        private ConfigEditorModel          _model;
        private Dictionary<string, string> _jsonPropertyNames;

        /*
         * Protected.
         */
            
        protected override void OnInitialize()
        {
            if (_model == null)
            {
                Close();
                return;
            }
            
            ConfigAssetsPostProcessor.onAssetsPostProcessed += OnReset;
            _model.OnReset += OnReset;

            _jsonPropertyNames = new Dictionary<string, string>();
            
            var properties = _model.SelectedConfig.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var jsonPropertyNames = new Dictionary<string, string>(properties.Length);
            
            foreach (var property in properties)
            {
                var jsonAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonAttr != null)
                    jsonPropertyNames.Add(property.Name, jsonAttr.PropertyName);
            }

            _jsonPropertyNames = jsonPropertyNames;
        }
        
        protected override void OnEGUI()
        {
            EGUI.PropertyList<string>(_model, _model.SelectedConfigSections, nameof(_model.SelectedConfigSections))
                .Title("Sections", EGUI.FontStyle(FontStyle.Bold))
                .OnItemRender(renderer =>
                 {
                     EGUI.Horizontally(() =>
                     {
                         var jsonPropertyName = _jsonPropertyNames[renderer.Item];
                         
                         EGUI.Label(renderer.Item, EGUI.Size(140, EGUI.ButtonHeight02));
                         EGUI.Label($"[\"{jsonPropertyName}\"]", EGUI.Height(EGUI.ButtonHeight02));
                         EGUI.Space();
                         
                         EGUI.Button("Copy Comp.", EGUI.Size(100, EGUI.ButtonHeight02)).OnClick(() =>
                         {
                             var section = ConfigEditorModel.GetSection(_model.SelectedConfig, _model.SelectedConfigSections.IndexOf(renderer.Item), out _);
                             EGUI.CopyToClipboard(section.ToJson(false).Compress());
                         });
                         
                         EGUI.Button("Copy Min.", EGUI.Size(100, EGUI.ButtonHeight02)).OnClick(() =>
                         {
                             var section = ConfigEditorModel.GetSection(_model.SelectedConfig, _model.SelectedConfigSections.IndexOf(renderer.Item), out _);
                             EGUI.CopyToClipboard(section.ToJson(false));
                         });
                         
                         EGUI.Button("Copy", EGUI.Size(100, EGUI.ButtonHeight02)).OnClick(() =>
                         {
                             var section = ConfigEditorModel.GetSection(_model.SelectedConfig, _model.SelectedConfigSections.IndexOf(renderer.Item), out _);
                             EGUI.CopyToClipboard(section.ToJson(true));
                         });
                         
                         EGUI.Button("View", EGUI.Size(100, EGUI.ButtonHeight02)).OnClick(() =>
                         {
                             var section = ConfigEditorModel.GetSection(_model.SelectedConfig, _model.SelectedConfigSections.IndexOf(renderer.Item), out var name);
                             JsonViewer.Open(name, section.ToJson(false));
                         });
                     });    
                 })
                .OnItemAddAvailable(() => false)
                .NoPanel()
                .ReadOnly()
                .Build();
            
            EGUI.Space();
                
            EGUI.Label("Complete Config", EGUI.FontStyle(FontStyle.Bold));
            EGUI.Space(3);
            
            EGUI.Horizontally(() =>
            {
                EGUI.Button("Copy Json Comp.", EGUI.Height(EGUI.ButtonHeight01)).OnClick(() =>
                {
                    EGUI.CopyToClipboard(_model.SelectedConfig.ToJson(false).Compress());
                });
                         
                EGUI.Button("Copy Json Min.", EGUI.Height(EGUI.ButtonHeight01)).OnClick(() =>
                {
                    EGUI.CopyToClipboard(_model.SelectedConfig.ToJson(false));
                });
                         
                EGUI.Button("Copy Json", EGUI.Height(EGUI.ButtonHeight01)).OnClick(() =>
                {
                    EGUI.CopyToClipboard(_model.SelectedConfig.ToJson(true));
                });
                         
                EGUI.Button("View Json", EGUI.Height(EGUI.ButtonHeight01)).OnClick(() =>
                {
                    JsonViewer.Open(_model.SelectedConfigName, _model.SelectedConfig.ToJson(false));             
                });
                
                EGUI.Button("Close", EGUI.Size(120, EGUI.ButtonHeight01)).OnClick(Close);
            });
        }
        
        /*
         * Private.
         */
        
        private ExportWindow SetConfig(ConfigEditorModel model)
        {
            _model = model;
            return this;
        }

        private void OnReset()
        {
            ConfigAssetsPostProcessor.onAssetsPostProcessed -= OnReset;
            
            if (_model != null)
                _model.OnReset -= OnReset;
            
            Close();
        }
        
        /*
         * Static.
         */

        public static void Open(ConfigEditorModel model)
        {
            const int itemHeight = 45;

            var sectionsCount = model.SelectedConfigSections.Count;
            var windowHeight = sectionsCount * itemHeight + 150;
            
            EGUI.Window<ExportWindow>("Config Export", true)
                .Size(850, windowHeight)
                .Get()
                .SetConfig(model);
        }
    }
}

#endif