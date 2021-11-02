
/// <summary>
/// 资源配置Item
/// </summary>
public class ResourceConfigItem : ConfigItem
{
    public int id;
    public string editorPath;

    public string desc;

    public string name;

    public override string GetKey()
    {
        return id.ToString();
    }

    public override void OnItemParsed()
    {
        parseName();
    }

    private void parseName()
    {
        int index0 = editorPath.LastIndexOf(".");
        int index1 = editorPath.LastIndexOf("/");
        if (index0 == -1 && index1 != -1)
        {
            name = editorPath.Substring(index1 + 1, editorPath.Length - index1 - 1);
        }
        else
        {
            name = editorPath.Substring(index1 + 1, index0 - index1 - 1);
        }
    }

    

    public override string ToString()
    {
        return string.Format("id = {0},path = {1},description = {2}", id, editorPath, desc);
    }


}
