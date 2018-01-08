define(function (require, exports, module) {
    var system = {};
    (function () {
        var ua = navigator.userAgent.toLowerCase();
        var s;
        (s = ua.match(/msie ([\d.]+)/)) ? system.ie = s[1] :
            (s = ua.match(/firefox\/([\d.]+)/)) ? system.firefox = s[1] :
            (s = ua.match(/chrome\/([\d.]+)/)) ? system.chrome = s[1] :
            (s = ua.match(/opera.([\d.]+)/)) ? system.opera = s[1] :
            (s = ua.match(/version\/([\d.]+).*safari/)) ? system.safari = s[1] : 0;

        Date.prototype.format = function(format) {
            var o = {
                "M+": this.getMonth() + 1, //month 
                "d+": this.getDate(), //day 
                "h+": this.getHours(), //hour 
                "m+": this.getMinutes(), //minute 
                "s+": this.getSeconds(), //second 
                "q+": Math.floor((this.getMonth() + 3) / 3), //quarter 
                "S": this.getMilliseconds() //millisecond 
            };

            if (/(y+)/.test(format)) {
                format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
            }

            for (var k in o) {
                if (new RegExp("(" + k + ")").test(format)) {
                    format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
                }
            }
            return format;
        };

        String.prototype.htmlEncode = function () { var re = this; var q1 = [/\x26/g, /\x3C/g, /\x3E/g, /\x20/g]; var q2 = ["&amp;", "&lt;", "&gt;", "&nbsp;"]; for (var i = 0; i < q1.length; i++) re = re.replace(q1[i], q2[i]); return re; };

        String.prototype.toDateTime = function () { var val = this.replace(/[-]/g, "/"); if (val.isDate() || val.isDateTime()) return new Date(Date.parse(val)); var r = this.match(/(\d+)/); if (r) return new Date(parseInt(r)); return new Date(val); };

        String.prototype.isDate = function () { var r = this.replace(/(^\s*)|(\s*$)/g, "").match(/^(\d{1,4})(-|\/)(\d{1,2})\2(\d{1,2})$/); if (r == null) return false; var d = new Date(r[1], r[3] - 1, r[4]); return (d.getFullYear() == r[1] && (d.getMonth() + 1) == r[3] && d.getDate() == r[4]); };

        String.prototype.isDateTime = function () { var r = this.replace(/(^\s*)|(\s*$)/g, "").match(/^(\d{1,4})(-|\/)(\d{1,2})\2(\d{1,2}) (\d{1,2}):(\d{1,2}):(\d{1,2})$/); if (r == null) return false; var d = new Date(r[1], r[3] - 1, r[4], r[5], r[6], r[7]); return (d.getFullYear() == r[1] && (d.getMonth() + 1) == r[3] && d.getDate() == r[4] && d.getHours() == r[5] && d.getMinutes() == r[6] && d.getSeconds() == r[7]); };

        String.prototype.isInt = function () { return /^[-\+]?\d+$/.test(this); };

        Date.prototype.dateAdd = function (strInterval, Number) { var dtTmp = this; switch (strInterval) { case 's': return new Date(Date.parse(dtTmp) + (1000 * Number)); case 'n': return new Date(Date.parse(dtTmp) + (60000 * Number)); case 'h': return new Date(Date.parse(dtTmp) + (3600000 * Number)); case 'd': return new Date(Date.parse(dtTmp) + (86400000 * Number)); case 'w': return new Date(Date.parse(dtTmp) + ((86400000 * 7) * Number)); case 'q': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number * 3, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds()); case 'm': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds()); case 'y': return new Date((dtTmp.getFullYear() + Number), dtTmp.getMonth(), dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds()); }; };

        String.prototype.format = function(args) {
            var result = this;
            if (arguments.length > 0) {
                if (arguments.length == 1 && typeof(args) == "object") {
                    for (var key in args) {
                        if (args[key] != undefined) {
                            var reg = new RegExp("({" + key + "})", "g");
                            result = result.replace(reg, args[key]);
                        }
                    }
                } else {
                    for (var i = 0; i < arguments.length; i++) {
                        if (arguments[i] != undefined) {
                            var reg = new RegExp("({[" + i + "]})", "g");
                            result = result.replace(reg, arguments[i]);
                        }
                    }
                }
            }
            return result;
        };
        Array.prototype.remove = function(dx) {
            if (isNaN(dx) || dx > this.length) {
                return false;
            }
            for (var i = 0, n = 0; i < this.length; i++) {
                if (this[i] != this[dx]) {
                    this[n++] = this[i];
                }
            }
            this.length -= 1;
        };
        //找到返回所在索引，不存在返回-1  
        Array.prototype.index = function (el) {
            var i = 0;
            for (var i = 0, len = this.length; i < len; i++) {
                if (el == this[i]) {
                    return i;
                }
            }
            return -1;
        };
        // 判断数组中包含element元素
        Array.prototype.contains = function(e) {
            for (var i = 0; i < this.length; i++) {
                if (this[i] == e) {
                    return true;
                }
            }
            return false;
        };

       
        
        jQuery.validator.addMethod("stringCheck", function (value, element) {
            var strCheck = /^[a-zA-Z0-9\u4e00-\u9fa5-_]+$/;
            return this.optional(element) || (strCheck.test(value));
        }, "只能包含中文、英文、数字、下划线、横线字符");

        Array.prototype.OrderByDesc = function(func) {
            var m = {};
            for (var i = 0; i < this.length; i++) {
                for (var k = 0; k < this.length; k++) {
                    if (func(this[i]) > func(this[k])) {
                        m = this[k];
                        this[k] = this[i];
                        this[i] = m;
                    }
                }
            }
            return this;
        };
		
        //+---------------------------------------------------  
        //| 比较日期差 dtEnd 格式为日期型或者 有效日期格式字符串  
        //+---------------------------------------------------  
        Date.prototype.DateDiff = function(strInterval, dtEnd) {
            var dtStart = this;
            if (typeof dtEnd == 'string')//如果是字符串转换为日期型  
            {
                dtEnd = StringToDate(dtEnd);
            }
            switch (strInterval) {
            case 's':
                return parseInt((dtEnd - dtStart) / 1000);
            case 'n':
                return parseInt((dtEnd - dtStart) / 60000);
            case 'h':
                return parseInt((dtEnd - dtStart) / 3600000);
            case 'd':
                return parseInt((dtEnd - dtStart) / 86400000);
            case 'w':
                return parseInt((dtEnd - dtStart) / (86400000 * 7));
            case 'm':
                return (dtEnd.getMonth() + 1) + ((dtEnd.getFullYear() - dtStart.getFullYear()) * 12) - (dtStart.getMonth() + 1);
            case 'y':
                return dtEnd.getFullYear() - dtStart.getFullYear();
            }
        };
    })();
});