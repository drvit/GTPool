window.SN = window.SN || {};

(function (sn, $, undefined) {

    sn.loadSourceResult = function() {
        $(".lazy-loading").each(function () {
            var $this = $(this);

            var queryString = { id: $this.attr("id") };

            setTimeout(function () { 
                sn._lazyLoadContent(
                    sn.options.sourceResultUrl, queryString, $this, 0, null);
            }, 1000);
        });
    };

    sn._onLazyLoadFail = function(placeholder, secTimeOut) {
        clearTimeout(secTimeOut);
        placeholder
            .removeClass("well")
            .removeClass("ajax-preloader")
            .addClass("alert alert-danger")
            .text("Failed to load results");
    };

    sn._lazyLoadContent = function (url, queryString, placeholder, attempts, secTimeOut) {

        // security in case the call fails, the placeholder with 
        // the ajax loader is removed...
        if (secTimeOut == null) {
            secTimeOut = setTimeout(function () {
                sn._onLazyLoadFail(placeholder, secTimeOut);
            }, 45000);
        }

        $.ajax({
            method: "GET",
            url: url,
            dataType: "html",
            cache: false,
            data: queryString

        }).done(function(data) {
            if (data != null && $.trim(data) !== "") {
                clearTimeout(secTimeOut);

                var hiddenData = $(data).hide();
                placeholder.replaceWith(hiddenData);
                hiddenData.fadeIn(1000);
            }
            else {
                if (attempts < 60) {
                    attempts++;
                    setTimeout(function () { sn._lazyLoadContent(url, queryString, placeholder, attempts, secTimeOut); }, 1000);
                } else {
                    sn._onLazyLoadFail(placeholder, secTimeOut);
                }
            }

        }).fail(function(xhr, status, error) {
            sn._onLazyLoadFail(placeholder, secTimeOut);
        });
    };

    sn.alwaysFocusOnSearch = function () {
        $("html").keydown(function () {
            //$(this).animate({ border: "1px solid orange" }, "slow");
            $("#SearchQuery").focus();
        });
    };

    sn.options = {
        sourceResultUrl: null
    };

    sn.init = function(options) {
        $.extend(true, sn.options, options);

        //sn.alwaysFocusOnSearch();

        $("form").submit(function (event) {
            if ($.trim($("#SearchQuery").val()) === "")
                event.preventDefault();
        });

    };
}(window.SN.SearchNews = window.SN.SearchNews || {}, jQuery));