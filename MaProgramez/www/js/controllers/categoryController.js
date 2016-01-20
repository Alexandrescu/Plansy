controllers.controller('CategoryCtrl', ['$scope', '$stateParams', '$ionicModal', '$translate', '$localStorage', 'categoryService', function ($scope, $stateParams, $ionicModal, $translate, $localStorage, categoryService) {
    $scope.Id = $stateParams.id;

    $translate('CATEGORY_FILTER').then(function (title) {
        $scope.FilterTitle = title;
    });

    if ($scope.Id != "" && $scope.Id != null && typeof $scope.Id != "undefined") {
        categoryService.getCategory($scope.Id).then(function (result) {
            $scope.PageTitle = result.data.name;
        });
    }

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

    categoryService.getCategories($scope.Id, $scope.SelectedCityId, $scope.FilterText).then(function (result) {
        $scope.Categories = result.data;
    });

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

        categoryService.getCategories($scope.Id, filterParameters.selectedCityId, filterParameters.filterText)
            .then(function (result) {
                $scope.Categories = result.data;
            });
    };

    $scope.filterClick = function () {
        angular.element('#filterButton').trigger('click');
    };
}]);