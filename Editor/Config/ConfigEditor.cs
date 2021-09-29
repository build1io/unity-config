#if UNITY_EDITOR

using Build1.UnityConfig.Editor.Config.States;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Types;
using Build1.UnityEGUI.Window;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config
{
    internal sealed class ConfigEditor : EGUIWindow
    {
        private ConfigEditorModel _model;
        private ConfigEditorState _stateDefault;
        private ConfigEditorState _stateConfigView;

        protected override void OnAwake()
        {
            Padding = 10;
        }

        protected override void OnInitialize()
        {
            _model ??= new ConfigEditorModel();
            _stateDefault ??= new DefaultState(_model);
            _stateConfigView ??= new ConfigState(_model);
        }

        protected override void OnEGUI()
        {
            EGUI.Title($"{Application.productName} Config", TitleType.H1, 5);
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

        /*
         * Static.
         */

        public static void Open()
        {
            EGUIWindow.Open<ConfigEditor>($"{Application.productName} Config", 800, 1000, false, true);
        }
    }
}

#endif