using System.Collections.Generic;
using System;

[Serializable]
public class SpType {
#if USE_OLD_SPRTYPE
	public string Name;
	public Dictionary<int, SpField> Fields = new Dictionary<int, SpField> ();
	public Dictionary<string, SpField> FieldNames = new Dictionary<string, SpField> ();

	public SpType (string name) {
		Name = name;
	}

	public void AddField (SpField f) {
		Fields.Add(f.Tag, f);
		FieldNames.Add(f.Name, f);
	}
	
	public SpField GetFieldByName (string name) {
		if (FieldNames.ContainsKey(name))
			return FieldNames[name];
		return null;
	}
	
	public SpField GetFieldByTag (int tag) {
		if (Fields.ContainsKey(tag))
			return Fields[tag];
		return null;
	}

	public bool CheckAndUpdate () {
		bool complete = true;
		foreach (SpField f in Fields.Values) {
			if (f.CheckAndUpdate ())
				continue;
			complete = false; 
		}
		return complete;
	}
#else
    public string Name;
    public SpField[] Fields;
    protected string[] mFiledsNames = null;
    public string[] FiledsNames { get {
        if (mFiledsNames != null)
            return mFiledsNames;
        if (Fields == null || Fields.Length <= 0)
            return null;
        int count = 0;
        for (int i = 0; i < Fields.Length; i++)
            if (Fields[i]!=null)
                count++;
        if (count <= 0)
            return null;
        int idx = 0;
        mFiledsNames = new string[count];
        for (int i = 0; i < Fields.Length; i++)
        {
            var f = Fields[i];
            if (f != null)
            {
                mFiledsNames[idx] = f.Name;
                idx++;
            }
        }
        
        return mFiledsNames;
    } }

    public SpType(string name)
    {
        Name = name;
        Fields = null;
    }

    public SpType(string name, int len)
    {
        Name = name;
        Fields = new SpField[len];
        System.Array.Clear(Fields, 0, Fields.Length);
    }

    public SpType(string name, params SpField[] args)
    {
        Name = name;
        if (args == null || args.Length == 0)
        {
            Fields = null;
        }
        else
        {
            Fields = args;
        }
    }

    public void AddField(SpField f)
    {
        int idx = f.Tag;

        if (Fields == null)
        {
            Fields = new SpField[1];
            System.Array.Clear(Fields, 0, Fields.Length);
        }

        if (idx > Fields.Length && idx - Fields.Length > 256)
        {
            throw new Exception("SpType:AddField f.Tag is Too Large");
        }
        
        int len = Fields.Length;
        if (len <= idx)
        {
            len = idx + 1;
            var newFields = new SpField[len];
            System.Array.Copy(Fields, newFields, Fields.Length);
            Fields = newFields;
        }
        Fields[idx] = f;
    }

    public SpField GetFieldByName(string name)
    {
        if (Fields == null)
            return null;
        for (int i = 0, count = Fields.Length; i < count; i++)
        {
            var f = Fields[i];
            if (f == null)
                continue;
            if (f.Name == name)
                return f;
        }
        return null;
    }

    public SpField GetFieldByTag(int tag)
    {
        if (Fields == null || tag < 0 || tag >= Fields.Length)
            return null;
        return Fields[tag];
    }

    public bool CheckAndUpdate(SpTypeManager typeManager)
    {
        if (Fields == null)
            return true;
        bool complete = true;
        foreach (SpField f in Fields)
        {
            if (f == null)
                continue;
            if (f.CheckAndUpdate(typeManager))
                continue;
            complete = false;
        }
        return complete;
    }
#endif
}
