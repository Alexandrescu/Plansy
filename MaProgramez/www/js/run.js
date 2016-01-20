mainApp.run(['$ionicPlatform', '$ionicLoading', '$ionicHistory', '$rootScope', '$state', 'authService', function ($ionicPlatform, $ionicLoading, $ionicHistory, $rootScope, $state, authService) {
    var loadingCount = 0;

    $ionicPlatform.ready(function () {
        // Hide the accessory bar by default (remove this to show the accessory bar above the keyboard
        // for form inputs)
        if (window.cordova && window.cordova.plugins.Keyboard) {
            cordova.plugins.Keyboard.hideKeyboardAccessoryBar(true);
        }
        if (window.StatusBar) {
            // org.apache.cordova.statusbar required
            StatusBar.styleDefault();
        }

        if (navigator && navigator.splashscreen) {
            setTimeout(function () {
                navigator.splashscreen.hide();
            }, 500);
        }
    });

    $rootScope.$on("$stateChangeStart", function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;
        authService.fillAuthData();
        var isAuthorize = authService.isAuthorize();

        if (isAuthorize == false) {
            if (requireLogin) {
                $ionicHistory.clearHistory();
                $ionicHistory.clearCache();

                $state.transitionTo("login");
                event.preventDefault();
            }
        } else {
            // IF ACCESS LOGIN BAGE WHEN IS LOGGED
            if (!requireLogin) {
                $ionicHistory.clearHistory();
                $ionicHistory.clearCache();

                $ionicHistory.nextViewOptions({
                    historyRoot: true
                });

                if (authService.authentication.phoneConfirmed) {
                    $state.go("app.home", {}, {
                        reload: true
                    });
                } else {
                    $state.go("confirm-phone", {}, {
                        reload: true
                    });
                }
                event.preventDefault();
            } else if (authService.authentication.phoneConfirmed == false && toState.name != "confirm-phone") {
                $ionicHistory.clearHistory();
                $ionicHistory.clearCache();

                $ionicHistory.nextViewOptions({
                    historyRoot: true
                });

                $state.transitionTo("confirm-phone");
                event.preventDefault();
            }
        }
    });

    $rootScope.$on('loading:show', function () {
        loadingCount++;
        $ionicLoading.show();
    });

    $rootScope.$on('loading:hide', function () {
        loadingCount--;
        if (loadingCount <= 0) {
            $ionicLoading.hide();
            loadingCount = 0;
        }
    });

    authService.fillAuthData();
}]);