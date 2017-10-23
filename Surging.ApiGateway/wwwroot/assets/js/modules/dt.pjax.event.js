define(function (require, exports, module) {
    var $ = require('jquery');
    require('jqueryPjax');
    function PjaxEvent(mainbox, options) {
        var self = this;
        var $mainbox = $(mainbox);
        if (options == undefined)
            options = {};
        var defaults = {
        };
        self.opts = $.extend(defaults, options || {});
        self.app_Btn = null;

        var appMode = function (target) {
            var $this = target;
            self.app_Btn = $this;
            $this.find(".app-icon").css("visibility", "hidden");
            $this.removeClass("active-reverse");
            $this.addClass("active");
            setTimeout(function () {
                if ($("#app_wrap").find("div").length == 0) {
                    $("#app_wrap").html('<div class="page-loading"><i class="icon-spinner icon-spin"></i> 请稍候，应用正在努力加载中...</div>');
                }
                $("#ov-btmright").attr("class", "active");
                $("#ov-btmright").css("position", "relative");
                $("#wrapper").hide();
            }, 900);

        }
        var windowHeight = function () {
            var docHeight = $(document).height(),
                winHeight = $(window).height();

            //if (docHeight > winHeight)
            //    return false;

            $(".app-wrapper", "#app_wrap").css("minHeight", winHeight);

            $(window).resize(function () {
                $(".app-wrapper", "#app_wrap").css("minHeight", winHeight);
            });
        }
        var closeAppMode = function () {
            var $appBtn = $(".pc-app-btn.active");
            if ($appBtn.length == 1) {
                $("#wrapper").show();
                $appBtn.removeClass("active");
                $appBtn.addClass("active-reverse");
                $("#app_wrap").empty();
                setTimeout(function () {
                    $("#ov-btmright").attr("class", "overlay").removeAttr("style");
                    $appBtn.find(".app-icon").css("visibility", "visible");
                    $appBtn.removeClass("active-reverse");
                }, 1100);
            } else {
                setTimeout(function () {
                    if ($("#wrapper").find(".work-page").length == 0)
                        window.location.reload();
                }, 500);

            }
        }
        var snsMode = function () {
            //关闭上传窗口
            $(".uploader-close", ".popover").click();
            setTimeout(function () {
                $(".pc-app", ".pc-wrapper").hide().show().addClass("pull-in activity");
                $(".pc-left", ".pc-wrapper").hide().show().css({ width: "86.333%" });
            }, 10);
        }
        var pageMode = function() {
            var $switchpage = $mainbox.find(".pc-switch-page");
            var $first = $switchpage.first();
            var $last = $switchpage.last();

            setTimeout(function() {

                $first.css("left", "-100%");
                $last.css("left", "0");
            }, 100);
            setTimeout(function() {
                $("body").attr("class", $last.data("class"));
                $first.remove();
            }, 880);
        };
        var createProgressBar = function() {
            var self = this;
            var template = [
                '<div id="nprogress" style="opacity:0;">',
                '<div class="bar" style="transition: all 500ms ease; -webkit-transition: all 500ms ease; -webkit-transform: translate3d(-100%, 0px, 0px);">',
                '</div>',
                '<div class="spinner">',
                '<div class="spinner-icon"></div>',
                '</div>',
                '</div>'
            ].join('');
            return template;
        };
        var pageStyleChange = function (o) {
            if (o.parents(".app-left-nav").length == 0) return;
            $(".app-left-nav").find(".active").removeClass("popup active");
            o.parents("li").find(".board-group ").addClass("popup active");
        };
        var BindEvent = function() {
            $mainbox.on('click', '[data-pjax]', function(event) {
                var $this = $(this);
                var isload = true;
                if ($this.data("mode") == "page" || $this.data("mode") == "appsHome") {
                    isload = false;
                }
                if ($this.data("mode") == "apps") {
                    appMode($this);
                }
                if ($this.data("mode") == "sns") {
                    //snsMode($this);
                }
                if ($this.data("mode") == "http") {
                    var keyWord = $this.data('key');
                    window.location.href = $this.attr("href") + "?keyWord=" + keyWord;
                    return;
                }
                if ($this.data("pjax").length > 0) {
                    $.pjax.click(event, { container: $($this.data("pjax")), isload: isload, timeout: 30000 });
                }
            });

            var onStart = function () {
                if ($("#nprogress").length == 0)
                    $("body").append(createProgressBar());

                setTimeout(function () {
                    $("#nprogress").css("opacity", "1");
                    $(".bar", "#nprogress").attr("style", " -webkit-transform: translate3d(-9.8%, 0px, 0px);-moz-transform: translate3d(-9.8%, 0px, 0px);");
                }, 200);
            }

            var onComplete = function () {
                $(".bar", "#nprogress").attr("style", " -webkit-transform: translate3d(-0%, 0px, 0px); -moz-transform: translate3d(-0%, 0px, 0px);");
                setTimeout(function () {
                    $("#nprogress").remove();
                }, 200);
            }

            $(document).ajaxStart(onStart)
                .ajaxSuccess(onComplete); 

            $mainbox.on('pjax:beforeSend', function(xhr, options, event) {
                //if ($("#nprogress").length == 0)
                //    $("body").append(createProgressBar());

                //setTimeout(function() {
                //    $("#nprogress").css("opacity", "1");
                //    $(".bar", "#nprogress").attr("style", " -webkit-transform: translate3d(-9.8%, 0px, 0px);-moz-transform: translate3d(-9.8%, 0px, 0px);");
                //}, 200);
                onStart();

            });

            $mainbox.on('pjax:complete', function(xhr, textStatus, options, event) {
                //$(".bar", "#nprogress").attr("style", " -webkit-transform: translate3d(-0%, 0px, 0px); -moz-transform: translate3d(-0%, 0px, 0px);");
                //setTimeout(function() {
                //    $("#nprogress").remove();
                //}, 200);
                onComplete();
                pageStyleChange($(event.target));
                //if ($(event.target).data("mode") == "popup") {  // 下版在做，弹出多层
                //    popupMode($(event.target));
                //}
                //else
                if ($(event.target).data("mode") == "sns") {
                    snsMode($(event.target));
                } else if ($(event.target).data("mode") == "page") {
                    if ($(".pc-switch-page").length == 1) {
                        $("#wrapper").append(textStatus.responseText);
                        pageMode();
                    }
                } else if ($(event.target).data("mode") == "apps") {
                    //windowHeight();
                } else if ($(event.target).data("mode") == "appsHome") {
                    closeAppMode();
                }
            });

            $mainbox.on('pjax:popstate', function(xhr) {
                window.history.replaceState(null, "", "#");
                window.location.replace(xhr.state.url);
            });
        };
        var Init = function() {
            BindEvent();
        };
        Init();
    }
    $.fn.pjaxEvent = function (options) {
        var args = arguments;
        return this.each(function () {
            var $this = $(this),
            plugin = $this.data('pjaxEvent');

            if (undefined === plugin) {
                plugin = new PjaxEvent(this, options);
                $this.data('pjaxEvent', plugin);
            }
        });
    };
});