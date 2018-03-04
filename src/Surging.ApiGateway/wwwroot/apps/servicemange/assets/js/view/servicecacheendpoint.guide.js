define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataServiceCache tbody",
        searchForm: "#searchForm",
        btnSearch: "#btnSearch",
        queryParam: "#queryParam"
    };
    var config = require('../url.config.js');
    var serviceCache = function (options) {
        var defaults = {
            servicecacheendpoint_tpl: require("../../templates/servicecacheendpoint_template.tpl")
        };
        var self = this;
        this.opts = $.extend(defaults, options || {});
    };
    serviceCache.prototype = {
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
        loadData: function () {
            var self = this;
            var formData = $(def.searchForm).serializeArray();
            $.when(
                $.post(config.GET_CACHEENDPOINT, formData))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.servicecacheendpoint_tpl, data);
                        $(def.wrap).html(tpl);
                    }
                });
        }
    };
    exports.init = function (options) {
        var obj = new serviceCache(options);
        obj.init();
    };

});