-- 登录逻辑

LoginLogic = LoginLogic or {}
local this = LoginLogic

-- 执行登录
function LoginLogic.DoLogin(account, pwdMd5, cb)
    if not this.CheckAccount(account) then
        return
    end

    -- TODO 调用SDK或与游戏服务端通信，执行登录流程
    log("DoLogin, account: " .. account .. ", pwdMd5: " .. pwdMd5)
    -- 缓存账号密码
    Cache.Set('ACCOUNT', account)
    Cache.Set('PASSWORD', pwdMd5)
    Network.Connect('127.0.0.1', 8888, function (ok)
        -- TODO 发送登录协议


        -- 回调
        if nil ~= cb then
            cb(true)
        end
    end)

end

-- 执行登出
function LoginLogic.DoLogout()
    -- TODO 调用SDK登出接口
    log("DoLogout")
    -- 显示登录界面
    LoginPanel.Show()
end

-- 检测账号合法性
function LoginLogic.CheckAccount(account)
    if LuaUtil.IsStrNullOrEmpty(account) then
        -- 请输入账号
        FlyTips.Create(I18N.GetStr(2))
        return false
    end
    return true
end

-- 检测密码合法性
function LoginLogic.CheckPwd(pwd)
    if LuaUtil.IsStrNullOrEmpty(pwd) then
        -- 请输入账号
        FlyTips.Create(I18N.GetStr(3))
        return false
    end
    return true
end
