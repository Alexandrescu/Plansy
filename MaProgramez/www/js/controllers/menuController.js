controllers.controller('MenuCtrl', ['$scope', '$location', '$ionicHistory', 'authService', function ($scope, $location, $ionicHistory, authService) {
    $scope.ShowCloseButton = false;

    if (ionic.Platform.isAndroid()) {
        $scope.ShowCloseButton = true;
    }

    $scope.exit = function () {
        $ionicHistory.clearHistory();
        $ionicHistory.clearCache();

        if (typeof navigator != 'undefined' && typeof navigator.app != 'undefined') {
            navigator.app.exitApp();
        }
    }
}]);