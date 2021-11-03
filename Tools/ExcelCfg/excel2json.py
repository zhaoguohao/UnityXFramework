import exceltool

if '__main__' == __name__:
    exceltool.excel2json(u"Excels/测试.xlsx", "./output/test.json")
    exceltool.excel2json(u"Excels/测试2.xlsx", "./output/test2.json")
    print('done!')