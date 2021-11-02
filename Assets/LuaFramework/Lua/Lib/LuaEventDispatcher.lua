--region *.lua
--Date
--此文件由[BabeLua]插件自动生成
--纯lua的事件处理器
--TODO:
--纯lua的事件处理 与 调用Unity里面封装的EventDispatcher效率相差多少? 有待测试 
--如果只需要在lua里面用 纯lua 的效率肯定是高

LuaEventDispatcher = LuaEventDispatcher or {}

--key:事件名称,value:事件处理方法表handles
LuaEventDispatcher.listeners = {}
local listeners = LuaEventDispatcher.listeners



--注册事件处理
function LuaEventDispatcher.RegistEvent(eventName, handle)

    local handles = listeners[eventName]
    if handles == nil then
        handles = {}
        listeners[eventName] = handles
    end

    table.insert(handles, handle)

end

--注销事件处理
function LuaEventDispatcher.UnRegistEvent(eventName, handle)

    local handles = listeners[eventName]
    if handles ~= nil then
        for k, v in pairs(handles) do
            if v == handle then
                handles[k] = nil
                return k
            end
        end
    end

end

--触发事件
function LuaEventDispatcher.FireEvent(eventName, ...)
    local handles = listeners[eventName]
    if handles ~= nil then
        for k, v in pairs(handles) do
            v(...)
        end
    end
end

--endregion

