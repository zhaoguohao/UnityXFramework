-- 登录界面

GameHallPanel = GameHallPanel or {}
GameHallPanel.__index = GameHallPanel

local instance = nil

function GameHallPanel.Show()
    instance = UITool.CreatePanelObj(instance, GameHallPanel, 'GameHallPanel', PANEL_ID.GAME_HALL_PANEL_ID, GlobalObjs.s_gamePanel)
end

function GameHallPanel.Hide()
    UITool.HidePanel(instance)
end

function GameHallPanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 10)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function GameHallPanel:SetUi(binder)
    UGUITool.SetButton(binder, "backBtn", function (btn)
        self.Hide()
        LoginLogic.DoLogout()
    end)

    -- 背包按钮
    UGUITool.SetButton(binder, "backpackBtn", function (btn)
        BackpackPanel.Show()
    end)
end

function GameHallPanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil
end