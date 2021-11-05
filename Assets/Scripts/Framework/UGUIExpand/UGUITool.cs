using UnityEngine;
using UnityEngine.UI;
using System;

public class UGUITool
{
    /// <summary>
    /// 设置文本内容
    /// </summary>
    /// <param name="textStr">文本内容</param>
    /// <returns></returns>
    public static Text SetText(PrefabBinder binder, string name, string textStr)
    {
        var text = binder.GetObj<Text>(name);
        if (null == text)
        {
            GameLogger.LogError("SetText Error, obj is null: " + name);
            return null;

        }
        text.text = textStr;
        return text;
    }

    /// <summary>
    /// 设置按钮点击回调
    /// </summary>
    /// <param name="onClick">点击回调</param>
    /// <returns></returns>
    public static Button SetButton(PrefabBinder binder, string name, Action<GameObject> onClick)
    {
        var btn = binder.GetObj<Button>(name);
        if (null == btn)
        {
            GameLogger.LogError("SetButton Error, obj is null: " + name);
            return null;
        }
        btn.onClick.AddListener(() =>
        {
            if (null != onClick)
                onClick(btn.gameObject);
        });
        return btn;
    }

    /// <summary>
    /// 设置输入框文本输入完毕回调
    /// </summary>
    /// <param name="onEndEdit">文本输入完毕后回调</param>
    /// <returns></returns>
    public static InputField SetInputField(PrefabBinder binder, string name, Action<string> onEndEdit)
    {
        var input = binder.GetObj<InputField>(name);
        if (null == input)
        {
            GameLogger.LogError("SetInputField Error, obj is null: " + name);
            return null;
        }
        input.onEndEdit.AddListener((v) =>
        {
            if (null != onEndEdit)
                onEndEdit(v);
        });
        return input;
    }
    
    /// <summary>
    /// 设置下拉框选择回调
    /// </summary>
    /// <param name="onValueChanged">选择回调</param>
    /// <returns></returns>
    public static Dropdown SetDropDown(PrefabBinder binder, string name, Action<int> onValueChanged)
    {
        var dropdown = binder.GetObj<Dropdown>(name);
        if (null == dropdown)
        {
            GameLogger.LogError("SetDropDown Error, obj is null: " + name);
            return null;
        }
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener((v) =>
        {
            if (null != onValueChanged)
                onValueChanged(v);
        });

        return dropdown;
    }

    /// <summary>
    /// 设置单选框选择回调
    /// </summary>
    /// <param name="onValueChanged">选择回调</param>
    /// <returns></returns>
    public static Toggle SetToggle(PrefabBinder binder, string name, Action<bool> onValueChanged)
    {
        var toogle = binder.GetObj<Toggle>(name);
        if (null == toogle)
        {
            GameLogger.LogError("SetToggle Error, obj is null: " + name);
            return null;
        }

        toogle.onValueChanged.RemoveAllListeners();
        toogle.onValueChanged.AddListener((v) =>
        {
            if (null != onValueChanged)
                onValueChanged(v);
        });

        return toogle;
    }

    /// <summary>
    /// 设置单选框选择回调
    /// </summary>
    /// <param name="onValueChanged">选择回调</param>
    /// <returns></returns>
    public static Slider SetSlider(PrefabBinder binder, string name, Action<float> onValueChanged)
    {
        var slider = binder.GetObj<Slider>(name);
        if (null == slider)
        {
            GameLogger.LogError("SetSlider Error, obj is null: " + name);
            return null;
        }

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((v) =>
        {
            if (null != onValueChanged)
                onValueChanged(v);
        });

        return slider;
    }


}
