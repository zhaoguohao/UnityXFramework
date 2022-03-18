-- 登录界面

GameHallPanel = GameHallPanel or {}
GameHallPanel.__index = GameHallPanel

local instance = nil
local RT = RedpointTree

function GameHallPanel.Show()
    instance = UITool.CreatePanelObj(instance, GameHallPanel, 'GameHallPanel', PANEL_ID.GAME_HALL_PANEL_ID, GlobalObjs.s_gamePanel)
end

function GameHallPanel.Hide()
    UITool.HidePanel(instance)
end

function GameHallPanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 10)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function GameHallPanel:SetUi(binder)
    UGUITool.SetButton(binder, "backBtn", function (btn)
        self.Hide()
        LoginLogic.DoLogout()
    end)

    -- 背包按钮
    UGUITool.SetButton(binder, "backpackBtn", function (btn)
        BackpackPanel.Show()
    end)

    -- 红点系统
    UGUITool.SetButton(binder, "redpointBtn", function (btn)
        RedpointPanel.Show()
    end)

    -- 树
    UGUITool.SetButton(binder, "treeBtn", function (btn)
        TreePanel.Show()
    end)

    self.redpointText = binder:GetObj("redpointText")
    -- 注册红点回调
    RT.SetCallBack(RT.NodeNames.Root, "Root", function (redpointCnt)
        self:UpdateRedPoint(redpointCnt)
    end)
    self:UpdateRedPoint(RT.GetRedpointCnt(RT.NodeNames.Root))

    
end

-- 更新红点
function GameHallPanel:UpdateRedPoint(redpointCnt)
    self.redpointText.text = tostring(redpointCnt)
    LuaUtil.SafeActiveObj(self.redpointText.transform.parent, redpointCnt > 0)
end

function GameHallPanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil
    -- 注销红点回调
    RT.SetCallBack(RT.NodeNames.Root, "Root", nil)
end