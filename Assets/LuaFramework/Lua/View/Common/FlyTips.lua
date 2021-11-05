-- 冒提示语

FlyTips = FlyTips or {}
FlyTips.__index = FlyTips

function FlyTips.Create(contentText)
    local self = {}
    self.gameObject = UITool.Instantiate(GlobalObjs.s_topPanel, 9)
    setmetatable(self, FlyTips)
    local binder = self.gameObject:GetComponent("PrefabBinder")
    self:SetUi(binder, contentText)
end

function FlyTips:SetUi(binder, contentText)
    UGUITool.SetText(binder, "tipsText", contentText)
  
    local aniEventTrigger = binder:GetObj("aniEventTrigger")
    aniEventTrigger.aniEvent = function ()
        LuaUtil.SafeDestroyObj(self.gameObject)
        self = nil
    end
end