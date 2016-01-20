controllers.controller("FavoriteCtrl", ['$scope', 'authService', 'providerService', function ($scope, authService, providerService) {
    authService.fillAuthData();
    providerService.getFavoriteProviders(authService.authentication.userId).then(function (result) {
        $scope.Providers = result.data;
    });
}]);