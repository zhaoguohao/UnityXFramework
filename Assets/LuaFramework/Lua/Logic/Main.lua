-- region *.lua
-- Date
-- 此文件由[BabeLua]插件自动生成

Main = Main or {}
local this = Main

function Main.Init()
    log("Lua Main.Init")
    
end

function Main.Start()
    log("Lua Main.Start")

    -- 显示登录界面
    LoginPanel.Show()
end

function Main.Send()
    Network.SendData(
        "sayhello",
        {what = "hi, i am unity"},
        function(data)
            log("lua on response: " .. data.error_code .. " " .. data.msg)
        end
    )
end

-- endregion
