-- 树界面
TreePanel = TreePanel or {}
TreePanel.__index = TreePanel

local this = TreePanel
local instance = nil
this.uiNodeTb = nil

function TreePanel.Show()
    instance = UITool.CreatePanelObj(instance, TreePanel, 'TreePanel', PANEL_ID.TREE_PANEL_ID, GlobalObjs.s_gamePanel)
end

function TreePanel.Hide()
    UITool.HidePanel(instance)
end

function TreePanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 16)
    self.panelObj = panelObj
    this.uiNodeTb = {}
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function TreePanel:SetUi(binder)
    UGUITool.SetButton(binder, "backBtn", function(btn)
        self.Hide()
    end)

    this.tiemForClone = binder:GetObj("itemForClone")
    LuaUtil.SafeActiveObj(this.tiemForClone, false)

    local tree = TreeLogic.GetTree()
    this.ExpanNode(tree)

end

-- 展开节点
function TreePanel.ExpanNode(node)
    if nil == node.child then
        return
    end
    local index = 1
    for _, child_node in pairs(node.child) do
        local uiUnit = {}
        if this.uiNodeTb[child_node] then
            -- 从缓存中取ui对象
            uiUnit = this.uiNodeTb[child_node]
            -- 显示
            LuaUtil.SafeActiveObj(uiUnit.obj, true)
            if child_node.isopen then
                -- 递归, 展开子节点
                this.ExpanNode(child_node)
            end
        else
            -- 创建节点的UI对象
            uiUnit.obj = LuaUtil.CloneObj(this.tiemForClone)
            uiUnit.text = uiUnit.obj.transform:GetChild(0):GetComponent("Text")
            uiUnit.btn = uiUnit.obj:GetComponent("Button")
            -- 坐标缩进
            uiUnit.text.transform.localPosition = uiUnit.text.transform.localPosition +
                                                      Vector3.New((child_node.tab - 1) * 50, 0, 0)
            child_node.uiObj = uiUnit.obj

            if not LuaUtil.IsNilOrNull(node.uiObj) then
                -- 子节点塞在父节点下面
                local siblingIndex = node.uiObj:GetComponent("RectTransform"):GetSiblingIndex()
                child_node.uiObj:GetComponent("RectTransform"):SetSiblingIndex(siblingIndex + index)
                index = index + 1
            end

            uiUnit.btn.onClick:AddListener(function()

                if not child_node.isopen then
                    child_node.isopen = true
                    -- 递归, 展开子节点
                    this.ExpanNode(child_node)
                else
                    -- 关闭子节点
                    this.CloseNode(child_node, false)
                end

                if type(child_node.value) == 'table' then
                    uiUnit.text.text = (child_node.isopen and '▼ ' or '► ') .. child_node.name
                end
            end)
            this.uiNodeTb[child_node] = uiUnit
        end
        -- 更新展开文本
        if type(child_node.value) == 'table' then
            uiUnit.text.text = (child_node.isopen and '▼ ' or '► ') .. child_node.name
        else
            uiUnit.text.text = '● ' .. child_node.name .. ': ' .. child_node.value
        end
    end
end

-- 关闭子节点
function TreePanel.CloseNode(node, onlyHide)
    if LuaUtil.IsNilOrNull(node.child) then
        return
    end
    if not onlyHide then
        node.isopen = false
    end

    for _, child in pairs(node.child) do
        LuaUtil.SafeActiveObj(child.uiObj, false)
        if nil ~= child.child then
            -- 递归关闭子节点
            this.CloseNode(child, true)
        end
    end
end

function TreePanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    this.uiNodeTb = nil
    instance = nil
end
