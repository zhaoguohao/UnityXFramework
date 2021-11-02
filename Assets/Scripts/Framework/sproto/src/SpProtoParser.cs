using System.Collections;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public interface SpProtoParserListener {
	void OnNewType(SpType type);
    void OnNewProtocol (SpProtocol protocol);
}

public class SpProtoParser {
	private SpProtoParserListener mListener;
	private static char[] sDelimiters = new char[] {'{', '}', '\n'};
	private static char[] sSpace = new char[] {' ', '\t', '\n'};

    private SpProtocol mCurrentProtocol;
    private SpType mCurrentType;
    private SpType mLastType;

	public SpProtoParser (SpProtoParserListener linstener) {
		mListener = linstener;
	}

	public void Parse (Stream stream) {
		Parse (ReadAll (stream));
	}

	public void Parse (string str) {
		mCurrentProtocol = null;
		mCurrentType = null;
		mLastType = null;

		str = PreProcess (str);
		Scan (str, 0);
	}

	private string ReadAll (Stream stream) {
		string str = "";

		byte[] buf = new byte[1024];
		int len = stream.Read (buf, 0, buf.Length);
		while (len > 0)	{
			str += Encoding.UTF8.GetString (buf, 0, len);
			len = stream.Read (buf, 0, buf.Length);
		}

		return str;
	}

	private string PreProcess (string str) {
		// TODO : trim comment
		return str.Replace ("\r", string.Empty).Trim ();
	}

	private void Scan (string str, int start) {
        System.Collections.Generic.List<SpField> mCurSpFieldList = new System.Collections.Generic.List<SpField>();
        while (start < str.Length)
        {

            int pos = str.IndexOfAny(sDelimiters, start);
            if (pos < 0)
                return;

            switch (str[pos])
            {
                case '{':
                    string title = str.Substring(start, pos - start).Trim();
                    if (IsProtocol(title))
                    {
                        mCurrentProtocol = NewProtocol(title);
                    }
                    else
                    {
                        mLastType = mCurrentType;
                        mCurrentType = NewType(title);
                    }
                    break;
                case '}':
                    if (mCurrentType != null)
                    {
                        if (mCurSpFieldList.Count > 0)
                        {
                            mCurrentType.Fields = mCurSpFieldList.ToArray();
                            mCurSpFieldList.Clear();
                        }
                        mListener.OnNewType(mCurrentType);
                        if (mCurrentProtocol != null)
                            mCurrentProtocol.AddType(mCurrentType);
                        mCurrentType = mLastType;
                        mLastType = null;
                    }
                    else if (mCurrentProtocol != null)
                    {
                        mListener.OnNewProtocol(mCurrentProtocol);
                        mCurrentProtocol = null;
                    }
                    break;
                case '\n':
                    SpField f = NewField(str, start, pos);
                    if (f!=null)
                    {
                        int curCount = mCurSpFieldList.Count; 
                        if (f.Tag == curCount){
                            mCurSpFieldList.Add(f);
                        }else if (f.Tag < curCount){
                            mCurSpFieldList[f.Tag] = f;
                        }else if (f.Tag > curCount){
                            for (int i = curCount; i < f.Tag; i++)
                                mCurSpFieldList.Add(null);
                            mCurSpFieldList.Add(f);
                        }
                    }

                    //if (f != null && mCurrentType != null)
                    //{
                    //    mCurrentType.AddField(f);
                    //}
                    break;
            }

            start = pos + 1;
        }
		//Scan (str, start);
	}

	private bool IsProtocol (string str) {
		return (str.IndexOfAny (sSpace) >= 0);
	}

    private SpProtocol NewProtocol (string str) {
        string[] words = str.Split (sSpace);
        if (words.Length != 2)
            return null;

        SpProtocol protocol = new SpProtocol (words[0], int.Parse (words[1]));
        return protocol;
    }

	private SpType NewType (string str) {
        if (str[0] == '.') {
            str = str.Substring (1);
        }
        else {
            if (mLastType != null)
                str = mLastType.Name + "." + str;

            if (mCurrentProtocol != null)
                str = mCurrentProtocol.Name + "." + str;
        }

		SpType t = new SpType (str);
		return t;
	}


    private static char[] sFieldSpace = new char[] { ' ', '\t', '\n', ':' };
    private static string[] SplitSpace(string str)
    {
        return str.Split(sFieldSpace, StringSplitOptions.RemoveEmptyEntries);
    }

    private SpField NewField(string str, int startPos, int endPos)
    {
        str = str.Substring(startPos, endPos - startPos);
        string[] words = null;
        words = SplitSpace(str);
        if (words == null)
            return null;
		if (words.Length != 3)
			return null;

		string name = words[0];
        short tag = short.Parse (words[1]);
		string type = words[2];
		bool array = false;
		if (type[0] == '*') {
			array = true;
			type = type.Substring (1);
		}
        SpField f = new SpField (name, tag, type, array);
		return f;
	}
}
