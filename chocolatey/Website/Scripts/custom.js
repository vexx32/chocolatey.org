// Active state in nav
$("nav .nav-link").on("click", function () {
    $("nav").find(".active").removeClass("active");
    $(this).addClass("active");
});

//Tooltip
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})
$('.tt').tooltip({
    trigger: 'hover',
    placement: 'top'
});
function setTooltip(btn, message) {
    btn.tooltip('hide')
        .attr('data-original-title', message)
        .tooltip('show');
}
function hideTooltip(btn) {
    setTimeout(function () {
        btn.tooltip('hide');
    }, 1000);
}

// Initialize clipboard and change text
var clipboard = new ClipboardJS('.tt');

clipboard.on('success', function (e) {
    var btn = $(e.trigger);
    setTooltip(btn, 'Copied');
    hideTooltip(btn);
});