using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.UI.Particles.Utils
{
    internal static class MeshPool
    {
        private static readonly Stack<Mesh> s_Pool = new Stack<Mesh>(32);
        private static readonly HashSet<int> s_HashPool = new HashSet<int>();

        public static void Init()
        {
        }

        static MeshPool()
        {
            for (var i = 0; i < 32; i++)
            {
                var m = new Mesh();
                m.MarkDynamic();
                s_Pool.Push(m);
                s_HashPool.Add(m.GetInstanceID());
            }
        }

        public static Mesh Rent()
        {
            Mesh m;
            while (0 < s_Pool.Count)
            {
                m = s_Pool.Pop();
                if (m)
                {
                    s_HashPool.Remove(m.GetInstanceID());
                    return m;
                }
            }

            m = new Mesh();
            m.MarkDynamic();
            return m;
        }

        public static void Return(Mesh mesh)
        {
            if (!mesh) return;

            var id = mesh.GetInstanceID();
            if (s_HashPool.Contains(id)) return;

            mesh.Clear(false);
            s_Pool.Push(mesh);
            s_HashPool.Add(id);
        }
    }
}