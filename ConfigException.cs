using System;
using Firebase;

namespace Build1.UnityConfig
{
    public sealed class ConfigException : Exception
    {
        public readonly ConfigError error;

        internal ConfigException(ConfigError error) : base(error.ToString())
        {
            this.error = error;
        }

        internal ConfigException(ConfigError error, string message) : base($"{error} [{message}]")
        {
            this.error = error;
        }

        internal static ConfigException FromException(Exception exception)
        {
            var error = ConfigError.Unknown;

            var baseException = exception.GetBaseException();
            if (baseException is FirebaseException firebaseException)
                error = (ConfigError)firebaseException.ErrorCode;

            return exception as ConfigException ?? new ConfigException(error);
        }
    }
}