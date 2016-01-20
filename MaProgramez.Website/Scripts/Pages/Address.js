$(document).ready(function () {
    addressButtonsInit();
});

$(document).ajaxComplete(function () {
    addressButtonsInit();
});

function addressButtonsInit() {
    $(".address-form").off("submit.pssh");
    $(".address-form").on("submit.pssh", function () {
        var form = $(this);
        form.validate();

        var flag = $(form).find('select option[value="0"]').length == 0;
        if (flag) {
            if (form.valid()) {
                var dataLoadingTarget = $(this).attr("data-loading-target");
                $(dataLoadingTarget).show();
            }
        } else {
            return false;
        }
    });

    $(".add-address-button").off("click.pssh");
    $(".add-address-button").on("click.pssh", function () {
        $(this).parent("div").find(".add-address-box").toggle();
    });
}

function onAddAddressSuccess(data) {
    if (data) {
        var addressId = data.addressId;
        var content = "<div id=\"AddressItem-" + addressId + "\" class=\"address-box\">";
        content += data.view;
        content += "</div>";

        $("#Addresses").append(content);
        $("#LoadingAddress").hide();
        $("#AddAddressBox").hide();
    }
}

function onUpdateAddressSuccess(data) {
    if (data) {
        var addressId = data.addressId;
        $("#AddressItem-" + addressId).html(data.view);
        $("#LoadingAddress").hide();
    }
}

function onDeleteAddressSuccess(data) {
    if (data) {
        var addressId = data;
        $("#AddressItem-" + addressId).remove();
        $("#LoadingAddress").hide();
    }
}