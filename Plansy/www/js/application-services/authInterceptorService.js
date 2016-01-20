'use strict';
mainApp.factory('authInterceptorService', ['$q', '$injector', '$location', '$rootScope', '$localStorage', function ($q, $injector, $location, $rootScope, $localStorage) {
    var authInterceptorServiceFactory = {};

    var _request = function (config) {
        $rootScope.$broadcast('loading:show');

        config.headers = config.headers || {};

        var authData = $localStorage.getObject('authorizationData');
        if (authData) {
            config.headers.Authorization = 'Bearer ' + authData.token;
        }

        return config;
    };

    var _requestError = function (rejection) {
        $rootScope.$broadcast('loading:hide');
        return $q.reject(rejection);
    };

    var _response = function (response) {
        $rootScope.$broadcast('loading:hide');
        return response;
    };

    var _responseError = function (rejection) {
        $rootScope.$broadcast('loading:hide');

        if (rejection.status === 401) {
            var authService = $injector.get('authService');
            var ionicHistory = $injector.get('$ionicHistory');
            var state = $injector.get('$state');

            var authData = $localStorage.getObject('authorizationData');

            if (authData) {
                if (authData.useRefreshTokens) {
                    authService.refreshToken().then(function (response) {
                            ionicHistory.clearHistory();
                            ionicHistory.clearCache();

                            ionicHistory.nextViewOptions({
                                historyRoot: true
                            });

                            authService.fillAuthData();
                            if (authService.authentication.phoneConfirmed) {
                                state.go("app.home", {}, {
                                    reload: true
                                });
                            } else {
                                state.go("confirm-phone", {}, {
                                    reload: true
                                });
                            }
                        },
                        function (err) {
                            authService.logOut();

                            ionicHistory.clearHistory();
                            ionicHistory.clearCache();

                            state.go("login", {}, {
                                reload: true
                            });
                        });
                }
            } else {
                authService.logOut();

                ionicHistory.clearHistory();
                ionicHistory.clearCache();

                state.go("login", {}, {
                    reload: true
                });
            }
        }

        if (rejection.status === 400) {
            var ionicPopup = $injector.get('$ionicPopup');
            var message = '';

            if (rejection.data != null && rejection.data.message != null) {
                message = rejection.data.message;
            } else if (rejection.data != null && rejection.data.error_description != null) {
                message = rejection.data.error_description;
            }

            ionicPopup.alert({
                title: "Error",
                template: message
            });
        }

        return $q.reject(rejection);
    };

    authInterceptorServiceFactory.request = _request;
    authInterceptorServiceFactory.requestError = _requestError;
    authInterceptorServiceFactory.response = _response;
    authInterceptorServiceFactory.responseError = _responseError;

    return authInterceptorServiceFactory;
}]);