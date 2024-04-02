#if UNITY_WEBGL

using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories.WebGL
{
    internal sealed class FirebaseRemoteConfigAgent : MonoBehaviour
    {
        public event Action OnInitialized;

        public event Action                OnFetchAndActivateSuccess;
        public event Action<FirebaseError> OnFetchAndActivateFail;

        public event Action<object>        OnGetSuccess;
        public event Action<FirebaseError> OnGetFail;

        [DllImport("__Internal")]
        public static extern void InitializeRemoteConfig(bool debug, int fallbackTimeout, string objectName, string OnSuccess);

        [DllImport("__Internal")]
        public static extern void FetchAndActivateRemoteConfig(string objectName, string OnSuccess, string onFail);

        [DllImport("__Internal")]
        public static extern void GetFromRemoteConfig(string field, string objectName, string OnSuccess, string onFail);

        /*
         * Initialize.
         */

        public void Initialize(bool debug, int fallbackTimeout)
        {
            InitializeRemoteConfig(debug, fallbackTimeout, gameObject.name, nameof(OnInitializedHandler));
        }

        public void OnInitializedHandler()
        {
            OnInitialized?.Invoke();
        }

        /*
         * Fetching.
         */

        public void FetchAndActivate()
        {
            FetchAndActivateRemoteConfig(gameObject.name, nameof(OnFetchSuccess), nameof(OnFetchFail));
        }

        public void OnFetchSuccess()
        {
            OnFetchAndActivateSuccess?.Invoke();
        }

        public void OnFetchFail(string payload)
        {
            var error = JsonConvert.DeserializeObject<FirebaseError>(payload);
            OnFetchAndActivateFail?.Invoke(error);
        }

        /*
         * Get.
         */

        public void Get(string field)
        {
            GetFromRemoteConfig(field, gameObject.name, nameof(OnGetSuccessHandler), nameof(OnGetFailHandler));
        }

        public void OnGetSuccessHandler(object value)
        {
            OnGetSuccess?.Invoke(value);
        }

        public void OnGetFailHandler(string payload)
        {
            var error = JsonConvert.DeserializeObject<FirebaseError>(payload);
            OnGetFail?.Invoke(error);
        }
    }
}

#endif