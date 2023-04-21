using System;

namespace CosmivengeonMod.Utility.Extensions {
	public static partial class Extensions {
		/// <summary>
		/// I made this fuction because fuck Re-Logic and their "npc.velocity.X = Utils.Clamp(npc.velocity.X, min, max)" BS
		/// </summary>
		/// <typeparam name="T">The type of the value being set.</typeparam>
		/// <param name="value">The value being clamped.</param>
		/// <param name="min">The minimum value to be clamped to.</param>
		/// <param name="max">The maximum value to be clamped to.</param>
		public static void Clamp<T>(this ref T value, T min, T max) where T : struct, IComparable<T>
			=> value = value.CompareTo(max) > 0 ? max : (value.CompareTo(min) < 0 ? min : value);
	}
}
