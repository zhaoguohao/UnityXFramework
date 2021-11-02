using System;
using System.Collections.Generic;

class SpStreamCache
{
    const int kSpStreamCacheMax = 10;
    static int sSpStreamCacheFreeIdx = -1;
    static SpStream[] sSpStreamCache = new SpStream[kSpStreamCacheMax];
    static readonly object sSpStreamCacheLockObj = new object();
    static readonly object sSpStreamCacheLockObj2 = new object();

    public static SpStream Get()
    {
        lock (sSpStreamCacheLockObj)
        {
            if (sSpStreamCacheFreeIdx < 0 || sSpStreamCacheFreeIdx >= sSpStreamCache.Length)
                return new SpStream();
            var tmp = sSpStreamCache[sSpStreamCacheFreeIdx];
            sSpStreamCacheFreeIdx--;
            return tmp;
        }
    }

    public static void Collect(SpStream sp)
    {
        if (sp == null)
            return;
        lock (sSpStreamCacheLockObj2)
        {
            int idx = sSpStreamCacheFreeIdx + 1;
            if (idx >= 0 && idx < sSpStreamCache.Length)
            {
                sp.Reset();
                sSpStreamCache[idx] = sp;
                sSpStreamCacheFreeIdx = idx;
            }
        }
    }
}

