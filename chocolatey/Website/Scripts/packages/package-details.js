// Show package modal on form submit
$('#package-save').modal('show');

// Prism for Description section
$(document).ready(function () {
    $('#description').find("pre").addClass('line-numbers border').wrapInner('<code></code>');
    $('#description').find("code").addClass('language-powershell');
    Prism.highlightAll();
});

//Chat Bubbles
$(document).ready(function () {

    // Define element containing user role
    $(".comments-list h4:contains('maintainer')").addClass("user-role");
    $(".comments-list h4:contains('reviewer')").addClass("user-role");

    // Define comment message
    $('.comments-list').children().not('.user-role').addClass("comment-body");

    // Split user-role into two elements, so date can go on the bottom of chat bubble
    $('.user-role').each(function () {
        var $h4 = $(this),
            text = $h4.text(),
            textParts = text.split(' on');

        if (textParts.length == 2) { 
            $h4.text(textParts[0]).after('<h6 class="comment-date">on ' + textParts[1] + '</h6>');
        }
    });

    // Wrap in li
    $('.user-role').each(function () {
        $(this).nextUntil('.user-role').addBack().wrapAll('<li></li>');
    });

    // Unknown User
    $(".comments-list.user-unknown .user-role:contains('maintainer')").parent().addClass("chat-left");
    $(".comments-list.user-unknown .user-role:contains('reviewer')").parent().addClass("chat-right");
    //Maintainer
    $(".comments-list.user-maintainer .user-role:contains('maintainer')").parent().addClass("chat-right");
    $(".comments-list.user-maintainer .user-role:contains('reviewer')").parent().addClass("chat-left");
    //Moderator
    $(".comments-list.user-moderator .user-role:contains('maintainer')").parent().addClass("chat-left");
    $(".comments-list.user-moderator .user-role:contains('reviewer')").parent().addClass("chat-right");

    // Reverse order of comments
    ul = $('.comments-list');
    ul.children().each(function (i, li) { ul.prepend(li) })

    // Load more comments
    var button = $(".btn-load-more");
    $(function () {
        if ($(".comments-list li:hidden").length < 4) {
            button.hide();
        }
        $(".comments-list li").slice(0, 4).show().addClass('d-flex');
        button.click(function (e) {
            e.preventDefault();
            $(".comments-list li:hidden").slice(0, 100).show().addClass('d-flex');
            if ($(".comments-list li:hidden").length == 0) {
                button.hide();
            }
        });
    });
});

// Replace Show/Hide on buttons containing more than 1 word when clicked
$('.btn').click(function () {
    var $this = $(this);
    if ($this.is(':contains("Show")')) {
        $this.each(function () {
            var text = $this.text().replace('Show', 'Hide');
            $this.text(text);
        });
    } else if ($this.is(':contains("Hide")')) {
        $this.each(function () {
            var text = $this.text().replace('Hide', 'Show');
            $this.text(text);
        });
    }
});

// Expand or Show all files
$('#files .btn-danger').click(function () {
    var $this = $(this);
    $this.toggleClass('btn-success');
    if ($this.hasClass('btn-success')) {
        $this.html('<i class="icon-plus-circle" alt="Show Files"></i> Expand All');
        $this.removeClass('btn-danger');
        $(".collapse-2-content").removeClass('d-block').addClass('d-none').find("pre").removeClass('line-numbers language-powershell').find("code").removeClass('language-powershell');
        var $this = $('#files .btn:contains("Hide")');
        $this.html('Show');
    } else {
        $this.html('<i class="icon-minus-circle" alt="Collapse Files"></i> Collapse Files');
        $this.removeClass('btn-success').addClass('btn-danger');
        $(".collapse-2-content").removeClass('d-none').addClass('d-block').find("pre").addClass('line-numbers language-powershell').find("code").addClass('language-powershell');
        var $this = $('#files .btn:contains("Show")');
        $this.html('Hide');
    }
});

// Approved packages
$('.collapse-2 .btn').click(function () {
    var $this = $(this).nextAll(".collapse-2-content");
    $this.toggleClass('d-none');
    if ($this.hasClass('d-none')) {
        $this.removeClass('d-block').find("pre").removeClass('line-numbers language-powershell').find("code").removeClass('language-powershell');
    } else {
        $this.removeClass('d-none').addClass('d-block').find("pre").addClass('line-numbers language-powershell').find("code").addClass('language-powershell');
        Prism.highlightAll();
    }
});
// If expanded for moderation
var $files = $('.moderation-view .collapse-2');
var $expand = $(".moderation-view .collapse-2:contains('.nuspec'), .moderation-view .collapse-2:contains('chocolateyinstall.ps1'), .moderation-view .collapse-2:contains('chocolateyuninstall.ps1'), .moderation-view .collapse-2:contains('chocolateybeforemodify.ps1'), .moderation-view .collapse-2:contains('verification.txt'), .moderation-view .collapse-2:contains('license.txt')");
$(document).ready(function () {
    // Expand Files 
    if ($files.length <= 9) {
        $('.moderation-view .collapse-2-content').addClass('d-block').removeClass('d-none');
        $('.moderation-view .collapse-2-content.d-block').find("pre").addClass('line-numbers language-powershell').find("code").addClass('language-powershell');
        Prism.highlightAll();
    }
    // Collapse Files
    else if ($files.length >= 10) {
        $('.moderation-view .collapse-2-content').removeClass('d-block').addClass('d-none');
        $('.moderation-view .collapse-2 .btn').html('Show');

        if ($files.length > 30) {
            $('.moderation-view .btn-danger').addClass('d-none');
        } else {
            // Show "Expand All" button
            $('.moderation-view .btn-danger').addClass('btn-success').removeClass('btn-danger').html('<i class="icon-plus-circle" alt="Show Files"></i> Expand All');
            $('.moderation-view .btn-success').click(function () {
                Prism.highlightAll();
            });
        }
    }
    // Expand Specified Files 
    if ($expand) {
        $expand.addClass('always-expand');
        $expand.find('.collapse-2-content').addClass('d-block').removeClass('d-none').find("pre").addClass('line-numbers language-powershell').find("code").addClass('language-powershell');
        $expand.find('.btn').html('Hide');
        Prism.highlightAll();
    }
});