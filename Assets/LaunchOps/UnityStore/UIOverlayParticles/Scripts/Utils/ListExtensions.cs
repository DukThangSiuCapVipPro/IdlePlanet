using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Voodoo.UI.Particles.Utils
{
	internal static class ListExtensions
	{
		public static bool SequenceEqualFast(this List<bool> self, List<bool> value)
		{
			if (self.Count != value.Count) return false;
			for (var i = 0; i < self.Count; ++i)
			{
				if (self[i] != value[i]) return false;
			}

			return true;
		}

		public static int CountFast(this List<bool> self)
		{
			var count = 0;
			for (var i = 0; i < self.Count; ++i)
			{
				if (self[i]) count++;
			}

			return count;
		}

		public static bool AnyFast<T>(this List<T> self) where T : Object
		{
			for (var i = 0; i < self.Count; ++i)
			{
				if (self[i]) return true;
			}

			return false;
		}

		public static bool AnyFast<T>(this List<T> self, Predicate<T> predicate) where T : Object
		{
			for (var i = 0; i < self.Count; ++i)
			{
				if (self[i] && predicate(self[i])) return true;
			}

			return false;
		}
	}

}