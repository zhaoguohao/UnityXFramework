-- 红点系统树，前缀树结构

RedpointTree = RedpointTree or {}
local this = RedpointTree

-- 根节点
this.root = nil

-- 节点名
RedpointTree.NodeNames = {
    Root = "Root",

    ModelA = "Root|ModelA",
    ModelA_Sub_1 = "Root|ModelA|ModelA_Sub_1",
    ModelA_Sub_2 = "Root|ModelA|ModelA_Sub_2",

    ModelB = "Root|ModelB",
    ModelB_Sub_1 = "Root|ModelB|ModelB_Sub_1",
    ModelB_Sub_2 = "Root|ModelB|ModelB_Sub_2",
}

function RedpointTree.Init()
    -- 先创建根节点
    this.root = RedpointNode.New("Root")
    -- 构建前缀树
    for _, name in pairs(RedpointTree.NodeNames) do
        this.InsertNode(name)
    end

    -- for test-----------------------------------------------
    -- 塞入红点数据
    this.ChangeRedpointCnt(this.NodeNames.ModelA_Sub_1, 1)
    this.ChangeRedpointCnt(this.NodeNames.ModelA_Sub_2, 1)
    this.ChangeRedpointCnt(this.NodeNames.ModelB_Sub_1, 1)
    this.ChangeRedpointCnt(this.NodeNames.ModelB_Sub_2, 1)
end

-- 插入节点
function RedpointTree.InsertNode(name)
    if LuaUtil.IsStrNullOrEmpty(name) then
        return
    end
    if this.SearchNode(name) then
        -- 如果已经存在，则不重复插入
        log("你已经插入过节点了, name: " .. name)
        return
    end

    -- node从根节点出发
    local node = this.root
    node.passCnt = node.passCnt + 1 
    local pathList = LuaUtil.SplitString(name, "|")
    for _, path in pairs(pathList) do
        if nil == node.children[path] then
            node.children[path] = RedpointNode.New(path)
        end
        node = node.children[path]
        node.passCnt = node.passCnt + 1
    end
    node.endCnt = node.endCnt + 1
end

-- 查询节点是否在树中并返回节点
function RedpointTree.SearchNode(name)
    if LuaUtil.IsStrNullOrEmpty(name) then
        return nil
    end
    local node = this.root
    local pathList = LuaUtil.SplitString(name, "|")
    for _, path in pairs(pathList) do
        if nil == node.children[path] then
            return nil
        end
        node = node.children[path]
    end
    if node.endCnt > 0 then
        return node
    end
    return nil
end

-- 删除某个节点
function RedpointTree.DeleteNode(name)
    if nil == this.SearchNode(name) then
        return
    end
    local node = this.root
    node.passCnt = node.passCnt - 1
    local pathList = LuaUtil.SplitString(name, '.')
    for _, path in pairs(pathList) do
        local childNode = node.children[path] 
        childNode.passCnt = childNode.passCnt - 1
        if 0 == childNode.passCnt then
            node.children[path] = nil
            return
        end
        node = childNode
    end
    node.endCnt = node.endCnt - 1
end

-- 修改节点的红点数
function RedpointTree.ChangeRedpointCnt(name, delta)
    local targetNode = this.SearchNode(name)
    if nil == targetNode then
        return
    end
    -- 如果是减红点，并且红点数不够减了，则调整delta，使其不减为0
    if delta < 0 and targetNode.redpointCnt + delta < 0 then
        delta = -targetNode.redpointCnt
    end

    local node = this.root
    local pathList = LuaUtil.SplitString(name, "|")
    for _, path in pairs(pathList) do
        local childNode = node.children[path]
        childNode.redpointCnt = childNode.redpointCnt + delta
        node = childNode
        -- 调用回调函数
        for _, cb in pairs(node.updateCb) do
            cb(node.redpointCnt)
        end
    end
end

-- 查询节点的红点数
function RedpointTree.GetRedpointCnt(name)
    local node = this.SearchNode(name)
    if nil == node then
        return 0
    end
    return node.redpointCnt or 0
end

-- 设置红点更新回调函数
function RedpointTree.SetCallBack(name, key, cb)
    local node = this.SearchNode(name)
    if nil == node then
        return
    end
    node.updateCb[key] = cb
end

-- 递归获取整棵树的路径
function RedpointTree.GetFullTreePath(parent, pathList)
    for path, node in pairs(parent.children) do
        table.insert(pathList, path)
        if LuaUtil.TableCount(node.children) > 0 then
            this.GetFullTreePath(node, pathList)
        end
    end
end

-- 打印整棵树的路径
function RedpointTree.PrintFullTreePath()
    local pathList = {}
    this.GetFullTreePath(this.root, pathList)
    LuaUtil.PrintTable(pathList)
end

