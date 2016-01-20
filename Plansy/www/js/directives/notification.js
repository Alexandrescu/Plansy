"use strict";
mainApp.directive("notification", ['$timeout', function ($timeout) {
    return {
        restrict: 'E',
        replace: true,
        link: function (scope, element, attrs) {
            scope.$on('showNotification', function (event, data) {
                scope.NotificationMessage = data.Message;
                scope.NotificationType = data.Type;

                $timeout(function () {
                    scope.closeNotification();
                }, 5000);
            });

            scope.closeNotification = function () {
                scope.NotificationMessage = null;
                scope.NotificationType = null;
            }
        },
        templateUrl: 'templates/Shared/Others/notification.html'
    }
}]);