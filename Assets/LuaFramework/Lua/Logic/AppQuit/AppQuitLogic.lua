-- App退出逻辑，弹出挽留提示框

AppQuitLogic = AppQuitLogic or {}
local this = AppQuitLogic

function AppQuitLogic.Defend()
    TipsDlg.Create("提示", "真的要退出吗？", "确定", function (okBtn)
        AppQuitDefend.DoQuit()
    end, "再玩一下", function (cancleBtn)

    end)
end