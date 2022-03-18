-- 树界面
TreePanel = TreePanel or {}
TreePanel.__index = TreePanel

local this = TreePanel
local instance = nil

function TreePanel.Show()
    instance = UITool.CreatePanelObj(instance, TreePanel, 'TreePanel', PANEL_ID.TREE_PANEL_ID, GlobalObjs.s_gamePanel)
end

function TreePanel.Hide()
    UITool.HidePanel(instance)
end

function TreePanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 16)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function TreePanel:SetUi(binder)
    UGUITool.SetButton(binder, "backBtn", function(btn)
        self.Hide()
    end)

    local tree = TreeLogic.GetTree()
    this.tiemForClone = binder:GetObj("itemForClone")
    LuaUtil.SafeActiveObj(this.tiemForClone, false)
    this.ExpanNode(tree)

end

-- 展开节点
function TreePanel.ExpanNode(node)
    if nil == node.child then
        return
    end
    local index = 1
    for _, child_node in pairs(node.child) do
        local item = LuaUtil.CloneObj(this.tiemForClone)

        local text = item.transform:GetChild(0):GetComponent("Text")
        child_node.uiObj = item

        if not LuaUtil.IsNilOrNull(node.uiObj) then
            -- 子节点塞在父节点下面
            local siblingIndex = node.uiObj:GetComponent("RectTransform"):GetSiblingIndex()
            child_node.uiObj:GetComponent("RectTransform"):SetSiblingIndex(siblingIndex + index)
            index = index + 1
        end
        if type(child_node.value) == 'table' then
            text.text = (child_node.isopen and '▼ ' or '► ') .. child_node.name
        else
            text.text = '● ' .. child_node.name .. ': ' .. child_node.value
        end
        -- 坐标缩进
        text.transform.localPosition = text.transform.localPosition + Vector3.New((child_node.tab-1)*50, 0,0)
        item:GetComponent("Button").onClick:AddListener(function()
     
            if not child_node.isopen then
                child_node.isopen = true
                -- 递归, 展开子节点
                this.ExpanNode(child_node)
            else
                -- 关闭子节点
                this.CloseNode(child_node)
            end

            if type(child_node.value) == 'table' then
                text.text =  ( child_node.isopen and '▼ ' or '► ') .. child_node.name
            end
        end)
    end
end

-- 关闭子节点
function TreePanel.CloseNode(node)
    if LuaUtil.IsNilOrNull(node.child) then
        return
    end
    node.isopen = false

    for _, child in pairs(node.child) do
        child.isopen = false
        LuaUtil.SafeDestroyObj(child.uiObj)
        if nil ~= child.child then
            -- 递归关闭子节点
            this.CloseNode(child)
        end
    end
end

function TreePanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil
end
