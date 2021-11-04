using UnityEngine;
using UnityEngine.UI;
using System;

public class UGUITool
{
    public static Text SetText(PrefabBinder binder, string name, string textStr)
    {
        var text = binder.GetObj<Text>(name);
        if (null != text)
        {
            text.text = textStr;
        }
        else
        {
            Debug.LogError("PrefabBinder SetText Error, obj is null: " + name);
        }
        return text;
    }

    public static Button SetButton(PrefabBinder binder, string name, Action<GameObject> onClick)
    {
        var btn = binder.GetObj<Button>(name);
        if (null != btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                onClick(btn.gameObject);
            });
        }
        else
        {
            Debug.LogError("PrefabBinder SetButton Error, obj is null: " + name);
        }
        return btn;
    }

    public static InputField SetInputField(PrefabBinder binder, string name, Action<string> onValueChanged)
    {
        var input = binder.GetObj<InputField>(name);
        if (null != input)
        {
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener((v) =>
            {
                onValueChanged(v);
            });
        }
        else
        {
            Debug.LogError("PrefabBinder SetInputField Error, obj is null: " + name);
        }
        return input;
    }
}
