define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataFaultTolerant tbody",
        btnSearch: "#btnSearch",
        searchForm: "#searchForm",
        queryParam: "#queryParam"
    };
    var config = require('../url.config.js');
    var serviceaddress = function (options) {
        var defaults = {
           faulttolerant_tpl: require("../../templates/faulttolerant_template.tpl")
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
            var formData = $(def.searchForm).serializeArray();
            formData[1].value = eval(formData[1].value);
            $.when(
                $.post(config.GET_COMMANDDESCRIPTOR, formData))
                .then(function (data) {
                    if (data.isSucceed) {
                        var tpl = $.tmpl(self.opts.faulttolerant_tpl, data);
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