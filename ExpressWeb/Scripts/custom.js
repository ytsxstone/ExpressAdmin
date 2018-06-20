var getCookie = function (name) {
    var value = "";
    var search = name + "="
    if (document.cookie.length > 0) {
        offset = document.cookie.indexOf(search);
        if (offset != -1) {
            offset += search.length;
            end = document.cookie.indexOf(";", offset);
            if (end == -1) {
                end = document.cookie.length;
            }

            value = unescape(document.cookie.substring(offset, end));
        }
    }
    return value;
};

//提示框
var MsgBox = (function () {
    var that = {};

    that.info = function (message, fn) {
        if (jQuery.isFunction(fn)) {
            $.messager.alert("提示", message, 'info', fn);
        } else {
            $.messager.alert("提示", message, 'info');
        }
    };

    that.error = function (message, fn) {
        if (jQuery.isFunction(fn)) {
            $.messager.alert("错误", message, 'error', fn);
        } else {
            $.messager.alert("错误", message, 'error');
        }
    };

    that.warning = function (message, fn) {
        if (jQuery.isFunction(fn)) {
            $.messager.alert("警告", message, 'warning', fn);
        } else {
            $.messager.alert("警告", message, 'warning');
        }
    };

    that.confirm = function (message, fn) {
        if (jQuery.isFunction(fn)) {
            $.messager.confirm('确认', message, function (isOK) {
                if (isOK) {
                    fn.call();
                }
            });
        };
    };

    that.confirm2 = function (message, fn) {
        if (jQuery.isFunction(fn)) {
            $.messager.confirm('确认', message, fn);
        };
    };
    /*
        例子：
        var test = function (isOK) {
        if (isOK) {
            alert('aok');
        };
        };

        util.MsgBox.confirm2("确定删除吗", test);
    */

    return that;
})();

// 弹出窗口
var Window = (function () {
    var that = {};

    //打开小模态窗口
    that.openSmallDialog = function (elementTag, titleName, icon, disposeCallback) {
        elementTag.dialog({
            title: titleName,
            height: 280,
            width: 430,
            modal: true, //模式窗口：窗口背景不可操作  
            //collapsible: true,     //可折叠，点击窗口右上角折叠图标将内容折叠起来  
            resizable: false, //可拖动边框大小  
            iconCls: icon,
            onBeforeClose: function () {
                that.clear($(this));
                if ($.isFunction(disposeCallback))
                    disposeCallback.call();
            },
            onClose: function () {
                //解决弹出窗口关闭后，验证消息还显示在最上面
                removeValidateBoxTip($(this));
            }
        });

        elementTag.dialog('open');
    };

    //打开中模态窗口
    that.openMiddleDialog = function (elementTag, titleName, icon, disposeCallback) {
        elementTag.dialog({
            title: titleName,
            height: 500,
            width: 650,
            modal: true, //模式窗口：窗口背景不可操作  
            //collapsible: true,     //可折叠，点击窗口右上角折叠图标将内容折叠起来  
            resizable: false, //可拖动边框大小  
            iconCls: icon,
            onBeforeClose: function () {
                that.clear($(this));
                if ($.isFunction(disposeCallback))
                    disposeCallback.call();
            },
            onClose: function () {
                //解决弹出窗口关闭后，验证消息还显示在最上面
                removeValidateBoxTip($(this));
            }
        });

        elementTag.dialog('open');
    };

    //打开大模态窗口
    that.openBigDialog = function (elementTag, titleName, icon, disposeCallback) {
        elementTag.dialog({
            title: titleName,
            height: 600,
            width: 800,
            modal: true, //模式窗口：窗口背景不可操作  
            //collapsible: true,     //可折叠，点击窗口右上角折叠图标将内容折叠起来  
            resizable: false, //可拖动边框大小  
            iconCls: icon,
            onBeforeClose: function () {
                that.clear($(this));
                if ($.isFunction(disposeCallback))
                    disposeCallback.call();
            },
            onClose: function () {
                //解决弹出窗口关闭后，验证消息还显示在最上面
                removeValidateBoxTip($(this));
            }
        });

        elementTag.dialog('open');
    };
    //上传图片模态窗口
    that.minopenBigDialog = function (elementTag, titleName, icon, disposeCallback) {
        elementTag.dialog({
            title: titleName,
            height: 480,
            width: 376,
            modal: true, //模式窗口：窗口背景不可操作  
            //collapsible: true,     //可折叠，点击窗口右上角折叠图标将内容折叠起来  
            resizable: false, //可拖动边框大小  
            iconCls: icon,
            onBeforeClose: function () {
                that.clear($(this));
                if ($.isFunction(disposeCallback))
                    disposeCallback.call();
            },
            onClose: function () {
                //解决弹出窗口关闭后，验证消息还显示在最上面
                removeValidateBoxTip($(this));
            }
        });

        elementTag.dialog('open');
    };

    // 关闭窗口
    that.closeDialog = function (elementTag) {
        elementTag.dialog('close');
    };

    //移除ValidateBoxTip
    var removeValidateBoxTip = function (elementTag) {
        //elementTag.find('.validatebox-tip').remove();
        if (elementTag.find(".validatebox-tip")) {
            elementTag.find(".validatebox-tip").remove();
        }

        if (elementTag.find(".validatebox-invalid")) {
            elementTag.find(".validatebox-invalid").removeClass("validatebox-invalid");
        }

        setTimeout(function () {
            if ($(document).find(".validatebox-tip")) {
                $(document).find(".validatebox-tip").remove();
            }

            if ($(document).find(".validatebox-invalid")) {
                $(document).find(".validatebox-invalid").removeClass("validatebox-invalid");
            }
        }, 230);
    };

    // pageTag：jquery对象，如$("#xxx")； url: 加载目标url
    that.loadPage = function (pageTag, url) {

        var idTag = "loadPagePanel__";
        var arrPageDiv = $("div[name='loadPage_panel']");
        var arrIdValue = [];
        var arrTemp = [];
        var curIdValue = 1;

        if (arrPageDiv.length > 0) {
            $.each(arrPageDiv, function (index, value) {
                arrTemp = value.id.split("__");
                arrIdValue.push(parseInt(arrTemp[1]));
            });

            curIdValue = Math.max.apply(null, arrIdValue) + 1;
        }

        idTag = idTag + curIdValue;
        pageTag.append("<div id=\"" + idTag + "\" name=\"loadPage_panel\" class=\"easyui-panel\"></div>");
        $('#' + idTag).panel({
            href: url,
            closable: true,
            width: 0,
            height: 0,
            border: false
        });

    };

    // 清除信息
    that.clear = function (elementTag) {
        var forms = elementTag.find("form");
        $.each(forms, function (index, item) {
            $(item).form('clear');
        });
    };

    return that;
})();

