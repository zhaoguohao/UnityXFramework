-- 提示框

TipsDlg = TipsDlg or {}
TipsDlg.__index = TipsDlg

function TipsDlg.Create(title, content, okText, okBtnCb, cancleTxt, cancleBtnCb)
    local self = {
        title = title,
        content = content,
        okText = okText,
        okBtnCb = okBtnCb,
        cancleTxt = cancleTxt,
        cancleBtnCb = cancleBtnCb
    }
    self.gameObject = UITool.Instantiate(GlobalObjs.s_topPanel, 12)
    setmetatable(self, TipsDlg)
    self:SetUi()
end

function TipsDlg:SetUi()
    local binder = self.gameObject:GetComponent("PrefabBinder")
    -- 标题
    UGUITool.SetText(binder, "titleText", self.title)
    -- 内容
    UGUITool.SetText(binder, "contentText", self.content)
    -- 确定按钮文本
    UGUITool.SetText(binder, "okText", self.okText or "")
    -- 确定按钮回调
    UGUITool.SetButton(binder, "okBtn", function (btn)
        if not LuaUtil.IsNilOrNull(self.okBtnCb) then
            self.okBtnCb()
        end
        LuaUtil.SafeDestroyObj(self.gameObject)
    end)
    -- 取消按钮文本
    UGUITool.SetText(binder, "cancleText", self.cancleTxt or "")
    -- 取消按钮回调
    local cancleBtn = UGUITool.SetButton(binder, "cancleBtn", function (btn)
        if not LuaUtil.IsNilOrNull(self.cancleBtnCb) then
            self.cancleBtnCb()
        end
        LuaUtil.SafeDestroyObj(self.gameObject)
    end)
    LuaUtil.SafeActiveObj(cancleBtn, not LuaUtil.IsNilOrNull(self.cancleBtnCb))
end
