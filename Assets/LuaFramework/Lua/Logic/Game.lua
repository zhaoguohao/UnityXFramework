-- region *.lua
-- Date
-- 此文件由[BabeLua]插件自动生成

Game = Game or {}
local this = Game

function Game.Init()
    log("Lua Game.Init")
    
end

function Game.Start()
    log("Lua Game.Start")

    -- 显示登录界面
    LoginPanel.Show()
end

function Game.Send()
    Network.SendData(
        "sayhello",
        {what = "hi, i am unity"},
        function(data)
            log("lua on response: " .. data.error_code .. " " .. data.msg)
        end
    )
    
end

-- endregion
