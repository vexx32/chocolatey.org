// Prism for Description section
$(function () {
    // Description Area
    $('#description').find("pre").addClass('line-numbers border').wrapInner('<code class="language-powershell"></code>');
    Prism.highlightAllUnder($('#description')[0]);

    $(".comments-list").each(function () {
        var commentList = $(this);

        commentList.find("h4:contains('(maintainer)')").addClass('comment-title comment-maintainer');
        commentList.find("h4:contains('(reviewer)')").addClass('comment-title comment-reviewer');
        commentList.children().not('.comment-title').addClass("comment-body");
        
        // Style Code
    	$('.comments-list pre').contents().unwrap().wrap('<p></p>');

        commentList.find(".comment-title").each(function () {
            var h4 = $(this),
                text = h4.text(),
                textParts = text.split(' on');

            // Wrap comment in span to form bubble
            h4.nextUntil('.comment-title').addBack().wrapAll('<span class="comment-group"></span>');
            // Disect date and wrap
            if (textParts.length == 2) {
                h4.text(textParts[0]).after('<h6 class="comment-date">on ' + textParts[1] + '</h6>');
            }
        });

        // Define left or right chat position
        var commentMaintainer = commentList.find(".comment-maintainer").parent();
        var commentReviewer = commentList.find(".comment-reviewer").parent();

        if (commentList.hasClass("user-unknown")) {
            commentMaintainer.addClass("chat-left");
            commentReviewer.addClass("chat-right");
        }
        if (commentList.hasClass("user-maintainer")) {
            commentMaintainer.addClass("chat-right");
            commentReviewer.addClass("chat-left");
        }
        if (commentList.hasClass("user-moderator")) {
            commentMaintainer.addClass("chat-left");
            commentReviewer.addClass("chat-right");
        }

        // Scroll to bottom of container to show newest comments first
        commentList.parent().scrollTop(commentList.parent()[0].scrollHeight - commentList.parent()[0].clientHeight);
    });

    // Files Section
    var fileCollapse = $('.moderation-view [class*="file-path-"]');
    // Files hidden on load and toggled
    $('[class*="file-path-"]').on('show.bs.collapse', function () {
        if (!$(this).find('pre').hasClass('line-numbers')) {
            $(this).find('pre').addClass('line-numbers').find("code").addClass('language-powershell');
            Prism.highlightElement($(this).find('code')[0]);
        }
    });
    // Expand or Show all files
    $('#files .btn-collapse-files').click(function () {
        var $this = $(this);
        var thisText = $this.text();
        if ($this.hasClass('btn-success')) {
            $this.text(thisText.replace('Expand', 'Collapse'));
            $this.removeClass('btn-success').addClass('btn-danger');
            $('#files .btn:contains("Show")').html('Hide');
            fileCollapse.collapse('show');
        } else if ($this.hasClass('btn-danger')) {
            $this.text(thisText.replace('Collapse', 'Expand'));
            $this.removeClass('btn-danger').addClass('btn-success');
            $('#files .btn:contains("Hide")').html('Show');
            fileCollapse.collapse('hide');
        }
    });

    // Installation Tabs
    $("code:contains('STEP 3 URL')").html(function (_, html) {
        $(this).parent().next().remove();
        return html.replace(/(STEP 3 URL)/g, '<span class="stepThreeUrl">$1</span>');
    });
    $('[id*="stepThreeUrl"]').keyup(function () {
        var value = $(this).val();
        var defaultUrl = "http://internal/odata/repo";

        $('.stepThreeUrl').text(value);
        if (value == 0) {
            $('.stepThreeUrl').text(defaultUrl);
            $('.stepThree .tab-pane').prepend('<p class="step-three-danger text-danger font-weight-bold small">You must enter your internal repository url in Step 3 before proceeding.</p>');
            $('#install-step4 .code-toolbar').prepend('<p class="step-three-danger text-danger font-weight-bold small">You must enter your internal repository url in Step 3 before proceeding.</p>');
        }
        else {
            $('.step-three-danger').remove();
        }
    }).keyup();
    
    // Initialize Text Editor
    $('.text-editor').each(function () {
        if ($(this).is('#NewReviewComments')) {
            var placeholder = "Add to Review Comments";
        }
        else if ($(this).is('#ExemptedFromVerificationReason')) {
            placeholder = "Exempted Reason";
        }

        var easymde = new EasyMDE({
            element: this,
            autoDownloadFontAwesome: false,
            placeholder: placeholder,
            toolbar: ["bold", "italic", "heading", "strikethrough", "|", "quote", "unordered-list", "ordered-list", "code", "|", "link", "image", "|", "side-by-side", "fullscreen", "|", "preview"]
        });
        easymde.render();
        $('<span> Preview</span>').insertAfter($(this).next().find('.fa-eye')).parent().addClass('font-weight-bold text-primary');
    });
});