-- region *.lua
-- Date
-- 此文件由[BabeLua]插件自动生成

-- 由于tolua重写了__index方法， 所以不能直接扩展 LuaFramework.Util 只能用名称LuaUtil

LuaUtil = LuaUtil or {}

local ONE_HOUR_SEC = 3600
-- 一天的秒数
local ONE_DAY_SEC = 86400
-- 一周的秒数
local ONE_WEEK_SEC = 604800
-- 两周的秒数
local TWO_WEEKS_SEC = 1209600

LuaUtil.ONE_DAY_SEC = ONE_DAY_SEC
LuaUtil.ONE_WEEK_SEC = ONE_WEEK_SEC
LuaUtil.TWO_WEEKS_SEC = TWO_WEEKS_SEC

local EventDispatcher = EventDispatcher.instance

function LuaUtil.RegistEvent(eventName, handle)
    EventDispatcher:Regist(eventName, handle)
end

function LuaUtil.UnRegistEvent(eventName, handle)
    EventDispatcher:UnRegist(eventName, handle)
end

function LuaUtil.FireEvent(eventName, ...)
    EventDispatcher:DispatchEvent(eventName, ...)
end

-- 获取table长度
function LuaUtil.TableCount(t)
    if nil == t then
        return 0
    end

    local cnt = 0
    for _ in pairs(t) do
        cnt = cnt + 1
    end
    return cnt
end

local function PrintSpace(p, count)
    for ii = 1, count do
        p("    ")
    end
end

local function PrintTable2(o, f, b, deep)
    if type(f) ~= "function" and f ~= nil then
        logError("expected second argument %s is a function", tostring(f))
    end
    if type(b) ~= "boolean" and b ~= nil then
        logError("expected third argument %s is a boolean", tostring(b))
    end
    p = f or io.write
    b = b or false
    if type(o) == "number" or type(o) == "function" or type(o) == "boolean" or type(o) == "nil" then
        p(tostring(o))
    elseif type(o) == "string" then
        p(string.format("%q", o))
    elseif type(o) == "table" then
        p("\n")
        PrintSpace(p, deep)
        p("{\n")
        for k, v in pairs(o) do
            PrintSpace(p, deep)
            if b then
                p("[")
            end
            PrintTable2(k, p, b, deep + 1)
            if b then
                p("]")
            end
            p(" = ")
            PrintTable2(v, p, b, deep + 1)
            p(",\n")
        end
        PrintSpace(p, deep)
        p("}")
    end
end

-- 打印一个lua表 用于调试输出
function LuaUtil.PrintTable(t)
    local printTabStr = ""
    PrintTable2(
        t,
        function(str)
            printTabStr = printTabStr .. str
        end,
        true,
        0
    )
    if printTabStr == "" then
        logError("LuaUtil.PrintTable printTabStr is nil")
    end
    log(printTabStr)
end

function LuaUtil.strToDate(s, sTime)
    local t = {}
    if sTime then
        t.year, t.month, t.day = s:match("(%d+)-(%d+)-(%d+)")
        t.hour, t.min, t.sec = sTime:match("(%d+):(%d+):(%d+)")
    else
        t.year, t.month, t.day, t.hour, t.min, t.sec = s:match("(%d+)-(%d+)-(%d+)%s+(%d+):(%d+):(%d+)")
    end
    for k, v in pairs(t) do
        t[k] = tonumber(v)
    end
    return t
end

function LuaUtil.strToTime(s, sTime)
    return os.time(LuaUtil.strToDate(s, sTime))
end

-- 获取今日指定时间的时间戳
-- clock格式, 例子：5:10:05
-- nowTime：该时间戳用于指定某一天
function LuaUtil.getTodayTimestamp(clock, nowTime)
    local nowTab = os.date("*t", nowTime)
    local _, _, hour, min, sec = string.find(clock, "(%d+):(%d+):(%d+)")
    local timestamp =
        os.time {
        year = nowTab.year,
        month = nowTab.month,
        day = nowTab.day,
        hour = hour,
        min = min,
        sec = sec
    }

    return timestamp
end

