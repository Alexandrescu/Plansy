controllers.controller('ProviderCtrl', ['$scope', '$stateParams', '$rootScope', '$localStorage', '$translate', 'authService', 'providerService', 'categoryService', 'utilityService', function ($scope, $stateParams, $rootScope, $localStorage, $translate, authService, providerService, categoryService, utilityService) {
    $scope.categoryId = $stateParams.categoryId; // categoryId;
    $scope.id = $stateParams.id; // providerId;

    $scope.PageSize = 10;
    $scope.Page = 0;

    $scope.IsFavorite = false;

    $translate('PROVIDERS_FILTER').then(function (title) {
        $scope.FilterTitle = title;
    });

    var filterData = $localStorage.getObject('filterData');
    if (filterData != null) {
        $scope.SelectedCountyId = filterData.countyId;
        $scope.SelectedCityId = filterData.cityId;
        $scope.FilterText = filterData.filterText;
    } else {
        $scope.SelectedCountyId = "";
        $scope.SelectedCityId = "";
        $scope.FilterText = "";
    }

    if (typeof $scope.categoryId !== "undefined") {
        categoryService.getCategory($scope.categoryId).then(function (result) {
            $scope.PageTitle = result.data.name;
        });

        providerService.getProviders($scope.categoryId, $scope.SelectedCityId, $scope.FilterText, $scope.Page, $scope.PageSize).then(function (result) {
            $scope.Providers = result.data;
        });
    }

    if (typeof $scope.id !== "undefined") {
        providerService.getProvider($scope.id, true).then(function (result) {
            $scope.Provider = result.data.provider;
            $scope.ProviderCategories = result.data.providerCategories
            $scope.PageTitle = $scope.Provider.alias;
            $scope.ProviderLogo = utilityService.getUserLogo($scope.Provider.logoPath);
        });

        authService.fillAuthData();
        providerService.isFavorite(authService.authentication.userId, $scope.id, null).then(function (result) {
            $scope.IsFavorite = result.data;
        });
    }

    $scope.addToFavorite = function () {
        authService.fillAuthData();

        if ($scope.IsFavorite == false) {
            providerService.addToFavorite(authService.authentication.userId, $scope.id, null).then(function (result) {
                $scope.IsFavorite = true;

                $translate('ADD_FAVORITE_MESSAGE').then(function (message) {
                    $rootScope.$broadcast('showNotification', {
                        Message: message,
                        Type: "success"
                    });
                });
            });
        } else {
            providerService.removeFavorite(authService.authentication.userId, $scope.id, null).then(function (result) {
                $scope.IsFavorite = false;

                $translate('REMOVE_FAVORITE_MESSAGE').then(function (message) {
                    $rootScope.$broadcast('showNotification', {
                        Message: message,
                        Type: "warning"
                    });
                });
            });
        }
    };

    $scope.getFilteredParameters = function (filterParameters) {
        $localStorage.setObject('filterData', {
            countyId: filterParameters.selectedCountyId,
            cityId: filterParameters.selectedCityId,
            countyName: filterParameters.selectedCountyName,
            cityName: filterParameters.selectedCityName,
            filterText: filterParameters.filterText
        });

        $scope.SelectedCountyId = filterParameters.selectedCountyId;
        $scope.SelectedCityId = filterParameters.selectedCityId;
        $scope.FilterText = filterParameters.filterText;

        $scope.Page = 0;
        providerService.getProviders($scope.categoryId, $scope.SelectedCityId, filterParameters.filterText, $scope.Page, $scope.PageSize).then(function (result) {
            $scope.Providers = result.data;
        });
    };

    $scope.loadMore = function () {
        $scope.Page = $scope.Page + 1;

        providerService.getProviders($scope.categoryId, $scope.SelectedCityId, $scope.FilterText, $scope.Page, $scope.PageSize).then(function (result) {
            $scope.Providers = $scope.Providers.concat(result.data);
            $scope.$broadcast('scroll.infiniteScrollComplete');
        });
    };

    $scope.moreDataCanBeLoaded = function () {
        if ($scope.Providers != null) {
            var loadedValues = $scope.Page * $scope.PageSize
            return loadedValues < $scope.Providers.length;
        }

        return true;
    };

}]);