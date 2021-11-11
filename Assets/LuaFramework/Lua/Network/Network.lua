-- 调用 LuaFramework.NetworkManager.AddLuaProcessS2C(tag, flag) 增加lua 的网络回调处理
-- tag:proto定义的标记 | flag 0:只C#层处理(不需要添加)，2:lua C#共同处理 3:只lua 处理
-- (如果需要改变方法名称可以在初始化的时候调用LuaFramework.NetworkManager.SetOnRequestDataFun / SetOnResponseDataFun)
-- 对于某些不同模式之间公用的处理，可以调用动态添加的方法，在进入Mode的时候AddLuaProcessC2S 退出的时候 RemoveLuaProcessC2S

require "Common/define"
require "Common/protocal"
require "Common/functions"
Event = require "events"

local sproto = require "3rd/sproto/sproto"
local core = require "sproto.core"
local print_r = require "3rd/sproto/print_r"

Network = Network or {}
local this = Network

local c2s_proto
local s2c_proto

local c2s_host
local s2c_host

-- 注：CsNetworkMgr在define中定义了，就是C#的NetworkManager对象
local networkMgr = CsNetworkMgr

-- 处理标记
NetProcessFlag = {
    -- 在C#层处理(不需要添加)
    Default = 0,
    -- c# lua 共同处理
    Common = 1,
    -- 只lua层处理
    Lua = 2
}

local NetProcessFlag = NetProcessFlag

local NetStateEnum = {
    ConnectStart = 0,
    ConnectSuccess = 1,
    ConnectFail = 2,
    Disconnect = 3,
    ResendStart = 4,
    ResendSuccess = 5,
    ResendFail = 6,
    Error = 7
}

local c2sProcess = c2sProcessTab
local s2cProcessTab = s2cProcessTab

-- 动态添加的处理方法
local exts2cProcessTab = {}
local extc2sProcessTab = {}

-- 断线重连成功后需要重新发送的协议
-- (有些请求在断线时候客户的点击UI发送过去，等待服务器返回，然后界面会处于不可点击所以重连成功后需要主动在发一次)
local resend_in_reconnect_proto = {}

this.sessionHandlers = {}

function Network.OnInit()
    log("Network.OnInit!!")

    -- 初始化sproto文件
    local c2s = Util.ReadFileFromPath("proto/c2s.sproto")
    if c2s ~= nil then
        c2s_proto = sproto.parse(c2s)
        c2s_host = c2s_proto:host "package"
    else
        logError("Network.OnInit proto/c2s.sproto is not exist ")
    end

    local s2c = Util.ReadFileFromPath("proto/s2c.sproto")
    if s2c ~= nil then
        s2c_proto = sproto.parse(s2c)
        s2c_host = s2c_proto:host "package"
    else
        logError("Network.OnInit proto/s2c.sproto is not exist ")
    end

    -- 添加网络处理
    -- networkMgr.AddLuaProcessS2C(1, NetProcessFlag.Lua)
    -- networkMgr.AddLuaProcessC2S(1, 2)

    for k, v in pairs(c2sProcessTab) do
        -- s2c只有 request部分
        local p = c2s_proto:findproto(k)
        if p ~= nil then
            networkMgr.AddLuaProcessC2S(p.tag, v[1])
        else
            logError("Network.OnInit c2sProcessTab key " .. k .. " not exist!!!")
        end
    end

    for k, v in pairs(s2cProcessTab) do
        -- s2c只有 request部分
        local p = s2c_proto:findproto(k)
        if p ~= nil then
            networkMgr.AddLuaProcessS2C(p.tag, v[1])
        else
            logError("Network.OnInit s2cProcessTab key " .. k .. " not exist!!!")
        end
    end

    this.resend_proto_tab = {}
    for k, v in pairs(resend_in_reconnect_proto) do
        local c2s_process_info = c2sProcessTab[k]
        if c2s_process_info ~= nil then
            if c2s_process_info[1] == NetProcessFlag.Default then
                logError("Network.OnInit porcess tag is defautl but add to resend_in_reconnect_proto~")
            end
        end
        local p = c2s_proto:findproto(k)
        if p ~= nil then
            this.resend_proto_tab[p.tag] = k
        else
            logError("Network.OnInit resend_in_reconnect_proto key " .. k .. " not exist!!!")
        end
    end

    -- 发送请求的缓存tab 主要是断线重连时候恢复重新发送的逻辑
    this.req_cache_tab = {}
    this.sessionHandlers = {}
end

function Network.Connect(host, ip, connectCb)
    this.connectCb = nil
    this.connectCb = connectCb
    WaitBoard.Create()
    ClientNet:Connect(host, ip, false)
end

-- 网络状态改变
function Network.OnNetStateChanged(state, param)
    log("Network.OnNetStateChanged, state: " .. tostring(state))
    if 1 == state then
        -- ConnectSuccess
        WaitBoard.Destroy()
        if nil ~= this.connectCb then
            this.connectCb(true)
            this.connectCb = nil
        end
    elseif 2 == state then
        -- ConnectFail
        WaitBoard.Destroy()
        if nil ~= this.connectCb then
            this.connectCb(false)
            this.connectCb = nil
        end
    end
end

-- 收到服务器请求处理
function Network.OnRequestDataFun(buffer, length)
    local type, protoname, request = s2c_host:dispatch_nopacked(buffer, length)

    local protoProcessTab = s2cProcessTab[protoname]
    -- LuaUtil.PrintTable(protoProcessTab)
    if protoProcessTab ~= nil then
        if protoProcessTab[2] ~= nil then
            local ret = protoProcessTab[2](protoname, request)
            if ret ~= nil then
                return ret
            end
        end
    else
        -- 动态添加的处理
        local extFun = exts2cProcessTab[protoname]
        if extFun ~= nil then
            local ret = extFun(protoname, request)
            if ret ~= nil then
                return ret
            end
        end
    end
