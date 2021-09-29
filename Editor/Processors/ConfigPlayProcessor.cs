#if UNITY_EDITOR

using UnityEditor;

namespace Build1.UnityConfig.Editor.Processors
{
    [InitializeOnLoad]
    internal static class ConfigPlayProcessor
    {
        static ConfigPlayProcessor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                ConfigProcessor.CheckRequiredResources();
        }
    }
}

#endif