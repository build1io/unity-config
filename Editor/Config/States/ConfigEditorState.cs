#if UNITY_EDITOR

namespace Build1.UnityConfig.Editor.Config.States
{
    internal abstract class ConfigEditorState
    {
        protected readonly ConfigEditorModel model;

        protected ConfigEditorState(ConfigEditorModel model)
        {
            this.model = model;
        }

        public abstract void Draw();
        public abstract void Reset();
    }
}

#endif