-- 通过距零点的秒数获取时间戳
function LuaUtil.GetTodayTimestampBySec(sec)
    local H, M, S = LuaUtil.GetTimeHMS(sec)
    local nowTab = os.date("*t", os.time())
    local timestamp = os.time {year = nowTab.year, month = nowTab.month, day = nowTab.day, hour = H, min = M, sec = S}

    return timestamp
end

-- 将秒数转换为 小时， 分， 秒(只限于一天)
function LuaUtil.GetTimeHMS(sec)
    local H = math.floor(sec / ONE_HOUR_SEC)
    local L = sec - (H * ONE_HOUR_SEC)
    local M = math.floor(L / 60)
    local S = L - (M * 60)

    return H, M, S
end

-- 将秒数转换为 小时， 分， 秒(只限于一天)
function LuaUtil.GetFormatTime(sec)
    local H, M, S = LuaUtil.GetTimeHMS(sec)
    local str = ""
    if H ~= nil and H > 0 then
        str = str .. tostring(H) .. Util.GetSperateResStr(14639, 1)
    end
    if M ~= nil and M > 0 then
        str = str .. " " .. tostring(M) .. Util.GetSperateResStr(14639, 2)
    end
    if S ~= nil and S > 0 then
        str = str .. " " .. tostring(S) .. Util.GetSperateResStr(14639, 3)
    end
    return str
end

-- 获取今天的秒数(相对于0点)
function LuaUtil.GetTodaySec()
    local cur_time = os.time()
    local nowTab = os.date("*t", os.time())
    local zero_time = os.time {year = nowTab.year, month = nowTab.month, day = nowTab.day, hour = 0, min = 0, sec = 0}
    return cur_time - zero_time
end

function LuaUtil.GetDefaultTimeStr(sec)
    local H, M, S = LuaUtil.GetTimeHMS(sec)
    return string.format("%02d:%02d:%02d", H, M, S)
end

--[[ 秒数转年月日时分 ]]
function LuaUtil.Second2DataTime(second)
    return os.date("%Y-%m-%d %H:%M", math.floor(second))
end
--[[ 秒数转月日时分 ]]
function LuaUtil.Second2MonthDateTime(second)
    return os.date("%m-%d %H:%M", math.floor(second))
end
--[[ 秒数转年月日 ]]
function LuaUtil.Second2Date(second)
    return os.date("%Y-%m-%d", math.floor(second))
end
--[[ 秒数转时间的table ]]
function LuaUtil.Second2DateTable(second)
    return os.date("*t", math.floor(second))
end

-- utils for string
function LuaUtil.SplitString(fullString, separator)
    local startIndex = 1
    local splitIndex = 1
    local splitArray = {}
    if not fullString or not separator or string.len(fullString) == 0 then
        return splitArray
    end
    while true do
        local lastIndex = string.find(fullString, separator, startIndex)
        if not lastIndex then
            splitArray[splitIndex] = string.sub(fullString, startIndex, string.len(fullString))
            break
        end
        splitArray[splitIndex] = string.sub(fullString, startIndex, lastIndex - 1)
        startIndex = lastIndex + string.len(separator)
        splitIndex = splitIndex + 1
    end
    return splitArray
end

function LuaUtil.StringToNumberList(fromString, separator)
    local retList = {}
    if fromString ~= nil and string.len(fromString) > 0 then
        local tempTable = LuaUtil.SplitString(fromString, separator)
        for k, v in pairs(tempTable) do
            if v ~= nil and string.len(v) then
                table.insert(retList, tonumber(v))
            end
        end
    end
    return retList
end
-- end of utils for string

function LuaUtil.ToNumber(strContent)
    local ret = 0
    if strContent then
        ret = tonumber(strContent)
    end
    return ret
end

function LuaUtil.IsStrNullOrEmpty(str)
    if str == nil or str == "" then
        return true
    end
    return false
end

function LuaUtil.GetTableValue(prototab, key, default)
    if nil ~= prototab[key] then
        return prototab[key]
    else
        return default
    end
end

