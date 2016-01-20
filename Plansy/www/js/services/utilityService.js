'user strict';
mainApp.factory("utilityService", ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var utilityServiceFactory = {};

    var _getCounties = function (countyId) {
        var params = {
            CountyId: (countyId != null && (typeof countyId != "undefined")) ? parseInt(countyId) : null
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Utility/GetCounties", params)
            .then(function (result) {
                return result;
            });
    };

    var _getCities = function (countyId) {
        var params = {
            CountyId: countyId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Utility/GetCities", params)
            .then(function (result) {
                return result;
            });
    };

    var _getDatefromString = function (stringDate) {
        if (stringDate != null && stringDate != "" && typeof stringDate != "undefined") {
            var result = stringDate.split("-");
            return new Date(parseInt(result[0]), parseInt(result[1]) - 1, parseInt(result[2]));
        }

        return null;
    };

    var _checkIfValueExistsInArray = function (list, value) {
        if (list != null) {
            for (var i = 0; i < list.length; i++) {
                if (list[i] == value) {
                    return true;
                    break;
                }
            }
        }

        return false;
    };

    var _checkIfObjectIdExistsInArray = function (list, value) {
        if (list != null) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].id == value) {
                    return true;
                    break;
                }
            }
        }

        return false;
    };

    var _getUserLogo = function (logoPath) {
        if (logoPath != null && logoPath != "") {
            return appAuthSettings.logosPath + logoPath + "?w=200&h=200&mode=crop";
        }

        return "";
    };

    utilityServiceFactory.getCounties = _getCounties;
    utilityServiceFactory.getCities = _getCities;
    utilityServiceFactory.getDatefromString = _getDatefromString;
    utilityServiceFactory.checkIfValueExistsInArray = _checkIfValueExistsInArray;
    utilityServiceFactory.checkIfObjectIdExistsInArray = _checkIfObjectIdExistsInArray;
    utilityServiceFactory.getUserLogo = _getUserLogo;
    return utilityServiceFactory;
}]);