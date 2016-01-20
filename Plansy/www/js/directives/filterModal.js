"use strict";
mainApp.directive("filterModal", ["$ionicModal", '$translate', "$localStorage", "utilityService", function ($ionicModal, $translate, $localStorage, utilityService) {
    return {
        restrict: 'E',
        scope: {
            title: '=',
            locationTitle: '=',
            countyId: '=',
            cityId: '=',
            filterText: '=',
            categoryId: '=',
            callback: '&',
        },
        link: function (scope, element, attrs) {
            $ionicModal.fromTemplateUrl('templates/Shared/Modals/filterModal.html', {
                scope: scope
            }).then(function (modal) {
                scope.filterModal = modal;
            });

            if (scope.cityId != null && scope.cityId != "") {
                var filterDate = $localStorage.getObject("filterData");
                if (filterDate != null) {
                    scope.LocationTitle = filterDate.cityName + ", " + filterDate.countyName;
                }
            } else {
                $translate('SELECT_YOUR_CITY').then(function (title) {
                    scope.LocationTitle = title;
                });
            }

            scope.filterModalParameters = {
                filterText: "",
                selectedCountyName: "",
                selectedCountyId: "",
                selectedCityName: "",
                selectedCityId: ""
            };

            scope.closeFilterModal = function () {
                scope.filterModal.hide();
            };

            scope.getLocationNames = function () {
                if (scope.Cities != null) {
                    for (var i = 0; i < scope.Cities.length; i++) {
                        if (scope.Cities[i].id == scope.filterModalParameters.selectedCityId) {
                            scope.filterModalParameters.selectedCityName = scope.Cities[i].name;
                            break;
                        }
                    }

                    for (var i = 0; i < scope.Counties.length; i++) {
                        if (scope.Counties[i].id == scope.filterModalParameters.selectedCountyId) {
                            scope.filterModalParameters.selectedCountyName = scope.Counties[i].name;
                            break;
                        }
                    }

                    scope.LocationTitle = scope.filterModalParameters.selectedCityName + ", " + scope.filterModalParameters.selectedCountyName;
                }
            };

            scope.getCities = function () {
                if (scope.filterModalParameters.selectedCountyId == "") {
                    scope.filterModalParameters.selectedCityId == "";
                    scope.Cities = null;
                } else {
                    utilityService.getCities(scope.filterModalParameters.selectedCountyId)
                        .then(function (result) {
                            scope.Cities = result.data;
                            scope.filterModalParameters.selectedCityId = result.data[0].id;
                        });
                }
            };

            scope.sendParameters = function () {
                scope.getLocationNames();
                scope.callback()(scope.filterModalParameters);
                scope.closeFilterModal();
            };

            scope.resetParameters = function () {
                scope.filterModalParameters = {
                    filterText: "",
                    selectedCountyName: "",
                    selectedCountyId: "",
                    selectedCityName: "",
                    selectedCityId: ""
                };

                scope.Cities = null;

                $translate('SELECT_YOUR_CITY').then(function (title) {
                    scope.LocationTitle = title;
                });

                scope.callback()(scope.filterModalParameters);
                scope.closeFilterModal();
            }

            element.on("click", function () {
                if (scope.filterModalParameters.selectedCityId == "") {
                    utilityService.getCounties(scope.countyId).then(function (result) {
                        scope.Counties = result.data.counties;
                        scope.Cities = result.data.cities;

                        if (typeof scope.countyId != 'undefined') {
                            scope.filterModalParameters.selectedCountyId = scope.countyId;
                        }

                        if (typeof scope.cityId != 'undefined') {
                            scope.filterModalParameters.selectedCityId = scope.cityId;
                        }

                        if (typeof scope.filterText != 'undefined') {
                            scope.filterModalParameters.filterText = scope.filterText;
                        }

                        scope.filterModal.show();
                    });
                } else {
                    scope.filterModal.show();
                }
            });
        },
        templateUrl: 'templates/Shared/Modals/filter.html'
    };
}]);