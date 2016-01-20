"use strict";
mainApp.factory("notificationService", ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var notificationServiceFactory = {};

    var _getNotifications = function (userId) {
        var params = {
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Notification/GetNotifications", params)
            .then(function (result) {
                return result;
            });
    };

    var _getNotification = function (userId, notificationId) {
        var params = {
            UserId: userId,
            NotificationId: notificationId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Notification/GetNotification", params)
            .then(function (result) {
                return result;
            });
    };

    var _deleteNotification = function (userId, notificationId) {
        var params = {
            UserId: userId,
            NotificationId: notificationId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Notification/DeleteNotification", params)
            .then(function (result) {
                return result;
            });
    };

    notificationServiceFactory.getNotifications = _getNotifications;
    notificationServiceFactory.getNotification = _getNotification;
    notificationServiceFactory.deleteNotification = _deleteNotification;
    return notificationServiceFactory;
}]);