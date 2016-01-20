'use strict';
app.controller('categoriesController', ['$scope', 'categoriesService', function ($scope, categoriesService) {

    $scope.categories = [];

    categoriesService.getCategories(1).then(function (results) {

        $scope.categories = results.data;

    }, function (error) {
        alert(error.data.message);
    });

}]);