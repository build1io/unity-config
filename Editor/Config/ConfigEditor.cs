#if UNITY_EDITOR

using Build1.UnityConfig.Editor.Config.States;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Components.Title;
using Build1.UnityEGUI.Window;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config
{
    internal sealed class ConfigEditor : EGUIWindow
    {
        protected override bool Initialized => _model != null;
        
        private ConfigEditorModel _model;
        private ConfigEditorState _stateDefault;
        private ConfigEditorState _stateConfigView;

        protected override void OnInitialize()
        {
            if (_model == null)
            {
                _model = new ConfigEditorModel();
                _model.OnReset += Reset;
                _model.OnConfigRemoved += Reset;
                _model.OnSectionChanged += Reset;
                _model.OnSectionReverted += Reset;
            }
            
            _stateDefault ??= new DefaultState(_model);
            _stateConfigView ??= new ConfigState(_model);
        }

        protected override void OnEGUI()
        {
            EGUI.Title($"{Application.productName} Config", TitleType.H1, EGUI.OffsetX(5));
            EGUI.Space(10);

            if (Application.isPlaying)
            {
                EGUI.MessageBox("Config editing disabled in Play mode.", MessageType.Warning);
                EGUI.Space(10);
            }

            EGUI.Enabled(!_model.InProgress && !Application.isPlaying, () =>
            {
                if (_model.ConfigSelected)
                    _stateConfigView.Draw();
                else
                    _stateDefault.Draw();
            });
        }

        private void Reset()
        {
            _stateDefault?.Reset();
            _stateConfigView?.Reset();
            
            EGUI.PropertyWindowCloseAll();
        }

        /*
         * Static.
         */

        public static void Open()
        {
            EGUI.Window<ConfigEditor>($"{Application.productName} Config", false)
                .Size(800, 1000)
                .Get();
        }
    }
}

#endif