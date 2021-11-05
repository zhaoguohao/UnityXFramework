

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

-- 创建Panel对象
function UITool.CreatePanelObj(instance, base, panelName, panelId, parent)
    if nil ~= instance then return instance end
    local panel = {
        panelName = panelName,
        basePanel = nil,
        panelObj = nil,
    }
    setmetatable(panel, base)
    panel.basePanel = PanelMgr:ShowPanel(panelId, panel, parent)
    return panel
end

-- 关闭Panel
function UITool.HidePanel(instance)
    if nil ~= instance and nil ~= instance.basePanel then
        instance.basePanel:Hide()
    end
end

-- endregion
