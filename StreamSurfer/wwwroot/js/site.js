$(function () {
    $("#global-search-input").click(function () {
        $("#global-search-form").submit();
    });
    $("#global-search-form").submit(function () {
        $(".loading-overlay").fadeIn(500);
    });

    // note this selects a lot of buttons
    $(".add-button").on("click", function (e) {
        var button = $(this);
        var selectedId = button.data("id");
        var selectedType = $(this).data("type");
        button.find(".add-button-text").text("Added to My List");
        button.find(".glyphicon").removeClass("glyphicon-plus");
        button.find(".glyphicon").addClass("glyphicon-ok");
        button.prop("disabled", true);
        $.ajax({
            method: "POST",
            url: "/Profile/AddToList",
            data: { id: selectedId, type: selectedType }
        })
        .done(function (data) {
            console.log(data);
            $(".list-change-group").fadeIn();
        });
    });
    $(".list-save-changes-button").click(function (e) {
        var select = $(this);
        var parent = select.parent();
        var rating = parent.find(".list-rating");
        var status = parent.find(".list-status");
        var selectedId = select.data("id");
        var selectedType = select.data("type");
        $.ajax({
            method: "POST",
            url: "/Profile/UpdateList",
            data: {
                status: status.val(),
                rating: rating.val(),
                type: selectedType,
                id: selectedId
            }
        })
        .done(function (data) {
            console.log(data);
            if (data.startsWith("ERROR")) {
                console.error(data);
            }
        });
        $(".list-save-notification")
            .animate({ opacity: 1 }, 750,
            function () {
                $(this).delay(2500).animate({ opacity: 0 },
                    750);
            });
    });
});