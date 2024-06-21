#if UNITY_EDITOR

using System.Globalization;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Window;

namespace Build1.UnityConfig.Editor.Metadata
{
    public sealed class MetadataWindow : EGUIWindow
    {
        private ConfigNodeMetadata _data;

        /*
         * Protected.
         */
        
        protected override void OnInitialize()
        {
            if (_data == null)
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
        }

        /*
         * Private.
         */

        private MetadataWindow SetData(ConfigNodeMetadata data)
        {
            _data = data;
            return this;
        }

        /*
         * Static.
         */

        public static void Open(ConfigNode node)
        {
            if (node.Metadata == null)
                node.UpdateMetadata();
            
            EGUI.Window<MetadataWindow>("Metadata", true)
                .Size(640, 480)
                .Get()
                .SetData(node.Metadata);
        }
    }
}

#endif