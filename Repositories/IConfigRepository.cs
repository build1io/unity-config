using System;

namespace Build1.UnityConfig.Repositories
{
    public interface IConfigRepository
    {
        void Load<T>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode;
    }
}