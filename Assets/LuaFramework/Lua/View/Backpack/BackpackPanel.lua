-- 背包界面

BackpackPanel = BackpackPanel or {}
BackpackPanel.__index = BackpackPanel

local instance = nil

function BackpackPanel.Show()
    instance = UITool.CreatePanelObj(instance, BackpackPanel, 'BackpackPanel', PANEL_ID.BACKPACK_PANEL_ID, GlobalObjs.s_windowPanel)
end

function BackpackPanel.Hide()
    UITool.HidePanel(instance)
end

function BackpackPanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 14)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function BackpackPanel:SetUi(binder)
    UGUITool.SetButton(binder, "closeBtn", function (btn)
        self.Hide()
        LoginLogic.DoLogout()
    end)
    self.propItem = binder:GetObj("propItem")
    -- 从服务端获取背包数据
    BackpackLogic.GetPropsData(function (data)
        self:CreatePropList(data)
    end)
end

function BackpackPanel:CreatePropList(data)
    coroutine.start(function ()
        for _, dataItem in pairs(data) do
            if LuaUtil.IsNilOrNull(self.panelObj) then return end
            local item = LuaUtil.CloneObj(self.propItem)
            coroutine.wait(0.005)
        end
    end)
end

function BackpackPanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil
end