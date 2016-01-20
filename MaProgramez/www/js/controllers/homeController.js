controllers.controller("HomeCtrl", ['$scope', 'authService', 'scheduleService', function ($scope, authService, scheduleService) {
    authService.fillAuthData();
    var userId = authService.authentication.userId;
    $scope.ScheduleNumber = 0;

    scheduleService.getNumberOfSchedules(userId).then(function (result) {
        $scope.ScheduleNumber = result.data;
    });
}]);