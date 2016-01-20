controllers.controller('AppointmentCtrl', ['$scope', '$state', '$stateParams', '$ionicModal', '$ionicPopup', '$ionicHistory', '$translate', 'authService', 'providerService', 'scheduleService', 'utilityService', function ($scope, $state, $stateParams, $ionicModal, $ionicPopup, $ionicHistory, $translate, authService, providerService, scheduleService, utilityService) {
    authService.fillAuthData();

    $scope.Appointment = {
        selectedDate: new Date(),
        selectedSlotId: null,
        selectedHour: null,
        selectedMinute: null,
        totalPrice: 0,
        address: ""
    };

    $scope.ProviderLogo = "";

    $scope.Hours = [];
    $scope.Minutes = [];

    $scope.ProviderId = $stateParams.providerId;
    $scope.CategoryId = $stateParams.categoryId;

    var successTitle = '';
    var successMessage = '';
    var errorTitle = '';
    $translate('SUCCESS').then(function (title) {
        successTitle = title;
    });

    $translate('APPOINTMENT_SUCCESS_MESSAGE').then(function (message) {
        successMessage = message;
    });

    $translate('ERROR').then(function (title) {
        errorTitle = title;
    });

    // Create the select operaions modal
    $ionicModal.fromTemplateUrl('templates/Appointment/Modals/operations.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.operationsModal = modal;
    });

    // Create the select slots modal
    $ionicModal.fromTemplateUrl('templates/Appointment/Modals/slots.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.slotsModal = modal;
    });

    // Create the select hours modal
    $ionicModal.fromTemplateUrl('templates/Appointment/Modals/hours.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.hoursModal = modal;
    });

    // Create the select hours modal
    $ionicModal.fromTemplateUrl('templates/Appointment/Modals/minutes.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.minutesModal = modal;
    });

    if ($scope.ProviderId != null && $scope.CategoryId != null) {
        providerService.getProvider($scope.ProviderId, false).then(function (result) {
            $scope.Provider = result.data.provider;
            $scope.PageTitle = $scope.Provider.alias;
            $scope.ProviderLogo = utilityService.getUserLogo($scope.Provider.logoPath);

            if ($scope.Provider.addresses != null && $scope.Provider.addresses.length > 0) {
                $scope.Appointment.address = $scope.Provider.addresses[0].addressText + ", " + $scope.Provider.addresses[0].userCity.name + ", " + $scope.Provider.addresses[0].userCity.cityCounty.name;
            }
        });

        providerService.getOperations(authService.authentication.userId, $scope.ProviderId, $scope.CategoryId, null, null, null, null, null).then(function (result) {
            $scope.populate(result);
        });
    }

    $scope.populate = function (result) {
        $scope.Operations = result.data.availableOperations;
        $scope.Slots = result.data.availableSlots;
        $scope.Hours = result.data.availableHours;
        $scope.Minutes = result.data.availableMinutesForSelectedHour;
        $scope.Appointment.selectedDate = utilityService.getDatefromString(result.data.firstAvailableDate);

        if ($scope.Appointment.selectedSlotId == null || !utilityService.checkIfObjectIdExistsInArray($scope.Slots, $scope.Appointment.selectedSlotId)) {
            $scope.Appointment.selectedSlotId = $scope.Slots[0].id;
        }

        if ($scope.Appointment.selectedHour == null || !utilityService.checkIfValueExistsInArray($scope.Hours, $scope.Appointment.selectedHour)) {
            $scope.Appointment.selectedHour = $scope.Hours[0];
        }

        if ($scope.Appointment.selectedMinute == null || !utilityService.checkIfValueExistsInArray($scope.Minutes, $scope.Appointment.selectedMinute)) {
            $scope.Appointment.selectedMinute = $scope.Minutes[0];
        }

        $scope.getSelectedSlotName();
        $scope.getAppointmentTotalPrice();
    };

    $scope.getSelectedOperations = function () {
        var selectedOperationIds = [];

        angular.forEach($scope.Operations, function (value, key) {
            if (value.selected == true) {
                selectedOperationIds.push(value.id);
            }
        });

        return selectedOperationIds;
    };

    $scope.getSelectedSlotName = function () {
        if ($scope.Provider.programmingPerSlot) {
            for (var i = 0; i < $scope.Slots.length; i++) {
                if ($scope.Slots[i].id == $scope.Appointment.selectedSlotId) {
                    $scope.SelectedSlotName = $scope.Slots[i].name;

                    if ($scope.Slots[i].fullAddress !== null && $scope.Slots[i].fullAddress !== "") {
                        $scope.Appointment.address = $scope.Slots[i].fullAddress;
                    }

                    break;
                }
            }
        }
    };

    $scope.getAppointmentTotalPrice = function () {
        $scope.Appointment.totalPrice = 0;

        angular.forEach($scope.Operations, function (value, key) {
            if (value.selected == true) {
                $scope.Appointment.totalPrice = $scope.Appointment.totalPrice + value.price;
            }
        });
    };

    $scope.operationClick = function () {
        var operations = $scope.getSelectedOperations();
        providerService.getOperations(authService.authentication.userId, $scope.ProviderId, $scope.CategoryId, operations, null, null, null, null).then(function (result) {
            $scope.populate(result);
        });
    };

    $scope.slotClick = function () {
        $scope.closeSlotsModal();
        var operations = $scope.getSelectedOperations();
        providerService.getOperations(authService.authentication.userId, $scope.ProviderId, $scope.CategoryId, operations, null, null, null, $scope.Appointment.selectedSlotId).then(function (result) {
            $scope.populate(result);
        });
    };

    $scope.hourClick = function () {
        $scope.closeHoursModal();
        var operations = $scope.getSelectedOperations();
        providerService.getOperations(authService.authentication.userId, $scope.ProviderId, $scope.CategoryId, operations, $scope.Appointment.selectedDate, $scope.Appointment.selectedHour, null, $scope.Appointment.selectedSlotId).then(function (result) {
            $scope.populate(result);
        });
    };

    $scope.selectedDateChanged = function (date) {
        var operations = $scope.getSelectedOperations();
        providerService.getOperations(authService.authentication.userId, $scope.ProviderId, $scope.CategoryId, operations, date, null, null, null).then(function (result) {
            $scope.populate(result);
        });
    };

    $scope.saveSchedule = function () {
        var operations = $scope.getSelectedOperations();

        scheduleService.saveSchedule(
                authService.authentication.userId,
                operations,
                $scope.Appointment.selectedSlotId,
                $scope.Appointment.selectedDate,
                $scope.Appointment.selectedHour,
                $scope.Appointment.selectedMinute)
            .then(function (result) {
                switch (result.data.saveResult) {
                case 0:
                    var alertPopup = $ionicPopup.alert({
                        title: successTitle,
                        template: successMessage
                    });
                    alertPopup.then(function (res) {
                        $ionicHistory.clearHistory();
                        $ionicHistory.clearCache();

                        $ionicHistory.nextViewOptions({
                            historyRoot: true
                        });

                        $state.go("app.home", {}, {
                            reload: true,
                        });
                    });
                    break;
                case 1:
                case 2:
                case 3:
                    var alertPopup = $ionicPopup.alert({
                        title: errorTitle,
                        template: result.data.message
                    });


                    break;
                default:
                    break;
                }
            });
    };

    $scope.openOperationsModal = function () {
        $scope.operationsModal.show();
    };

    $scope.closeOperationsModal = function () {
        $scope.operationsModal.hide();
    };

    $scope.openSlotsModal = function () {
        $scope.slotsModal.show();
    };

    $scope.closeSlotsModal = function () {
        $scope.slotsModal.hide();
    };

    $scope.openHoursModal = function () {
        $scope.hoursModal.show();
    };

    $scope.closeHoursModal = function () {
        $scope.hoursModal.hide();
    };

    $scope.openMinutesModal = function () {
        $scope.minutesModal.show();
    };

    $scope.closeMinutesModal = function () {
        $scope.minutesModal.hide();
    };

    $scope.unselect = function (operation) {
        operation.selected = false;
        $scope.operationClick();
    };
}]);