$(document).ready(function () {
    $("#IsCompanyYes").off("change.pssh");
    $("#IsCompanyYes").on("change.pssh", function () {
        if ($(this).is(':checked')) {
            $(".company-box").show();
            $(".client-box").hide();
        }
    });

    $("#IsCompanyNo").off("change.pssh");
    $("#IsCompanyNo").on("change.pssh", function () {
        if ($(this).is(':checked')) {
            $(".company-box").hide();
            $(".client-box").show();
        }
    });

    $("#CreateSlotUserYes").off("change.pssh");
    $("#CreateSlotUserYes").on("change.pssh", function () {
        if ($(this).is(':checked')) {
            $(".slot-box").show();
        
        }
    });

    $("#CreateSlotUserNo").off("change.pssh");
    $("#CreateSlotUserNo").on("change.pssh", function () {
        if ($(this).is(':checked')) {
            $(".slot-box").hide();    
        }
    });

    $("#UploadImage").off("click.pssh");
    $("#UploadImage").on("click.pssh", function () {
        var target = $(this).data("target");
        if (target) {
            $(target).click();
        }
    });

    $("input[type='file']").off("change.pssh");
    $("input[type='file']").on("change.pssh", function (e) {
        var target = $(this).data("target");
        loadImage(e.target.files[0], function (img) {
            if (target) {
                $(target).html("");
                $(target).append(img);
            }
        });
    });
});