appUtilities.factory("$localStorage", ["$window", function ($window) {
    return {
        set: function (key, value) {
            $window.localStorage[key] = value;
        },
        get: function (key, defaultValue) {
            return $window.localStorage[key] || defaultValue;
        },
        setObject: function (key, value) {
            $window.localStorage[key] = JSON.stringify(value);
        },
        getObject: function (key) {
            var value = $window.localStorage.getItem(key);
            if (value === null || (value === "undefined")) {
                return null;
            }
            return JSON.parse(value || '{}');
        },
        remove: function (key) {
            $window.localStorage.removeItem(key);
        }
    };
}]);