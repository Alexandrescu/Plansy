Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
}

$(document).ready(function () {
    timelineInit();
    buttonsInit();
    selectPickerInit();
    clocksInit();
    tooltipInit();
    getCities();
    getOperations();
});

$(document).ajaxComplete(function () {
    getOperations();
    getCities();
    buttonsInit();
    selectPickerInit();

    //$.validator.unobtrusive.parse('form');
});

function timelineInit() {
    $(".timeline-slider-table, .timeline-slider").each(function () {
        var myWidth = $(this).data("width");

        $(this).animate({
            width: myWidth + "%"
        }, 600, function () {
            $(".timeline-point").fadeIn();
            $(".timeline-best").fadeIn();
            $(".timeline-stop-point").fadeIn();
        });
    });

    if ($('.timeline-slider-table').length) {
        setInterval(function () {
            $('.timeline-slider-table[data-time]').each(function () {
                var time, serverTime, minutes, availableTime, pendingTime, warningTime, dangerTime, message;

                if ($(this).attr("data-time")) {
                    var dataTime = $(this).attr("data-time").split(",");
                    time = new Date(dataTime[0], dataTime[1], dataTime[2], dataTime[3], dataTime[4], dataTime[5], 0);
                }

                if ($(this).attr("data-server-time")) {
                    var dataServer = $(this).attr("data-server-time").split(",");
                    serverTime = new Date(dataServer[0], dataServer[1], dataServer[2], dataServer[3], dataServer[4], dataServer[5], 0);
                }

                if ($(this).attr("data-available-time")) {
                    availableTime = parseInt($(this).attr("data-available-time"));
                }

                if ($(this).attr("data-pending-time")) {
                    pendingTime = parseInt($(this).attr("data-pending-time"));
                }

                if ($(this).attr("data-warning-time")) {
                    warningTime = parseInt($(this).attr("data-warning-time"));
                }

                if ($(this).attr("data-danger-time")) {
                    dangerTime = parseInt($(this).attr("data-danger-time"));
                }

                if (time != null && serverTime != null && availableTime != null && pendingTime != null && warningTime != null && dangerTime != null) {
                    // GET TOTAL MINUTES
                    var diff = Math.abs(serverTime - time);
                    minutes = Math.floor((diff / 1000) / 60);

                    var hours = parseInt(minutes / 60);
                    var remainingHours = parseInt(parseInt(availableTime / 60) - hours);
                    var remainingMinutes = parseInt(availableTime - minutes);

                    // STYLE AND TEXT
                    var remainingHoursText = $(this).attr("data-remaining-hours-text");
                    var remainingMinutesText = $(this).attr("data-remaining-minutes-text");

                    if (minutes < pendingTime) {
                        message = remainingHours + " " + remainingHoursText;
                    } else if (minutes >= pendingTime && minutes <= warningTime) {
                        message = remainingHours + " " + remainingHoursText;
                        $(this).addClass("timeline-slider-pending");
                    } else if (minutes > warningTime && minutes <= dangerTime) {
                        message = remainingHours + " " + remainingHoursText;
                        $(this).removeClass("timeline-slider-pending");
                        $(this).addClass("timeline-slider-danger");
                    } else {
                        message = remainingMinutes + " " + remainingMinutesText;
                    }

                    if (minutes >= availableTime) {
                        $(this).removeAttr("data-time");
                        message = "Timp expirat";
                        $(this).removeClass("timeline-slider-danger");
                        $(this).addClass("timeline-slider-expired");

                        $(this).css("width", "100%");
                    } else {
                        var newWidth = parseInt((minutes * 100) / availableTime);

                        // CALCULATE NEW DATE
                        var newDate = new Date(serverTime.getTime() + (5000));
                        var newDateFormat = newDate.getFullYear() + "," + (newDate.getMonth()) + "," + newDate.getDate() + "," + newDate.getHours() + "," + newDate.getMinutes() + "," + newDate.getSeconds();

                        $(this).css("width", newWidth + "%");
                        $(this).attr("data-server-time", newDateFormat);
                    }

                    $(this).parent(".timeline-parent-table").attr("data-original-title", message);
                    $(this).parent(".timeline-parent-table")
                        .attr('data-original-title', message)
                        .tooltip('fixTitle');

                }
            });
        }, 5000);
    }

    if ($('.timeline-slider').length) {
        setInterval(function () {
            $('.timeline-slider[data-time]').each(function () {
                var time, offerTime, serverTime, minutes, availableTime, pendingTime, warningTime, dangerTime, message, dataReceiveHoursLimit, dataDecideHoursLimit;

                if ($(this).attr("data-time")) {
                    var dataTime = $(this).attr("data-time").split(",");
                    time = new Date(dataTime[0], dataTime[1], dataTime[2], dataTime[3], dataTime[4], dataTime[5], 0);
                }

                if ($(this).attr("data-offer-time")) {
                    var dataOfferTime = $(this).attr("data-offer-time").split(",");
                    offerTime = new Date(dataOfferTime[0], dataOfferTime[1], dataOfferTime[2], dataOfferTime[3], dataOfferTime[4], dataOfferTime[5], 0);
                }

                if ($(this).attr("data-server-time")) {
                    var dataServer = $(this).attr("data-server-time").split(",");
                    serverTime = new Date(dataServer[0], dataServer[1], dataServer[2], dataServer[3], dataServer[4], dataServer[5], 0);
                }

                if ($(this).attr("data-available-time")) {
                    availableTime = parseInt($(this).attr("data-available-time"));
                }

                if ($(this).attr("data-pending-time")) {
                    pendingTime = parseInt($(this).attr("data-pending-time"));
                }

                if ($(this).attr("data-warning-time")) {
                    warningTime = parseInt($(this).attr("data-warning-time"));
                }

                if ($(this).attr("data-danger-time")) {
                    dangerTime = parseInt($(this).attr("data-danger-time"));
                }

                if ($(this).attr("data-receive-hours-limit")) {
                    dataReceiveHoursLimit = parseInt($(this).attr("data-receive-hours-limit"));
                }

                if ($(this).attr("data-decide-hours-limit")) {
                    dataDecideHoursLimit = parseInt($(this).attr("data-decide-hours-limit"));
                }

                if (time != null && serverTime != null && availableTime != null && pendingTime != null && warningTime != null && dangerTime != null) {
                    var remainingHoursText = $(this).attr("data-remaining-hours-text");
                    var remainingMinutesText = $(this).attr("data-remaining-minutes-text");


                    // GET TOTAL MINUTES
                    var diff = Math.abs(serverTime - time);
                    minutes = Math.floor((diff / 1000) / 60);

                    var hours = parseInt(minutes / 60);
                    var remainingHours = parseInt(parseInt(availableTime / 60) - hours);
                    var remainingMinutes = parseInt(availableTime - minutes);


                    // WAITING TIME AND DECISION TIME
                    var requestEndTime = new Date(time);
                    //requestEndTime.addHours(72);
                    requestEndTime.addHours(dataReceiveHoursLimit);
                    var requestDiff = parseInt(Math.abs(requestEndTime - time));
                    var totalRequestTimeMinutes = parseInt(Math.floor(parseInt((requestDiff / 1000) / 60)));
                    var remainingWaitingHours = parseInt((totalRequestTimeMinutes / 60)) - parseInt((minutes / 60));
                    remainingWaitingHours = remainingWaitingHours < 0 ? 0 : remainingWaitingHours;
                    $(".waiting-text").html(remainingWaitingHours + " " + remainingHoursText);

                    if (offerTime) {
                        var offerEndTime = new Date(offerTime);
                        offerEndTime.addHours(dataDecideHoursLimit);
                        var offerDiff = parseInt(Math.abs(offerEndTime - offerTime));
                        var totalDecitionTimeMinutes = parseInt(Math.floor(parseInt((offerDiff / 1000) / 60)));

                        var serverOfferDiff = parseInt(Math.abs(serverTime - offerTime));
                        var totalServerOfferTime = parseInt(Math.floor((serverOfferDiff / 1000) / 60));

                        var remainingDecisionHours = parseInt((totalDecitionTimeMinutes / 60)) - parseInt((totalServerOfferTime / 60));
                        remainingDecisionHours = remainingDecisionHours < 0 ? 0 : remainingDecisionHours;
                        $(".decision-text").html(remainingDecisionHours + " " + remainingHoursText);
                    }


                    // STYLE AND TEXT
                    if (minutes < pendingTime) {
                        message = remainingHours + " " + remainingHoursText;
                    } else if (minutes >= pendingTime && minutes <= warningTime) {
                        message = remainingHours + " " + remainingHoursText;
                        $(this).addClass("timeline-slider-pending");
                    } else if (minutes > warningTime && minutes <= dangerTime) {
                        message = remainingHours + " " + remainingHoursText;
                        $(this).removeClass("timeline-slider-pending");
                        $(this).addClass("timeline-slider-danger");
                    } else {
                        message = remainingMinutes + " " + remainingMinutesText;
                    }

                    if (minutes >= availableTime) {
                        $(this).removeAttr("data-time");
                        message = "Timp expirat";
                        $(this).removeClass("timeline-slider-danger");
                        $(this).addClass("timeline-slider-expired");

                        $(this).css("width", "100%");
                    } else {
                        var newWidth = parseInt((minutes * 100) / availableTime);

                        // CALCULATE NEW DATE
                        var newDate = new Date(serverTime.getTime() + (5000));
                        var newDateFormat = newDate.getFullYear() + "," + (newDate.getMonth()) + "," + newDate.getDate() + "," + newDate.getHours() + "," + newDate.getMinutes() + "," + newDate.getSeconds();

                        $(this).css("width", newWidth + "%");
                        $(this).attr("data-server-time", newDateFormat);
                    }
                }
            });
        }, 5000);
    }
}