// 加载数据、查询数据时， 等待框
var showLoading = function (elementTag, message) {
    var msg = message ? message : "加载数据，请稍候...";
    $("<div class=\"datagrid-mask\"></div>").css({ display: "block", width: "100%", height: $(window).height() }).appendTo(elementTag);
    $("<div class=\"datagrid-mask-msg\"></div>")
        .html(msg)
        .appendTo(elementTag).css({ display: "block", left: "20%", top: ($(window).height() - 45) / 2 });
};

var closeLoading = function (elementTag) {
    elementTag.find('.datagrid-mask').remove();
    elementTag.find('.datagrid-mask-msg').remove();
};

// 增、删、改时， 等待框
var showProgress = function () {
    var win = $.messager.progress({
        title: '请稍候...'
    });
};

var closeProgress = function () {
    $.messager.progress('close');
};

//关闭模态窗口
var closeModelDialog = function (elementTag) {
    elementTag.dialog('close');
    $('.validatebox-tip').remove();
};

// 分页设置
var paginationConfig = function (pSize) {
    var defaultPageSize = 20;
    pSize = pSize ? pSize : defaultPageSize;

    return {
        layout: ['first', 'links', 'last'],
        showRefresh: false,
        pageNumber: 1,
        pageSize: pSize,
        pageList: [pSize],
        showPageList: false
    };
};

