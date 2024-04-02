#if UNITY_EDITOR || !UNITY_WEBGL

using System;
using Firebase;

namespace Build1.UnityConfig
{
    internal static class ConfigExceptionsUtil
    {
        public static ConfigException ToConfigException(this Exception exception)
        {
            var error = ConfigError.Unknown;

            var baseException = exception.GetBaseException();
            if (baseException is FirebaseException firebaseException)
                error = (ConfigError)firebaseException.ErrorCode;
            
            return exception as ConfigException ?? new ConfigException(error);
        }
    }
}

#endif