GuideMask = GuideMask or {}
GuideMask.__index = GuideMask

function GuideMask.Create(target)
    if LuaUtil.IsNilOrNull(target) then
        logError("GuideMask Error, null == target")
        return nil
    end
    local self = {}
    self.gameObject = UITool.Instantiate(GlobalObjs.s_topPanel, 11)
    setmetatable(self, GuideMask)
    local binder = self.gameObject:GetComponent("PrefabBinder")

    self.mask = binder:GetObj("mask")
    self.mask:DoGuide(target.gameObject)
    return self
end

function GuideMask:Destroy()
    LuaUtil.SafeDestroyObj(self.gameObject)
end
