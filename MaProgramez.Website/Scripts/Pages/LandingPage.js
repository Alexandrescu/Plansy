$(document).ready(function () {
    PageEvents();

    // SHOW CONTEST MODAL
    setTimeout(function () {
        $('#ContestModal').modal('show');
    }, 5000);
    pageEffects();

});

function PageEvents() {
    $('.slider').each(function () {
        $(this).slick({
            dots: false,
            infinite: true,
            autoplay: true,
            arrows: false,
            speed: 300,
            slidesToShow: 6,
            slidesToScroll: 1,
            responsive: [
              {
                  breakpoint: 1024,
                  settings: {
                      slidesToShow: 3,
                      slidesToScroll: 1,
                      infinite: true,
                      dots: false
                  }
              },
              {
                  breakpoint: 600,
                  settings: {
                      slidesToShow: 2,
                      slidesToScroll: 1,
                      infinite: true,
                  }
              },
              {
                  breakpoint: 480,
                  settings: {
                      slidesToShow: 1,
                      slidesToScroll: 1,
                      infinite: true,
                  }
              }
            ]
        });
    });

    $(".language-btn a").off("click.pssh");
    $(".language-btn a").on("click.pssh", function () {
        var value = $(this).data("value");
        document.cookie = "_culture=" + value + "; expires=Session; path=/";

        if (pageCulture != "") {
            var href = window.location.href.toLowerCase();
            href = href.replace(("/" + pageCulture), "/" + value);
            window.location.href = href;
        }
    });

    $(".accept-cookies").off("click.pssh");
    $(".accept-cookies").on("click.pssh", function () {
        setCookie("accept-cookies", "true", 365);
        $(".landingpage-cookie").hide();
    });

    $("#go-to-video").off("click.pssh");
    $("#go-to-video").on("click.pssh", function () {
        $('html, body').animate({
            scrollTop: $("#video-section").offset().top
        }, 800);
    });
}

function OnSuccess(data) {
    if (data && data.Error == false) {
        $(".contact-form").hide();
        $(".contact-message").html(data.Message);
        $(".contact-message").show();
    }
}

function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) +
      ((exdays == null) ? "" : ("; expires=" + exdate.toUTCString()));
    document.cookie = c_name + "=" + c_value;
}

function getCookie(c_name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == c_name) {
            return unescape(y);
        }
    }
}

function pageEffects() {

    $('.box1').waypoint(function () {
        $('.box1').addClass('animated bounce');
    }, {
        offset: '75%'
    });

    $('.box2').waypoint(function () {
        $('.box2').addClass('animated fadeInDown');
    }, {
        offset: '75%'
    });

    $('.box3').waypoint(function () {
        $('.box3').addClass('animated fadeInDown');
    }, {
        offset: '75%'
    });

    $('.box4').waypoint(function () {
        $('.box4').addClass('animated fadeInDown');
    }, {
        offset: '75%'
    });

    $('.box5').waypoint(function () {
        $('.box5').addClass('animated fadeInDown');
    }, {
        offset: '75%'
    });


    $('.box6').waypoint(function () {
        $('.box6').addClass('animated fadeIn');
    }, {
        offset: '75%'
    });


    $('.box7').waypoint(function () {
        $('.box7').addClass('animated fadeIn');
    }, {
        offset: '75%'
    });


    $('.box8').waypoint(function () {
        $('.box8').addClass('animated fadeIn');
    }, {
        offset: '75%'
    });
}

