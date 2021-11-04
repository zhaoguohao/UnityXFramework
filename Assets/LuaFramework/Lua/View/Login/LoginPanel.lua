-- 登录界面

LoginPanel = LoginPanel or {}
local this = LoginPanel

this.basePanel = nil
this.panelObj = nil
this.accountInput = nil
this.pwdInput = nil

function LoginPanel.Show()
    this.basePanel = PanelMgr:ShowPanel(PANEL_ID.LOGIN_PANEL_ID, "LoginPanel", GlobalObjs.s_gamePanel)
end

function LoginPanel.OnShow(parent)
    local panelObj = UITools.Instantiate(parent, 3)
    this.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    this.SetUi(binder)
end

-- UI交互
function LoginPanel.SetUi(binder)
    -- 账号输入框
    this.accountInput = binder:GetObj("accountInput")
    -- 密码输入框
    this.pwdInput = binder:GetObj("pwdInput")
    -- 登录按钮
    binder:SetButton("loginBtn", function (btn)
        -- 执行登录
        local account = this.accountInput.text
        local pwd = this.pwdInput.text
        LoginLogic.DoLogin(account, pwd, function (ok)
            if not ok then return end
            -- 登录成功，关闭登录界面
            this.Hide()
            -- 进入大厅界面
            GameHallPanel.Show()
        end)
    end)
end

function LoginPanel.Hide()
    if nil ~= this.basePanel then
        this.basePanel:Hide()
    end
end

function LoginPanel.OnHide()
    LuaUtil.SafeDestroyObj(this.panelObj)
end