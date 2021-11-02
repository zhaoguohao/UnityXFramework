using System.Collections;
using System;

[Serializable]
public class SpField {
    public string Name;
    [NonSerialized]
    public SpType Type;
    public string TypeName;
    public int Tag;
    public bool IsArray;

    //private SpTypeManager mTypeManager;

	public SpField (string name, short tag, string type, bool array) {
		Name = name;
		Tag = tag;
		TypeName = type;
		IsArray = array;

        //mTypeManager = m;
	}

    public bool CheckAndUpdate(SpTypeManager typeManager)
    {
		if (Type != null)
			return true;

		// use GetTypeNoCheck instead of GetType, to prevent infinit GetType call
		// when a type reference itself like : foobar { a 0 : foobar }
        Type = typeManager.GetTypeNoCheck(TypeName);
		return (Type != null);
	}
}
