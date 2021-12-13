-- 红点系统界面

RedpointPanel = RedpointPanel or {}
RedpointPanel.__index = RedpointPanel

local RT = RedpointTree

local instance = nil

function RedpointPanel.Show()
    instance = UITool.CreatePanelObj(instance, RedpointPanel, 'RedpointPanel', PANEL_ID.REDPOINT_PANEL_ID, GlobalObjs.s_windowPanel)
end

function RedpointPanel.Hide()
    UITool.HidePanel(instance)
end

function RedpointPanel:OnShow(parent)
    local panelObj = UITool.Instantiate(parent, 15)
    self.panelObj = panelObj
    local binder = panelObj:GetComponent("PrefabBinder")
    self:SetUi(binder)
end

-- UI交互
function RedpointPanel:SetUi(binder)
    UGUITool.SetButton(binder, "closeBtn", function (btn)
        self.Hide()
        LoginLogic.DoLogout()
    end)
    self.propItem = binder:GetObj("propItem")
    
    self.modelARedpointText = binder:GetObj("modelARedpointText")
    self.modelBRedpointText = binder:GetObj("modelBRedpointText")
    self.modelASub1Redpoint = binder:GetObj("modelASub1Redpoint")
    self.modelASub2Redpoint = binder:GetObj("modelASub2Redpoint")
    self.modelBSub1Redpoint = binder:GetObj("modelBSub1Redpoint")
    self.modelBSub2Redpoint = binder:GetObj("modelBSub2Redpoint")

    -- 注册红点更新回调-----------------------------------------------------------------
    RT.SetCallBack(RT.NodeNames.ModelA, "ModelA", function (redpointCnt)
        self:UpdateRedPoint_ModelA(redpointCnt)
    end)
    self:UpdateRedPoint_ModelA(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelA))

    RT.SetCallBack(RT.NodeNames.ModelA_Sub_1, "ModelA_Sub1", function (redpointCnt)
        self:UpdateRedPoint_ModelA_Sub1(redpointCnt)
    end)
    self:UpdateRedPoint_ModelA_Sub1(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelA_Sub_1))

    RT.SetCallBack(RT.NodeNames.ModelA_Sub_2, "ModelA_Sub2", function (redpointCnt)
        self:UpdateRedPoint_ModelA_Sub2(redpointCnt)
    end)
    self:UpdateRedPoint_ModelA_Sub2(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelA_Sub_2))

    RT.SetCallBack(RT.NodeNames.ModelB, "ModelB", function (redpointCnt)
        self:UpdateRedPoint_ModelB(redpointCnt)
    end)
    self:UpdateRedPoint_ModelB(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelB))

    RT.SetCallBack(RT.NodeNames.ModelB_Sub_1, "ModelB_Sub1", function (redpointCnt)
        self:UpdateRedPoint_ModelB_Sub1(redpointCnt)
    end)
    self:UpdateRedPoint_ModelB_Sub1(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelB_Sub_1))

    RT.SetCallBack(RT.NodeNames.ModelB_Sub_2, "ModelB_Sub2", function (redpointCnt)
        self:UpdateRedPoint_ModelB_Sub2(redpointCnt)
    end)
    self:UpdateRedPoint_ModelB_Sub2(RedpointTree.GetRedpointCnt(RT.NodeNames.ModelB_Sub_2))
    ------------------------------------------------------------------------------------

    UGUITool.SetButton(binder, "modelASub1Btn", function (btn)
        RT.ChangeRedpointCnt(RT.NodeNames.ModelA_Sub_1, -1)
    end)
    UGUITool.SetButton(binder, "modelASub2Btn", function (btn)
        RT.ChangeRedpointCnt(RT.NodeNames.ModelA_Sub_2, -1)
    end)
    UGUITool.SetButton(binder, "modelBSub1Btn", function (btn)
        RT.ChangeRedpointCnt(RT.NodeNames.ModelB_Sub_1, -1)
    end)
    UGUITool.SetButton(binder, "modelBSub2Btn", function (btn)
        RT.ChangeRedpointCnt(RT.NodeNames.ModelB_Sub_2, -1)
    end)
end

function RedpointPanel:UpdateRedPoint_ModelA(redpointCnt)
    self.modelARedpointText.text = tostring(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelARedpointText.transform.parent, redpointCnt > 0)
end

function RedpointPanel:UpdateRedPoint_ModelA_Sub1(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelASub1Redpoint, redpointCnt > 0)
end

function RedpointPanel:UpdateRedPoint_ModelA_Sub2(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelASub2Redpoint, redpointCnt > 0)
end

function RedpointPanel:UpdateRedPoint_ModelB(redpointCnt)
    self.modelBRedpointText.text = tostring(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelBRedpointText.transform.parent, redpointCnt > 0)
end

function RedpointPanel:UpdateRedPoint_ModelB_Sub1(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelBSub1Redpoint, redpointCnt > 0)
end

function RedpointPanel:UpdateRedPoint_ModelB_Sub2(redpointCnt)
    LuaUtil.SafeActiveObj(self.modelBSub2Redpoint, redpointCnt > 0)
end

function RedpointPanel:OnHide()
    LuaUtil.SafeDestroyObj(self.panelObj)
    instance = nil

    -- 注销红点回调-------------------------------------------------------------------  
    RT.SetCallBack(RT.NodeNames.ModelA, "ModelA", nil)
    RT.SetCallBack(RT.NodeNames.ModelA_Sub_1, "ModelA_Sub1", nil)
    RT.SetCallBack(RT.NodeNames.ModelA_Sub_2, "ModelA_Sub2", nil)
    RT.SetCallBack(RT.NodeNames.ModelB, "ModelB", nil)
    RT.SetCallBack(RT.NodeNames.ModelB_Sub_1, "ModelB_Sub1", nil)
    RT.SetCallBack(RT.NodeNames.ModelB_Sub_2, "ModelB_Sub2", nil)
end