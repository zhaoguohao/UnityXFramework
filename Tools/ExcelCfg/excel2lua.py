import exceltool

if '__main__' == __name__:
    exceltool.excel2lua(u"Excels/测试.xlsx", "./output/test.lua")
    exceltool.excel2lua(u"Excels/测试2.xlsx", "./output/test2.lua")
    print('done!')