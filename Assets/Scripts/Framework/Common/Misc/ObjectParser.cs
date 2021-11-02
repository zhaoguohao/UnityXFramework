using System.Collections.Generic;
using System.Xml;
using System;


/// <summary>
/// 解析xml进行字段反射
/// </summary>
public static class ObjectParser
{
    public enum Result
    {
        OK,
        FieldNotExist,
        InvalidEnum,
        FormatError
    }

    public static string lastError { get; private set; }

    private delegate object StringConverter(string raw);

    private static Dictionary<Type, StringConverter> _predefinedConverters;


    private delegate TOutput Converter<TInput, TOutput>(TInput input);

    static StringConverter MakeConverter<T>(Converter<string, T> converter)
    {
        return s =>
        {
            if (string.IsNullOrEmpty(s))
                return default(T);

            return converter(s);
        };
    }

    public static T GetConvert<T>(int index, params object[] objArr)
    {
        if (objArr.Length <= index) return default(T);
        return GetConvert<T>(objArr[index]);
    }

    public static T GetConvert<T>(object obj)
    {
        if (obj == null)
            return default(T);
        StringConverter del = null;
        _predefinedConverters.TryGetValue(typeof(T), out del);

        return (T)del(obj.ToString());

    }

    static ObjectParser()
    {
        _predefinedConverters = new Dictionary<Type, StringConverter>();
        _predefinedConverters[typeof(long)] = MakeConverter<long>(Convert.ToInt64);
        _predefinedConverters[typeof(ulong)] = MakeConverter<ulong>(Convert.ToUInt64);
        _predefinedConverters[typeof(int)] = MakeConverter<int>(Convert.ToInt32);
        _predefinedConverters[typeof(uint)] = MakeConverter<uint>(Convert.ToUInt32);
        _predefinedConverters[typeof(short)] = MakeConverter<short>(Convert.ToInt16);
        _predefinedConverters[typeof(ushort)] = MakeConverter<ushort>(Convert.ToUInt16);
        _predefinedConverters[typeof(byte)] = MakeConverter<byte>(Convert.ToByte);
        _predefinedConverters[typeof(sbyte)] = MakeConverter<sbyte>(Convert.ToSByte);
        _predefinedConverters[typeof(float)] = MakeConverter<float>(Convert.ToSingle);
        _predefinedConverters[typeof(double)] = MakeConverter<double>(Convert.ToDouble);
        _predefinedConverters[typeof(string)] = s => s ?? "";
    }

    public static Result Parse(XmlNode node, ref object obj, Type type)
    {
        foreach (XmlNode attrNode in node.Attributes)
        {
            string fieldName = attrNode.Name;
            string fieldValueStr = attrNode.Value;

            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null)
                continue;

            var fieldType = fieldInfo.FieldType;
            try
            {
                StringConverter converter;
                if (_predefinedConverters.TryGetValue(fieldType, out converter))
                    fieldInfo.SetValue(obj, converter(fieldValueStr));
                else
                {
                    if (fieldType.IsSubclassOf(typeof(Enum)) && !string.IsNullOrEmpty(fieldValueStr))
                    {
                        int ivalue = 0;
                        if (Enum.IsDefined(fieldType, fieldValueStr))
                        {
                            fieldInfo.SetValue(obj, Enum.Parse(fieldType, fieldValueStr, true));
                        }
                        else if (int.TryParse(fieldValueStr, out ivalue) && Enum.IsDefined(fieldType, ivalue))
                        {
                            fieldInfo.SetValue(obj, Enum.ToObject(fieldType, ivalue));
                        }
                        else
                        {
                            //lastError = string.Format("invalid enum {0} for field {1}", fieldValueStr, fieldName);
                            //return Result.InvalidEnum;
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                lastError = string.Format("format error for field {0}, value is {1}, error message = {2}", fieldName, fieldValueStr, e.Message);
                return Result.FormatError;
            }
        }

        return Result.OK;
    }
}
