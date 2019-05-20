// Allow Moderator Queue to be set to default view
$(function () {
    var moderatorQueue = window.location.href.indexOf('moderatorQueue') > 0;
    if (moderatorQueue) {
        window.sessionStorage.setItem('moderatorQueue', true);
    }
    else {
        window.sessionStorage.removeItem('moderatorQueue');
    }

    // Set or unset preferences
    var select = $('#package-view');
    select.change(function () {
        if (select.prop("checked") == true) {
            window.localStorage.setItem('view', true);
        }
        else {
            localStorage.removeItem("view");
        }
    });

    // Get Items
    var view = window.localStorage.getItem('view');
    if (view) {
        select.prop("checked", true);
    }
    if (view && !stop) {
        localStorage.setItem("stop", "true"); //Prevents continuous reloads
    }
    else if (view && !moderatorQueue) {
        localStorage.removeItem("stop");
        location.search = '' + "?q=&moderatorQueue=true"; // Change Parameter
    }
});

// Package Filtering
$(function () {
    $("#sortOrder,#prerelease,#moderatorQueue,#moderationStatus").change(function () {
        $(this).closest("form").submit();
    });
    Closeable.modal("chocolatey_hide_packages_disclaimer");
    if (!getCookie('chocolatey_hide_packages_disclaimer')) {
        $(".modal-closeable").css('display', 'block');
    }
});

// Documentation Search Results
(function () {
    var cx = '013536524443644524775:xv95wv156yw';
    var gcse = document.createElement('script');
    gcse.type = 'text/javascript';
    gcse.async = true;
    gcse.src = 'https://cse.google.com/cse.js?cx=' + cx;
    var s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(gcse, s);
})();