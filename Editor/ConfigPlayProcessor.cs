#if UNITY_EDITOR

using UnityEditor;

namespace Build1.UnityConfig.Editor
{
    [InitializeOnLoad]
    public static class ConfigPlayProcessor
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