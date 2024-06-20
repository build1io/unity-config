mergeInto(LibraryManager.library, {

    InitializeRemoteConfig: function (modular, debug, fallback, objectName, callback) {
        const f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        const parsedModular = f(modular);
        const parsedDebug = f(debug);
        const parsedObjectName = f(objectName);
        const parsedCallback = f(callback);
        if (parsedModular) {
            window.remoteConfig.settings.minimumFetchIntervalMillis = parsedDebug ? 0 : 300000;
            window.remoteConfig.settings.fetchTimeoutMillis = fallback > 0 ? fallback : 60000;
        } else {
            remoteConfig.settings = {                    
                minimumFetchIntervalMillis: parsedDebug ? 0 : 300000,
                fetchTimeoutMillis: fallback > 0 ? fallback : 60000
            };
        }
        unityInstance.Module.SendMessage(parsedObjectName, parsedCallback);
    },

    FetchAndActivateRemoteConfig: function (modular, objectName, callback, fallback) {
        const f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        const parsedModular = f(modular);
        const parsedObjectName = f(objectName);
        const parsedCallback = f(callback);
        const parsedFallback = f(fallback);
        try {
            if (parsedModular) {
                window.fetchAndActivate(window.remoteConfig).then(() => {
                    unityInstance.Module.SendMessage(parsedObjectName, parsedCallback);
                })
                .catch((error) => {
                    unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
                });
            } else {
                remoteConfig.fetchAndActivate().then(() => {
                    unityInstance.Module.SendMessage(parsedObjectName, parsedCallback);
                })
                .catch((error) => {
                    unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
                });
            }
        } catch (error) {
            unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },
    
    GetFromRemoteConfig: function (modular, field, objectName, callback) {
        const f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        const parsedModular = f(modular);
        const parsedField = f(field);
        const parsedObjectName = f(objectName);
        const parsedCallback = f(callback);
        let value;
        if (parsedModular) {
            value = window.getValue(window.remoteConfig, parsedField).asString();
        } else {
            value = remoteConfig.getValue(parsedField).asString();
        }
        unityInstance.Module.SendMessage(parsedObjectName, parsedCallback, value);    
    }
});
