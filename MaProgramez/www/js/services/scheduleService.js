'use strict';
mainApp.factory('scheduleService', ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var scheduleServiceFactory = {};

    var _saveSchedule = function (userId, operationIds, slotId, selectedDate, hour, minute) {
        var date = selectedDate.getFullYear() + "-" + (selectedDate.getMonth() + 1) + "-" + selectedDate.getDate() + " " + hour + ":" + minute + ":0";
        var params = {
            UserId: userId,
            SelectedOperationIds: operationIds,
            SlotId: parseInt(slotId),
            AppointmenDateTime: date
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Schedules/SaveNewSchedule", params)
            .then(function (results) {
                return results;
            });
    };

    var _getNumberOfSchedules = function (userId) {
        var params = {
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Schedules/GetClientNoSchedulesFromNowOn", params)
            .then(function (result) {
                return result;
            });
    };

    var _getClientSchedulesHistory = function (userId) {
        var params = {
            UserId: userId,
            ShowHistory: true,
            ShowCurrent: false
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Schedules/GetClientSchedules", params)
            .then(function (result) {
                return result;
            });
    };

    var _getActiveSchedules = function (userId, selectedDate) {
        var params = {
            UserId: userId,
            ShowHistory: false,
            ShowCurrent: true
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Schedules/GetClientSchedules", params)
            .then(function (result) {
                return result;
            });
    };

    var _getSchedule = function (id, userId) {
        var params = {
            Id: parseInt(id),
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "/api/Schedules/GetSchedule", params)
            .then(function (result) {
                return result;
            });
    };

    var _cancelSchedule = function (id, userId) {
        var params = {
            Id: parseInt(id),
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "/api/Schedules/CancelSchedule", params)
            .then(function (result) {
                return result;
            });
    };

    scheduleServiceFactory.saveSchedule = _saveSchedule;
    scheduleServiceFactory.getNumberOfSchedules = _getNumberOfSchedules;
    scheduleServiceFactory.getClientSchedulesHistory = _getClientSchedulesHistory;
    scheduleServiceFactory.getActiveSchedules = _getActiveSchedules;
    scheduleServiceFactory.getSchedule = _getSchedule;
    scheduleServiceFactory.cancelSchedule = _cancelSchedule;
    return scheduleServiceFactory;
}]);