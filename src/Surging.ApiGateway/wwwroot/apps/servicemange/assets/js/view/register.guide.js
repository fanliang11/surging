define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataService tbody",
        btnSearch: "#btnSearch",
        queryParam: "#queryParam"
    };
    var config = require('../url.config.js');
    var serviceaddress = function (options) {
        var defaults = {
            servicemanage_tpl: require("../../templates/registermanage_template.tpl")
        };
        var self = this;
        this.opts = $.extend(defaults, options || {});
    };
    serviceaddress.prototype = {
        init: function () {
            var self = this;
            self.initEvent();
            self.loadData();

        },
        initEvent: function () {
            var self = this;
            $(def.btnSearch).off("click").bind("click", function () {
                self.loadData($(def.queryParam).val());
            });
        },
        loadData: function (condition) {
            var self = this;
            $.when(
                $.post(config.GET_REGISTERADDRESS, { QueryParam: condition }))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.servicemanage_tpl, data);
                        $(def.wrap).html(tpl);
                    }
                });
        }
    };
    exports.init = function (options) {
        var obj = new serviceaddress(options);
        obj.init();
    };

});