end

-- 收到服务器响应处理
function Network.OnResponseDataFun(buffer, length, protoname)
    if protoname == nil then
        logWarn("Network.OnResponseDataFun protoname is nil ....")
        return true
    end

    local type, session, response = c2s_host:dispatch_nopacked(buffer, length, protoname)

    this.req_cache_tab[session] = nil

    local handler = this.sessionHandlers[session]
    if handler then
        local ret = handler(response)
        this.sessionHandlers[session] = nil
        return
    end

    local protoProcessTab = c2sProcessTab[protoname]
    if protoProcessTab ~= nil then
        if protoProcessTab[2] ~= nil then
            local ret = protoProcessTab[2](session, response)
            if ret ~= nil then
                return ret
            end
        end
    else
        -- 动态添加的处理
        local extFun = extc2sProcessTab[protoname]
        if extFun ~= nil then
            local ret = extFun(session, response)
            if ret ~= nil then
                return ret
            end
        end
    end
end

-- lua发送消息接口， protoname:协议名称, prototab:协议内容 lua table
function Network.SendData(protoname, prototab, handler)
    local p = c2s_proto:findproto(protoname)
    if p ~= nil then
        if prototab == nil then
            prototab = {}
        end
        local session = networkMgr.GetNextSession()
        local v = c2s_host:gen_request(p.request, p.tag, session, prototab)
        if v ~= nil then
            networkMgr.SendData(protoname, session, p.tag, v)
            -- 加入缓存
            if this.resend_proto_tab[p.tag] ~= nil then
                this.req_cache_tab[session] = {[1] = protoname, [2] = session, [3] = p.tag, [4] = v, [5] = prototab}
            end
            if handler then
                this.sessionHandlers[session] = handler
            end
        else
            logError("Network.SendData v is nil tag = " .. p.tag .. " protoname = " .. protoname)
        end
    else
        logError("Network.SendData p is nil tag = " .. p.tag .. " protoname = " .. protoname)
    end
end

-- 在断线重连的时候重发数据
function Network.ResendCacheDataOnReconnect()
    local temp_tab = this.req_cache_tab
    -- this.req_cache_tab = {}
    local cnt = 0
    for k, v in pairs(temp_tab) do
        -- 重发
        if v ~= nil then
            log("ResendCacheDataOnReconnect " .. v[1])
            -- TODO:此处用了同一个session所以需要在C#里面重写一个 ReSend方法，否则会重复添加sessioin爆错， 暂时不能改...
            networkMgr.SendData(v[1], v[2], v[3], v[4])
        else
            logError("Network.ResendCacheDataOnReconnect v is nil")
            temp_tab[k] = nil
        end
        cnt = cnt + 1
    end
    if cnt > 0 then
        LuaUtil.FireEvent(EventNameDef.LUA_NETWORK_RESEND_SUCCESSFULLY)
    end
end



-- 仅部分公用的网络消息需要动态的添加 和 删除(某些不同的模式公用)
function Network.AddLuaProcessC2S(protoname, flag, processFun)
    local p = c2s_proto:findproto(protoname)
    if p ~= nil then
        networkMgr.AddLuaProcessC2S(p.tag, flag)
        extc2sProcessTab[protoname] = processFun
    else
        logError("Network.AddLuaProcessC2S key " .. protoname .. " not exist!!!")
    end
end

function Network.AddLuaProcessS2C(protoname, flag, processFun)
    -- s2c只有 request部分
    local p = s2c_proto:findproto(protoname)
    if p ~= nil then
        networkMgr.AddLuaProcessS2C(p.tag, flag)
        exts2cProcessTab[protoname] = processFun
    else
        logError("Network.AddLuaProcessS2C key " .. protoname .. " not exist!!!")
    end
end

function Network.RemoveLuaProcessC2S(protoname)
    local p = c2s_proto:findproto(protoname)
    if p ~= nil then
        networkMgr.RemoveLuaProcessC2S(p.tag)
        extc2sProcessTab[protoname] = nil
    else
        logError("Network.RemoveLuaProcessC2S key " .. protoname .. " not exist!!!")
    end
end

function Network.RemoveLuaProcessS2C(protoname)
    -- s2c只有 request部分
    local p = s2c_proto:findproto(protoname)
    if p ~= nil then
        networkMgr.RemoveLuaProcessS2C(p.tag)
        exts2cProcessTab[protoname] = nil
    else
        logError("Network.RemoveLuaProcessS2C key " .. protoname .. " not exist!!!")
    end
end

function Network.RemoveAllExtProcess()
    for k, v in pairs(extc2sProcessTab) do
        local p = c2s_proto:findproto(k)
        if p ~= nil then
            networkMgr.RemoveLuaProcessC2S(p.tag)
        end
    end

    for k, v in pairs(exts2cProcessTab) do
        local p = s2c_proto:findproto(k)
        if p ~= nil then
            networkMgr.RemoveLuaProcessS2C(p.tag)
        end
    end
    extc2sProcessTab = {}
    exts2cProcessTab = {}
end

function Network.SetNetProcess(bProcessMsg, mode)
    LuaFramework.NetworkManager.SetNetProcess(bProcessMsg, mode)
end

-- 卸载网络监听--
function Network.Unload()
    logWarn("Unload Network...")
end
