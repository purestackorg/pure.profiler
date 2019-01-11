/*tab*/
function setPureProfilerTab(m, n) {
    var tli = document.getElementById("menu" + m).getElementsByTagName("li");
    var mli = document.getElementById("tab-main" + m).getElementsByTagName("ul");
    for (i = 0; i < tli.length; i++) {
        tli[i].className = i == n ? "hover" : "";
        mli[i].style.display = i == n ? "block" : "none";
    }
}

function getStyle(obj, attr) {
    if (obj.currentStyle) {
        return obj.currentStyle[attr];
    } else {
        return getComputedStyle(obj, false)[attr];
    }
}


function clickGlobal() {
    
    var divProfile = document.getElementById("tabs1box");
    if (getStyle(divProfile, "display") == "none")
    {
        divProfile.style.display = "block"; 
    }
    else
    { divProfile.style.display = "none"; }

    return false;
}

function clickGlobal() {

    var divProfile = document.getElementById("tabs1box");
    if (getStyle(divProfile, "display") == "none") {
        divProfile.style.display = "block";
    }
    else { divProfile.style.display = "none"; }

    return false;
}


function clickRequestBody() {

    var divProfile = document.getElementById("pureprofiler-RequestBody");
    if (getStyle(divProfile, "display") == "none") {
        divProfile.style.display = "block";
    }
    else { divProfile.style.display = "none"; }

    return false;
}

function clickResponseBody() {

    var divProfile = document.getElementById("pureprofiler-ResponseBody");
    if (getStyle(divProfile, "display") == "none") {
        divProfile.style.display = "block";
    }
    else { divProfile.style.display = "none"; }

    return false;
}

//写cookies
function setCookie(c_name, value, expiredays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + expiredays);
    document.cookie = c_name + "=" + escape(value) + ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString());
}

//读取cookies
function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");

    if (arr = document.cookie.match(reg))

        return (arr[2]);
    else
        return null;
}

//删除cookies
function delCookie(name) {
    var exp = new Date();
    exp.setTime(exp.getTime() - 1);
    var cval = getCookie(name);
    if (cval != null)
        document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
}