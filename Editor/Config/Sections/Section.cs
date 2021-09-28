#if UNITY_EDITOR

namespace Build1.UnityConfig.Editor.Config.Sections
{
    public abstract class Section
    {
        public abstract void   OnEGUI(object dto);
        public abstract string OnValidate(object dto);
    }

    public abstract class Section<T> : Section where T : ConfigNode
    {
        protected T Data { get; private set; }

        public override void OnEGUI(object dto)
        {
            Data = (T)dto;
            OnEGUI(Data);
        }

        protected abstract void OnEGUI(T dto);

        public override string OnValidate(object dto)
        {
            Data = (T)dto;
            return OnValidate(Data);
        }
        
        protected abstract string OnValidate(T dto);
    }
}

#endif