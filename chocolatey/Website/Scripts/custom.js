// Preloader 
$(window).on('load', function () {
    $('#shape').fadeOut();
    $('#shadow').fadeOut();
    $('#loader').delay(350).fadeOut('slow');
    $('body').delay(350).css({ 'overflow': 'visible' });
});

// Top Navigation
$(document).ready(function () {
    // Top Alert
    var notice = window.sessionStorage.getItem('notice');
    if (notice) {
        $(".notice-text").addClass("d-none");
    }
    $('.notice-text').click(function () {
        sessionStorage.setItem('notice', 'true');
    });
    // Dropdowns on desktop
    $(".dropdown").on("click.bs.dropdown", function (e) {
        $target = $(e.target);
        // Stop dropdown from collapsing if clicked inside, otherwise collapse
        if (!$target.hasClass("dropdown-toggle")) {
            e.stopPropagation();
        }
    });
    // Fade in animation
    $('.dropdown').on('show.bs.dropdown', function () {
        if ($(window).width() < 992) {
            var height = $('header .alert').outerHeight();
        }
        else if ($(window).width() > 992 && $(window).width() < 1200) {
            var height = $('nav.navbar').outerHeight() - $('nav .navbar-collapse').outerHeight() + $('header .alert').outerHeight();
        }
        else {
            var height = $('nav.navbar').outerHeight() + $('header .alert').outerHeight();
        }
        var top = -$(window).scrollTop() + height;
        var $dropdown = $(this).find('.dropdown-menu').first();
        $dropdown.css("top", top);
        $dropdown.stop(true, true).fadeIn();
    });
    // Fade out animation
    $('.dropdown').on('hide.bs.dropdown', function () {
        $(this).find('.dropdown-menu').first().stop(true, true).fadeOut();
    });
    // Close the dropdown when page is scrolled
    $(window).on("scroll", function () {
        if ($(this).width() > 1200) {
            closeDropdowns();
        }
    });
    // Close the dropdown when viewport is resized on desktop
    $(window).on("resize", function () {
        if ($(this).width() > 1200) {
            closeDropdowns();
            closeNav();
        }
    });
    // Close the dropdown on mobile devices
    $('.goback').click(function () {
        closeDropdowns();
    });
    // Add/Remove fixed positioning for mobile
    $('#topNav').on('show.bs.collapse', function () {
        if ($(window).width() < 1200) {
            $(this).parent().addClass("position-fixed").css("z-index", "999").css("top", "0");
            $("body").addClass("position-fixed");
        }
    });
    $('#topNav').on('hide.bs.collapse', function () {
        $(this).parent().removeClass("position-fixed");
        $("body").removeClass("position-fixed");
    });
    // Closes Sub Nav
    function closeDropdowns() {
        $(".dropdown.show").find(".dropdown-toggle").dropdown('toggle');
    }
    // Closes Main Nav
    function closeNav() {
        $(".navbar-collapse.show").collapse('toggle');
    }
});

