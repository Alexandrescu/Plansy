'use strict';
mainApp.factory('categoryService', ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var categoryServiceFactory = {};

    var _getCategories = function (parentCategoryId, cityId, searchText) {
        var params = {
            ParentCategoryId: parentCategoryId != null ? parseInt(parentCategoryId) : null,
            CityId: cityId != null ? parseInt(cityId) : null,
            SearchText: searchText
        };
        
        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Categories/GetCategories", params).then(function (result){
            return result;
        });
    };

    var _getCategory = function (categoryId) {
        var params = {
            CategoryId: parseInt(categoryId)
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Categories/GetCategory", params)
            .then(function (result) {
                return result;
            });
    };

    categoryServiceFactory.getCategories = _getCategories;
    categoryServiceFactory.getCategory = _getCategory;
    return categoryServiceFactory;
}]);