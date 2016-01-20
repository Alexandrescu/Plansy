'use strict';
mainApp.factory('providerService', ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var providerServiceFactory = {};

    var _getProviders = function (categoryId, cityId, filterText, page, pageSize) {
        var params = {
            CategoryId: parseInt(categoryId),
            CityId: cityId != null ? parseInt(cityId) : null,
            Page: page != null ? parseInt(page) : null,
            PageSize: pageSize != null ? parseInt(pageSize) : null,
            SearchText: filterText
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/GetProviders", params)
            .then(function (results) {
                return results;
            });
    };

    var _getProvider = function (providerId, fullyLoadProvider) {
        var params = {
            ProviderId: providerId,
            FullyLoadProvider: fullyLoadProvider
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/GetProvider", params)
            .then(function (result) {
                return result;
            });
    };

    var _getOperations = function (userId, providerId, categoryId, selectedOperationsIds, selectedDate, selectedHour, selectedMinute, selectedSlotId) {
        var date = null;
        if (selectedDate != null && selectedDate != "" && typeof selectedDate != "undefined") {
            date = selectedDate.getFullYear() + "-" + (selectedDate.getMonth() + 1) + "-" + selectedDate.getDate()
        }

        var params = {
            UserId: userId,
            ProviderId: providerId,
            CategoryId: parseInt(categoryId),
            SelectedOperations: selectedOperationsIds,
            SelectedDate: date,
            SelectedHour: selectedHour,
            SelectedMinute: selectedMinute,
            SelectedSlotId: selectedSlotId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/GetProviderOperations", params)
            .then(function (results) {
                return results;
            });
    };

    var _isFavorite = function (userId, providerId, slotId) {
        var params = {
            UserId: userId,
            ProviderId: providerId,
            SlotId: (slotId != null ? parseInt(slotId) : null)
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/IsFavorite", params)
            .then(function (result) {
                return result;
            });
    };

    var _addToFavorite = function (userId, providerId, slotId) {
        var params = {
            UserId: userId,
            ProviderId: providerId,
            SlotId: (slotId != null ? parseInt(slotId) : null)
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/AddProviderOrSlotToFavourites", params)
            .then(function (result) {
                return result;
            });
    };

    var _removeFavorite = function (userId, providerId, slotId) {
        var params = {
            UserId: userId,
            ProviderId: providerId,
            SlotId: (slotId != null ? parseInt(slotId) : null)
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/RemoveProviderOrSlotToFavorites", params)
            .then(function (result) {
                return result;
            });
    };

    var _getFavoriteProviders = function (userId) {
        var params = {
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Provider/GetFavoriteProviders", params)
            .then(function (result) {
                return result;
            });
    };

    providerServiceFactory.getProviders = _getProviders;
    providerServiceFactory.getProvider = _getProvider;
    providerServiceFactory.getOperations = _getOperations;
    providerServiceFactory.isFavorite = _isFavorite;
    providerServiceFactory.addToFavorite = _addToFavorite;
    providerServiceFactory.removeFavorite = _removeFavorite;
    providerServiceFactory.getFavoriteProviders = _getFavoriteProviders;
    return providerServiceFactory;
}]);