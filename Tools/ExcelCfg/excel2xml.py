import exceltool

if '__main__' == __name__:
    exceltool.excel2xml(u"Excels/测试.xlsx", "./output/test.xml")
    exceltool.excel2xml(u"Excels/测试2.xlsx", "./output/test2.xml")
    print('done!')