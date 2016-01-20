$(function () {
    var signalR = {
        available: false
    };

    var notificationHub = $.connection.notificationHub;

    $.connection.hub.start().done(function () {
        signalR.availble = true;
        signalR.hubs = $.connection;
    });

    notificationHub.client.sendNotification = function (message) {
        if (typeof message != "undefined") {
            $(".notification-list").append(message);
            var count = parseInt($(".notification-count").html());
            count++;
            $(".notification-count").html(count);
            $(".notification-count").show();
        }
    };


    notificationHub.client.sendNewRequestMessageForSuppliers = function (message) {
        if (typeof message != "undefined") {
            $("#new-request-list").prepend(message);
        }
    }

    notificationHub.client.sendActiveRequestsCountToSuppliers = function (message) {
        if ((typeof message != "undefined") && (typeof message.ActiveRequetsCount != "undefined")) {
            $(".active-request-count").html(message.ActiveRequetsCount);
        }
    }

    notificationHub.client.refreshClientActiveRequest = function (message) {
        if (typeof message != "undefined") {
            $("#client-active-request-" + message.RequestId).html(message.View);
        }
    }

    notificationHub.client.refreshRequestTableList = function (message) {
        if (typeof message != "undefined") {
            if ((typeof message.RequestId != "undefined") && typeof message.View != "undefined") {
                var requestId = message.RequestId;
                var view = message.View;
                $("#request-list-item-" + requestId).html(view);
            }
        }
    }

    notificationHub.client.removeRequestFromList = function (requestId) {
        if (typeof requestId != "undefined") {
            $("#request-list-item-" + requestId).remove();
        }
    }

    $('#NotificationHeaderBox').on('shown.bs.dropdown', function () {
        $(".nano").nanoScroller({ scroll: 'top' });
    });

});