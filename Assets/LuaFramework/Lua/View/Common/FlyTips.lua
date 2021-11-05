-- 冒提示语

FlyTips = FlyTips or {}
FlyTips.__index = FlyTips

function FlyTips.Create(content)
    local self = {
        content = content
    }
    self.gameObject = UITool.Instantiate(GlobalObjs.s_topPanel, 9)
    setmetatable(self, FlyTips)
    self:SetUi()
end

function FlyTips:SetUi()
    local binder = self.gameObject:GetComponent("PrefabBinder")
    UGUITool.SetText(binder, "tipsText", self.content)
  
    local aniEventTrigger = binder:GetObj("aniEventTrigger")
    aniEventTrigger.aniEvent = function ()
        LuaUtil.SafeDestroyObj(self.gameObject)
        self = nil
    end
end