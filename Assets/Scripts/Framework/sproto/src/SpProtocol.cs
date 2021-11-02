using System.Collections.Generic;
using System;

[Serializable]
public class SpProtocol {
    public string Name;
    public int Tag;

    [NonSerialized]
    public SpType Request;
    [NonSerialized]
    public SpType Response;

    public SpProtocol (string name, int tag) {
        Name = name;
        Tag = tag;
    }

    public void AddType (SpType type) {
        if (type.Name.Equals (Name + ".request"))
            Request = type;
        else if (type.Name.Equals (Name + ".response"))
            Response = type;
    }
}
