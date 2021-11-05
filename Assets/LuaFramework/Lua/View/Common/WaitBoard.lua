-- 请稍等，全屏挡板

WaitBoard = WaitBoard or {}
local this = WaitBoard
this.boardObj = nil

function WaitBoard.Create()
    if not LuaUtil.IsNilOrNull(this.boardObj) then return end
    this.boardObj = UITool.Instantiate(GlobalObjs.s_topPanel, 13)
end

function WaitBoard.Destroy()
    LuaUtil.SafeDestroyObj(this.boardObj)
    this.boardObj = nil
end