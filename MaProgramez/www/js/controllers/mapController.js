controllers.controller('MapCtrl', ['$scope', '$stateParams', '$ionicLoading', 'authService', 'scheduleService', 'providerService', function ($scope, $stateParams, $ionicLoading, authService, scheduleService, providerService) {
    $scope.ProviderId = $stateParams.providerId;

    authService.fillAuthData();
    var userId = authService.authentication.userId;

    if ($scope.ProviderId != null && $scope.ProviderId != "" && (typeof $scope.ProviderId != 'undefined')) {
        providerService.getProvider($scope.ProviderId, false)
            .then(function (result) {
                $scope.Provider = result.data.provider;

                var lat = 0,
                    lng = 0;

                $ionicLoading.show();

                navigator.geolocation.getCurrentPosition(function (position) {
                    lat = position.coords.latitude;
                    lng = position.coords.longitude

                    $scope.MyLocation = lat + "," + lng;

                    $ionicLoading.hide();
                });

                if ($scope.Provider.addresses != null && $scope.Provider.addresses.length > 0) {
                    $scope.DestinationLatitude = $scope.Provider.addresses[0].latitude;
                    $scope.DestinationLongitude = $scope.Provider.addresses[0].longitude;
                }


                $scope.$on('mapInitialized', function (event, map) {
                    $scope.map = map;

                    $ionicLoading.show();

                    if (lat == 0 && lng == 0) {
                        navigator.geolocation.getCurrentPosition(function (position) {
                            lat = position.coords.latitude;
                            lng = position.coords.longitude

                            $scope.MyLocation = lat + "," + lng;
                            var pos = new google.maps.LatLng(lat, lng);
                            $scope.map.setCenter(pos);

                            $ionicLoading.hide();
                        });
                    } else {
                        var pos = new google.maps.LatLng(lat, lng);
                        $scope.map.setCenter(pos);

                        $ionicLoading.hide();
                    }
                });
            });
    } else {
        scheduleService.getActiveSchedules(userId)
            .then(function (result) {
                $scope.Schedules = result.data;
            });
    }
}]);