define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataServiceDescriptor tbody",
        btnSearch: "#btnSearch",
        searchForm: "#searchForm",
        queryParam: "#queryParam"
    };
    var config = require('../url.config.js');
    var descriptor = function (options) {
        var defaults = {
            servicedescriptor_tpl: require("../../templates/servicedescriptor_template.tpl")
        };
        var self = this;
        this.opts = $.extend(defaults, options || {});
    };
    descriptor.prototype = {
        init: function () {
            var self = this;
            self.initEvent();
            self.loadData();

        },
        initEvent: function () {
            var self = this;
            $(def.searchForm).off("submit").bind("submit", function () {
                self.loadData();
                return false;
            });
        },
        loadData: function () {
            var self = this;
            var formData = $(def.searchForm).serializeArray();
            $.when(
                $.post(config.GET_SERVICEDESCRIPTOR, formData))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.servicedescriptor_tpl, data);
                        $(def.wrap).html(tpl);
                    }
                });
        }
    };
    exports.init = function (options) {
        var obj = new descriptor(options);
        obj.init();
    };

});