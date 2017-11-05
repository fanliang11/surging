define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataService tbody",
        modal: "#myModal",
        modalBody: "#myModal .modal-body",
        modalForm: "#eventForm",
        editServiceToken: ".editServiceToken",
        btnSearch: "#btnSearch",
        queryParam: "#queryParam"
    };
    var config = require('../url.config.js');
    var serviceaddress = function (options) {
        var defaults = {
            servicemanage_tpl: require("../../templates/authmanage_template.tpl")
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
        openDiag: function (id) {
            $.when(
                $.get(config.EDIT_SERVICETOKEN, { address: id }))
                .then(function (data) {
                    $(def.modalBody).replaceWith(data);
                    $(def.modal).modal('show');
                });

        },
        initEvent: function () {
            var self = this;
            $(def.btnSearch).off("click").bind("click", function () {
                self.loadData($(def.queryParam).val());
            });
            $(def.editServiceToken).off("click").bind("click", function () {
                var $tr = $(this).parents("tr");
                var address = $.tmplItem($tr).data.entity[$tr.index()].address;
                var serviceAddress = [address.ip, address.port].join(":");
                self.openDiag(serviceAddress);
            });
        },
        loadData: function (condition) {
            var self = this;
            $.when(
                $.post(config.GET_ADDRESS, { QueryParam: condition }))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.servicemanage_tpl, data);
                        $(def.wrap).html(tpl);
                        self.initEvent();
                    }
                });
        }
    };
    exports.init = function (options) {
        var obj = new serviceaddress(options);
        obj.init();
    };

});