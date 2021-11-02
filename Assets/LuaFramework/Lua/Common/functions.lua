local Util = LuaFramework.Util
local is_editor = UnityEngine.Application.isEditor

-- 输出日志--
function log(str)
    if is_editor then
        local luaStackStr = debug.traceback(str, 0)
        Util.Log(luaStackStr)
    else
        Util.Log(str)
    end

end

function logGreen(str)
    if is_editor then
        str = str.."</color>"
        local luaStackStr = debug.traceback(str, 0)
        Util.LogGreen(luaStackStr)
    else
        Util.LogGreen(str)
    end
end

function LogYellow(str)
    if is_editor then
         str = str.."</color>"
        local luaStackStr = debug.traceback(str, 0)
        Util.LogYellow(luaStackStr)
    else
        Util.LogYellow(str)
    end
end

function LogBlue(str)
    if is_editor then
         str = str.."</color>"
        local luaStackStr = debug.traceback(str, 0)
        Util.LogBlue(luaStackStr)
    else
        Util.LogBlue(str)
    end
end

-- 错误日志--
function logError(str)
    local luaStackStr = debug.traceback(str, 0)
    Util.LogError(luaStackStr)

end

-- 警告日志--
function logWarn(str)
    local luaStackStr = debug.traceback(str, 0)
    Util.LogWarning(luaStackStr);
end