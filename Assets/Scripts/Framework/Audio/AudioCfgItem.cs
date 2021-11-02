
public class AudioCfgItem : ConfigItem
{
    public int id;
    public string name;
    public float volume;
    public int channel;

    public override string GetKey()
    {
        return name;
    }
}

