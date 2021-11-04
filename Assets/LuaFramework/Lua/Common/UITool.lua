

UITool = UITool or { }
local this = UITool 

-- 实例化UI
function UITool.Instantiate(parent, prefabId)
    local obj = ResourceMgr:InstantiateGameObject(prefabId)
    if not LuaUtil.IsNilOrNull(parent) then
        obj.transform:SetParent(parent.transform, false)
    end
    return obj
end



-- endregion
