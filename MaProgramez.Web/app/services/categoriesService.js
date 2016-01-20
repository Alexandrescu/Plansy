'use strict';
app.factory('categoriesService', ['$http', 'maProgramezAuthSettings', function ($http, maProgramezAuthSettings) {

    var serviceBase = maProgramezAuthSettings.apiServiceBaseUri;

    var categoriesServiceFactory = {};

    var getAllCategories = function () {

        return $http.get(serviceBase + 'api/Categories').then(function (results) {
            return results;
        });
    };

    var getCategories = function (parentCategoryId) {

        return $http.get(serviceBase + 'api/Categories/GetCategories?parentCategoryId=' + parentCategoryId).then(function (results) {
            return results;
        });
    };

    categoriesServiceFactory.getAllCategories = getAllCategories;
    categoriesServiceFactory.getCategories = getCategories;

    return categoriesServiceFactory;

}]);