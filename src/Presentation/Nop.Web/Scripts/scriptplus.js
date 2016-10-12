$(document).ready(function () {

    if (/android|webos|iphone|blackberry|iemobile|opera mini/i.test(navigator.userAgent.toLowerCase())) {
        $('.box-product .item-grid').addClass('slick-onmobile');
    }

    $('.slick-onmobile').slick({
        slidesToShow: 2,
        slidesToScroll: 2,
        //arrows: false,
        autoplay: true,
        infinite: false,
        //autoplaySpeed: 3000
    });

    $('.content-slider').slick({
    slidesToShow: 1,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 3000,
    prevArrow: '.prev',
    nextArrow: '.next',
    dots: true,
    //customPaging: function (slider, i) {
    //    return '<div class="tab">' + $(slider.$slides[i]).find('.slide-title').text() + '</div>';
    //},
});
    $('.picture-thumbs').slick({
        infinite: false,
        slidesToShow: 3,
        slidesToScroll: 1,
        autoplay: true,
        centerMode: true,
        centerPadding: '10px',
        autoplaySpeed: 3000
       
    });
    $('.slick-related').slick({
        slidesToShow: 5,
        slidesToScroll: 1,
        arrows: true,
        autoplay: false,
        //autoplaySpeed: 3000
        responsive: [
      {
          breakpoint: 1100,
          settings: {
              slidesToShow: 5,
              slidesToScroll: 5,
              dots: false
          }
      },
      {
          breakpoint: 992,
          settings: {
              slidesToShow: 4,
              slidesToScroll: 4,
              dots: false
          }
      },
          {
              breakpoint: 767,
              settings: {
                  slidesToShow: 2,
                  slidesToScroll: 2,
                  dots: false
              }
          },
          {
              breakpoint: 480,
              settings: {
                  slidesToShow: 2,
                  slidesToScroll: 2,
                  dots: false
              }
          }
        ]
    });

    $('.categiry-product .row').slick({
    slidesToShow: 3,
    slidesToScroll: 3,
    autoplay: true,
    autoplaySpeed: 4000,
    dots: false,
    responsive: [
      {
          breakpoint: 767,
          settings: {
              slidesToShow: 2,
              slidesToScroll: 2,
              arrows: false,
              dots: false
          }
      },
      {
          breakpoint: 480,
          settings: {
              slidesToShow: 1,
              slidesToScroll: 1,
              arrows: false,
              dots: false
          }
      }
    ]
    });

    $('.news-items').slick({
        slidesToShow: 3,
        slidesToScroll: 3,
        autoplay: true,
        autoplaySpeed: 4500,
        dots: false,
        responsive: [
          {
              breakpoint: 767,
              settings: {
                  slidesToShow: 2,
                  slidesToScroll: 2,
                  arrows: false,
                  dots: false
              }
          },
          {
              breakpoint: 480,
              settings: {
                  slidesToShow: 1,
                  slidesToScroll: 1,
                  arrows: false,
                  dots: false
              }
          }
        ]
    });

$('.produk-slick').slick({
    slidesToShow: 5,
    slidesToScroll: 5,
    autoplay: true,
    autoplaySpeed: 4000,
    dots: false,
    responsive: [
      {
          breakpoint: 1100,
          settings: {
              slidesToShow: 5,
              slidesToScroll: 5,
              infinite: true,
              arrows: false,
              dots: false
          }
      },
      {
          breakpoint: 992,
          settings: {
              slidesToShow: 4,
              slidesToScroll: 4,
              infinite: true,
              arrows: false,
              dots: false
          }
      },
      {
          breakpoint: 767,
          settings: {
              slidesToShow: 3,
              slidesToScroll: 3,
              arrows: false,
              dots: false
          }
      },
      {
          breakpoint: 480,
          settings: {
              slidesToShow: 2,
              slidesToScroll: 2,
              arrows: false,
              dots: false
          }
      }
    ]
});

    $(".mobile-menu").click(function () {
        $(this).children('.fa-bars, .fa-times').toggleClass("fa-bars fa-times");
        if (!$(".header-menuV2").hasClass("in")) {
            $(".header-menuV2").addClass("in").removeClass("out");
            $(".bg-overlay").attr("style", "display:block");
            $("body").attr("style", "overflow-y: hidden;");
        }
        else {
            $(".header-menuV2").removeClass("in");
            $(".closed").removeClass("in");
            $(".sub-menu-content").removeClass("in");
            $(".bg-overlay").attr("style", "display:none");
            $("body").attr("style", "overflow-y: auto;");
        }
    });
    $(".showmorecat").click(function () {
        $(".heightshow").toggleClass("xi");
    });
    $(".btn-shower").click(function () {
        $(".shower").slideToggle();
    });
    $(".have-submenu").click(function () {
        $(this).closest(".li-menu").find(".sub-menu-content").addClass("in");
        $(".closed").addClass("in");
        $(".header-menuV2").addClass("out");
    });
    $(".closed").click(function () {
        $(".sub-menu-content").removeClass("in");
        $(this).removeClass("in");
        $(".header-menuV2").removeClass("out");
    });
    $(".bg-overlay").click(function () {
        $(".header-menuV2").removeClass("out");
        $(".closed").removeClass("in");
        $(".header-menuV2").removeClass("in");
        $(this).attr("style", "display:none");
        $("body").attr("style", "overflow-y: auto;");
        $(".sub-menu-content").removeClass("in");
        $(".mobile-menu").children('.fa-bars, .fa-times').toggleClass("fa-bars fa-times");
    });

    //for moblie device
    $(".icon-header-right .fa.fa-search").click(function () {
        if (!$(".top-header .col-sm-5.col-xs-5").hasClass("in")) {
            $(".top-header .col-sm-5.col-xs-5").addClass("in");
        }
        else {
            $(".top-header .col-sm-5.col-xs-5").removeClass("in");
        }
    });
    $('.title-footer').after().click(function () {
        $(this).parentsUntil(".item-sec").find("ul").slideToggle();
        $(this).toggleClass("open");
    });

    if (/android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i.test(navigator.userAgent.toLowerCase())) {
        $('.iner-filter .block-title').addClass('tutup');
        $('.iner-filter .block-content').css('display', 'none');

        $(".showmorecat").click(function () {
            $('.heightshow').toggleClass('xs');
        });
    }

});

