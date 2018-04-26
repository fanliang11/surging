define(function (require, exports, module) {
    var $ = jQuery = require('jquery');
    require('bootbox');
    require('jquerytmpl');
    var def = {
        wrap: "#dataServiceCache tbody",
        modal: "#myModal",
        modalBody: "#myModal .modal-body",
        modalForm: "#eventForm",
        editCacheEndpoint: ".editCacheEndpoint",
        delCacheEndpoint: ".delCacheEndpoint",
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
        openDiag: function (id, endpoint) {
            $.when(
                $.get(config.EDIT_CACHEENDPOINT, { cacheId: id, endpoint: endpoint}))
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
            $(def.modalForm).off("submit").bind("submit", function () {
                self.bindSubmit();
                return false;
            });
            $(def.editCacheEndpoint).off("click").bind("click", function () {
                var $tr = $(this).parents("tr");
                var cacheEndpoint = $.tmplItem($tr).data.Entity[$tr.index()];
                self.openDiag(self.opts.Id, [cacheEndpoint.Host, ":", cacheEndpoint.Port].join(""));
            });
            $(def.delCacheEndpoint).off("click").bind("click", function () {
                var $tr = $(this).parents("tr");
                var cacheEndpoint = $.tmplItem($tr).data.Entity[$tr.index()];
                bootbox.confirm({
                    message: "是否删除",
                    buttons: {
                        confirm: {
                            label: '确定'  
                        },
                        cancel: {
                            label: '取消' 
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            self.delEndpoint(self.opts.Id, [cacheEndpoint.Host, ":", cacheEndpoint.Port].join(""));
                        }
                    }
                });
            });
        }, 
        bindSubmit: function () {
            var self = this;
            var formData = $(def.modalForm).serializeObject();
            var endpoint = [$("#Host").val(), ":", $("#Port").val()].join(""); 
            var data = {};
            data.CacheId = self.opts.Id;
            data.Endpoint = endpoint;
            data.CacheEndpoint = formData; 
            $.when(
                $.post(config.EDIT_CACHEENDPOINT, data))
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
                $.post(config.GET_CACHEENDPOINT, formData))
                .then(function (data) {
                    if (data.IsSucceed) {
                        var tpl = $.tmpl(self.opts.servicecacheendpoint_tpl, data);
                        $(def.wrap).html(tpl);
                        self.initEvent();
                    }
                });
        },
        delEndpoint: function (id, endpoint) {
            var self = this;
            $.when(
                $.post(config.DEL_CACHEENDPOINT, { cacheId: id, endpoint: endpoint }))
                .then(function (data) {
                    if (data.IsSucceed) { 
                        self.loadData();
                    }
                });
        }
    };
    exports.init = function (options) {
        var obj = new serviceCache(options);
        obj.init();
    };

});