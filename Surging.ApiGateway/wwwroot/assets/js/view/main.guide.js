define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require("bootstrap");
    require('jquerytmpl');
    require('pjaxEvent');
    var bindPjaxEvent = function () {
        $(document).pjaxEvent();
    };
    var init = function (options) {
        bindPjaxEvent();
    };
    exports.init = init;
});