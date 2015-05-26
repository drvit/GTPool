window.SN = window.SN || {};

(function (sn, $, undefined) {

    sn.alwaysFocusOnSearch = function () {
        $('html').keydown(function () {
            //$(this).animate({ border: "1px solid orange" }, "slow");
            $('#SearchQuery').focus();
        });
    };

    sn.options = {};

    sn.init = function(options) {
        $.extend(true, sn.options, options);

        sn.alwaysFocusOnSearch();

    };
}(window.SN.SearchNews = window.SN.SearchNews || {}, jQuery));