using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.UI.Particles.Utils
{
    internal static class CombineInstanceArrayPool
    {
        private static readonly Dictionary<int, CombineInstance[]> s_Pool;

        public static void Init()
        {
            s_Pool.Clear();
        }

        static CombineInstanceArrayPool()
        {
            s_Pool = new Dictionary<int, CombineInstance[]>();
        }

        public static CombineInstance[] Get(List<CombineInstance> src)
        {
            var count = src.Count;
            if (!s_Pool.TryGetValue(count, out var dst))
            {
                dst = new CombineInstance[count];
                s_Pool.Add(count, dst);
            }

            for (var i = 0; i < src.Count; i++)
            {
                dst[i].mesh = src[i].mesh;
                dst[i].transform = src[i].transform;
            }

            return dst;
        }

        public static CombineInstance[] Get(List<CombineInstanceEx> src, int count)
        {
            if (!s_Pool.TryGetValue(count, out var dst))
            {
                dst = new CombineInstance[count];
                s_Pool.Add(count, dst);
            }

            for (var i = 0; i < count; i++)
            {
                dst[i].mesh = src[i].mesh;
                dst[i].transform = src[i].transform;
            }

            return dst;
        }
    }

}