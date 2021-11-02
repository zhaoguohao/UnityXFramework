//default 默认的加载的bundle 文件名，  
//Lua/Logic Logic文件夹下所有文件加载的Bundle名称 
//ShuangYuNoticePanel.lua加在的Bundle名称
//加载文件时候，先根据名称找，然后在递归根据上层名字找，最后在从default中加载


default | lua.bundle
Lua/Logic | lua_logic.bundle
Lua/View/ShuangYuNoticePanel.lua | lua_view_shuangyunoticepanel.lua


