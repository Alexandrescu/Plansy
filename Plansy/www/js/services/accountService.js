'use strict';
mainApp.factory("accountService", ['$http', 'appAuthSettings', function ($http, appAuthSettings) {
    var accountServiceFactory = [];

    var _getUserDetails = function (userId) {
        var params = {
            UserId: userId
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/GetUserDetails", params)
            .then(function (result) {
                return result;
            });
    };

    var _saveUserDetails = function (userId, firstName, lastName, phone) {
        var params = {
            UserId: userId,
            FirstName: firstName,
            LastName: lastName,
            Phone: phone
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/SaveUserDetails", params)
            .then(function (result) {
                return result;
            });
    };

    var _changePassword = function (userId, oldPassword, newPassword, confirmPassword) {
        var params = {
            UserId: userId,
            OldPassword: oldPassword,
            NewPassword: newPassword,
            ConfirmPassword: confirmPassword
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/ChangePassword", params)
            .then(function (result) {
                return result;
            });
    };

    var _forgotPassword = function (email) {
        var params = {
            Email: email
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/ForgotPassword", params)
            .then(function (result) {
                return result;
            });
    };

    var _sendCode = function (userId, phoneNumber) {
        var params = {
            UserId: userId,
            Number: phoneNumber
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/SendPhoneNumberValidationCode", params)
            .then(function (result) {
                return result;
            });
    };

    var _verifyPhoneNumber = function (userId, phoneNumber, code) {
        var params = {
            UserId: userId,
            PhoneNumber: phoneNumber,
            Code: code
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/VerifyPhoneNumber", params)
            .then(function (result) {
                return result;
            });
    };

    var _changePhoneNumber = function (userId, phoneNumber) {
        var params = {
            UserId: userId,
            Number: phoneNumber
        };

        return $http.post(appAuthSettings.apiServiceBaseUri + "api/Account/SendPhoneNumberValidationCode", params)
            .then(function (result) {
                return result;
            });
    };

    accountServiceFactory.getUserDetails = _getUserDetails;
    accountServiceFactory.saveUserDetails = _saveUserDetails;
    accountServiceFactory.changePassword = _changePassword;
    accountServiceFactory.forgotPassword = _forgotPassword;
    accountServiceFactory.sendCode = _sendCode;
    accountServiceFactory.verifyPhoneNumber = _verifyPhoneNumber;
    accountServiceFactory.changePhoneNumber = _changePhoneNumber;
    return accountServiceFactory;
}]);