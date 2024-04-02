using System;

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
    }
}