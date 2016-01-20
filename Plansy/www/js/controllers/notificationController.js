controllers.controller("NotificationCtrl", ['$scope', '$state', '$stateParams', '$ionicPopup', '$translate', 'authService', 'notificationService', function ($scope, $state, $stateParams, $ionicPopup, $translate, authService, notificationService) {
    $scope.Id = $stateParams.id;
    authService.fillAuthData();

    if ($scope.Id != null && $scope.Id != "" && (typeof $scope.Id != "undefined")) {
        notificationService.getNotification(authService.authentication.userId, $scope.Id)
            .then(function (result) {
                $scope.Notification = result.data;
            });

        $scope.DeleteNotification = function () {
            var successTitle = "";
            var successMessage = "";
            var errorTitle = "";
            var errorMessage = "";

            $translate('SUCCESS').then(function (title) {
                successTitle = title;
            });
            $translate('SUCCESS_DELETE_NOTIFICATION').then(function (title) {
                successMessage = title;
            });

            $translate('ERROR').then(function (title) {
                errorTitle = title;
            });
            $translate('ERROR_DELETE_NOTIFICATION').then(function (title) {
                errorMessage = title;
            });


            notificationService.deleteNotification(authService.authentication.userId, $scope.Id)
                .then(function (result) {
                    if (result.statusText == "OK") {
                        var popup = $ionicPopup.alert({
                            title: successTitle,
                            template: successMessage
                        });
                        popup.then(function (result) {
                            $state.transitionTo("app.notifications");
                        });
                    } else {
                        $ionicPopup.alert({
                            title: errorTitle,
                            template: errorMessage
                        });
                    }
                });
        };
    } else {
        notificationService.getNotifications(authService.authentication.userId)
            .then(function (result) {
                $scope.Notifications = result.data;
            });
    }
}]);