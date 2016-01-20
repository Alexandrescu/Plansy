controllers.controller('LoginCtrl', ['$scope', '$state', '$location', '$ionicModal', '$ionicHistory', '$ionicPopup', '$translate', '$timeout', 'authService', 'accountService', 'appAuthSettings', function ($scope, $state, $location, $ionicModal, $ionicHistory, $ionicPopup, $translate, $timeout, authService, accountService, appAuthSettings) {
    if (authService.isAuthorize()) {
        $location.path("/app/home");
    }

    $scope.savedSuccessfully = false;
    $scope.message = "";

    // Form data for the login modal
    $scope.loginData = {
        userName: "",
        password: "",
        useRefreshTokens: appAuthSettings.useRefreshTokens
    };

    $scope.registration = {
        userName: "",
        password: "",
        confirmPassword: "",
        lastName: "",
        firstName: "",
        email: "",
        phone: ""
    };

    $scope.passwordData = {
        email: ""
    };

    // Create the login modal that we will use later
    $ionicModal.fromTemplateUrl('templates/Login/loginModal.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.modal = modal;
    });

    // Create the register modal that we will use later
    $ionicModal.fromTemplateUrl('templates/Login/registerModal.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.RegisterModal = modal;
    });

    // Create the forgot password modal that we will use later
    $ionicModal.fromTemplateUrl('templates/Login/passwordModal.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.PasswordModal = modal;
    });

    // Login function
    function authentication(data, isLogin) {
        authService.login(data).then(function (response) {
                $ionicHistory.clearHistory();
                $ionicHistory.clearCache();

                $ionicHistory.nextViewOptions({
                    historyRoot: true
                });

                if (authService.authentication.phoneConfirmed) {
                    $state.go("app.home", {}, {
                        reload: true,
                    });
                } else {
                    $state.go("confirm-phone", {}, {
                        reload: true,
                    });
                }

                if (isLogin) {
                    $scope.closeLogin();

                    $scope.loginData.password = "";
                } else {
                    $scope.closeRegister();

                    $scope.registration.password = "";
                    $scope.registration.confirmPassword = "";
                }
            },
            function (err) {
                $scope.message = err.error_description;
            });
    }

    ///
    ///
    /// Triggered in the login modal to close it
    $scope.closeLogin = function () {
        $scope.modal.hide();
        $scope.message = "";
    };

    // Open the login modal
    $scope.login = function () {
        $scope.modal.show();
    };

    // Perform the login action when the user submits the login form
    $scope.doLogin = function () {
        //console.log('Doing login', $scope.loginData);

        authentication($scope.loginData, true);
    };

    ///
    ///
    /// Register Actions
    $scope.register = function () {
        $scope.RegisterModal.show();
    };

    $scope.closeRegister = function () {
        $scope.RegisterModal.hide();
        $scope.message = "";
    };

    $scope.doRegister = function () {
        //console.log('Doing register', $scope.registration);

        $scope.registration.email = $scope.registration.userName;
        
        authService.saveRegistration($scope.registration).then(function (response) {
                $scope.savedSuccessfully = true;

                $scope.loginData.userName = $scope.registration.userName;
                $scope.loginData.password = $scope.registration.password;                
                $scope.loginData.useRefreshTokens = appAuthSettings.useRefreshTokens;

                // after register do login
                authentication($scope.loginData, false);
            },
            function (response) {
                var errors = [];
                for (var key in response.data.modelState) {
                    for (var i = 0; i < response.data.modelState[key].length; i++) {
                        errors.push(response.data.modelState[key][i]);
                    }
                }
                $scope.message = "Eroare! Corectati urmatoarele: " + errors.join(';  ');
            });
    };

    ///
    ///
    /// Forgot Password Actions
    $scope.password = function () {
        $scope.PasswordModal.show();
    };

    $scope.closePassword = function () {
        $scope.PasswordModal.hide();
    };

    $scope.doPassword = function () {
        var successTitle = "";
        var successMessage = "";
        var errorTitle = "";
        var errorMessage = "";

        $translate('SUCCESS').then(function (title) {
            successTitle = title;
        });
        $translate('SUCCESS_FORGOT_PASSWORD').then(function (title) {
            successMessage = title;
        });

        $translate('ERROR').then(function (title) {
            errorTitle = title;
        });
        $translate('ERROR_FORGOT_PASSWORD').then(function (title) {
            errorMessage = title;
        });

        accountService.forgotPassword($scope.passwordData.email).then(function (result) {
            if (result.data) {
                $ionicPopup.alert({
                    title: successTitle,
                    template: successMessage
                });
            } else {
                $ionicPopup.alert({
                    title: errorTitle,
                    template: errorMessage
                });
            }
        });
    };
}]);