using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

// TODO: manage firebase presence

namespace Build1.UnityConfig.Repositories.Firebase
{
    public sealed class FirebaseRemoteConfigLoader
    {
        public event Action<string>    onComplete;
        public event Action<Exception> onError;

        public void Load()
        {
            try
            {
                var configSettings = new ConfigSettings();
                if (Debug.isDebugBuild)
                    configSettings.MinimumFetchInternalInMilliseconds = 0; // Refresh immediately when debugging.

                FirebaseRemoteConfig.DefaultInstance.SetConfigSettingsAsync(configSettings).ContinueWithOnMainThread(OnConfigSet);
            }
            catch (Exception exception)
            {
                HandleError(exception);
            }
        }

        private void OnConfigSet(Task task)
        {
            try
            {
                FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread(OnFetchComplete);
            }
            catch (Exception exception)
            {
                HandleError(exception);
            }
        }

        private void OnFetchComplete(Task<bool> task)
        {
            if (task.IsFaulted)
            {
                HandleError(task.Exception);
                return;
            }

            try
            {
                if (!FirebaseRemoteConfig.DefaultInstance.AllValues.ContainsKey("config"))
                {
                    HandleError(new Exception("Key \"config\" not found in Firebase Remote Config."));
                    return;
                }

                var json = FirebaseRemoteConfig.DefaultInstance.AllValues["config"].StringValue;
                HandleComplete(json);
            }
            catch (Exception exception)
            {
                HandleError(exception);
            }
        }

        private void HandleComplete(string json)
        {
            onComplete?.Invoke(json);
        }

        private void HandleError(Exception exception)
        {
            onError?.Invoke(exception);
        }
    }
}