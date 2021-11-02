LoginPanel = LoginPanel or {}
local this = LoginPanel
this.basePanel = nil
this.panelObj = nil

function LoginPanel.Show()
    this.basePanel = PanelMgr:ShowPanel(PANEL_ID.LOGIN_PANEL_ID, "LoginPanel", GlobalObjs.s_gamePanel)
end

function LoginPanel.OnShow(parent)
    log("LoginPanel.OnShow-----------")
    local panelObj = UITools.Instantiate(parent, 3)
    this.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    this.SetUi(binder)
end

function LoginPanel.SetUi(binder)
    binder:SetButton("closeBtn", function (btn)
        this.Hide()
        -- 测试延时调用
        DelayCallMgr:Call(2, function ()
            this.Show()
        end)
    end)
    SpriteMgr:SetSprite(binder:GetObj("img"), "btn")
end

function LoginPanel.Hide()
    if nil ~= this.basePanel then
        this.basePanel:Hide()
    end
end

function LoginPanel.OnHide()
    LuaUtil.SafeDestroyObj(this.panelObj)
end