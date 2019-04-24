seajs.config({
    alias: {
        "jquery": "common/base/jquery.min.js",
        "bootstrap": "common/base/bootstrap.min.js",
        "extensions": "common/base/extensions.js",
        "ace-extra": "common/plugins/ace-extra.min.js",
        "ace-elements": "common/plugins/ace-elements.min.js",
        "ace": "common/plugins/ace.min.js",
        "jquerytmpl": "common/plugins/jquery.tmpl.min.js",
        "jqueryPjax": "common/plugins/jquery.pjax_n.js",
        "pjaxEvent": "common/modules/dt.pjax.event.js",
        "bootbox": "common/plugins/bootbox.min.js",
    },
    paths: {
        "common": "assets/js",
        "lib": "assets/lib",
        "apps_servicemange": "apps/servicemange/assets/js",
        "apps_authmanage":"apps/authmanage/assets/js"
    },
    map: [
        [/^(.*\.(?:css|js))(.*)$/i, '$1?t=201806201']
    ],
    debug: false,
    base: "/"
});