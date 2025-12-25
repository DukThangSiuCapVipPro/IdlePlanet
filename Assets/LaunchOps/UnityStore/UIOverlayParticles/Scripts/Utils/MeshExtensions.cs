using UnityEngine;

namespace Voodoo.UI.Particles.Utils
{
	internal static class MeshExtensions
	{
		public static void Clear(this CombineInstance[] self)
		{
			for (var i = 0; i < self.Length; i++)
			{
				MeshPool.Return(self[i].mesh);
				self[i].mesh = null;
			}
		}
	}
}