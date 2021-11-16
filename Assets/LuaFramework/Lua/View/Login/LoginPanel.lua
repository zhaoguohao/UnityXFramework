-- 登录界面

LoginPanel = LoginPanel or {}
LoginPanel.__index = LoginPanel

local instance = nil

function LoginPanel.Show()
    instance = UITool.CreatePanelObj(instance, LoginPanel, 'LoginPanel', PANEL_ID.LOGIN_PANEL_ID, GlobalObjs.s_gamePanel)
end

function LoginPanel.Hide()
    UITool.HidePanel(instance)
end

function LoginPanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 3)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function LoginPanel:SetUi(binder)
    -- 账号输入框
    local accountInput = binder:GetObj("accountInput")
    accountInput.text = Cache.Get("ACCOUNT", "")
    -- 密码输入框
    local cachePwdMd5 = Cache.Get("PASSWORD", "")
    local finalPwdMd5 = cachePwdMd5
    
    local pwdInput = UGUITool.SetInputField(binder, "pwdInput", function (v)
        -- 输入框编辑完毕后会进入这个回调
        if "********" == v then
            return
        end
        local newPwdMd5 = Util.md5(v)
        if newPwdMd5 ~= cachePwdMd5 then
            finalPwdMd5 = newPwdMd5
        end
    end)

    -- 当缓存了密码，修改任何字符都会清掉密码
    pwdInput.onValueChanged:AddListener(function (v)
        if "********" == v then
            return
        end
        if not LuaUtil.IsStrNullOrEmpty(cachePwdMd5) then
            cachePwdMd5 = ""
            pwdInput.text = ""
        end
    end)

    -- 如果缓存了密码，则密码框显示为********
    if not LuaUtil.IsStrNullOrEmpty(cachePwdMd5) then
        pwdInput.text = "********"
    end

    -- 登录按钮
    UGUITool.SetButton(binder, "loginBtn", function (btn)
        if not LoginLogic.CheckPwd(pwdInput.text) then
            return
        end

        -- 执行登录
        local account = accountInput.text
        -- logGreen("pwd :" .. pwdInput.text .. ", md5: " .. Util.md5(pwdInput.text))
        LoginLogic.DoLogin(account, finalPwdMd5, function (ok)
            if not ok then return end
            -- 登录成功，关闭登录界面
            self.Hide()
            -- 进入大厅界面
            GameHallPanel.Show()
        end)
    end)

    -- 多语言设置
    local language = UGUITool.SetDropDown(binder, "languageDropdown", function (v)
        LanguageMgr:ChangeLanguageType(v)
    end)
    language.value = LanguageMgr.languageIndex
end

function LoginPanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil
end