//Makes :contains case insensitive
$.expr[":"].contains = $.expr.createPseudo(function (arg) {
    return function (elem) {
        return $(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
    };
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

// Make input text selectable with one click
$(document).on('click', 'input[type=text]', function () {
    this.select();
});

// Smooth Scroll
// Select all links with hashes
$('a[href*="#"]')
    // Remove links that don't actually link to anything
    .not('[href="#"]')
    .not('[href="#0"]')
    .not('[data-toggle="collapse"]')
    .not('[data-toggle="tab"]')
    .not('[data-toggle="pill"]')
    .not('[data-slide="prev"]')
    .not('[data-slide="next"]')
    .click(function (event) {
        // Highlight active link if vertical nav
        var verticalNav = /pricing/.test(window.location.href);
        if (verticalNav) {
            $(".vertical-nav").find(".active").removeClass("active");
            $(this).parent().addClass('active');
        }
        // On-page links
        if (
            location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '')
            &&
            location.hostname == this.hostname
        ) {
            // Figure out element to scroll to
            var target = $(this.hash);
            var top = $('.sticky-top').outerHeight();
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            // Does a scroll target exist?
            if (target.length) {
                // Only prevent default if animation is actually gonna happen
                event.preventDefault();
                $('html, body').animate({
                    scrollTop: target.offset().top
                }, 1100, function () {
                    // Callback after animation
                    // Must change focus!
                    var $target = $(target);
                    $target.focus();
                    if ($target.is(":focus")) { // Checking if the target was focused
                        return false;
                    } else {
                        $target.attr('tabindex', '-1'); // Adding tabindex for elements not focusable
                        $target.focus(); // Set focus again
                    };
                });
        }
    }
});

// Right vertical navigation active highlight on scroll
$(function () {
    $(document).on("scroll", onScroll);
});
function onScroll(event) {
    var scrollPos = $(document).scrollTop();
    $('.docs-right a[href*="#"]').each(function () {
        var currLink = $(this);
        var refElement = $(currLink.attr("href"));
        if (refElement.position().top <= scrollPos) {
            $('.docs-right ul li').removeClass("active");
            currLink.parent().addClass("active");
        }
        else {
            currLink.parent().removeClass("active");
        }
    });
}

// Copy Button for use throughout the website
var clipboard = new ClipboardJS('.btn-copy');
$('.btn-copy').click(function () {
    var $this = $(this);
    $this.html('<span class="icon-check text-white"></span> Command Text Coppied').removeClass('btn-secondary').addClass('btn-success');
    setTimeout(function () {
        $this.html('<span class="icon-clipboard"></span> Copy Command Text').removeClass('btn-success').addClass('btn-secondary');
    }, 2000);
});

// Allow Callouts to be dismissible
$('[class*="callout-"] .close').click(function () {
    $(this).closest('[class*="callout-"]').hide();
});

// Documentation & Styleguide left side navigation
$(function () {
    setNavigation();
});
function setNavigation() {
    var path = window.location.pathname;
    path = path.replace(/\/$/, "");
    path = decodeURIComponent(path);

    $(".docs-left a").each(function () {
        var href = $(this).attr('href');
        if (path.substring(0, href.indexOf('docs/').length) === href || path.substring(0, href.indexOf('styleguide/').length) === href) {
            $(this).closest('li').addClass('active').parent().parent().collapse('show').parent().parent().parent().collapse('show');
        }
    });
}

// Delete extra space from code blocks
$(function () {
    var pre = document.getElementsByTagName("code");
    for (var i = 0, len = pre.length; i < len; i++) {
        var text = pre[i].firstChild.nodeValue;
        if (text != null) {
            pre[i].firstChild.nodeValue = text.replace(/^\n+|\n+$/g, "");
        }
    }
});

// Allow touch swiping of carousels on mobile devices
$(".carousel").on("touchstart", function (event) {
    var xClick = event.originalEvent.touches[0].pageX;
    $(this).one("touchmove", function (event) {
        var xMove = event.originalEvent.touches[0].pageX;
        if (Math.floor(xClick - xMove) > 5) {
            $(this).carousel('next');
        }
        else if (Math.floor(xClick - xMove) < -5) {
            $(this).carousel('prev');
        }
    });
    $(".carousel").on("touchend", function () {
        $(this).off("touchmove");
    });
});

// Stops video from playing when modal is closed or carousel is transitioned
$('.information-carousel')
    .on('shown.bs.modal', function () {
        $(this).carousel('pause');
    })
    .on('hide.bs.modal', function () {
        $(this).carousel('cycle');
    })
    .on('slide.bs.carousel', function () {
        $(this).find(".video-story .modal").modal('hide');
    });
$(window).scroll(function () {
    $(".video-story .modal").modal('hide');
});
$(".video-story .modal").on('hidden.bs.modal', function (e) {
    $(this).find("iframe").attr("src", $(this).find("iframe").attr("src"));
});