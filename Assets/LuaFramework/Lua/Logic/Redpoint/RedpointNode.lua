-- 红点系统，树节点

RedpointNode = RedpointNode or {}
RedpointNode.__index = RedpointNode


function RedpointNode.New(name)
    local self = {}
    -- 节点名
    self.name = name
    -- 节点被经过的次数
    self.passCnt = 0
    -- 节点作为末尾节点的次数
    self.endCnt = 0
    -- 红点数（子节点的红点数的和）
    self.redpointCnt = 0
    -- 子节点
    self.children = {}
    -- 红点更新时回调
    self.updateCb = {}
    setmetatable(self, RedpointNode)
    return self
end