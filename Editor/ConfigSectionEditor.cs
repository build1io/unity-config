#if UNITY_EDITOR

using UnityEngine;

namespace Build1.UnityConfig.Editor
{
    public abstract class ConfigSectionEditor
    {
        public abstract string SectionName { get; }
        
        public abstract void   OnEGUI(object dto);
        public abstract string OnValidate(object dto);
    }

    public abstract class ConfigSectionEditor<T> : ConfigSectionEditor where T : ConfigNode
    {
        protected T Data { get; private set; }

        public override void OnEGUI(object dto)
        {
            try
            {
                Data = (T)dto;
            }
            catch
            {
                Debug.LogError($"Wrong section name for {GetType().FullName} [\"{SectionName}\"]. Cast type failed: {dto.GetType().FullName} != {typeof(T).FullName}");
                return;
            }
            
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