#if UNITY_EDITOR

using System.Globalization;
using Build1.UnityConfig.Editor.Config;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Window;
using UnityEditor;

namespace Build1.UnityConfig.Editor.Metadata
{
    public sealed class MetadataWindow : EGUIWindow
    {
        private ConfigNodeMetadata _data;
        private ConfigEditorModel  _model;

        /*
         * Protected.
         */
        
        protected override void OnInitialize()
        {
            if (_data == null || _model == null)
                Close();
        }
        
        protected override void OnFocusLost()
        {
            Close();
        }
        
        protected override void OnEGUI()
        {
            var dto = _data;
            
            EGUI.Property(dto, dto.Note, nameof(dto.Note));

            EGUI.Enabled(false, () =>
            {
                EGUI.Property(dto, dto.LastChangeAuthor, nameof(dto.LastChangeAuthor));

                EGUI.Horizontally(() =>
                {
                    EGUI.Label("Last Change:", EGUI.Width(EGUI.PropertyLabelWidth));
                    EGUI.TextField(dto.LastChangeDate.ToString(CultureInfo.InvariantCulture));
                });
            });

            if (_model.Settings.Mode == ConfigMode.Decomposed)
            {
                EGUI.Space(20);
            
                EGUI.Property(dto, dto.Disabled, nameof(dto.Disabled));
                EGUI.MessageBox("Disabling allows to hide this section from editor config. This is useful for multiple A/B/C tests config storing.", MessageType.Info);    
            }
        }

        /*
         * Private.
         */

        private MetadataWindow SetData(ConfigNodeMetadata data, ConfigEditorModel model)
        {
            _data = data;
            _model = model;
            return this;
        }

        /*
         * Static.
         */

        internal static void Open(ConfigNode node, ConfigEditorModel model)
        {
            if (node.Metadata == null)
                node.UpdateMetadata();
            
            EGUI.Window<MetadataWindow>("Metadata", true)
                .Size(640, 480)
                .Get()
                .SetData(node.Metadata, model);
        }
    }
}

#endif