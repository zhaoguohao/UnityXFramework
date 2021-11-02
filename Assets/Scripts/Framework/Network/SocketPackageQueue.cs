
using System.Collections.Generic;

/// <summary>
/// Socket包队列
/// </summary>
public class PackageQueue
{
    public PackageQueue()
    {
        @_cmdList = new List<SpStream>();
    }

    public void pushCmd(SpStream result)
    {
        lock (@_dataLocker)
        {
            @_cmdList.Add(result);
        }
    }

    public void getCmdListAndClear(List<SpStream> ret)
    {
        lock (@_dataLocker)
        {
            if (@_cmdList.Count > 0)
            {
                for (int i = 0; i < @_cmdList.Count; ++i)
                {
                    ret.Add(@_cmdList[i]);
                }

                @_cmdList.Clear();
            }
        }
    }

    public void Clear()
    {
        lock (@_dataLocker)
        {
            @_cmdList.Clear();
        }
    }

    private List<SpStream> @_cmdList;

    private byte[] @_dataLocker = new byte[0];
}
