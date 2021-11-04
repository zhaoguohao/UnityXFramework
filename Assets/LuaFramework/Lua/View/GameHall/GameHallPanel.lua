-- 登录界面

GameHallPanel = GameHallPanel or {}
local this = GameHallPanel

this.basePanel = nil
this.panelObj = nil
this.accountInput = nil
this.pwdInput = nil

function GameHallPanel.Show()
    this.basePanel = PanelMgr:ShowPanel(PANEL_ID.GAME_HALL_PANEL_ID, "GameHallPanel", GlobalObjs.s_gamePanel)
end

function GameHallPanel.OnShow(parent)
    local panelObj = UITools.Instantiate(parent, 10)
    this.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    this.SetUi(binder)
end

-- UI交互
function GameHallPanel.SetUi(binder)
    binder:SetButton("backBtn", function ()
        this.Hide()
        LoginLogic.DoLogout()
    end)
end

function GameHallPanel.Hide()
    if nil ~= this.basePanel then
        this.basePanel:Hide()
    end
end

function GameHallPanel.OnHide()
    LuaUtil.SafeDestroyObj(this.panelObj)
end