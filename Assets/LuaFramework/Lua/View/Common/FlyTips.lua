-- 冒提示语

FlyTips = FlyTips or {}
FlyTips.__index = FlyTips

function FlyTips.Show(contentText)
    local self = {}
    self.gameObject = UITools.Instantiate(GlobalObjs.s_topPanel, 9)
    setmetatable(self, FlyTips)
    local binder = self.gameObject:GetComponent("PrefabBinder")
    self:SetUi(binder, contentText)
    return self
end

function FlyTips:SetUi(binder, contentText)
    binder:SetText("tipsText", contentText)
    local aniEventTrigger = binder:GetObj("aniEventTrigger")
    aniEventTrigger.aniEvent = function ()
        LuaUtil.SafeDestroyObj(self.gameObject)
        self = nil
    end
end