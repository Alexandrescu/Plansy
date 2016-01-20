'use strict';
mainApp.factory('authService', ['$http', '$q', '$rootScope', '$localStorage', 'appAuthSettings', function ($http, $q, $rootScope, $localStorage, appAuthSettings) {
    var serviceBase = appAuthSettings.apiServiceBaseUri;
    var authServiceFactory = {};

    var _authentication = {
        isAuth: false,
        userName: "",
        userId: "",
        phoneNumber: "",
        phoneConfirmed: false,
        useRefreshTokens: appAuthSettings.useRefreshTokens,
        token: "",
        refreshToken: ""
    };

    var _externalAuthData = {
        provider: "",
        userName: "",
        externalAccessToken: ""
    };

    var _fillAuthData = function () {
        var authData = $localStorage.getObject('authorizationData');
        if (authData) {
            _authentication.isAuth = true;
            _authentication.userName = authData.userName;            
            _authentication.userId = authData.userId;
            _authentication.phoneNumber = authData.phoneNumber;
            _authentication.phoneConfirmed = authData.phoneConfirmed;
            _authentication.useRefreshTokens = authData.useRefreshTokens;
            _authentication.token = authData.token;
            _authentication.refreshToken = authData.refreshToken;

            $rootScope.loggedInUser = authData.userName;
        }
    };

    var _copyAuthDataToLocalStorage = function () {
        $localStorage.remove('authorizationData');

        $localStorage.setObject('authorizationData', {
            token: _authentication.token,
            userName: _authentication.userName,
            userId: _authentication.userId,
            phoneNumber: _authentication.phoneNumber,
            phoneConfirmed: _authentication.phoneConfirmed,
            refreshToken: _authentication.refreshToken,
            useRefreshTokens: _authentication.useRefreshTokens
        });
    };

    var _isAuthorize = function () {
        _fillAuthData();

        if (((typeof $rootScope.loggedInUser === 'undefined') || ($rootScope.loggedInUser == null))) {
            return false;
        }

        return true;
    };

    var _saveRegistration = function (registration) {
        _logOut();

        return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
            return response;
        });
    };

    var _login = function (loginData) {
        var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

        if (loginData.useRefreshTokens) {
            data = data + "&client_id=" + appAuthSettings.clientId;
        }

        var deferred = $q.defer();

        $http.post(serviceBase + 'token', data, {
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        }).success(function (response) {
            if (loginData.useRefreshTokens) {
                $localStorage.setObject('authorizationData', {
                    token: response.access_token,
                    userName: loginData.userName,
                    userId: response.userId,
                    phoneNumber: response.phoneNumber,
                    phoneConfirmed: (response.phoneConfirmed.toLowerCase() === 'true'),
                    refreshToken: response.refresh_token,
                    useRefreshTokens: true
                });
            } else {
                $localStorage.setObject('authorizationData', {
                    token: response.access_token,
                    userName: loginData.userName,
                    userId: response.userId,
                    phoneNumber: response.phoneNumber,
                    phoneConfirmed: (response.phoneConfirmed.toLowerCase() === 'true'),
                    refreshToken: "",
                    useRefreshTokens: false
                });
            }

            _fillAuthData();

            $rootScope.loggedInUser = loginData.userName;

            deferred.resolve(response);
        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);
        });

        return deferred.promise;
    };

    var _logOut = function () {
        $localStorage.remove('authorizationData');

        _authentication.isAuth = false;
        _authentication.userName = "";
        _authentication.userId = "";
        _authentication.phoneNumber = "";
        _authentication.phoneConfirmed = false;
        _authentication.useRefreshTokens = appAuthSettings.useRefreshTokens;
        _authentication.token = "";
        _authentication.refreshToken = "";

        $rootScope.loggedInUser = null;
    };

    var _refreshToken = function () {
        var deferred = $q.defer();

        var authData = $localStorage.getObject('authorizationData');

        if (authData) {
            if (authData.useRefreshTokens) {
                var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + appAuthSettings.clientId;
                $localStorage.remove('authorizationData');

                $http.post(serviceBase + 'token', data, {
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    }
                }).success(function (response) {
                    $localStorage.setObject('authorizationData', {
                        token: response.access_token,
                        userName: response.userName,
                        userId: response.userId,
                        phoneNumber: response.phoneNumber,
                        phoneConfirmed: (response.phoneConfirmed.toLowerCase() === 'true'),
                        refreshToken: response.refresh_token,
                        useRefreshTokens: appAuthSettings.useRefreshTokens
                    });

                    _fillAuthData();

                    deferred.resolve(response);

                }).error(function (err, status) {
                    _logOut();
                    deferred.reject(err);
                });
            }
        }

        return deferred.promise;
    };

    var _obtainAccessToken = function (externalData) {
        var deferred = $q.defer();

        $http.get(serviceBase + 'api/account/ObtainLocalAccessToken', {
            params: {
                provider: externalData.provider,
                externalAccessToken: externalData.externalAccessToken
            }
        }).success(function (response) {
            $localStorage.setObject('authorizationData', {
                token: response.access_token,
                userName: response.userName,
                userId: response.userId,
                phoneNumber: response.phoneNumber,
                phoneConfirmed: (response.phoneConfirmed.toLowerCase() === 'true'),
                refreshToken: "",
                useRefreshTokens: false
            });

            _fillAuthData();

            $rootScope.loggedInUser = response.userName;

            deferred.resolve(response);

        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    };

    var _registerExternal = function (registerExternalData) {
        var deferred = $q.defer();

        $http.post(serviceBase + 'api/account/registerexternal', registerExternalData).success(function (response) {

            $localStorage.setObject('authorizationData', {
                token: response.access_token,
                userName: response.userName,
                userId: response.userId,
                phoneNumber: response.phoneNumber,
                phoneConfirmed: (response.phoneConfirmed.toLowerCase() === 'true'),
                refreshToken: "",
                useRefreshTokens: false
            });

            _fillAuthData();

            $rootScope.loggedInUser = response.userName;

            deferred.resolve(response);

        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    };

    authServiceFactory.isAuthorize = _isAuthorize;
    authServiceFactory.saveRegistration = _saveRegistration;
    authServiceFactory.login = _login;
    authServiceFactory.logOut = _logOut;
    authServiceFactory.fillAuthData = _fillAuthData;
    authServiceFactory.copyAuthDataToLocalStorage = _copyAuthDataToLocalStorage;
    authServiceFactory.authentication = _authentication;
    authServiceFactory.refreshToken = _refreshToken;

    authServiceFactory.obtainAccessToken = _obtainAccessToken;
    authServiceFactory.externalAuthData = _externalAuthData;
    authServiceFactory.registerExternal = _registerExternal;

    return authServiceFactory;
}]);