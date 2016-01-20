$(function(){
        var hasBeenTrigged = false;
        $(window).scroll(function() {
            if ($(this).scrollTop() >= 350 ) { // if scroll is greater/equal then 100 and hasBeenTrigged is set to false.
                $(document).ready(function () {
                  $('.slide .content').css({"opacity":"0"});
                  $('#team-section').css({"opacity":"1"});
                  $('.prezentare .title h2').css({"color":"#027e62"})
                  
                });
              
             }


             if ($(this).scrollTop() >= 50 ) { // if scroll is greater/equal then 100 and hasBeenTrigged is set to false.
                $(document).ready(function () {
                 
                  $('.navbar').css({
                      "position": "fixed",
                      "top": "0px",
                      "width": "100%",
                      "z-index": "999",
                      
                      
                      "height": "60px",
                      "padding-top": "0px",
                      "border-radius": "0px",
                      "padding-bottom": "15px"
                  }),
                  $('.over-bar').css({
                      "display": "none"
                  })
                });
              
             }
             else {$('.navbar').css({"position":"relative","box-shadow":"0px 0px 0px","height": "110px",
                      }),
                  $('.over-bar').css({
                      "display": "block"
                  })}



            if ($(this).scrollTop() <= 250 ) { // if scroll is greater/equal then 100 and hasBeenTrigged is set to false.
                $(document).ready(function () {
                  $('.slide .content').css({"opacity":"1"});
                  $('.prezentare .title h2').css({"color":"inherit"})

                  
                  
                });

              }
          });
      });

