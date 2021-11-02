

public class SpriteCfgItem : ConfigItem
{
    public string name;
    public int resId;

    public override string GetKey()
    {
        return name;
    }
}