// grid配置，操作函数
var gridHandler = (function () {
    var that = {};

    //  无数据时显示横向滚动条 
    that.scrollShow = function (datagrid) {
        datagrid.prev(".datagrid-view2").children(".datagrid-body")
            .html("<div style='width:" + datagrid.prev(".datagrid-view2")
                .find(".datagrid-header-row").width() + "px;border:solid 0px;height:1px;'></div>");
    };

    that.getHeight = function (elementTag) {
        var height = 500;
        var arrElment = $("div").find(".panel-body").find(".panel-body-noborder");
        if (arrElment.length > 0) {
            var height = $("div").find(".panel-body").find(".panel-body-noborder").first().height();               
            height = height - elementTag.height() - 15;
        }

        return height;
    };

    that.dataBinding = function (elementTag, data) {
        var dataStr = JSON.stringify(data).replace(eval("/</gi"), "&lt;");
        dataStr = dataStr.replace(eval("/>/gi"), "&gt;");
        data = JSON.parse(dataStr);

        elementTag.datagrid('getPager').pagination('refresh', {
            pageNumber: data.PageCurrent
        });

        var gridData = { total: data.RowsCount, rows: data.Result, footer: data.Footer };
        elementTag.datagrid('loadData', gridData);
        if (data.Result.length <= 0) { // 无数据时，窗口过小 显示滚动条
            that.scrollShow(elementTag);
            elementTag.datagrid('clearSelections');
        } else {
            elementTag.datagrid('selectRow', 0);
        }
    };

    return that;
})();

