﻿/*tab*/
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

function autoRefresh() {
    var btnRefresh = document.getElementById("btnRefresh");
    if (btnRefresh) {
        btnRefresh.click();
    }
}
var autoRefreshProfilingEvent = null;
var cookieNameAutoRefreshProfiling = "cookieNameAutoRefreshProfiling";
//自动刷新
function onChangeSelIntervalVal() {

    var myselect = document.getElementById("selInterval");
    if (myselect) {
        var index = myselect.selectedIndex;
        if (index < 0) {
            index = 0;
        }
        var selval = myselect.options[index].value;
        var ival = parseInt(selval);
        if (ival > 0) {
            if (autoRefreshProfilingEvent !== null) {
                window.clearInterval(autoRefreshProfilingEvent); 
            }
            var cookieValue = getCookie(cookieNameAutoRefreshProfiling);
            if (cookieValue !== "" && cookieValue !== null) {
                delCookie(cookieNameAutoRefreshProfiling);
            }
            //重复执行某个方法 
            autoRefreshProfilingEvent = window.setInterval(autoRefresh, (ival * 1000)); 

            setCookie(cookieNameAutoRefreshProfiling, ival, 7);
        }
        else {
            delCookie(cookieNameAutoRefreshProfiling);
        }
    }
    return ;
    //myselect.options[index].text;
     
}

