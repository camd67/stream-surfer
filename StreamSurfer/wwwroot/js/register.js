$(function () {
    var regButton = $("#register-submit");
    var acceptCheck = $("#accept-terms");

    acceptCheck.click(function () {
        if (acceptCheck.prop("checked")) {
            $("#accept-terms-error").css("visibility", "hidden");
            regButton.prop("disabled", false);
        } else {
            $("#accept-terms-error").css("visibility", "visible");
            regButton.prop("disabled", true);
        }
    });

    regButton.click(function (e) {
        if (!acceptCheck.prop("checked")) {
            $("#accept-terms-error").css("visibility", "visible");
            e.preventDefault();
            return false;
        }
    });
})