function buttonsInit() {
    $("input[pret-ofertat-primit]").off("change.pssh");
    $("input[pret-ofertat-primit]").on("change.pssh", function () {
        var target = $(this).attr("pret-ofertat-primit");
        var value = 0;
        if ($(this).val() * 5 / 100 > 15) {
            value = $(this).val() - $(this).val() * 5 / 100;
        } else {
            value = $(this).val() - 15;
        }
        $(target).val(value);
    });

    $("input[pret-primit-ofertat]").off("change.pssh");
    $("input[pret-primit-ofertat]").on("change.pssh", function () {
        var target = $(this).attr("pret-primit-ofertat");
        var value = 0;
        if ($(this).val() * 5 / 100 > 15) {
            value = +$(this).val() + +($(this).val() * 5 / 100);
        } else {
            value = +$(this).val() + +15;
        }
        $(target).val(value);
    });

    $("input[data-kwh-hp]").off("change.pssh");
    $("input[data-kwh-hp]").on("change.pssh", function () {
        var target = $(this).attr("data-kwh-hp");
        var value = parseInt(Math.round($(this).val() * 1.3636));
        $(target).val(value);
    });

    $("input[data-hp-kwh]").off("change.pssh");
    $("input[data-hp-kwh]").on("change.pssh", function () {
        var target = $(this).attr("data-hp-kwh");
        var value = parseInt(Math.round($(this).val() / 1.3636));
        $(target).val(value);
    });

    $(".language-btn a").off("click.pssh");
    $(".language-btn a").on("click.pssh", function () {
        var value = $(this).data("value");
        document.cookie = "_culture=" + value + "; expires=Session; path=/";

        if (pageCulture != "") {
            var href = window.location.href.toLowerCase();
            href = href.replace(("/" + pageCulture), "/" + value);
            window.location.href = href;
        }
    });

    $(".clickable-row").off("click.pssh");
    $(".clickable-row").on("click.pssh", function () {
        var link = $(this).data("href");
        window.location.href = link;
    });

    $(".switch-buttons input").off("change.pssh");
    $(".switch-buttons input").on("change.pssh", function () {
        $(this).parents("form").submit();
    });

    $("#HeaderLogoutButton").off("click.pssh");
    $("#HeaderLogoutButton").on("click.pssh", function (e) {
        $("#HeaderLogoutForm").submit();
        e.preventDefault();
    });

    $(".popup-delete-button").off("click.pssh");
    $(".popup-delete-button").on("click.pssh", function () {
        var target = $(this).attr("data-target");
        var id = $(this).attr("data-id");
        var name = $(this).attr("data-name");

        if (target && id && name) {
            $(target).find('input[name="id"]').val(id);
            $(target).find('.delete-name').html(name);
        }
    });

    $(".inline-cancel-button").off("click.pssh");
    $(".inline-cancel-button").on("click.pssh", function () {
        $(this).parents(".inline-delete-box").hide();
    });

    $(".inline-delete-button").off("click.pssh");
    $(".inline-delete-button").on("click.pssh", function () {
        var target = $(this).attr("data-target");
        $(target).show();
    });

    $(".hide-panel-button").off("click.pssh");
    $(".hide-panel-button").on("click.pssh", function () {
        var targetShow = $(this).attr("data-target-show");
        var targetHide = $(this).attr("data-target-hide");

        $(targetShow).show();
        $(targetHide).hide();
    });

    $(".car-pref-box").off("click.pssh");
    $(".car-pref-box").on("click.pssh", function () {
        if ($(this).hasClass("checked")) {
            $(this).removeClass("checked");
            $(this).find("input").prop('checked', false);
        } else {
            $(this).addClass("checked");
            $(this).find("input").prop('checked', true);
        }
    });

    $(".oferta").off("click.pssh");
    $(".oferta").on("click.pssh", function () {
        if ($(this).hasClass("checked")) {
            $(this).removeClass("checked");
            $(this).find("input").prop('checked', false);
        } else {
            $(this).addClass("checked");
            $(this).find("input").prop('checked', true);
        }
    });
}