var DateKit = (function () {
    var that = {};

    that.getNow = function () {
        var day = new Date();
        var Year = 0;
        var Month = 0;
        var Day = 0;
        var CurrentDate = "";
        //初始化时间 
        //Year= day.getYear();//有火狐下2008年显示108的bug 
        Year = day.getFullYear(); //ie火狐下都可以 
        Month = day.getMonth() + 1;
        Day = day.getDate();
        //Hour = day.getHours(); 
        // Minute = day.getMinutes(); 
        // Second = day.getSeconds(); 
        CurrentDate += Year + "-";
        if (Month >= 10) {
            CurrentDate += Month + "-";
        } else {
            CurrentDate += "0" + Month + "-";
        }
        if (Day >= 10) {
            CurrentDate += Day;
        } else {
            CurrentDate += "0" + Day;
        }
        return CurrentDate;
    }

    //+---------------------------------------------------  
    //| 日期计算  
    //+---------------------------------------------------  
    that.add = function (date, strInterval, Number) {
        var dtTmp = date;
        switch (strInterval) {
            case 's':
                return new Date(Date.parse(dtTmp) + (1000 * Number));
            case 'n':
                return new Date(Date.parse(dtTmp) + (60000 * Number));
            case 'h':
                return new Date(Date.parse(dtTmp) + (3600000 * Number));
            case 'd':
                return new Date(Date.parse(dtTmp) + (86400000 * Number));
            case 'w':
                return new Date(Date.parse(dtTmp) + ((86400000 * 7) * Number));
            case 'q':
                return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number * 3, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
            case 'm':
                return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
            case 'y':
                return new Date((dtTmp.getFullYear() + Number), dtTmp.getMonth(), dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
        }
    };

    //+---------------------------------------------------  
    //| 比较日期差 dtEnd 格式为日期型或者有效日期格式字符串  
    //+---------------------------------------------------  
    that.diff = function (date, strInterval, dtEnd) {
        var dtStart = date;
        if (typeof dtEnd == 'string') //如果是字符串转换为日期型  
        {
            dtEnd = that.StringToDate(dtEnd);
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

    //+---------------------------------------------------  
    //| 字符串转成日期类型   
    //| 格式 MM/dd/YYYY MM-dd-YYYY YYYY/MM/dd YYYY-MM-dd  
    //+---------------------------------------------------  
    that.StringToDate = function (DateStr) {
        var converted = Date.parse(DateStr);
        var myDate = new Date(converted);
        if (isNaN(myDate)) {
            //var delimCahar = DateStr.indexOf('/')!=-1?'/':'-';  
            var arys = DateStr.split('-');
            myDate = new Date(arys[0], --arys[1], arys[2]);
        }
        return myDate;
    }

    that.getIntervalDate = function (date, interval) {
        //获取系统时间
        var baseDate = new Date(Date.parse(date.replace(/-/g, "/")));
        var baseYear = baseDate.getFullYear();
        var baseMonth = baseDate.getMonth();
        var baseDate = baseDate.getDate();
        //处理
        var newDate = new Date(baseYear, baseMonth, baseDate);
        newDate.setDate(newDate.getDate() + interval); //取得系统时间的相差日期,interval 为负数时是前几天,正数时是后几天
        var temMonth = newDate.getMonth();
        temMonth++;
        var lastMonth = temMonth >= 10 ? temMonth : ("0" + temMonth)
        var temDate = newDate.getDate();
        var lastDate = temDate >= 10 ? temDate : ("0" + temDate)
        //得到最终结果
        newDate = newDate.getFullYear() + "-" + lastMonth + "-" + lastDate;
        return newDate;
    }

    //---------------------------------------------------  
    // 日期格式化  
    // 格式 YYYY/yyyy/YY/yy 表示年份  
    // MM/M 月份  
    // W/w 星期  
    // dd/DD/d/D 日期  
    // hh/HH/h/H 时间  
    // mm/m 分钟  
    // ss/SS/s/S 秒  
    //---------------------------------------------------  
    that.format = function (date, formatStr) {
        var str = formatStr;
        var Week = ['日', '一', '二', '三', '四', '五', '六'];

        str = str.replace(/yyyy|YYYY/, date.getFullYear());
        str = str.replace(/yy|YY/, (date.getYear() % 100) > 9 ? (date.getYear() % 100).toString() : '0' + (date.getYear() % 100));

        var month = date.getMonth() == 0 ? 1 : date.getMonth() + 1;
        str = str.replace(/MM/, month > 9 ? month.toString() : '0' + month);
        str = str.replace(/M/g, month);

        str = str.replace(/w|W/g, Week[date.getDay()]);

        str = str.replace(/dd|DD/, date.getDate() > 9 ? date.getDate().toString() : '0' + date.getDate());
        str = str.replace(/d|D/g, date.getDate());

        str = str.replace(/hh|HH/, date.getHours() > 9 ? date.getHours().toString() : '0' + date.getHours());
        str = str.replace(/h|H/g, date.getHours());
        str = str.replace(/mm/, date.getMinutes() > 9 ? date.getMinutes().toString() : '0' + date.getMinutes());
        str = str.replace(/m/g, date.getMinutes());

        str = str.replace(/ss|SS/, date.getSeconds() > 9 ? date.getSeconds().toString() : '0' + date.getSeconds());
        str = str.replace(/s|S/g, date.getSeconds());

        return str;
    };

    //+---------------------------------------------------  
    //| 日期合法性验证  
    //| 格式为：YYYY-MM-DD或YYYY/MM/DD  
    //+---------------------------------------------------  
    that.isValidDate = function (DateStr) {
        var sDate = DateStr.replace(/(^\s+|\s+$)/g, ''); //去两边空格;   
        if (sDate == '') return true;
        //如果格式满足YYYY-(/)MM-(/)DD或YYYY-(/)M-(/)DD或YYYY-(/)M-(/)D或YYYY-(/)MM-(/)D就替换为''   
        //数据库中，合法日期可以是:YYYY-MM/DD(2003-3/21),数据库会自动转换为YYYY-MM-DD格式   
        var s = sDate.replace(/[\d]{ 4,4 }[\-/]{ 1 }[\d]{ 1,2 }[\-/]{ 1 }[\d]{ 1,2 }/g, '');
        if (s == '') //说明格式满足YYYY-MM-DD或YYYY-M-DD或YYYY-M-D或YYYY-MM-D   
        {
            var t = new Date(sDate.replace(/\-/g, '/'));
            var ar = sDate.split(/[-/:]/);
            if (ar[0] != t.getYear() || ar[1] != t.getMonth() + 1 || ar[2] != t.getDate()) {
                //alert('错误的日期格式！格式为：YYYY-MM-DD或YYYY/MM/DD。注意闰年。');   
                return false;
            }
        } else {
            //alert('错误的日期格式！格式为：YYYY-MM-DD或YYYY/MM/DD。注意闰年。');   
            return false;
        }
        return true;
    };

    // 查询条件 默认日期时间段
    that.getSearchBetweenDate = function () {
        var period = { startDate: "", endDate: "" };

        period.endDate = this.getNow();
        //var dtArr = period.endDate.split("-");
        //var dateNow = new Date(dtArr[0], dtArr[1], dtArr[2]);
        //var lastMonth = this.add(dateNow, 'm', -1);
        //var lastMonth = this.add(lastMonth, 'd', 1);
        period.startDate = this.getIntervalDate(period.endDate, -30); // this.format(lastMonth, 'yyyy-MM-dd');

        return period;
    };

    // 查询条件 默认日期时间段
    that.getSearchThreeMonthsDate = function () {
        var period = { startDate: "", endDate: "" };

        period.endDate = this.getNow();
        //var dtArr = period.endDate.split("-");
        //var dateNow = new Date(dtArr[0], dtArr[1], dtArr[2]);
        //var lastMonth = this.add(dateNow, 'm', -1);
        //var lastMonth = this.add(lastMonth, 'd', 1);
        var day = new Date();
        day.setMonth(day.getMonth() - 3);
        var Year = 0;
        var Month = 0;
        var Day = 0;
        var CurrentDate = "";
        //初始化时间 
        //Year= day.getYear();//有火狐下2008年显示108的bug 
        Year = day.getFullYear(); //ie火狐下都可以 
        Month = day.getMonth() + 1;
        Day = day.getDate();
        //Hour = day.getHours(); 
        // Minute = day.getMinutes(); 
        // Second = day.getSeconds(); 
        CurrentDate += Year + "-";
        if (Month >= 10) {
            CurrentDate += Month + "-";
        } else {
            CurrentDate += "0" + Month + "-";
        }
        if (Day >= 10) {
            CurrentDate += Day;
        } else {
            CurrentDate += "0" + Day;
        }
        period.startDate = CurrentDate;
        //period.startDate = this.getIntervalDate(period.endDate,0); // this.format(lastMonth, 'yyyy-MM-dd');

        return period;
    };

    ///获取一周时间
    that.getWeekSearchBetweenDate = function () {
        var period = { startDate: "", endDate: "" };

        period.endDate = this.getNow();
        //var dtArr = period.endDate.split("-");
        //var dateNow = new Date(dtArr[0], dtArr[1], dtArr[2]);
        //var lastMonth = this.add(dateNow, 'm', -1);
        //var lastMonth = this.add(lastMonth, 'd', 1);
        period.startDate = this.getIntervalDate(period.endDate, -7); // this.format(lastMonth, 'yyyy-MM-dd');

        return period;
    };

    ///根据当前日期获取上一周日期
    that.getLastWeekSearchBetweenDate = function () {
        var period = { startDate: "", endDate: "" };

        var todayDate = new Date();
        if (todayDate.getDay() == 5) {
            period.endDate = this.getIntervalDate(this.getNow(), -5);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 6) {
            period.endDate = this.getIntervalDate(this.getNow(), -6);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 0) {
            period.endDate = this.getIntervalDate(this.getNow(), -7);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 1) {
            period.endDate = this.getIntervalDate(this.getNow(), -1);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 2) {
            period.endDate = this.getIntervalDate(this.getNow(), -2);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 3) {
            period.endDate = this.getIntervalDate(this.getNow(), -3);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        }
        else if (todayDate.getDay() == 4) {
            period.endDate = this.getIntervalDate(this.getNow(), -4);
            period.startDate = this.getIntervalDate(period.endDate, -6);
        } else {
            period.endDate = this.getNow();
            period.startDate = this.getIntervalDate(period.endDate, -7);
        }
        //period.endDate = this.getNow();
        //var dtArr = period.endDate.split("-");
        //var dateNow = new Date(dtArr[0], dtArr[1], dtArr[2]);
        //var lastMonth = this.add(dateNow, 'm', -1);
        //var lastMonth = this.add(lastMonth, 'd', 1);
        // period.startDate = this.getIntervalDate(period.endDate, -7); // this.format(lastMonth, 'yyyy-MM-dd');

        return period;
    };

    that.validDate = function (startDateObj, endDateObj) {
        var defaultDate = that.getSearchBetweenDate();
        //startDateObj.datebox('setValue', defaultDate.startDate);
        //endDateObj.datebox('setValue', defaultDate.endDate);

        startDateObj.datebox({
            onSelect: function (date) {
                var endDate = endDateObj.datebox("getValue");
                if (endDate == "") {
                    endDate = defaultDate.endDate;
                }

                if (that.StringToDate(endDate) < date) {
                    endDateObj.datebox("setValue", that.format(date, 'yyyy-MM-dd'));
                }
            }
        });

        endDateObj.datebox({
            onSelect: function (date) {
                var startDate = startDateObj.datebox("getValue");
                if (startDate == "") {
                    startDate = defaultDate.startDate;
                }

                if (that.StringToDate(startDate) > date) {
                    startDateObj.datebox("setValue", that.format(date, 'yyyy-MM-dd'));
                }
            }
        });

        return true;
    };

    return that;
})();

// 文件类
var FileKit = (function () {
    var that = {};
    var MSG_UPLOADING = "正在上传文件，请稍候...";
    var MSG_NOT_CONNECT = "请检查网络是否正常，请稍候再试...";
    var MSG_SERVER_BUSY = "服务器非常忙碌，请稍候再试...";
    var MSG_UNRIGHT = "您没有访问权限，请重新登录！";
    var MSG_LOGIN_TIMEOUT = "登录超时，请重新登录！";

    // panel: 当前面板id或对话框id ， 不带 # 符号；
    // fileId: type=file html元素的id， 不带 # 符号；
    // 上传文件的 url
    // 附属 属性数据（或 防跨域攻击标签）
    // callback :回调函数，即上传文件后的接收 返回数据 函数
    that.UploadFile = function (panelId, fileId, url, ant, callback) {
        $("#" + fileId).ajaxStart(function () {
            showLoading($("#" + panelId), MSG_UPLOADING);
        });

        $.ajaxFileUpload
        (
            ant,
            {
                url: url,
                secureuri: false,
                fileElementId: fileId,
                dataType: 'json',
                success: function (json, status) {
                    closeLoading($("#" + panelId));
                    if (callback != null)
                        callback(json, status);
                },
                error: function (json, status, e) {
                    closeLoading($("#" + panelId));
                    switch (XMLHttpRequest.status) {
                        case 500:
                            MsgBox.error(MSG_SERVER_BUSY);
                            break;
                        case 401:
                            MsgBox.warning(MSG_UNRIGHT);
                            break;
                        case 405:
                            MsgBox.warning(MSG_LOGIN_TIMEOUT);
                            break;
                        case 200:
                            break;
                        default:
                            MsgBox.error(MSG_NOT_CONNECT);
                            break;
                    }
                },
                complete: function () {
                    $("#" + fileId).val("");
                }
            }
        ) //ajaxFileUpload
    };

    // 字节转换 kb mb单位
    that.ConvertBytesSize = function (bytes) {
        var fldSalar = parseFloat(1024);
        var fldBytes = parseFloat(bytes);
        var strRet = "";

        if (fldBytes >= Math.pow(fldSalar, 2)) {
            strRet = (fldBytes / Math.pow(fldSalar, 2)).toFixed(2) + " MB";
        } else {
            strRet = (fldBytes / fldSalar).toFixed(1) + " KB";
        }

        return strRet;
    };

    return that; //Math.pow(x,y)
})();

var Ajax = (function () {
    var MSG_NOT_CONNECT = "请检查网络是否正常，请稍候再试...";
    var MSG_SERVER_BUSY = "服务器非常忙碌，请稍候再试...";
    var MSG_UNRIGHT = "您没有访问权限，请重新登录！";
    var MSG_LOGIN_TIMEOUT = "登录超时，请重新登录！";

    var that = {};
    that.getJSON = function (panelId, url, dataOrCallback, callback) {
        if (typeof ($(panelId)) != "undefined") {
            showLoading($(panelId)); //等待加载数据..
        }

        var funSuccess = {};
        var json = {};
        if (jQuery.isFunction(dataOrCallback)) {
            funSuccess = dataOrCallback;
            json = null;
        } else {
            json = dataOrCallback;
            funSuccess = callback;
        }

        $.ajax({
            type: "GET",
            url: url,
            dataType: "json",
            data: json,
            success: funSuccess,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (typeof ($(panelId)) != "undefined") {
                    closeLoading($(panelId)); //结束等待加载数据..
                }
                switch (XMLHttpRequest.status) {
                    case 500:
                        MsgBox.error(MSG_SERVER_BUSY);
                        break;
                    case 401:
                        MsgBox.warning(MSG_UNRIGHT, function () {
                            if (window && window.location) {
                                window.location.href = $("#LoginUrl").val();
                            }
                        });
                        break;
                    case 405:
                        MsgBox.warning(MSG_LOGIN_TIMEOUT, function () {
                            if (window && window.location) {
                                window.location.href = $("#LoginUrl").val();
                            }
                        });
                        break;
                    case 200:
                        break;
                    default:
                        MsgBox.error(MSG_NOT_CONNECT);
                        break;
                }
            },
            complete: function (XHR, TS) {
                if (typeof ($(panelId)) != "undefined") {
                    closeLoading($(panelId)); //结束等待加载数据..
                }
            }
        });
    };

    that.Post = function (url, dataOrCallback, callback) {
        showProgress();
        var funSuccess = {};
        var json = {};
        if (jQuery.isFunction(dataOrCallback)) {
            funSuccess = dataOrCallback;
            json = null;
        } else {
            json = dataOrCallback;
            funSuccess = callback;
        }

        $.ajax({
            type: "POST",
            url: url,
            dataType: "json",
            data: json,
            success: function (data, textStatus) {
                closeProgress();
                funSuccess(data, status);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                closeProgress();
                switch (XMLHttpRequest.status) {
                    case 500:
                        MsgBox.error(MSG_SERVER_BUSY);
                        break;
                    case 401:
                        MsgBox.warning(MSG_UNRIGHT, function () {
                            if (window && window.location) {
                                window.location.href = $("#LoginUrl").val();
                            }
                        });
                        break;
                    case 405:
                        MsgBox.warning(MSG_LOGIN_TIMEOUT, function () {
                            if (window && window.location) {
                                window.location.href = $("#LoginUrl").val();
                            }
                        });
                        break;
                    case 200:
                        break;
                    default:
                        MsgBox.error(MSG_NOT_CONNECT);
                        break;
                }
            },
            complete: function (XHR, TS) {
                closeProgress(); //结束 
            }
        });
    };

    return that;
})();

//获取路径
var SysUrl = (function () {
    var that = {};

    that.getUrl = function (path) {
        var strFullPath = window.document.location.href;
        var strPath = window.document.location.pathname;
        var pos = strFullPath.indexOf(strPath);
        var prePath = strFullPath.substring(0, pos);
        var postPath = strPath.substring(0, strPath.substr(1).indexOf('/') + 1);
        var vPath = postPath + path;
        //return (prePath + postPath);
        return vPath;
    };

    return that;
})();

//扩展easyui表单的验证  
$.extend($.fn.validatebox.defaults.rules, {
    //验证汉子  
    CHS: {
        validator: function (value) {
            return /^[\u0391-\uFFE5]+$/.test(value);
        },
        message: '只能输入汉字'
    },
    //移动手机号码验证  
    mobile: {//value值为文本框中的值  
        validator: function (value) {
            var reg = /^1[3|4|5|8|9]\d{9}$/;
            return reg.test(value);
        },
        message: '输入手机号码格式不准确.'
    },
    //国内邮编验证  
    zipcode: {
        validator: function (value) {
            var reg = /^[1-9]\d{5}$/;
            return reg.test(value);
        },
        message: '邮编必须是非0开始的6位数字.'
    },
    //用户账号验证(只能包括 _ 数字 字母)   
    account: {//param的值为[]中值  
        validator: function (value, param) {
            if (value.length < param[0] || value.length > param[1]) {
                $.fn.validatebox.defaults.rules.account.message = '用户名长度必须在' + param[0] + '至' + param[1] + '范围';
                return false;
            } else {
                if (!/^[\w]+$/.test(value)) {
                    $.fn.validatebox.defaults.rules.account.message = '用户名只能数字、字母、下划线组成.';
                    return false;
                } else {
                    return true;
                }
            }
        }, message: ''
    },
    integer: {//验证整数
        validator: function (value) {
            return /(0|^[+]?[1-9]+\d*)$/i.test(value);
        },
        message: '请输入整数'
    }
});

$.extend($.fn.datagrid.methods, {
    fixRowNumber: function (jq) {
        return jq.each(function () {
            var panel = $(this).datagrid("getPanel");
            var clone = $(".datagrid-cell-rownumber", panel).last().clone();
            clone.css({
                "position": "absolute",
                left: -1000
            }).appendTo("body");
            var width = clone.width("auto").width();
            if (width > 25) {
                //多加5个像素,保持一点边距  
                $(".datagrid-header-rownumber,.datagrid-cell-rownumber", panel).width(width + 5);
                $(this).datagrid("resize");
                //一些清理工作  
                clone.remove();
                clone = null;
            } else {
                //还原成默认状态  
                $(".datagrid-header-rownumber,.datagrid-cell-rownumber", panel).removeAttr("style");
            }
        });
    }
});  