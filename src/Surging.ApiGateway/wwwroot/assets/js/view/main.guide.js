define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require("bootstrap");
    require('jquerytmpl');
    require('pjaxEvent');
    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name]) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };
    var bindPjaxEvent = function () {
        $(document).pjaxEvent();
    };
    var init = function (options) {
        bindPjaxEvent();
   
    };
    exports.init = init;
});