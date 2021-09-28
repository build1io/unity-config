#if UNITY_EDITOR

namespace Build1.UnityConfig.Editor.Config.States
{
    public abstract class State
    {
        protected readonly ConfigModel model;
        
        protected State(ConfigModel model)
        {
            this.model = model;
        }
        
        public abstract void Draw();
    }
}

#endif