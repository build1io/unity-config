mergeInto(LibraryManager.library, {

    InitializeRemoteConfig: function (debug, fallback, objectName, callback) {
        var f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        var parsedDebug = f(debug);
        var parsedObjectName = f(objectName);
        var parsedCallback = f(callback);
        remoteConfig.settings = {                    
            fetchTimeoutMillis: fallback > 0 ? fallback : 60000,
            minimumFetchIntervalMillis: debug ? 0 : 300000
        };
        unityInstance.Module.SendMessage(parsedObjectName, parsedCallback);
    },

    FetchAndActivateRemoteConfig: function (objectName, callback, fallback) {
        var f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        var parsedObjectName = f(objectName);
        var parsedCallback = f(callback);
        var parsedFallback = f(fallback);
        try {
            remoteConfig.fetchAndActivate().then(() => {
                unityInstance.Module.SendMessage(parsedObjectName, parsedCallback);
            })
            .catch((error) => {
                unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
            });
        } catch (error) {
            unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },
    
    GetFromRemoteConfig: function (field, objectName, callback) {
        var f = typeof UTF8ToString === 'function' ? UTF8ToString : Pointer_stringify;
        var parsedField = f(field);
        var parsedObjectName = f(objectName);
        var parsedCallback = f(callback);
        var value = remoteConfig.getValue(parsedField).asString();
        unityInstance.Module.SendMessage(parsedObjectName, parsedCallback, value);    
    }
});
