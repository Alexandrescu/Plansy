mainApp.config(function ($stateProvider, $urlRouterProvider, $httpProvider, $ionicConfigProvider, $compileProvider) {
    $stateProvider
        .state('app', {
            url: "/app",
            abstract: true,
            templateUrl: "templates/Shared/menu.html",
            controller: 'MenuCtrl',
            data: {
                requireLogin: true // this property will apply to all children of 'app'
            }
        })
        .state('login', {
            url: "/login",
            templateUrl: "templates/Login/login.html",
            controller: 'LoginCtrl',
            data: {
                requireLogin: false
            }
        })
        .state('confirm-phone', {
            url: "/confirm-phone",
            cache: false,
            controller: 'SettingsCtrl',
            templateUrl: "templates/Settings/confirmPhone.html",
            data: {
                requireLogin: true
            }
        })
        .state('app.home', {
            url: "/home",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/home.html",
                    controller: 'HomeCtrl'
                }
            }
        })
        .state('app.select-category', {
            url: "/select-category",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Category/select-category.html",
                    controller: 'CategoryCtrl'
                }
            }
        })
        .state('app.select-category-tree', {
            url: "/select-category/:id",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Category/select-category-tree.html",
                    controller: 'CategoryCtrl'
                }
            }
        })
        .state('app.select-provider', {
            url: "/select-provider/:categoryId",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Provider/providers.html",
                    controller: 'ProviderCtrl'
                }
            }
        })
        .state('app.provider', {
            url: "/provider/:id",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Provider/provider.html",
                    controller: 'ProviderCtrl'
                }
            }
        })
        .state('app.appointment', {
            url: "/appointment/:providerId/:categoryId",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Appointment/appointment.html",
                    controller: "AppointmentCtrl"
                }
            }
        })
        .state('app.appointments', {
            url: "/appointments",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Appointment/appointments.html",
                    controller: "ScheduleCtrl"
                }
            }
        })
        .state('app.schedule', {
            url: "/schedule/:scheduleId",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Schedule/details.html",
                    controller: "ScheduleCtrl"
                }
            }
        })
        .state('app.map', {
            url: "/direction",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Map/index.html",
                    controller: "MapCtrl"
                }
            }
        })
        .state('app.map-direction', {
            url: "/direction/:providerId",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Map/direction.html",
                    controller: "MapCtrl"
                }
            }
        })
        .state('app.favorite', {
            url: "/favorite",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Favorite/providers.html",
                    controller: "FavoriteCtrl"
                }
            }
        })
        .state('app.help', {
            url: "/help",
            views: {
                'menuContent': {
                    templateUrl: "templates/Help/index.html",
                }
            }
        })
        .state('app.terms', {
            url: "/terms",
            views: {
                'menuContent': {
                    templateUrl: "templates/Help/terms.html",
                }
            }
        })
        .state('app.faq', {
            url: "/faq",
            views: {
                'menuContent': {
                    templateUrl: "templates/Help/faq.html",
                }
            }
        })
        .state('app.settings', {
            url: "/settings",
            views: {
                'menuContent': {
                    templateUrl: "templates/Settings/index.html",
                    controller: "SettingsCtrl"
                }
            }
        })
        .state('app.account', {
            url: "/account",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Settings/account.html",
                    controller: "SettingsCtrl"
                }
            }
        })
        .state('app.password', {
            url: "/password",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Settings/password.html",
                    controller: "SettingsCtrl"
                }
            }
        })
        .state('app.notifications', {
            url: "/notifications",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Notification/index.html",
                    controller: "NotificationCtrl"
                }
            }
        })
        .state('app.notification', {
            url: "/notification/:id",
            cache: false,
            views: {
                'menuContent': {
                    templateUrl: "templates/Notification/details.html",
                    controller: "NotificationCtrl"
                }
            }
        });

    $urlRouterProvider.otherwise(function ($injector, $location) {
        var $state = $injector.get("$state");
        var authService = $injector.get("authService");
        if (authService.isAuthorize()) {
            if (authService.authentication.phoneConfirmed) {
                $state.go("app.home", {}, {
                    reload: true
                });
            } else {
                $state.go("confirm-phone", {}, {
                    reload: true
                });
            }
        } else {
            $state.go("login");
        }
    });

    $httpProvider.interceptors.push('authInterceptorService');
    $ionicConfigProvider.tabs.position('bottom');
    $compileProvider.aHrefSanitizationWhitelist(/^\s*(http?|https?|ftp|mailto|geo|blob|file|chrome-extension):/);
});