window.onload = function () {

    var selInterval = document.getElementById("selInterval");
    if (selInterval) {
        var cookieValue = getCookie(cookieNameAutoRefreshProfiling);
        if (cookieValue !== "" && cookieValue !== null) {
            document.all.selInterval.value = cookieValue; 
            onChangeSelIntervalVal();
        }
     
    }
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


/*! highlight.js v9.13.1 | BSD3 License | git.io/hljslicense */
!function (e) { var n = "object" == typeof window && window || "object" == typeof self && self; "undefined" != typeof exports ? e(exports) : n && (n.hljs = e({}), "function" == typeof define && define.amd && define([], function () { return n.hljs })) }(function (e) { function n(e) { return e.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;") } function t(e) { return e.nodeName.toLowerCase() } function r(e, n) { var t = e && e.exec(n); return t && 0 === t.index } function a(e) { return k.test(e) } function i(e) { var n, t, r, i, o = e.className + " "; if (o += e.parentNode ? e.parentNode.className : "", t = M.exec(o)) return w(t[1]) ? t[1] : "no-highlight"; for (o = o.split(/\s+/), n = 0, r = o.length; r > n; n++)if (i = o[n], a(i) || w(i)) return i } function o(e) { var n, t = {}, r = Array.prototype.slice.call(arguments, 1); for (n in e) t[n] = e[n]; return r.forEach(function (e) { for (n in e) t[n] = e[n] }), t } function c(e) { var n = []; return function r(e, a) { for (var i = e.firstChild; i; i = i.nextSibling)3 === i.nodeType ? a += i.nodeValue.length : 1 === i.nodeType && (n.push({ event: "start", offset: a, node: i }), a = r(i, a), t(i).match(/br|hr|img|input/) || n.push({ event: "stop", offset: a, node: i })); return a }(e, 0), n } function u(e, r, a) { function i() { return e.length && r.length ? e[0].offset !== r[0].offset ? e[0].offset < r[0].offset ? e : r : "start" === r[0].event ? e : r : e.length ? e : r } function o(e) { function r(e) { return " " + e.nodeName + '="' + n(e.value).replace('"', "&quot;") + '"' } l += "<" + t(e) + E.map.call(e.attributes, r).join("") + ">" } function c(e) { l += "</" + t(e) + ">" } function u(e) { ("start" === e.event ? o : c)(e.node) } for (var s = 0, l = "", f = []; e.length || r.length;) { var g = i(); if (l += n(a.substring(s, g[0].offset)), s = g[0].offset, g === e) { f.reverse().forEach(c); do u(g.splice(0, 1)[0]), g = i(); while (g === e && g.length && g[0].offset === s); f.reverse().forEach(o) } else "start" === g[0].event ? f.push(g[0].node) : f.pop(), u(g.splice(0, 1)[0]) } return l + n(a.substr(s)) } function s(e) { return e.v && !e.cached_variants && (e.cached_variants = e.v.map(function (n) { return o(e, { v: null }, n) })), e.cached_variants || e.eW && [o(e)] || [e] } function l(e) { function n(e) { return e && e.source || e } function t(t, r) { return new RegExp(n(t), "m" + (e.cI ? "i" : "") + (r ? "g" : "")) } function r(a, i) { if (!a.compiled) { if (a.compiled = !0, a.k = a.k || a.bK, a.k) { var o = {}, c = function (n, t) { e.cI && (t = t.toLowerCase()), t.split(" ").forEach(function (e) { var t = e.split("|"); o[t[0]] = [n, t[1] ? Number(t[1]) : 1] }) }; "string" == typeof a.k ? c("keyword", a.k) : B(a.k).forEach(function (e) { c(e, a.k[e]) }), a.k = o } a.lR = t(a.l || /\w+/, !0), i && (a.bK && (a.b = "\\b(" + a.bK.split(" ").join("|") + ")\\b"), a.b || (a.b = /\B|\b/), a.bR = t(a.b), a.endSameAsBegin && (a.e = a.b), a.e || a.eW || (a.e = /\B|\b/), a.e && (a.eR = t(a.e)), a.tE = n(a.e) || "", a.eW && i.tE && (a.tE += (a.e ? "|" : "") + i.tE)), a.i && (a.iR = t(a.i)), null == a.r && (a.r = 1), a.c || (a.c = []), a.c = Array.prototype.concat.apply([], a.c.map(function (e) { return s("self" === e ? a : e) })), a.c.forEach(function (e) { r(e, a) }), a.starts && r(a.starts, i); var u = a.c.map(function (e) { return e.bK ? "\\.?(" + e.b + ")\\.?" : e.b }).concat([a.tE, a.i]).map(n).filter(Boolean); a.t = u.length ? t(u.join("|"), !0) : { exec: function () { return null } } } } r(e) } function f(e, t, a, i) { function o(e) { return new RegExp(e.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&"), "m") } function c(e, n) { var t, a; for (t = 0, a = n.c.length; a > t; t++)if (r(n.c[t].bR, e)) return n.c[t].endSameAsBegin && (n.c[t].eR = o(n.c[t].bR.exec(e)[0])), n.c[t] } function u(e, n) { if (r(e.eR, n)) { for (; e.endsParent && e.parent;)e = e.parent; return e } return e.eW ? u(e.parent, n) : void 0 } function s(e, n) { return !a && r(n.iR, e) } function p(e, n) { var t = R.cI ? n[0].toLowerCase() : n[0]; return e.k.hasOwnProperty(t) && e.k[t] } function d(e, n, t, r) { var a = r ? "" : j.classPrefix, i = '<span class="' + a, o = t ? "" : I; return i += e + '">', i + n + o } function h() { var e, t, r, a; if (!E.k) return n(k); for (a = "", t = 0, E.lR.lastIndex = 0, r = E.lR.exec(k); r;)a += n(k.substring(t, r.index)), e = p(E, r), e ? (M += e[1], a += d(e[0], n(r[0]))) : a += n(r[0]), t = E.lR.lastIndex, r = E.lR.exec(k); return a + n(k.substr(t)) } function b() { var e = "string" == typeof E.sL; if (e && !L[E.sL]) return n(k); var t = e ? f(E.sL, k, !0, B[E.sL]) : g(k, E.sL.length ? E.sL : void 0); return E.r > 0 && (M += t.r), e && (B[E.sL] = t.top), d(t.language, t.value, !1, !0) } function v() { y += null != E.sL ? b() : h(), k = "" } function m(e) { y += e.cN ? d(e.cN, "", !0) : "", E = Object.create(e, { parent: { value: E } }) } function N(e, n) { if (k += e, null == n) return v(), 0; var t = c(n, E); if (t) return t.skip ? k += n : (t.eB && (k += n), v(), t.rB || t.eB || (k = n)), m(t, n), t.rB ? 0 : n.length; var r = u(E, n); if (r) { var a = E; a.skip ? k += n : (a.rE || a.eE || (k += n), v(), a.eE && (k = n)); do E.cN && (y += I), E.skip || E.sL || (M += E.r), E = E.parent; while (E !== r.parent); return r.starts && (r.endSameAsBegin && (r.starts.eR = r.eR), m(r.starts, "")), a.rE ? 0 : n.length } if (s(n, E)) throw new Error('Illegal lexeme "' + n + '" for mode "' + (E.cN || "<unnamed>") + '"'); return k += n, n.length || 1 } var R = w(e); if (!R) throw new Error('Unknown language: "' + e + '"'); l(R); var x, E = i || R, B = {}, y = ""; for (x = E; x !== R; x = x.parent)x.cN && (y = d(x.cN, "", !0) + y); var k = "", M = 0; try { for (var C, A, S = 0; ;) { if (E.t.lastIndex = S, C = E.t.exec(t), !C) break; A = N(t.substring(S, C.index), C[0]), S = C.index + A } for (N(t.substr(S)), x = E; x.parent; x = x.parent)x.cN && (y += I); return { r: M, value: y, language: e, top: E } } catch (O) { if (O.message && -1 !== O.message.indexOf("Illegal")) return { r: 0, value: n(t) }; throw O } } function g(e, t) { t = t || j.languages || B(L); var r = { r: 0, value: n(e) }, a = r; return t.filter(w).filter(x).forEach(function (n) { var t = f(n, e, !1); t.language = n, t.r > a.r && (a = t), t.r > r.r && (a = r, r = t) }), a.language && (r.second_best = a), r } function p(e) { return j.tabReplace || j.useBR ? e.replace(C, function (e, n) { return j.useBR && "\n" === e ? "<br>" : j.tabReplace ? n.replace(/\t/g, j.tabReplace) : "" }) : e } function d(e, n, t) { var r = n ? y[n] : t, a = [e.trim()]; return e.match(/\bhljs\b/) || a.push("hljs"), -1 === e.indexOf(r) && a.push(r), a.join(" ").trim() } function h(e) { var n, t, r, o, s, l = i(e); a(l) || (j.useBR ? (n = document.createElementNS("http://www.w3.org/1999/xhtml", "div"), n.innerHTML = e.innerHTML.replace(/\n/g, "").replace(/<br[ \/]*>/g, "\n")) : n = e, s = n.textContent, r = l ? f(l, s, !0) : g(s), t = c(n), t.length && (o = document.createElementNS("http://www.w3.org/1999/xhtml", "div"), o.innerHTML = r.value, r.value = u(t, c(o), s)), r.value = p(r.value), e.innerHTML = r.value, e.className = d(e.className, l, r.language), e.result = { language: r.language, re: r.r }, r.second_best && (e.second_best = { language: r.second_best.language, re: r.second_best.r })) } function b(e) { j = o(j, e) } function v() { if (!v.called) { v.called = !0; var e = document.querySelectorAll("pre code"); E.forEach.call(e, h) } } function m() { addEventListener("DOMContentLoaded", v, !1), addEventListener("load", v, !1) } function N(n, t) { var r = L[n] = t(e); r.aliases && r.aliases.forEach(function (e) { y[e] = n }) } function R() { return B(L) } function w(e) { return e = (e || "").toLowerCase(), L[e] || L[y[e]] } function x(e) { var n = w(e); return n && !n.disableAutodetect } var E = [], B = Object.keys, L = {}, y = {}, k = /^(no-?highlight|plain|text)$/i, M = /\blang(?:uage)?-([\w-]+)\b/i, C = /((^(<[^>]+>|\t|)+|(?:\n)))/gm, I = "</span>", j = { classPrefix: "hljs-", tabReplace: null, useBR: !1, languages: void 0 }; return e.highlight = f, e.highlightAuto = g, e.fixMarkup = p, e.highlightBlock = h, e.configure = b, e.initHighlighting = v, e.initHighlightingOnLoad = m, e.registerLanguage = N, e.listLanguages = R, e.getLanguage = w, e.autoDetection = x, e.inherit = o, e.IR = "[a-zA-Z]\\w*", e.UIR = "[a-zA-Z_]\\w*", e.NR = "\\b\\d+(\\.\\d+)?", e.CNR = "(-?)(\\b0[xX][a-fA-F0-9]+|(\\b\\d+(\\.\\d*)?|\\.\\d+)([eE][-+]?\\d+)?)", e.BNR = "\\b(0b[01]+)", e.RSR = "!|!=|!==|%|%=|&|&&|&=|\\*|\\*=|\\+|\\+=|,|-|-=|/=|/|:|;|<<|<<=|<=|<|===|==|=|>>>=|>>=|>=|>>>|>>|>|\\?|\\[|\\{|\\(|\\^|\\^=|\\||\\|=|\\|\\||~", e.BE = { b: "\\\\[\\s\\S]", r: 0 }, e.ASM = { cN: "string", b: "'", e: "'", i: "\\n", c: [e.BE] }, e.QSM = { cN: "string", b: '"', e: '"', i: "\\n", c: [e.BE] }, e.PWM = { b: /\b(a|an|the|are|I'm|isn't|don't|doesn't|won't|but|just|should|pretty|simply|enough|gonna|going|wtf|so|such|will|you|your|they|like|more)\b/ }, e.C = function (n, t, r) { var a = e.inherit({ cN: "comment", b: n, e: t, c: [] }, r || {}); return a.c.push(e.PWM), a.c.push({ cN: "doctag", b: "(?:TODO|FIXME|NOTE|BUG|XXX):", r: 0 }), a }, e.CLCM = e.C("//", "$"), e.CBCM = e.C("/\\*", "\\*/"), e.HCM = e.C("#", "$"), e.NM = { cN: "number", b: e.NR, r: 0 }, e.CNM = { cN: "number", b: e.CNR, r: 0 }, e.BNM = { cN: "number", b: e.BNR, r: 0 }, e.CSSNM = { cN: "number", b: e.NR + "(%|em|ex|ch|rem|vw|vh|vmin|vmax|cm|mm|in|pt|pc|px|deg|grad|rad|turn|s|ms|Hz|kHz|dpi|dpcm|dppx)?", r: 0 }, e.RM = { cN: "regexp", b: /\//, e: /\/[gimuy]*/, i: /\n/, c: [e.BE, { b: /\[/, e: /\]/, r: 0, c: [e.BE] }] }, e.TM = { cN: "title", b: e.IR, r: 0 }, e.UTM = { cN: "title", b: e.UIR, r: 0 }, e.METHOD_GUARD = { b: "\\.\\s*" + e.UIR, r: 0 }, e }); hljs.registerLanguage("sql", function (e) { var t = e.C("--", "$"); return { cI: !0, i: /[<>{}*]/, c: [{ bK: "begin end start commit rollback savepoint lock alter create drop rename call delete do handler insert load replace select truncate update set show pragma grant merge describe use explain help declare prepare execute deallocate release unlock purge reset change stop analyze cache flush optimize repair kill install uninstall checksum restore check backup revoke comment with", e: /;/, eW: !0, l: /[\w\.]+/, k: { keyword: "as abort abs absolute acc acce accep accept access accessed accessible account acos action activate add addtime admin administer advanced advise aes_decrypt aes_encrypt after agent aggregate ali alia alias allocate allow alter always analyze ancillary and any anydata anydataset anyschema anytype apply archive archived archivelog are as asc ascii asin assembly assertion associate asynchronous at atan atn2 attr attri attrib attribu attribut attribute attributes audit authenticated authentication authid authors auto autoallocate autodblink autoextend automatic availability avg backup badfile basicfile before begin beginning benchmark between bfile bfile_base big bigfile bin binary_double binary_float binlog bit_and bit_count bit_length bit_or bit_xor bitmap blob_base block blocksize body both bound buffer_cache buffer_pool build bulk by byte byteordermark bytes cache caching call calling cancel capacity cascade cascaded case cast catalog category ceil ceiling chain change changed char_base char_length character_length characters characterset charindex charset charsetform charsetid check checksum checksum_agg child choose chr chunk class cleanup clear client clob clob_base clone close cluster_id cluster_probability cluster_set clustering coalesce coercibility col collate collation collect colu colum column column_value columns columns_updated comment commit compact compatibility compiled complete composite_limit compound compress compute concat concat_ws concurrent confirm conn connec connect connect_by_iscycle connect_by_isleaf connect_by_root connect_time connection consider consistent constant constraint constraints constructor container content contents context contributors controlfile conv convert convert_tz corr corr_k corr_s corresponding corruption cos cost count count_big counted covar_pop covar_samp cpu_per_call cpu_per_session crc32 create creation critical cross cube cume_dist curdate current current_date current_time current_timestamp current_user cursor curtime customdatum cycle data database databases datafile datafiles datalength date_add date_cache date_format date_sub dateadd datediff datefromparts datename datepart datetime2fromparts day day_to_second dayname dayofmonth dayofweek dayofyear days db_role_change dbtimezone ddl deallocate declare decode decompose decrement decrypt deduplicate def defa defau defaul default defaults deferred defi defin define degrees delayed delegate delete delete_all delimited demand dense_rank depth dequeue des_decrypt des_encrypt des_key_file desc descr descri describ describe descriptor deterministic diagnostics difference dimension direct_load directory disable disable_all disallow disassociate discardfile disconnect diskgroup distinct distinctrow distribute distributed div do document domain dotnet double downgrade drop dumpfile duplicate duration each edition editionable editions element ellipsis else elsif elt empty enable enable_all enclosed encode encoding encrypt end end-exec endian enforced engine engines enqueue enterprise entityescaping eomonth error errors escaped evalname evaluate event eventdata events except exception exceptions exchange exclude excluding execu execut execute exempt exists exit exp expire explain export export_set extended extent external external_1 external_2 externally extract failed failed_login_attempts failover failure far fast feature_set feature_value fetch field fields file file_name_convert filesystem_like_logging final finish first first_value fixed flash_cache flashback floor flush following follows for forall force foreign form forma format found found_rows freelist freelists freepools fresh from from_base64 from_days ftp full function general generated get get_format get_lock getdate getutcdate global global_name globally go goto grant grants greatest group group_concat group_id grouping grouping_id groups gtid_subtract guarantee guard handler hash hashkeys having hea head headi headin heading heap help hex hierarchy high high_priority hosts hour http id ident_current ident_incr ident_seed identified identity idle_time if ifnull ignore iif ilike ilm immediate import in include including increment index indexes indexing indextype indicator indices inet6_aton inet6_ntoa inet_aton inet_ntoa infile initial initialized initially initrans inmemory inner innodb input insert install instance instantiable instr interface interleaved intersect into invalidate invisible is is_free_lock is_ipv4 is_ipv4_compat is_not is_not_null is_used_lock isdate isnull isolation iterate java join json json_exists keep keep_duplicates key keys kill language large last last_day last_insert_id last_value lax lcase lead leading least leaves left len lenght length less level levels library like like2 like4 likec limit lines link list listagg little ln load load_file lob lobs local localtime localtimestamp locate locator lock locked log log10 log2 logfile logfiles logging logical logical_reads_per_call logoff logon logs long loop low low_priority lower lpad lrtrim ltrim main make_set makedate maketime managed management manual map mapping mask master master_pos_wait match matched materialized max maxextents maximize maxinstances maxlen maxlogfiles maxloghistory maxlogmembers maxsize maxtrans md5 measures median medium member memcompress memory merge microsecond mid migration min minextents minimum mining minus minute minvalue missing mod mode model modification modify module monitoring month months mount move movement multiset mutex name name_const names nan national native natural nav nchar nclob nested never new newline next nextval no no_write_to_binlog noarchivelog noaudit nobadfile nocheck nocompress nocopy nocycle nodelay nodiscardfile noentityescaping noguarantee nokeep nologfile nomapping nomaxvalue nominimize nominvalue nomonitoring none noneditionable nonschema noorder nopr nopro noprom nopromp noprompt norely noresetlogs noreverse normal norowdependencies noschemacheck noswitch not nothing notice notnull notrim novalidate now nowait nth_value nullif nulls num numb numbe nvarchar nvarchar2 object ocicoll ocidate ocidatetime ociduration ociinterval ociloblocator ocinumber ociref ocirefcursor ocirowid ocistring ocitype oct octet_length of off offline offset oid oidindex old on online only opaque open operations operator optimal optimize option optionally or oracle oracle_date oradata ord ordaudio orddicom orddoc order ordimage ordinality ordvideo organization orlany orlvary out outer outfile outline output over overflow overriding package pad parallel parallel_enable parameters parent parse partial partition partitions pascal passing password password_grace_time password_lock_time password_reuse_max password_reuse_time password_verify_function patch path patindex pctincrease pctthreshold pctused pctversion percent percent_rank percentile_cont percentile_disc performance period period_add period_diff permanent physical pi pipe pipelined pivot pluggable plugin policy position post_transaction pow power pragma prebuilt precedes preceding precision prediction prediction_cost prediction_details prediction_probability prediction_set prepare present preserve prior priority private private_sga privileges procedural procedure procedure_analyze processlist profiles project prompt protection public publishingservername purge quarter query quick quiesce quota quotename radians raise rand range rank raw read reads readsize rebuild record records recover recovery recursive recycle redo reduced ref reference referenced references referencing refresh regexp_like register regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_sxy reject rekey relational relative relaylog release release_lock relies_on relocate rely rem remainder rename repair repeat replace replicate replication required reset resetlogs resize resource respect restore restricted result result_cache resumable resume retention return returning returns reuse reverse revoke right rlike role roles rollback rolling rollup round row row_count rowdependencies rowid rownum rows rtrim rules safe salt sample save savepoint sb1 sb2 sb4 scan schema schemacheck scn scope scroll sdo_georaster sdo_topo_geometry search sec_to_time second section securefile security seed segment select self sequence sequential serializable server servererror session session_user sessions_per_user set sets settings sha sha1 sha2 share shared shared_pool short show shrink shutdown si_averagecolor si_colorhistogram si_featurelist si_positionalcolor si_stillimage si_texture siblings sid sign sin size size_t sizes skip slave sleep smalldatetimefromparts smallfile snapshot some soname sort soundex source space sparse spfile split sql sql_big_result sql_buffer_result sql_cache sql_calc_found_rows sql_small_result sql_variant_property sqlcode sqldata sqlerror sqlname sqlstate sqrt square standalone standby start starting startup statement static statistics stats_binomial_test stats_crosstab stats_ks_test stats_mode stats_mw_test stats_one_way_anova stats_t_test_ stats_t_test_indep stats_t_test_one stats_t_test_paired stats_wsr_test status std stddev stddev_pop stddev_samp stdev stop storage store stored str str_to_date straight_join strcmp strict string struct stuff style subdate subpartition subpartitions substitutable substr substring subtime subtring_index subtype success sum suspend switch switchoffset switchover sync synchronous synonym sys sys_xmlagg sysasm sysaux sysdate sysdatetimeoffset sysdba sysoper system system_user sysutcdatetime table tables tablespace tan tdo template temporary terminated tertiary_weights test than then thread through tier ties time time_format time_zone timediff timefromparts timeout timestamp timestampadd timestampdiff timezone_abbr timezone_minute timezone_region to to_base64 to_date to_days to_seconds todatetimeoffset trace tracking transaction transactional translate translation treat trigger trigger_nestlevel triggers trim truncate try_cast try_convert try_parse type ub1 ub2 ub4 ucase unarchived unbounded uncompress under undo unhex unicode uniform uninstall union unique unix_timestamp unknown unlimited unlock unnest unpivot unrecoverable unsafe unsigned until untrusted unusable unused update updated upgrade upped upper upsert url urowid usable usage use use_stored_outlines user user_data user_resources users using utc_date utc_timestamp uuid uuid_short validate validate_password_strength validation valist value values var var_samp varcharc vari varia variab variabl variable variables variance varp varraw varrawc varray verify version versions view virtual visible void wait wallet warning warnings week weekday weekofyear wellformed when whene whenev wheneve whenever where while whitespace with within without work wrapped xdb xml xmlagg xmlattributes xmlcast xmlcolattval xmlelement xmlexists xmlforest xmlindex xmlnamespaces xmlpi xmlquery xmlroot xmlschema xmlserialize xmltable xmltype xor year year_to_month years yearweek", literal: "true false null unknown", built_in: "array bigint binary bit blob bool boolean char character date dec decimal float int int8 integer interval number numeric real record serial serial8 smallint text time timestamp varchar varying void" }, c: [{ cN: "string", b: "'", e: "'", c: [e.BE, { b: "''" }] }, { cN: "string", b: '"', e: '"', c: [e.BE, { b: '""' }] }, { cN: "string", b: "`", e: "`", c: [e.BE] }, e.CNM, e.CBCM, t, e.HCM] }, e.CBCM, t, e.HCM] } });
