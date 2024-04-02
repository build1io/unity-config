namespace Build1.UnityConfig
{
    public enum ConfigError
    {
        FirebaseRemoteConfigUnavailable = -2,
        Unknown                         = -1,
        None                            = 0,
        NetworkError                    = 1,
        ConfigFieldNotFound             = 2,
        ConfigResourceNotFound          = 3,
        ParsingError                    = 4
    }
}