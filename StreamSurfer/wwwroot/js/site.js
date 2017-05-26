$(function () {
    $("#global-search-input").click(function () {
        $("#global-search-form").submit();
    });

    // note this selects a lot of buttons
    $(".add-button").click(function (e) {
        var button = $(this);
        var selectedId = button.data("id");
        button.find(".add-button-text").text("Added to My List");
        button.find(".glyphicon").removeClass("glyphicon-plus");
        button.find(".glyphicon").addClass("glyphicon-ok");
        button.prop("disabled", true);
        $.ajax({
            method: "POST",
            url: "/Profile/AddToList",
            data: { id: selectedId }
        })
        .done(function (data) {
            console.log(data);
        });
    });
});