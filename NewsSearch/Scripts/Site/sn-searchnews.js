window.SN = window.SN || {};

(function (sn, $, undefined) {

    sn.loadSourceResult = function () {
        $(".lazy-loading").each(function () {
            var $this = $(this);
            var queryString = { id: $this.attr("id") };

            setTimeout(function () { 
                sn._lazyLoadContent(
                    sn.options.sourceResultUrl, queryString, $this, 0, null);
            }, 1000);
        });
    };

    sn._lazyLoadContent = function (url, queryString, placeholder, attempts, secTimeOut) {

        // security in case the call fails, the placeholder with 
        // the ajax loader is removed...
        if (secTimeOut == null) {
            secTimeOut = setTimeout(function () {
                sn.options.stopLazyLoading = true;
                sn._onLazyLoadFail(placeholder, secTimeOut, "Timed out to load results");
            }, 60000);
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
                if (!sn.options.stopLazyLoading && attempts < sn.options.lazyLoadingAttempts) {
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

    sn._onLazyLoadFail = function (placeholder, secTimeOut, message) {
        clearTimeout(secTimeOut);
        placeholder
            .removeClass("well")
            .removeClass("ajax-preloader")
            .addClass("alert alert-danger")
            .text(message || "Failed to load results");
    };

    sn.options = {
        sourceResultUrl: null,
        stopLazyLoading: false,
        lazyLoadingAttempts: 60
    };

    sn.init = function(options) {
        $.extend(true, sn.options, options);

        $("form").submit(function (event) {
            if ($.trim($("#SearchQuery").val()) === "")
                event.preventDefault();
        });

    };
}(window.SN.SearchNews = window.SN.SearchNews || {}, jQuery));