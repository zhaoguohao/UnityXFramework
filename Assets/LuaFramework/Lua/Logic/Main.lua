-- 入口脚本

Main = Main or {}
local this = Main

-- 测试lua配置加载
local TestLuaCfg = require "Config/TestLuaCfg"

function Main.Init()
    log("Lua Main.Init")

    -- 输出配置表
    LuaUtil.PrintTable(TestLuaCfg)
end

function Main.Start()
    log("Lua Main.Start")

    -- 显示登录界面
    LoginPanel.Show()
end

function Main.Send()
    Network.SendData("sayhello", { what = "hi, i am unity from lua" }, function(data)
            log("on response: " .. data.error_code .. " " .. data.msg)
        end
    )
end

-- endregion
