$(document).ready(function () {
    var number = 0;

    $("#UploadButton").off("click");
    $("#UploadButton").on("click", function () {
        var count = 0;
        $("#UploadImagePanel").find(".pssh-file").each(function () {
            if (!$(this).val() && !$(this).attr("data-session")) {
                $(this).click();
                return false;
            }
        });
    });

    $(".pssh-file").off("change");
    $(".pssh-file").on("change", function (e) {
        verifyEmptyInputFiles();

        var element = $(this);
        var name = element[0].files[0].name;

        // Load image preview
        loadImage(e.target.files[0], function (img) {
            number++;
            var result = "  <div class=\"panel panel-default\" id=\"panel-default-" + number + "\" data-target=\"" + $(e.target).attr("id") + "\">";
            result += "         <div class=\"panel-body\">";
            result += "             <div class=\"col-md-5\" id=\"panel-body-img-" + number + "\">";
            result += "             </div>";
            result += "             <div class=\"col-md-4\">";
            //result += "                 " + name + "";
            result += "             </div>";
            result += "             <div class=\"col-md-3\">";
            result += "                   <i class=\"fa fa-times-circle-o close-button float-right\" onclick=\"deleteUploadImage(" + number + ")\"></i>";
            result += "             </div>";
            result += "         </div>";
            result += "     </div>";
            $("#UploadImagePreview").append(result);
            $("#panel-body-img-" + number + "").append(img);
        }, {
            maxWidth: 200
        });
    });
});

function verifyEmptyInputFiles() {
    // Hide or show upload button
    var count = 0;
    var total = $("#UploadImagePanel").find(".pssh-file").length;
    var empty = false;
    $("#UploadImagePanel").find(".pssh-file").each(function () {
        count++;
        if (!$(this).val() && !$(this).attr("data-session")) {
            empty = true;
        }

        if (total == count) {
            if (empty) {
                $("#UploadButton").show();
            } else {
                $("#UploadButton").hide();
            }
        }
    });
}

function deleteUploadImage(number) {
    var target = $("#panel-default-" + number + "").data("target");
    $("#" + target).val("");
    $("#" + target).attr("data-session", "");
    $("#panel-default-" + number + "").remove();

    verifyEmptyInputFiles();
}

function deleteUploadImage(number, name) {
    var target = $("#panel-default-" + number + "").data("target");
    $("#" + target).val("");
    $("#" + target).attr("data-session", "");
    $("#panel-default-" + number + "").remove();

    if (name) {
        $.get("/Service/RemoveImageFromSession", { name: name }, function (data) {
            var test = data;
        }, 'json').fail(function (error) {
            alert(error);
        });
    }

    verifyEmptyInputFiles();
}