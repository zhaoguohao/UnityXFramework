BackpackLogic = BackpackLogic or {}
local this = BackpackLogic

-- TODO 加载背包配资表
-- require "res/backpack_cfg"

this.data = {}

function BackpackLogic.GetPropsData(cb)
    -- TODO 从服务器获取背包道具数据，存到this.data中
    -- 模拟
    for i = 1,50 do
        this.data[i] = {
            id = i,
            cnt = 1,
        }
    end
    if nil ~= cb then
        cb(this.data)
    end
end

