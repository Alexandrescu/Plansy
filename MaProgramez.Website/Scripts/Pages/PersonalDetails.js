$(document).ready(function () {
    personalDetailsButtonsInit();
});

$(document).ajaxComplete(function () {
    personalDetailsButtonsInit();
});

function personalDetailsButtonsInit() {
    $(".update-personal-details-form").off("submit.pssh");
    $(".update-personal-details-form").on("submit.pssh", function () {
        var form = $(this);
        form.validate();

        if (form.valid()) {
            var dataLoadingTarget = $(this).attr("data-loading-target");
            $(dataLoadingTarget).show();
        }
    });
}

function onUpdatePersonalDetailsSuccess(data) {
    if (data) {
        $("#PersonalDetails").html(data.view);
        $("#LoadingPersonalDetails").hide();
    }
}