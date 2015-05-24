window.SN = window.SN || {};

(function(site, $, undefined) {

    site.options = {};

    site.init = function(options) {
        $.extend(true, site.options, options);


    };
}(window.SN.Site = window.SN.Site || {}, jQuery));