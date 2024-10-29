using System;
using Firebase;

namespace Build1.UnityConfig
{
    public sealed class ConfigException : Exception
    {
        public readonly ConfigError error;

        internal ConfigException(ConfigError error, string message, Exception innerException) : base($"{error}. {message}", innerException)
        {
            this.error = error;
        }

        internal static ConfigException FromFirebaseException(Exception exception, string message = null)
        {
            var error = ConfigError.Unknown;
            
            var baseException = exception.GetBaseException();
            if (baseException is FirebaseException firebaseException)
                error = (ConfigError)firebaseException.ErrorCode;

            return new ConfigException(error, message, exception);
        }
    }
}