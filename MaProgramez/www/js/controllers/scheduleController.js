controllers.controller("ScheduleCtrl", ['$scope', '$stateParams', '$ionicPopup', '$translate', 'authService', 'scheduleService', function ($scope, $stateParams, $ionicPopup, $translate, authService, scheduleService) {
    $scope.ScheduleId = $stateParams.scheduleId;

    authService.fillAuthData();
    var userId = authService.authentication.userId;

    if ($scope.ScheduleId != null && $scope.ScheduleId != "" && (typeof $scope.ScheduleId != "undefined")) {
        scheduleService.getSchedule($scope.ScheduleId, userId).then(function (result) {
            $scope.Schedule = result.data;

            $scope.getAddress();
            $scope.calculateTotalPrice();
        });
    } else {
        scheduleService.getClientSchedulesHistory(userId).then(function (result) {
            $scope.ScheduleHistory = result.data;
        });

        scheduleService.getActiveSchedules(userId).then(function (result) {
            $scope.Schedules = result.data;
        });
    }

    var popupTitle = '';
    var popupQuestion = '';
    var cancelScheduleTitle = ''
    var cancelScheduleSuccessMessage = '';
    var cancelScheduleErrorMessage = '';
    $translate('POPUP_CANCEL_TITLE').then(function (title) {
        popupTitle = title;
    });

    $translate('POPUP_CANCEL_QUESTION').then(function (question) {
        popupQuestion = question;
    });

    $translate('POPUP_CANCEL_SCHEDULE_TITLE').then(function (title) {
        cancelScheduleTitle = title;
    });

    $translate('POPUP_CANCEL_SCHEDULE_SUCCESS_MESSAGE').then(function (message) {
        cancelScheduleSuccessMessage = message;
    });

    $translate('POPUP_CANCEL_SCHEDULE_ERROR_MESSAGE').then(function (message) {
        cancelScheduleErrorMessage = message;
    });

    $scope.showConfirm = function () {
        var confirmPopup = $ionicPopup.confirm({
            title: popupTitle,
            template: popupQuestion
        });
        confirmPopup.then(function (res) {
            if (res) {
                scheduleService.cancelSchedule($scope.Schedule.id, userId)
                    .then(function (result) {
                        if (result.data != null) {
                            var successAlertPopup = $ionicPopup.alert({
                                title: cancelScheduleTitle,
                                template: cancelScheduleSuccessMessage
                            });
                            successAlertPopup.then(function (res) {
                                scheduleService.getSchedule($scope.Schedule.id, userId)
                                    .then(function (result) {
                                        $scope.Schedule = result.data;
                                    });
                            });
                        } else {
                            var errorsAlertPopup = $ionicPopup.alert({
                                title: cancelScheduleTitle,
                                template: cancelScheduleErrorMessage
                            });
                        }
                    });
            }
        });
    };

    $scope.calculateTotalPrice = function () {
        var total = 0;
        angular.forEach($scope.Schedule.operationsForSchedule, function (value, key) {
            total += value.price;
        });

        $scope.TotaPrice = total;
    };

    $scope.getAddress = function () {
        if ($scope.Schedule.slot.provider.programmingPerSlot && $scope.Schedule.slot.fullAddress != null && $scope.Schedule.slot.fullAddress != "") {
            $scope.ScheduleAddress = $scope.Schedule.slot.fullAddress;
        } else if ($scope.Schedule.slot.provider.addresses != null && $scope.Schedule.slot.provider.addresses.length > 0) {
            $scope.ScheduleAddress = $scope.Schedule.slot.provider.addresses[0].addressText + ", " + $scope.Schedule.slot.provider.addresses[0].userCity.name + ", " + $scope.Schedule.slot.provider.addresses[0].userCity.cityCounty.name;
        }
    };
}]);