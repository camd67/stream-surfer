$(function() {
    $("#settings-save-changes").click(function () {
        var bio = $("#bio-input");
        var profilePic = $(".profile-picture-carousel");
        var profilePath = profilePic.find(".profile-picture-selector:checked").data("image");
        $.ajax({
            method: "POST",
            url: "/Manage/Update",
            data: {
                bio: bio.val(),
                profilePicture: profilePath
            }
        })
        .done(function (data) {
            $(".update-result").text(data).show();
            setTimeout(function () {
                $(".update-result").hide().text("No profile changes detected");
            }, 5000);
        });
    });
})