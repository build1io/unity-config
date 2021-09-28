using System;

namespace Build1.UnityConfig.Repositories
{
    public interface IConfigRepository
    {
        event Action<string>    onComplete;
        event Action<Exception> onError;

        void Load();
    }
}