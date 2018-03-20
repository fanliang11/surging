define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('jquerytmpl');
    var def = {
        wrap: "#dataFaultTolerant tbody",
        modal: "#myModal",
        modalBody: "#myModal .modal-body",
        modalForm: "#eventForm",
        editFaultTolerant:".editFaultTolerant",
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
        openDiag: function (id)
        {
            $.when(
                $.get(config.EDIT_FAULTTOLERANT, {serviceId:id}))
                .then(function (data) {
                    $(def.modalBody).replaceWith(data);
                    $(def.modal).modal('show');
                });

        },
        initEvent: function () {
            var self = this;
            $(def.btnSearch).off("click").bind("click", function () {
                self.loadData();
            });
            $(def.modalForm).off("submit").bind("submit", function () {
                self.bindSubmit();
                return false;
            });
            $(def.editFaultTolerant).off("click").bind("click", function () {
                var $tr = $(this).parents("tr");
                var serviceId = $.tmplItem($tr).data.Entity[$tr.index()].ServiceId;
                self.openDiag(serviceId);
            });
        },
        bindSubmit: function () {
            var self = this;
            var formData = $(def.modalForm).serializeObject();
            formData.InjectionNamespaces = formData.InjectionNamespaces.split(',');
            $.when(
                $.post(config.EDIT_FAULTTOLERANT, formData))
                .then(function (data) {
                    if (data.IsSucceed) {
                        $(def.modal).modal('hide');
                        self.loadData();
                    }
                });
        },
        loadData: function () {
            var self = this;
            var formData = $(def.searchForm).serializeArray();
            $.when(
                $.post(config.GET_COMMANDDESCRIPTOR, formData))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.faulttolerant_tpl, data);
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