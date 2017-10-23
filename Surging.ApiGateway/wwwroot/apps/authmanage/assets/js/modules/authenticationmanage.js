define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    var init = function () {
        $(".app-board").on("click", "#leftSide-toggle", function () {
            var $this = $(this);
            if ($this.is(".icon-double-angle-right")) {
                $this.attr("class", "icon icon-double-angle-left");
                $(".app-left-side").removeClass("menu-min");
            } else {
                $this.attr("class", "icon icon-double-angle-right");
                $(".app-left-side").addClass("menu-min");
            }

        });
    }
    exports.init = init;
});