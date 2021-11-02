

UITools = UITools or { }
local this = UITools 

-- 实例化UI
function UITools.Instantiate(parent, prefabId)
    local obj = ResourceMgr:InstantiateGameObject(prefabId)
    if not LuaUtil.IsNilOrNull(parent) then
        obj.transform:SetParent(parent.transform, false)
    end
    return obj
end



-- endregion
