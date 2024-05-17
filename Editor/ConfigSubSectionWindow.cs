#if UNITY_EDITOR

using System;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Window;
using UnityEngine;

namespace Build1.UnityConfig.Editor
{
    public sealed class ConfigSubSectionWindow : EGUIWindow
    {
        private ConfigSubSectionEditor _editor;
        private Vector2                _scrollPosition = new(0, 1);

        /*
         * Public.
         */

        public ConfigSubSectionWindow Initialize(ConfigSubSectionEditor editor)
        {
            _editor = editor;
            return this;
        }

        protected override void OnFocusLost()
        {
            Close();
        }

        public new ConfigSubSectionWindow Show()
        {
            base.Show();
            return this;
        }

        /*
         * Protected.
         */

        protected override void OnEGUI()
        {
            Padding = 10;

            EGUI.Scroll(ref _scrollPosition, () =>
            {
                try
                {
                    _editor.OnEGUI();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Close();
                }
            });
            
            EGUI.Space(10);
            EGUI.Horizontally(() =>
            {
                EGUI.Space();
                EGUI.Button("Close", EGUI.Size(120, EGUI.ButtonHeight02)).OnClick(Close);
            });
        }
        
        /*
         * Static.
         */

        public static void Open(ConfigSubSectionEditor sectionEditor)
        {
            EGUI.Window<ConfigSubSectionWindow>(sectionEditor.SubSectionName, true)
                .Size(850, 950)
                .Get()
                .Initialize(sectionEditor);
        }
    }
}

#endif