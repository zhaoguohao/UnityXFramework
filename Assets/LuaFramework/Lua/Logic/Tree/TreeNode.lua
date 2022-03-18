-- TreeNode.lua 树节点

TreeNode = TreeNode or {}
TreeNode.__index = TreeNode

function TreeNode.New(name)
    local self = {}
    -- 节点名
    self.name = name
    -- 值
    self.value = nil
    -- 父节点
    self.parent = nil
    -- 子节点
    self.child = nil
    -- 缩进
    self.tab = 0

    -- 是否展开
    self.isopen = true
    -- UI对象
    self.uiObj = nil

    setmetatable(self, TreeNode)
    return self
end
