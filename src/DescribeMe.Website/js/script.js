$(document).ready(function () {
    var statisticsHub = $.connection.statisticsHub;

    statisticsHub.updateStatistics = function (describedCount, describedPercentage, approvedCount, approvedPercentage) {
        $('.progress-bar.describe #desc-count').html(describedCount);
        $('.progress-bar.describe .progress').animate({
            width: describedPercentage + '%'
        }, 500);
        $('.progress-bar.describe .image-count').animate({
            left: describedPercentage + '%'
        }, 500);

        $('.progress-bar.review #desc-count').html(approvedCount);
        $('.progress-bar.review #total-count').html(describedCount);
        $('.progress-bar.review .progress').animate({
            width: approvedPercentage + '%'
        }, 500);
        $('.progress-bar.review .image-count').animate({
            left: approvedPercentage + '%'
        }, 500);
    };

    $('#message').delay(3000).fadeOut(300);
    
    $.connection.hub.start();
});