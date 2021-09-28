#if UNITY_EDITOR

using Build1.UnityConfig.Editor.Config.States;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Types;
using Build1.UnityEGUI.Window;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config
{
    public sealed class ConfigWindow : EGUIWindow
    {
        private ConfigModel _model;
        private State       _stateDefault;
        private State       _stateConfigView;

        protected override void OnAwake()
        {
            Padding = 10;
        }

        protected override void OnInitialize()
        {
            _model ??= new ConfigModel();
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
            EGUIWindow.Open<ConfigWindow>($"{Application.productName} Config", 800, 1000, false, true);
        }
    }
}

#endif