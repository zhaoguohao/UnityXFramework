-- 登录界面

LoginPanel = LoginPanel or {}
local this = LoginPanel

this.basePanel = nil
this.panelObj = nil


function LoginPanel.Show()
    this.basePanel = PanelMgr:ShowPanel(PANEL_ID.LOGIN_PANEL_ID, "LoginPanel", GlobalObjs.s_gamePanel)
end

function LoginPanel.OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 3)
    this.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    this.SetUi(binder)
end

-- UI交互
function LoginPanel.SetUi(binder)
    -- 账号输入框
    local accountInput = binder:GetObj("accountInput")
    accountInput.text = Cache.Get("ACCOUNT", "")
    -- 密码输入框
    local pwdMd5 = Cache.Get("PASSWORD", "")
    local pwdInput = UGUITool.SetInputField(binder, "pwdInput", function (v)
        if "********" == v then
            return
        end
        local newPwdMd5 = Util.md5(v)
        if newPwdMd5 ~= pwdMd5 then
            pwdMd5 = newPwdMd5
        end
    end)
    
    if not LuaUtil.IsStrNullOrEmpty(pwdMd5) then
        pwdInput.text = "********"
    end

    -- 登录按钮
    UGUITool.SetButton(binder, "loginBtn", function (btn)
        -- 执行登录
        local account = accountInput.text
        LoginLogic.DoLogin(account, pwdMd5, function (ok)
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