-- utf8字符串截取
function LuaUtil.sub_chars(s, len)
    local ss = {}
    if len > #s then
        return s
    end
    local k = 1
    while true do
        len = len - 1
        if len < 0 then
            break
        end
        local c = string.byte(s, k)
        if not c then
            break
        end
        if c < 192 then
            table.insert(ss, string.char(c))
            k = k + 1
        elseif c < 224 then
            local c1 = string.byte(s, k + 1)
            if c1 then
                table.insert(ss, string.char(c, c1))
            end
            k = k + 2
        elseif c < 240 then
            local c1 = string.byte(s, k + 1)
            local c2 = string.byte(s, k + 2)
            if c1 and c2 then
                table.insert(ss, string.char(c, c1, c2))
            end
            k = k + 3
        elseif c < 248 then
            local c1 = string.byte(s, k + 1)
            local c2 = string.byte(s, k + 2)
            local c3 = string.byte(s, k + 3)
            if c1 and c2 and c3 then
                table.insert(ss, string.char(c, c1, c2, c3))
            end
            k = k + 4
        elseif c < 252 then
            local c1 = string.byte(s, k + 1)
            local c2 = string.byte(s, k + 2)
            local c3 = string.byte(s, k + 3)
            local c4 = string.byte(s, k + 4)
            if c1 and c2 and c3 and c4 then
                table.insert(ss, string.char(c, c1, c2, c3, c4))
            end
            k = k + 5
        else
            local c1 = string.byte(s, k + 1)
            local c2 = string.byte(s, k + 2)
            local c3 = string.byte(s, k + 3)
            local c4 = string.byte(s, k + 4)
            local c5 = string.byte(s, k + 5)
            if c1 and c2 and c3 and c4 and c5 then
                table.insert(ss, string.char(c, c1, c2, c3, c4, c5))
            end
            k = k + 6
        end
    end
    return table.concat(ss)
end

--[[ 分割字符串 ]]
function LuaUtil.StringSplit(origin_str, separator)
    -- origin_str = "2"
    -- origin_str = "2,3"
    -- origin_str = "2,3,4,5,6,7,8,9"
    local mFindStartIndex = 1
    local mSplitIndex = 1
    local mSplitArray = {}
    while true do
        local mFindLastIndex = string.find(origin_str, separator, mFindStartIndex)
        if mFindLastIndex == nil then
            mSplitArray[mSplitIndex] = string.sub(origin_str, mFindStartIndex, string.len(origin_str))
            break
        end

        mSplitArray[mSplitIndex] = string.sub(origin_str, mFindStartIndex, mFindLastIndex - 1)
        mFindStartIndex = mFindLastIndex + string.len(separator)
        mSplitIndex = mSplitIndex + 1
    end
    -- log("LuaUtil.StringSplit origin_str " .. origin_str)
    -- LuaUtil.PrintTable(mSplitArray)
    return mSplitArray
end

-- 将秒转为日、时、分、秒
function LuaUtil.GetTimeDHMS(sec)
    local D = math.floor(sec / ONE_DAY_SEC)
    local leftHour = sec - D * ONE_DAY_SEC
    local H = math.floor(leftHour / ONE_HOUR_SEC)
    local L = leftHour - (H * ONE_HOUR_SEC)
    local M = math.floor(L / 60)
    local S = L - (M * 60)
    return D, H, M, S
end

-- 克隆物体
function LuaUtil.CloneObj(obj)
    local go = GameObject.Instantiate(obj)    
    LuaUtil.SafeActiveObj(go, true)
    go.transform:SetParent(obj.transform.parent, false)
    return go
end

-- 判空
function LuaUtil.IsNilOrNull(obj)
    return nil == obj or null == obj
end

-- 安全销毁
function LuaUtil.SafeDestroyObj(obj)
    if LuaUtil.IsNilOrNull(obj) then
        return
    end
    GameObject.Destroy(obj.gameObject)
end

-- 安全激活或禁用
function LuaUtil.SafeActiveObj(obj, active)
    if LuaUtil.IsNilOrNull(obj) then
        return
    end
    obj.gameObject:SetActive(active)
end
-- endregion
