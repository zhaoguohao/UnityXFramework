--region *.lua
--Date
--此文件由[BabeLua]插件自动生成
--lua 的一些功能测试方法
--验证一些方法的正确性
--为某些逻辑模块提供测试数据

local json = require 'cjson'

LuaTest = LuaTest or {}

local this = LuaTest 

function LuaTest.OnBeginFun(fun_name)
    log("---------------------begin "..fun_name.."-----------------------")
end

function LuaTest.OnEndFun(fun_name)
    log("---------------------end  "..fun_name.."-----------------------")
end

function LuaTest.Assert(result)
    if result == 0 then
        logError("Assert failed..")
    else
        
    end
end

function LuaTest.Print(...)
    if ... ~= nil then
        local args = {...}
        local output_str = ""
        for k, v in pairs(args) do
            output_str = output_str.."  "..v
        end
        log(output_str)
    end
end

--基础方法 不依赖其他
function LuaTest.TestBaseFun()
    this.LuaUtil_GetTimeHMS()
    this.LuaUtil_GetTodaySec()
end

------------------------------------------LuaUtil--------------------------------------
--测试 LuaUtil的GetTimeHMS方法
function LuaTest.LuaUtil_GetTimeHMS()
    this.OnBeginFun("LuaUtil_GetTimeHMS")

    local sec = 0
    local h, m, s = LuaUtil.GetTimeHMS(sec)
    
    this.Assert(h == 0)
    this.Assert(m == 0)
    this.Assert(s == 0)

    sec = 3600
    local h, m, s = LuaUtil.GetTimeHMS(sec)
    this.Assert(h == 1)
    this.Assert(m == 0)
    this.Assert(s == 0)

    sec = 3661
    local h, m, s = LuaUtil.GetTimeHMS(sec)
    this.Assert(h == 1)
    this.Assert(m == 1)
    this.Assert(s == 1)


    this.OnEndFun("LuaUtil_GetTimeHMS")
end

function LuaTest.LuaUtil_GetTodaySec()
    this.OnBeginFun("LuaUtil_GetTodaySec")

    local sec = LuaUtil.GetTodaySec()
    this.Print(sec)
    local h, m, s = LuaUtil.GetTimeHMS(sec)

    this.Print(h, m, s)

    this.OnEndFun("LuaUtil_GetTodaySec")
end


------------------------------------------PlayerManager--------------------------------------
function LuaTest.PlayerManager_OnPlayerConsume()
    this.OnBeginFun("PlayerManager_OnPlayerConsume")
    --local pre_item_60005 = BackPackMgr.getServerNumberById(60005)
    --6005 青铜核弹头
    local joson_data = '[{"props":[{"id":60005,"quantity":1}]},{"gold":200000}]';
    local cost_tab = json.decode(joson_data)
    LuaUtil.PrintTable(cost_tab)
    PlayerManager.OnPlayerConsume(cost_tab)

    this.OnEndFun("PlayerManager_OnPlayerConsume")
end

function LuaTest.PlayerManager_OnPlayerConsumeTab()
    this.OnBeginFun("PlayerManager_OnPlayerConsumeTab")

    local joson_data = '{"gold":1000, "props":[{"id":60005,"quantity":1}]}'
    local cost_tab = json.decode(joson_data)
    PlayerManager.OnPlayerConsumeByTab(cost_tab)


    this.OnEndFun("PlayerManager_OnPlayerConsumeTab")
end

--endregion