function clocksInit() {
    $(".dial").knob();
}

function selectPickerInit() {
    $(".selectpicker").each(function () {
        if ($(this).attr("data-class")) {
            $(this).selectpicker({
                style: $(this).attr("data-class")
            });
        } else {
            $(this).selectpicker();
        }
    });
}

function tooltipInit() {
    $('*[data-toggle="tooltip"]').tooltip();
    $('*[data-toggle="popover"]').popover();
}

function getOperations() {
    $(".slot-select").off("change.pssh");
    $(".slot-select").on("change.pssh", function () {
        var target = $(this).attr("data-select-target");
        var slotId = $(this).val();

        if (target && slotId) {
            $.get("/Service/GetOperations", {id: slotId }, function (data) {
                var select = $(target);
                select.empty();
                $.each(data, function (index, itemData) {
                    select.append($('<option/>', {
                        value: itemData.id,
                        text: itemData.name
                    }));

                    if (index == data.length - 1) {
                        $(target).selectpicker("refresh");
                        $(target).change(); // ADDED FOR SELECT CHANGE EVENT TRIGGER
                    }
                });
            }, 'json').fail(function (error) {
                alert(error);
            });
        }
    });
}

function getCities() {
    $(".county-select").off("change.pssh");
    $(".county-select").on("change.pssh", function () {
        var target = $(this).attr("data-select-target");
        var countyId = $(this).val();

        if (target && countyId) {
            $.get("/Service/GetCities", { id: countyId }, function (data) {
                var select = $(target);
                select.empty();
                $.each(data, function (index, itemData) {
                    select.append($('<option/>', {
                        value: itemData.id,
                        text: itemData.name
                    }));

                    if (index == data.length - 1) {
                        $(target).selectpicker("refresh");
                        $(target).change(); // ADDED FOR SELECT CHANGE EVENT TRIGGER
                    }
                });
            }, 'json').fail(function (error) {
                alert(error);
            });
        }
    });
}
