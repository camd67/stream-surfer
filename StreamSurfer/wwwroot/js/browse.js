$(document).ready(function () {
    $(".right-button").click(function () {
        var scrollLeft = $(this).parent().scrollLeft();
        $("#browseScrollDiv").animate({ scrollLeft: scrollLeft + 200 }, 300);
    });
});