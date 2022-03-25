TreeLogic = TreeLogic or {}
local this = TreeLogic

-- 根节点
this.root = nil

-- 初始化
function TreeLogic.Init()

    -- 测试数据
    local data_table = {
        name = "林新发",
        university = "华南理工大学",
        major = '信息工程',
        job = 'Unity3D游戏开发工程师',
        blog = 'https://blog.csdn.net/linxinfa',
        hobby = {'吉他', '钢琴', '画画', '撸猫'},
        dream = {
            developer = {
                target = '成为一名优秀的独立游戏开发者',
                style = {'ARPG', 'FPS', 'SLG', 'MOBA'}
            },
            painter = {
                target = '成为一个独立画家',
                magnum_opus = {'暴走柯南', '皮皮猫', '光'}
            },
            musician = {
                target = '成为一个独立音乐人',
                magnum_opus = {'尘土', '树与风'}
            }
        }

    }
    -- 根节点
    this.root = TreeNode.New("Root")
    this.root = this.MakeTree(data_table, this.root)

    -- 打印树
    local str = ''
    str = this.TreeToString(this.root, str)
    log(str)
end

-- 构造树
-- tb: 数据table
-- parent: 父节点
function TreeLogic.MakeTree(tb, parent)
    -- 遍历table
    for k, v in pairs(tb) do
        -- 新建一个节点
        local node = TreeNode.New(k)
        node.value = v
        -- 设置父节点
        node.parent = parent
        -- 子节点缩进+1
        node.tab = parent.tab + 1
        -- 父节点的child塞入node
        if nil == parent.child then
            parent.child = {}
        end
        parent.child[k] = node
        -- 如果v是table，则递归遍历
        if type(v) == 'table' then
            -- 有子节点，默认不展开
            node.isopen = false
            this.MakeTree(v, node)
        end
    end
    return parent
end

-- 把树转为字符串
function TreeLogic.TreeToString(node, str)
    if nil ~= node.value then
        local tabspace = ''
        for i = 1, node.tab do
            tabspace = tabspace .. '    '
        end
        if 'table' == type(node.value) then
            str = str .. string.format('%s▼ %s :\n', tabspace, node.name)
        else
            str = str .. string.format('%s● %s : %s\n', tabspace, node.name, tostring(node.value))
        end
    end

    if nil ~= node.child then
        for _, child_node in pairs(node.child) do
            -- 递归
            str = this.TreeToString(child_node, str)
        end
    end
    return str
end

function TreeLogic.GetTree()
    return this.root
end
