
public class SpriteCfg 
{
    public void Init()
    {
        m_cfg = new ConfigFile<SpriteCfgItem>("sprite2atlas.bytes");
    }

    public SpriteCfgItem GetCfg(string name)
    {
        return m_cfg.GetItem(name);
    }

    private ConfigFile<SpriteCfgItem> m_cfg;
}
