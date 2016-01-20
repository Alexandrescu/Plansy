controllers.controller('SettingsCtrl', ['$scope', '$state', '$location', '$ionicHistory', '$ionicPopup', '$ionicModal', '$translate', 'authService', 'accountService', function ($scope, $state, $location, $ionicHistory, $ionicPopup, $ionicModal, $translate, authService, accountService) {
    'use strict';

    authService.fillAuthData();

    $scope.User = {
        FirstName: "",
        LastName: "",
        Phone: "",
        Code: ""
    };

    // Create the login modal that we will use later
    $ionicModal.fromTemplateUrl('templates/Settings/Modals/changePhoneModal.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.ChangePhoneModal = modal;
    });

    $scope.logOut = function () {
        $ionicHistory.clearHistory();
        $ionicHistory.clearCache();

        authService.logOut();
        $state.go("login", {}, {
            reload: true
        });
    };

    switch ($state.current.name) {
    case "confirm-phone":
        $scope.sendCode = function () {
            var successTitle = "";
            var successMessage = "";
            var errorTitle = "";
            var errorMessage = "";

            $translate('SUCCESS').then(function (title) {
                successTitle = title;
            });
            $translate('SUCCESS_SEND_CODE').then(function (title) {
                successMessage = title;
            });

            $translate('ERROR').then(function (title) {
                errorTitle = title;
            });
            $translate('ERROR_SEND_CODE').then(function (title) {
                errorMessage = title;
            });

            accountService.sendCode(authService.authentication.userId, authService.authentication.phoneNumber)
                .then(function (result) {
                    if (result.statusText == "OK") {
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

        $scope.confirmPhone = function () {
            var successTitle = "";
            var successMessage = "";
            var errorTitle = "";
            var errorMessage = "";

            $translate('SUCCESS').then(function (title) {
                successTitle = title;
            });
            $translate('SUCCESS_PHONE_CONFIRM').then(function (title) {
                successMessage = title;
            });

            $translate('ERROR').then(function (title) {
                errorTitle = title;
            });
            $translate('ERROR_PHONE_CONFIRM').then(function (title) {
                errorMessage = title;
            });

            accountService.verifyPhoneNumber(authService.authentication.userId, authService.authentication.phoneNumber, $scope.User.Code)
                .then(function (result) {
                    if (result.statusText == "OK") {
                        var alertPopup = $ionicPopup.alert({
                            title: successTitle,
                            template: successMessage
                        });
                        alertPopup.then(function (res) {
                            authService.authentication.phoneConfirmed = true;
                            authService.copyAuthDataToLocalStorage();

                            $ionicHistory.clearHistory();
                            $ionicHistory.clearCache();

                            $ionicHistory.nextViewOptions({
                                historyRoot: true
                            });

                            $state.transitionTo("app.home");
                        });
                    } else {
                        $ionicPopup.alert({
                            title: errorTitle,
                            template: errorMessage
                        });
                    }
                });
        };

        break;
    case "app.account":
        accountService.getUserDetails(authService.authentication.userId)
            .then(function (result) {
                $scope.User.FirstName = result.data.firstName;
                $scope.User.LastName = result.data.lastName;
                $scope.User.Phone = result.data.phone;
            });

        $scope.SaveUserDetails = function () {
            var successTitle = "";
            var successMessage = "";
            var errorTitle = "";
            var errorMessage = "";

            $translate('SUCCESS').then(function (title) {
                successTitle = title;
            });
            $translate('SUCCESS_SAVE_USER_DETAILS').then(function (title) {
                successMessage = title;
            });

            $translate('ERROR').then(function (title) {
                errorTitle = title;
            });
            $translate('ERROR_SAVE_USER_DETAILS').then(function (title) {
                errorMessage = title;
            });

            accountService.saveUserDetails(authService.authentication.userId,
                    $scope.User.FirstName,
                    $scope.User.LastName,
                    $scope.User.Phone)
                .then(function (result) {
                    if (result.data != null) {
                        var confirmPopup = $ionicPopup.alert({
                            title: successTitle,
                            template: successMessage
                        });

                        confirmPopup.then(function (res) {
                            if (result.data.phoneNumberConfirmed == false) {
                                authService.authentication.phoneConfirmed = false;
                                authService.authentication.phoneNumber = result.data.phoneNumber;
                                authService.copyAuthDataToLocalStorage();

                                $ionicHistory.clearHistory();
                                $ionicHistory.clearCache();

                                $ionicHistory.nextViewOptions({
                                    historyRoot: true
                                });

                                $state.transitionTo("confirm-phone");
                            }
                        });

                    } else {
                        $ionicPopup.alert({
                            title: errorTitle,
                            template: errorMessage
                        });
                    }
                });
        };
        break;
    case "app.password":
        $scope.Password = {
            OldPassword: "",
            NewPassword: "",
            ConfirmPassword: ""
        };

        var successTitle = "";
        var successMessage = "";
        var errorTitle = "";

        $translate('SUCCESS').then(function (title) {
            successTitle = title;
        });
        $translate('SUCCESS_CHANGE_PASSWORD').then(function (title) {
            successMessage = title;
        });

        $translate('ERROR').then(function (title) {
            errorTitle = title;
        });

        $scope.ChangePassword = function () {
            accountService.changePassword(authService.authentication.userId,
                    $scope.Password.OldPassword,
                    $scope.Password.NewPassword,
                    $scope.Password.ConfirmPassword)
                .then(function (result) {
                    if (result.data == "") {
                        var changePasswordPopup = $ionicPopup.alert({
                            title: successTitle,
                            template: successMessage
                        });

                        changePasswordPopup.then(function (result) {
                            $scope.logOut();
                        });
                    } else {
                        $ionicPopup.alert({
                            title: errorTitle,
                            template: result.data
                        });
                    }
                });
        };

        break;
    }

    $scope.changePhoneNumber = function () {
        accountService.changePhoneNumber(authService.authentication.userId, $scope.User.Phone)
            .then(function (result) {
                if (result.statusText == "OK") {
                    authService.authentication.phoneNumber = $scope.User.Phone;
                    authService.copyAuthDataToLocalStorage();

                    $scope.closeChangePhoneModal();
                }
            });
    };

    $scope.openChangePhoneModal = function () {
        $scope.ChangePhoneModal.show();
    };

    $scope.closeChangePhoneModal = function () {
        $scope.ChangePhoneModal.hide();
    };
}]);